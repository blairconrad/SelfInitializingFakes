using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bullseye;
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

        Targets.Add("default", DependsOn("pack", "test"));

        Targets.Add("outputDirectory", () => Directory.CreateDirectory(outputDirectory));

        Targets.Add("logsDirectory", () => Directory.CreateDirectory(logsDirectory));

        Targets.Add("testsDirectory", () => Directory.CreateDirectory(testsDirectory));

        Targets.Add(
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

        Targets.Add(
            "build",
            DependsOn("versionInfoFile", "logsDirectory"),
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
                    Run("dotnet", $"test --configuration Release", testProjectDirectory);
                }
            });

        Targets.Add(
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

        Targets.Run(args);
    }

    private static string[] DependsOn(params string[] dependencies) => Targets.DependsOn(dependencies);
}
