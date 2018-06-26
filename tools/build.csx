#r "../packages/Bullseye.1.0.0-rc.4/lib/netstandard2.0/Bullseye.dll"
#r "../packages/SimpleExec.2.2.0/lib/netstandard2.0/SimpleExec.dll"

#r "System.Runtime.Serialization"
#r "System.Xml.Linq"

using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using Bullseye;
using static Bullseye.Targets;
using static SimpleExec.Command;

// options
var solutionName = "SelfInitializingFakes";
var frameworks = new[] { "net452", "netcoreapp1.0" };

// solution file locations
var testProjectDirectories = Directory.GetDirectories("tests");
var mainProjectFile = "src/SelfInitializingFakes/SelfInitializingFakes.csproj";
var releaseNotesFile = "./release_notes.md";
var solutionFile = "./" + solutionName + ".sln";
var versionInfoFile = "./src/VersionInfo.cs";

// tool locations
var nuget = @".\.nuget\NuGet.exe";

// artifact locations
var logsDirectory = "./artifacts/logs";
var outputDirectory = Path.GetFullPath("./artifacts/output");
var testsDirectory = "./artifacts/tests";

string version;

// targets
Targets.Add("default", DependsOn("pack", "test"));

Targets.Add("outputDirectory", () => Directory.CreateDirectory(outputDirectory));

Targets.Add("logsDirectory", () => Directory.CreateDirectory(logsDirectory));

Targets.Add("testsDirectory", () => Directory.CreateDirectory(testsDirectory));

Targets.Add("versionInfoFile",
    DependsOn("readVersion"),
    () =>
    {
        var assemblyVersion = version.Split('-', '+')[0];
        var assemblyFileVersion = assemblyVersion;
        var assemblyInformationalVersion = version;
        var versionContents =
$@"using System.Reflection;

[assembly: AssemblyVersion(""{assemblyVersion}"")]
[assembly: AssemblyFileVersion(""{assemblyFileVersion}"")]
[assembly: AssemblyInformationalVersion(""{assemblyInformationalVersion}"")]
";
        if (!File.Exists(versionInfoFile) || versionContents != File.ReadAllText(versionInfoFile, Encoding.UTF8))
        {
            File.WriteAllText(versionInfoFile, versionContents, Encoding.UTF8);
        }
    });

Targets.Add(
    "restore",
    () => Run("dotnet", $"restore {solutionFile} --packages packages"));

Targets.Add(
    "build",
    DependsOn("restore", "versionInfoFile", "logsDirectory"),
    () => Run(
        "dotnet",
        $"build {solutionFile} /p:Configuration=Release /nologo /m /v:m " +
            $"/fl /flp:LogFile={logsDirectory}/build.log;Verbosity=Detailed;PerformanceSummary"));

Targets.Add(
    "pack",
    DependsOn("build", "outputDirectory", "readVersion"),
    () =>
    {
        Run("dotnet", $"pack {mainProjectFile} --configuration Release --no-build --output {outputDirectory} /p:Version={version}");
    });

Targets.Add(
    "test",
    DependsOn("build", "testsDirectory"),
    () =>
    {
        foreach (var testProjectDirectory in testProjectDirectories)
        {
            var outputBase = Path.GetFullPath(Path.Combine(testsDirectory, Path.GetFileName(testProjectDirectory)));
            Run("dotnet", $"xunit -configuration Release -nologo -xml {outputBase}.xml -html {outputBase}.html", testProjectDirectory);
        }
    });

Targets.Add(
    "readVersion", () =>
    {
        var versionFromReleaseNotes = File.ReadLines(releaseNotesFile, Encoding.UTF8)
             .First(line => line.StartsWith("## ")).Substring(3).Trim();
        Console.WriteLine($"Read version '{versionFromReleaseNotes}' from release notes");
        var tagName = Environment.GetEnvironmentVariable("APPVEYOR_REPO_TAG_NAME");
        if (versionFromReleaseNotes != tagName)
        {
            Console.WriteLine($"Release notes version does not match tag name '{tagName}'. Disambiguating.");
            if (!versionFromReleaseNotes.Contains('-'))
            {
                versionFromReleaseNotes += "-adhoc";
            }

            version = versionFromReleaseNotes +
                "+Build." + (Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER") ?? "adhoc") +
                "-Sha." + (Environment.GetEnvironmentVariable("APPVEYOR_REPO_COMMIT") ?? "adhoc");
        }
        else
        {
            version = versionFromReleaseNotes;
        }
    });

Targets.Run(Args);
