#load "packages/simple-targets-csx.5.2.0/simple-targets.csx"

#r "System.Runtime.Serialization"
#r "System.Xml.Linq"

using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using static SimpleTargets;

// options
var solutionName = "SelfInitializingFakes";
var frameworks = new[] { "net452", "netcoreapp1.0" };

// solution file locations
var nuspecFiles = new [] { "src/SelfInitializingFakes.nuspec" };
var testProjectDirectories = new[] { "tests/SelfInitializingFakes.Tests" };
var mainProjectFile = "src/SelfInitializingFakes/SelfInitializingFakes.csproj";
var releaseNotesFile = "./release_notes.md";
var solutionFile = "./" + solutionName + ".sln";
var versionInfoFile = "./src/VersionInfo.cs";

// tool locations
var gitversion = @".\packages\GitVersion.CommandLine.4.0.0-beta0011\tools\GitVersion.exe";
var nuget = @".\.nuget\NuGet.exe";

// artifact locations
var logsDirectory = "./artifacts/logs";
var outputDirectory = "./artifacts/output";
var testsDirectory = "./artifacts/tests";

// targets
var targets = new TargetDictionary();

targets.Add("default", DependsOn("pack", "test"));

targets.Add("outputDirectory", () => Directory.CreateDirectory(outputDirectory));

targets.Add("logsDirectory", () => Directory.CreateDirectory(logsDirectory));

targets.Add("testsDirectory", () => Directory.CreateDirectory(testsDirectory));

targets.Add("versionInfoFile",
    () => Cmd(gitversion, $"/updateAssemblyInfo {versionInfoFile} /ensureAssemblyInfo"));

targets.Add(
    "restore",
    () => Cmd("dotnet", $"restore {solutionFile} --packages packages"));

targets.Add(
    "build",
    DependsOn("restore", "versionInfoFile", "logsDirectory"),
    () => Cmd(
        "dotnet",
        $"build {solutionFile} /p:Configuration=Release /nologo /m /v:m " +
            $"/fl /flp:LogFile={logsDirectory}/build.log;Verbosity=Detailed;PerformanceSummary"));

targets.Add(
    "pack",
    DependsOn("build", "outputDirectory"),
    () =>
    {
        var fakeItEasyVersion = GetDependencyVersion("FakeItEasy");
        foreach (var nuspecFile in nuspecFiles)
        {
            Cmd(gitversion, $@"/exec cmd /execargs ""/c {nuget} pack {nuspecFile} -Version %GitVersion_NuGetVersionV2% -OutputDirectory {outputDirectory} -NoPackageAnalysis -Properties FakeItEasyVersion={fakeItEasyVersion}""");
        }
    });

targets.Add(
    "test",
    DependsOn("build", "testsDirectory"),
    () =>
    {
        foreach (var testProjectDirectory in testProjectDirectories)
        {
            var outputBase = Path.GetFullPath(Path.Combine(testsDirectory, Path.GetFileName(testProjectDirectory)));
            Cmd(testProjectDirectory, "dotnet", $"xunit -configuration Release -nologo -xml {outputBase}.xml -html {outputBase}.html");
        }
    });

Run(Args, targets);

// helpers
public void Cmd(string fileName, string args)
{
    Cmd(".", fileName, args);
}

public void Cmd(string workingDirectory, string fileName, string args)
{
    using (var process = new Process())
    {
        process.StartInfo = new ProcessStartInfo
        {
            FileName = $"\"{fileName}\"",
            Arguments = args,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };

        var workingDirectoryMessage = workingDirectory == "." ? "" : $" in '{process.StartInfo.WorkingDirectory}'";
        Console.WriteLine($"Running '{process.StartInfo.FileName} {process.StartInfo.Arguments}'{workingDirectoryMessage}...");
        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"The command exited with code {process.ExitCode}.");
        }
    }
}

public string GetDependencyVersion(string packageName)
{
    var xml = XDocument.Load(mainProjectFile);

    return  xml.Root.Elements("ItemGroup")
                .SelectMany(i=>i.Elements("PackageReference"))
                .Where(pr=>pr.Attribute("Include").Value == packageName)
                .Single()
                .Attribute("Version")
                .Value;
}
