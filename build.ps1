
$NuGetVersion = 'v3.5.0'
$VisualStudioVersion = '15.0'
$VSWhereVersion = '1.0.58'

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

if ( ! ( Test-Path .nuget ) ) {
    Write-Output "Creating .nuget directory"
    New-Item -ItemType Directory .nuget > $nul
}

if ( ! ( Test-Path .nuget\NuGet.exe ) ) {
    Write-Output "Copying NuGet.exe into .nuget directory"
    Copy-Item $NuGetExecutable .nuget\NuGet.exe
}

# restore packages
Write-Output "Restoring NuGet packages for build script"
.nuget\NuGet.exe restore .\packages.config -PackagesDirectory .\packages -Verbosity quiet

$VSDir = & ".\packages\vswhere.$VSWhereVersion\tools\vswhere.exe" -version $VisualStudioVersion -products * -requires Microsoft.Component.MSBuild -property installationPath
if ($VSDir) {
    $CSIPath = join-path $VSDir "MSBuild\$VisualStudioVersion\Bin\Roslyn\csi.exe"
    if (test-path $CSIPath) {
        & $CSIPath .\build.csx $args
    }
}