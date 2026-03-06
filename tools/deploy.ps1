$ErrorActionPreference = "Stop"

Push-Location (Get-Item $PSScriptRoot).Parent.FullName

try {
    $releaseName = $env:GITHUB_REF_NAME
    if (! $releaseName) {
        Write-Output "No tag name supplied. Not deploying."
        return
    }

    $gitHubAuthToken = $env:GITHUB_TOKEN
    $repo = $env:GITHUB_REPOSITORY

    $releaseNotesFile = 'release_notes.md'
    # Use Tls12 to communicate with GitHub
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    $nugetServer = "https://api.nuget.org/v3/index.json"
    $artifactsPattern = "artifacts/output/*.nupkg"
    $releasesUrl = "https://api.github.com/repos/$repo/releases"
    $headers = @{
        "Authorization" = "Bearer $gitHubAuthToken"
        "Content-Type"  = "application/json; charset=utf-8";
    }

    Write-Output "Deploying $releaseName"

    $releaseNotes = Get-Content -Encoding UTF8 $releaseNotesFile
    if (!$releaseNotes[0].StartsWith('## ')) {
        throw "$releaseNotesFile doesn't start with a release. First line is '$($releaseNotes[0])'"
    }

    $releaseNotesVersion = $releaseNotes[0].SubString(3)
    if ($releaseNotesVersion -ne $releaseName) {
        throw "Release notes version '$releaseNotesVersion' does not match release name from tag '$releaseName'. Aborting."
    }

    Write-Output "Looking for GitHub release $releaseName"

    $releases = Invoke-RestMethod -Uri $releasesUrl -Headers $headers -Method GET
    $release = $releases | Where-Object { $_.name -eq $releaseName }
    if ($release) {
        throw "Release $releaseName already exists. Aborting."
    }

    $releaseBody = @()
    $releaseNotesLine = 1
    while (! $releaseNotes[$releaseNotesLine].StartsWith('## ')) {
        $releaseBody += $releaseNotes[$releaseNotesLine]
        $releaseNotesLine++
    }

    $createReleaseBody = @{
        tag_name   = $releaseName
        name       = $releaseName
        body       = ($releaseBody -join "`r`n").Trim()
        draft      = $false
        prerelease = $releaseName.Contains('-')
    } | ConvertTo-Json

    $createReleaseBody = [System.Text.Encoding]::UTF8.GetBytes($createReleaseBody)

    Write-Output "Creating GitHub release $releaseName"
    $release = Invoke-RestMethod -Uri $releasesUrl -Headers $headers -Method POST -Body $createReleaseBody -ContentType 'application/json'

    $headers["Content-type"] = "application/octet-stream"
    $uploadsUrl = "https://uploads.github.com/repos/$repo/releases/$($release.id)/assets?name="

    Write-Output "Uploading artifacts to GitHub release"

    $artifacts = Get-ChildItem -Path $artifactsPattern
    if (! $artifacts) {
        throw "Can't find any artifacts to publish"
    }

    $artifacts | ForEach-Object {
        Write-Output "Uploading $($_.Name)"
        $asset = Invoke-RestMethod -Uri ($uploadsUrl + $_.Name) -Headers $headers -Method POST -InFile $_
        Write-Output "Uploaded  $($asset.name)"
    }

    Write-Output "Pushing nupkgs to nuget.org"
    if (! $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN) {
        throw "No GitHub OIDC token available. Ensure workflow permission id-token: write is set for NuGet trusted publishing."
    }

    $artifacts | ForEach-Object {
        Write-Output "Pushing $($_.Name) using NuGet trusted publishing (OIDC)"
        & dotnet nuget push $_.FullName --source $nugetServer --force-english-output

        if ($LASTEXITCODE -ne 0) {
            throw "Push failed with error $LASTEXITCODE"
        }
        Write-Output "Pushed  $($_.Name)"
    }

    Write-Output "Finished deploying $releaseName"
}
finally {
    Pop-Location
}
