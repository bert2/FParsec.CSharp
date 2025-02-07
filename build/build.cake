#tool nuget:?package=GitVersion.CommandLine&version=5.8.1
#tool nuget:?package=Codecov&version=1.13.0
#addin nuget:?package=Cake.Codecov&version=1.0.1
#addin nuget:?package=Cake.Git&version=2.0.0
#load prompt.cake
#load format-rel-notes.cake

var target = Argument("target", "Default");
var config = Argument("configuration", "Release");
var nugetKey = Argument<string>("nugetKey", null) ?? EnvironmentVariable("nuget_key");

var rootDir = Directory("..");
var srcDir = rootDir + Directory("src");
var testDir = srcDir + Directory("Tests");

var lastCommitMsg = EnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE") ?? GitLogTip(rootDir).MessageShort;
var lastCommitSha = EnvironmentVariable("APPVEYOR_REPO_COMMIT") ?? GitLogTip(rootDir).Sha;
var currBranch = GitBranchCurrent(rootDir).FriendlyName;
GitVersion semVer = null;

Task("SemVer")
    .Does(() => {
        semVer = GitVersion();
        Information($"{semVer.FullSemVer} ({lastCommitMsg})");
    });

Task("Clean")
    .Does(() => DotNetClean(srcDir, new DotNetCleanSettings {
        Configuration = config,
        Verbosity = DotNetVerbosity.Minimal
    }));

Task("Build")
    .IsDependentOn("SemVer")
    .Does(() => DotNetBuild(srcDir, new DotNetBuildSettings {
        Configuration = config,
        MSBuildSettings = new DotNetMSBuildSettings()
            .SetVersion(semVer.AssemblySemVer)
    }));

Task("Test")
    .IsDependentOn("Build")
    .Does(() => DotNetTest(srcDir, new DotNetTestSettings {
        Configuration = config,
        NoBuild = true,
        ArgumentCustomization = args => {
            var msbuildSettings = new DotNetMSBuildSettings()
                .WithProperty("CollectCoverage", "true")
                .WithProperty("CoverletOutputFormat", "opencover");
            args.AppendMSBuildSettings(msbuildSettings, environment: null);
            return args;
        }
    }));

Task("UploadCoverage")
    .Does(() => Codecov(new[] {
        testDir + File("coverage.net9.0.opencover.xml"),
        testDir + File("coverage.net481.opencover.xml")
    }.Select(f => f.ToString())));

Task("Pack-FParsec.CSharp")
    .IsDependentOn("SemVer")
    .Does(() => {
        var relNotes = FormatReleaseNotes(lastCommitMsg);
        Information($"Packing {semVer.NuGetVersion} ({relNotes})");

        var pkgName = "FParsec.CSharp";
        var pkgDesc = "FParsec.CSharp is a thin C# wrapper for FParsec.";
        var pkgTags = "parser; parser combinator; c#; csharp; parsec; fparsec";
        var pkgAuthors = "Robert Hofmann";
        var repoUrl = "https://github.com/bert2/FParsec.CSharp";
        var libDir = srcDir + Directory(pkgName);
        var pkgDir = libDir + Directory($"bin/{config}");

        var msbuildSettings = new DotNetMSBuildSettings()
            // Cannot set PackageId here due to error "Ambiguous project name 'FParsec.CSharp'"
        	//.WithProperty("PackageId",                new[] { pkgName })
            .SetVersion(semVer.AssemblySemVer) // have to set assembly version here, because pack needs to rebuild
        	.WithProperty("PackageVersion",           semVer.NuGetVersion)
        	.WithProperty("Title",                    pkgName)
        	.WithProperty("Description",              $"{pkgDesc}\r\n\r\nDocumentation: {repoUrl}\r\n\r\nRelease notes: {relNotes}")
        	.WithProperty("PackageTags",              pkgTags)
        	.WithProperty("PackageReleaseNotes",      relNotes)
        	.WithProperty("Authors",                  pkgAuthors)
        	.WithProperty("PackageProjectUrl",        repoUrl)
        	.WithProperty("RepositoryCommit",         lastCommitSha)
        	.WithProperty("PackageLicenseExpression", "MIT")
        	.WithProperty("IncludeSource",            "true")
        	.WithProperty("IncludeSymbols",           "true")
        	.WithProperty("SymbolPackageFormat",      "snupkg");

        DotNetPack(libDir, new DotNetPackSettings {
            Configuration = config,
            OutputDirectory = pkgDir,
            NoBuild = false, // cannot disable, because of work-around to include LambdaConvert.dll
            NoDependencies = false,
            MSBuildSettings = msbuildSettings
        });
    });

Task("Release-FParsec.CSharp")
    .IsDependentOn("Pack-FParsec.CSharp")
    .Does(() => {
        if (currBranch != "master") {
            Information($"Will not release package built from branch '{currBranch}'.");
            return;
        }

        if (lastCommitMsg.Contains("without release")) {
            Information($"Skipping release to nuget.org");
            return;
        }

        Information($"Releasing {semVer.NuGetVersion} to nuget.org");

        if (string.IsNullOrEmpty(nugetKey))
            nugetKey = Prompt("Enter nuget API key: ");

        var pkgName = "FParsec.CSharp";
        var libDir = srcDir + Directory(pkgName);
        var pkgDir = libDir + Directory($"bin/{config}");

        DotNetNuGetPush(
            pkgDir + File($"{pkgName}.{semVer.NuGetVersion}.nupkg"),
            new DotNetNuGetPushSettings {
                Source = "nuget.org",
                ApiKey = nugetKey
            });
    });

Task("Default")
    .IsDependentOn("SemVer")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("Release")
    .IsDependentOn("UploadCoverage")
    .IsDependentOn("Release-FParsec.CSharp");

RunTarget(target);
