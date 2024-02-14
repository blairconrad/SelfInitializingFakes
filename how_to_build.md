# How to build

These instructions are *only* for building from the command line, which includes compilation, test execution and packaging. This is the simplest way to build.
It also replicates the build on the Continuous Integration build server and is the best indicator of whether a pull request will build.

You can also build the solution using Visual Studio 2019 or later, but this doesn't provide the same assurances as the command line build.

At the time of writing the build is only confirmed to work on Windows using the Microsoft .NET framework.

## Prerequisites

The build requires that a few pieces of software be installed on the host computer. We're somewhat aggressive about adoption new language features and the like, so rather than specifying exactly which versions are required, we'll tend toward
"latest" or "at least" forms of guidance. If it seems you have an incompatible version of the software, prefer to upgrade rather than downgrade.

Ensure that recent versions of the following are installed:

1. Visual Studio 2022 or the Build Tools for Visual Studio 2022
1. .NET Core 1.0, 2.0, and 3.1 runtimes
1. .NET 7 SDK

## Building

Using a command prompt, navigate to your clone root folder and execute `build.cmd`.

This executes the default build targets to produce both .NET Standard and .NET Framework artifacts.

After the build has completed, the build artifacts will be located in `artifacts`.
