#addin Cake.Git
#tool GitVersion.CommandLine
#load prompt.cake
#load format-rel-notes.cake

var target = Argument("target", "Default");
var config = Argument("configuration", "Release");
var nugetKey = Argument<string>("nugetKey", null) ?? EnvironmentVariable("nuget_key");
var lastCommit = EnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE");
var rootDir = Directory("..");
var srcDir = rootDir + Directory("src");
var libDir = srcDir + Directory("FParsec.CSharp");
var pkgDir = libDir + Directory($"bin/{config}");
GitVersion semVer = null;

Task("SemVer").Does(() => {
    semVer = GitVersion();
    lastCommit = lastCommit ?? GitLogTip(rootDir).MessageShort;
    Information($"{semVer.FullSemVer} ({lastCommit})");
});

Task("Clean").Does(() =>
    DotNetCoreClean(srcDir, new DotNetCoreCleanSettings {
        Configuration = config,
        Verbosity = DotNetCoreVerbosity.Minimal
    }));

Task("Build").Does(() =>
    DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
        Configuration = config
    }));

Task("Test")
    .IsDependentOn("Build")
    .Does(() => DotNetCoreTest(srcDir, new DotNetCoreTestSettings {
        Configuration = config,
        NoBuild = true
    }));

Task("Pack")
    .IsDependentOn("SemVer")
    .Does(() => {
        var relNotes = FormatReleaseNotes(lastCommit);
        Information($"Packing {semVer.NuGetVersion} ({relNotes})");

        var msbuildSettings = new DotNetCoreMSBuildSettings();
        msbuildSettings.Properties["PackageVersion"] = new[] { semVer.NuGetVersion };
        msbuildSettings.Properties["PackageReleaseNotes"] = new[] { relNotes };
        msbuildSettings.Properties["PackageDescription"] = new[] {
$@"A thin C# wrapper for Parsec.

Documentation: https://github.com/bert2/FParsec.CSharp

Changed in v{semVer.NuGetVersion}: {relNotes}" };

        DotNetCorePack(libDir, new DotNetCorePackSettings {
            Configuration = config,
            OutputDirectory = pkgDir,
            NoBuild = true,
            NoDependencies = false,
            MSBuildSettings = msbuildSettings
        });
    });

Task("Release")
    .IsDependentOn("Pack")
    .Does(() => {
        if (lastCommit.Contains("without release")) {
            Information($"Skipping release to nuget.org");
            return;
        }

        Information($"Releasing {semVer.NuGetVersion} to nuget.org");

        if (string.IsNullOrEmpty(nugetKey))
            nugetKey = Prompt("Enter nuget API key: ");

        DotNetCoreNuGetPush(
            pkgDir + File($"FParsec.CSharp.{semVer.NuGetVersion}.nupkg"),
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

RunTarget(target);
