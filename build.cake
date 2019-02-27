#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var outputDir = "./artifacts/";
var isAppVeyor = BuildSystem.IsRunningOnAppVeyor;
var isWindows = IsRunningOnWindows();

// libs come here
var libs = new List<string>{
    "./src/Fernet/Fernet.csproj"
};

Task("Clean")
    .Does(() => {
        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir, recursive:true);
        }
    });

Task("Restore")
    .Does(() => {
        DotNetCoreRestore("./Fernet.sln", new DotNetCoreRestoreSettings{
            Verbosity = DotNetCoreVerbosity.Minimal,
        });
    });

GitVersion versionInfo = null;
DotNetCoreMSBuildSettings msBuildSettings = null;

Task("UpdateVersionInfo")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        var tag = AppVeyor.Environment.Repository.Tag.Name;
        AppVeyor.UpdateBuildVersion(tag);
    });

Task("Version")
    .IsDependentOn("Restore")
    .Does(() => {
        GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = false,
            OutputType = GitVersionOutput.BuildServer
        });

        versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });

        Information(versionInfo);

        msBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", versionInfo.Major + "." + versionInfo.Minor + "." + versionInfo.BuildMetaData)
            .WithProperty("AssemblyVersion", versionInfo.AssemblySemVer)
            .WithProperty("FileVersion", versionInfo.AssemblySemVer);
    });

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .IsDependentOn("Restore")
    .Does(() => {
        DotNetCoreBuild("./Fernet.sln", new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
            MSBuildSettings = msBuildSettings
        });
    });

Task("Package")
    .IsDependentOn("Build")
    .Does(() => {
        foreach (var lib in libs)
            DotNetCorePack(lib, new DotNetCorePackSettings
            {
                Configuration = configuration,
                OutputDirectory = outputDir,
                MSBuildSettings = msBuildSettings
            });

        if (!isWindows) return;

        if (isAppVeyor)
        {
            foreach (var file in GetFiles(outputDir + "**/*"))
                AppVeyor.UploadArtifact(file.FullPath);
        }
    });

Task("DeployNuget")
    .IsDependentOn("Package")
    .Does(() =>
    {
        if (isAppVeyor)
        {
            foreach (var file in GetFiles(outputDir + "**/*.nupkg")) {
                Information(file.ToString());
                NuGetPush(
                    file,
                    new NuGetPushSettings {
                        ApiKey = EnvironmentVariable("NuGetApiKey"),
                        Source = "https://www.nuget.org/api/v2/package"
                });
            }
        }

    });

Task("Default")
    .IsDependentOn("DeployNuget");

RunTarget(target);
