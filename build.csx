#load "packages/simple-targets-csharp.4.0.0/simple-targets-csharp.csx"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static SimpleTargets;

// options
var solutionName = "SelfInitializingFakes";

// locations
var nuspecs = new string[0];
var acceptanceTests = new[] { "tests/SelfInitializingFakes.AcceptanceTests" };

var solution = "./" + solutionName + ".sln";
var logs = "./artifacts/logs";
var msBuild = $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}/MSBuild/14.0/Bin/msbuild.exe";
var output = "./artifacts/output";
var nuget = "./.nuget/NuGet.exe";
var xunit = "./packages/xunit.runner.console.2.1.0/tools/xunit.console.exe";

// version
var versionSuffix = Environment.GetEnvironmentVariable("VERSION_SUFFIX") ?? "";
var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "000000";
var buildNumberSuffix = versionSuffix == "" ? "" : "-build" + buildNumber;
var version = "0.1.0";
// File.ReadAllText("src/CommonAssemblyInfo.cs")
//    .Split(new[] { "AssemblyInformationalVersion(\"" }, 2, StringSplitOptions.RemoveEmptyEntries)[1]
//    .Split('\"').First() + versionSuffix + buildNumberSuffix;

// targets
var targets = new TargetDictionary();

targets.Add("default", DependsOn("pack", "accept"));

targets.Add("logs", () => Directory.CreateDirectory(logs));

targets.Add(
    "build",
    DependsOn("logs"),
    () => Cmd(
        msBuild,
        $"{solution} /p:Configuration=Release /nologo /m /v:m /nr:false " +
            $"/fl /flp:LogFile={logs}/msbuild.log;Verbosity=Detailed;PerformanceSummary"));

targets.Add("output", () => Directory.CreateDirectory(output));

targets.Add(
    "pack",
    DependsOn("build", "output"),
    () =>
    {
        foreach (var nuspec in nuspecs)
        {
            var originalNuspec = $"{nuspec}.original";
            File.Move(nuspec, originalNuspec);
            var originalContent = File.ReadAllText(originalNuspec);
            var content = originalContent.Replace("[99.99.99-dev]", $"[{version}]");
            File.WriteAllText(nuspec, content);
            try
            {
                Cmd(nuget, $"pack {nuspec} -Version {version} -OutputDirectory {output} -NoPackageAnalysis");
            }
            finally
            {
                File.Delete(nuspec);
                File.Move(originalNuspec, nuspec);
            }
        }
    });

targets.Add(
    "accept",
    DependsOn("build"),
    () =>
    {
        foreach (var testDir in acceptanceTests)
        {
            RunDotNet(testDir, "test", "-c Release");
        }
    });

Run(Args, targets);

// helpers
public static void Cmd(string fileName, string args)
{
    using (var process = new Process())
    {
        process.StartInfo = new ProcessStartInfo { FileName = $"\"{fileName}\"", Arguments = args, UseShellExecute = false, };
        Console.WriteLine($"Running '{process.StartInfo.FileName} {process.StartInfo.Arguments}'...");
        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"The command exited with code {process.ExitCode}.");
        }
    }
}

public static void RunDotNet(string workingDirectory, string command, string args="")
{
    using (var process = new Process())
    {
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = command + " " + args,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };

        Console.WriteLine($"Running '{process.StartInfo.FileName} {process.StartInfo.Arguments}' in '{process.StartInfo.WorkingDirectory}'...");
        process.Start();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"The command exited with code {process.ExitCode}.");
        }
    }
}
