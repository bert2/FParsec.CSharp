var target = Argument("target", "All");
var configuration = Argument("configuration", "Release");
var srcDir = Directory("../src");

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
