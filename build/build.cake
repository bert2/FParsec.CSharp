#tool "nuget:?package=GitVersion.CommandLine"
#load "prompt.cake"

var target = Argument("target", "Default");
var config = Argument("configuration", "Release");
var nugetKey = Argument<string>("nugetKey", null) ?? EnvironmentVariable("nuget_key");
var srcDir = Directory("../src");
var libDir = srcDir + Directory("FParsec.CSharp");
var pkgDir = libDir + Directory($"bin/{config}");
GitVersion semVer = null;

Task("SemVer").Does(() => {
    semVer = GitVersion();
    Information(semVer.FullSemVer);
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
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .Does(() => {
        Information($"Packing {semVer.NuGetVersion} to nuget.org");

        var msbuildSettings = new DotNetCoreMSBuildSettings();
        msbuildSettings.Properties.Add("PackageVersion", new[] { semVer.NuGetVersion });
        //msbuildSettings.Properties.Add("PackageReleaseNotes", new[] { latestCommitMsg });

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
