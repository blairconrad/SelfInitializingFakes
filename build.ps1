
$NuGetVersion = 'v3.5.0-rc1'

$SolutionFile = "SelfInitializingFakes.sln"

#####

$NuGetUrl = "https://dist.nuget.org/win-x86-commandline/$NuGetVersion/NuGet.exe"

$ScriptDir = Split-Path $script:MyInvocation.MyCommand.Path

Push-Location $ScriptDir

# determine cache dir
$NuGetCacheDir = "$env:LocalAppData\.nuget\$NuGetVersion"
$NuGetExecutable = "$NuGetCacheDir/NuGet.exe"

# download nuget to cache dir
if ( ! ( Test-Path $NuGetExecutable ) ) {
    if ( ! ( Test-Path $NuGetCacheDir ) ) {
        New-Item -Type directory $NuGetCacheDir > $nul
    }

    Write-Output "Downloading '$NuGetUrl' to '$NuGetExecutable'..."
    $ProgressPreference = 'SilentlyContinue'
    Invoke-WebRequest $NuGetUrl -OutFile "$NuGetExecutable"
}

if ( ! ( Test-Path .nuget\NuGet.exe ) ) {
    Copy-Item $NuGetExecutable .nuget\NuGet.exe
}

# restore packages
.nuget\NuGet.exe restore $SolutionFile -MSBuildVersion 14

# run script
& "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\csi.exe" .\build.csx $args
