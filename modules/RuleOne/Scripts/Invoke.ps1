$script:EtlExecutablePath = $null
$script:EtlBuildNoticeShown = $false

# Resolves a pre-built RuleOne.ETL.dll, building it once if no build output exists yet.
function Resolve-EtlExecutablePath {
    [CmdletBinding()]
    param()

    if ($script:EtlExecutablePath -and (Test-Path $script:EtlExecutablePath)) {
        return $script:EtlExecutablePath
    }

    foreach ($configuration in @('Debug', 'Release')) {
        $candidate = Join-Path $script:EtlProjectPath "bin/$configuration/net8.0/RuleOne.ETL.dll"
        if (Test-Path $candidate) {
            $script:EtlExecutablePath = $candidate
            return $script:EtlExecutablePath
        }
    }

    if (-not $script:EtlBuildNoticeShown) {
        Write-Information 'RuleOne ETL build output not found. Building it now (this happens once per session)...' -InformationAction Continue
        $script:EtlBuildNoticeShown = $true
    }

    & dotnet build $script:EtlProjectPath --configuration Debug --nologo | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build RuleOne.ETL project at $script:EtlProjectPath"
    }

    $candidate = Join-Path $script:EtlProjectPath 'bin/Debug/net8.0/RuleOne.ETL.dll'
    if (-not (Test-Path $candidate)) {
        throw "RuleOne.ETL build succeeded but the expected output was not found at $candidate"
    }

    $script:EtlExecutablePath = $candidate
    return $script:EtlExecutablePath
}

# Runs the RuleOne ETL executable with the given arguments, propagating SecContact
# from config, and throws on a non-zero exit code. Returns trimmed stdout/stderr text.
function Invoke-EtlCommand {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $exePath = Resolve-EtlExecutablePath
    $config = Get-Config

    $previousSecContact = $env:RULEONE_SEC_CONTACT
    if ($config.SecContact) {
        $env:RULEONE_SEC_CONTACT = $config.SecContact
    }

    Push-Location $script:RepoRoot
    try {
        $output = & dotnet $exePath @Arguments 2>&1
    }
    finally {
        Pop-Location
        $env:RULEONE_SEC_CONTACT = $previousSecContact
    }

    $text = ($output | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw $text
    }

    return $text
}
