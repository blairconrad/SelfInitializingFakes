# How to build

These instructions are *only* for building from the command line, which includes compilation, test execution and packaging.
This is the simplest way to build.
It also replicates the build on the Continuous Integration build server (GitHub Actions) and is the best indicator of whether a pull request will build.

You may build the solution using a modern IDE, but this doesn't provide the same assurances as the command line build.

## Prerequisites

The build requires that a few pieces of software be installed on the host computer.
We're somewhat aggressive about adoption new language features and the like,
so rather than specifying exactly which versions are required, we'll tend toward
"latest" or "at least" forms of guidance.
If it seems you have an incompatible version of the software, prefer to upgrade rather than downgrade.

Ensure that the following are installed:

1. a recent version of the .NET SDK (currently targeting .NET 10.0)

## Building

Using a command prompt, navigate to your clone root folder and execute `./build.ps1`.

This executes the default build targets to produce .NET artifacts and runs all tests.

After the build has completed, the build artifacts will be located in `artifacts`.

## Publishing

Package publishing is done in CI from `.github/workflows/release.yml` using NuGet trusted publishing (GitHub OIDC).
No `NUGET_API_KEY` secret is required or used by this repository's release process.
