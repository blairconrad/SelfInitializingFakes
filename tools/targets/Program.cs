using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Bullseye.Targets;
using static SimpleExec.Command;

internal class Program
{
    public static void Main(string[] args)
    {
        var testProjectDirectories = Directory.GetDirectories("tests");
        var mainProjectFile = "src/SelfInitializingFakes/SelfInitializingFakes.csproj";
        var releaseNotesFile = "./release_notes.md";
        var solutionFile = "./SelfInitializingFakes.sln";
        var versionInfoFile = "./src/VersionInfo.cs";

        var logsDirectory = "./artifacts/logs";
        var outputDirectory = Path.GetFullPath("./artifacts/output");
        var testsDirectory = "./artifacts/tests";

        string version = null;

        Target("default", DependsOn("pack", "test"));

        Target("outputDirectory", () => Directory.CreateDirectory(outputDirectory));

        Target("logsDirectory", () => Directory.CreateDirectory(logsDirectory));

        Target("testsDirectory", () => Directory.CreateDirectory(testsDirectory));

        Target(
            "versionInfoFile",
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

        Target(
            "build",
            DependsOn("versionInfoFile", "logsDirectory"),
            () => Run(
                "dotnet",
                $"build {solutionFile} /p:Configuration=Release /nologo /m /v:m " +
                    $"/fl /flp:LogFile={logsDirectory}/build.log;Verbosity=Detailed;PerformanceSummary"));

        Target(
            "pack",
            DependsOn("build", "outputDirectory", "readVersion"),
            () =>
            {
                Run("dotnet", $"pack {mainProjectFile} --configuration Release --no-build --output {outputDirectory} /p:Version={version}");
            });

        Target(
            "test",
            DependsOn("build", "testsDirectory"),
            forEach: testProjectDirectories,
            action: testProjectDirectory => Run("dotnet", $"test --configuration Release", testProjectDirectory));

        Target(
            "readVersion", () =>
            {
                var versionFromReleaseNotes = File.ReadLines(releaseNotesFile, Encoding.UTF8)
                    .First(line => line.StartsWith("## ", StringComparison.Ordinal)).Substring(3).Trim();
                Console.WriteLine($"Read version '{versionFromReleaseNotes}' from release notes");
                var tagName = Environment.GetEnvironmentVariable("APPVEYOR_REPO_TAG_NAME");
                if (versionFromReleaseNotes != tagName)
                {
                    Console.WriteLine($"Release notes version does not match tag name '{tagName}'. Disambiguating.");
                    if (!versionFromReleaseNotes.Contains('-', StringComparison.Ordinal))
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

        RunTargets(args);
    }
}
