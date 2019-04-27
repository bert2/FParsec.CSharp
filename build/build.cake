#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "All");
var configuration = Argument("configuration", "Release");
var srcDir = Directory("../src");

Task("SemVer")
    .Does(() => Information(GitVersion().FullSemVer));

Task("Clean")
    .Does(() => DotNetCoreClean(srcDir));

Task("Restore")
    .Does(() => DotNetCoreRestore(srcDir));

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() => DotNetCoreBuild(srcDir));

Task("Test")
    .IsDependentOn("Build")
    .Does(() => DotNetCoreTest(srcDir));

Task("All")
    .IsDependentOn("Test");

RunTarget(target);
