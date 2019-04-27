using static System.Threading.Tasks.Task;
#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetKey = Argument<string>("nugetKey", null) ?? EnvironmentVariable("nugetKey");
var srcDir = Directory("../src");
var libDir = srcDir + Directory("FParsec.CSharp");
var pkgDir = libDir + Directory($"bin/{configuration}");
var cleanSettings = new DotNetCoreCleanSettings {
    Configuration = configuration, Verbosity = DotNetCoreVerbosity.Minimal };
var buildSettings = new DotNetCoreBuildSettings { Configuration = configuration };
var testSettings = new DotNetCoreTestSettings { Configuration = configuration };
GitVersion semVer = null;

Task("SemVer").Does(() => {
    semVer = GitVersion();
    Information(semVer.FullSemVer);
});

Task("Clean").Does(() =>
    DotNetCoreClean(srcDir, cleanSettings));

Task("Build").Does(() =>
    DotNetCoreBuild(srcDir, buildSettings));

Task("Test").Does(() =>
    DotNetCoreTest(srcDir, testSettings));

Task("Pack")
    .IsDependentOn("SemVer")
    .IsDependentOn("Clean")
    .IsDependentOn("Test")
    .Does(() => {
        Information($"Packing {semVer.NuGetVersion} to nuget.org");

        var msbuildSettings = new DotNetCoreMSBuildSettings();
        msbuildSettings.Properties.Add("PackageVersion", new[] { semVer.NuGetVersion });

        DotNetCorePack(libDir, new DotNetCorePackSettings {
            Configuration = configuration,
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

string Prompt(string message, int timeoutSeconds = 60) {
    Console.Write(message);

    string response = null;

    WhenAny(
        Run(() => response = Console.ReadLine()),
        Delay(TimeSpan.FromSeconds(timeoutSeconds))
    ).Wait();

    if (response == null)
        throw new Exception($"User prompt timed out.");

    return response;
}
