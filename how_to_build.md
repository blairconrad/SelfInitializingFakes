# How to build

At the time of writing the build is only confirmed to work on Windows using the Microsoft .NET framework.

## Prerequisites

1. Ensure you have .NET framework 4.x installed.

1. Ensure you have Visual Studio 2015 or MSBuild 14 installed.

## Building

Using a command prompt, navigate to your clone root folder and execute the Powershell script `build.ps1`.

This executes the default build tasks to produce both the .NET Standard and the .NET 4.0 artifacts.

After the build has completed, the build artifacts will be located in `artifacts`.
