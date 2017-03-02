
$NuGetVersion = 'v3.5.0'

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

if ( ! ( Test-Path .nuget ) )
{
    Write-Output "Creating .nuget directory"
    New-Item -ItemType Directory .nuget > $nul
}

if ( ! ( Test-Path .nuget\NuGet.exe ) ) {
    Write-Output "Copying NuGet.exe into .nuget directory"
    Copy-Item $NuGetExecutable .nuget\NuGet.exe
}

# restore packages
Write-Output "Restoring essential NuGet packages"
.nuget\NuGet.exe restore .\packages.config -PackagesDirectory .\packages -Verbosity quiet

# run script
& "${env:ProgramFiles(x86)}\MSBuild\14.0\Bin\csi.exe" .\build.csx $args

Pop-Location