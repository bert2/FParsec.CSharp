#tool nuget:?package=GitVersion.CommandLine&version=5.6.3
#tool Codecov
#addin Cake.Codecov
#addin Cake.Git
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
    .Does(() =>
        DotNetCoreClean(srcDir, new DotNetCoreCleanSettings {
            Configuration = config,
            Verbosity = DotNetCoreVerbosity.Minimal
        }));

Task("Build")
    .IsDependentOn("SemVer")
    .Does(() =>
        DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
            Configuration = config,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .SetVersion(semVer.AssemblySemVer)
        }));

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
        DotNetCoreTest(srcDir, new DotNetCoreTestSettings {
            Configuration = config,
            NoBuild = true,
            ArgumentCustomization = args => {
                var msbuildSettings = new DotNetCoreMSBuildSettings()
                    .WithProperty("CollectCoverage", new[] { "true" })
                    .WithProperty("CoverletOutputFormat", new[] { "opencover" });
                args.AppendMSBuildSettings(msbuildSettings, environment: null);
                return args;
            }
        }));

Task("UploadCoverage")
    .Does(() =>
        Codecov(testDir + File("coverage.opencover.xml")));

Task("Pack-FParsec.CSharp")
    .IsDependentOn("SemVer")
    .Does(() => {
        var relNotes = FormatReleaseNotes(lastCommitMsg);
        Information($"Packing {semVer.NuGetVersion} ({relNotes})");

        var pkgName = "FParsec.CSharp";
        var pkgDesc = "FParsec.CSharp is a thin C# wrapper for FParsec.";
        var pkgTags = "parser; parser combinator; c#; csharp; parsec; fparsec";
        var pkgAuthors = "Robert Hofmann";
        var docUrl = "https://github.com/bert2/FParsec.CSharp";
        var repoUrl = "https://github.com/bert2/FParsec.CSharp.git";
        var libDir = srcDir + Directory(pkgName);
        var pkgDir = libDir + Directory($"bin/{config}");

        var msbuildSettings = new DotNetCoreMSBuildSettings()
            // Cannot set PackageId here due to error "Ambiguous project name 'FParsec.CSharp'"
        	//.WithProperty("PackageId",                new[] { pkgName })
            .SetVersion(semVer.AssemblySemVer) // have to set assembly version here, because pack needs to rebuild
        	.WithProperty("PackageVersion",           new[] { semVer.NuGetVersion })
        	.WithProperty("Title",                    new[] { pkgName })
        	.WithProperty("Description",              new[] { $"{pkgDesc}\r\n\r\nDocumentation: {docUrl}\r\n\r\nRelease notes: {relNotes}" })
        	.WithProperty("PackageTags",              new[] { pkgTags })
        	.WithProperty("PackageReleaseNotes",      new[] { relNotes })
        	.WithProperty("Authors",                  new[] { pkgAuthors })
        	.WithProperty("RepositoryUrl",            new[] { repoUrl })
        	.WithProperty("RepositoryCommit",         new[] { lastCommitSha })
        	.WithProperty("PackageLicenseExpression", new[] { "MIT" })
        	.WithProperty("IncludeSource",            new[] { "true" })
        	.WithProperty("IncludeSymbols",           new[] { "true" })
        	.WithProperty("SymbolPackageFormat",      new[] { "snupkg" });

        DotNetCorePack(libDir, new DotNetCorePackSettings {
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

        DotNetCoreNuGetPush(
            pkgDir + File($"{pkgName}.{semVer.NuGetVersion}.nupkg"),
            new DotNetCoreNuGetPushSettings {
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
