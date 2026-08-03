Set-StrictMode -Version Latest

$script:ModuleRoot = Split-Path -Parent $PSCommandPath
$script:RepoRoot = Split-Path -Parent (Split-Path -Parent $script:ModuleRoot)
$script:EtlProjectPath = Join-Path $script:RepoRoot 'src/RuleOne.ETL'
$script:DefaultConfigPath = Join-Path $script:ModuleRoot 'RuleOne.config.json'

function Get-R1Config {
    [CmdletBinding()]
    param()

    if (-not (Test-Path $script:DefaultConfigPath)) {
        return [pscustomobject]@{
            DatabasePath = Join-Path $script:RepoRoot 'ruleone.db'
            FormType = '10-K'
            ConceptFilterPath = $null
        }
    }

    try {
        $config = Get-Content -Path $script:DefaultConfigPath -Raw | ConvertFrom-Json
        return [pscustomobject]@{
            DatabasePath = $config.DatabasePath
            FormType = $config.FormType
            ConceptFilterPath = $config.ConceptFilterPath
        }
    }
    catch {
        Write-Information "Unable to read config at $script:DefaultConfigPath. Using defaults."
        return [pscustomobject]@{
            DatabasePath = Join-Path $script:RepoRoot 'ruleone.db'
            FormType = '10-K'
            ConceptFilterPath = $null
        }
    }
}

function Set-R1Config {
    [CmdletBinding()]
    param(
        [string]$DatabasePath,
        [ValidateSet('10-K', '10-Q')]
        [string]$FormType = '10-K',
        [string]$ConceptFilterPath
    )

    $config = [ordered]@{
        DatabasePath = $DatabasePath
        FormType = $FormType
        ConceptFilterPath = $ConceptFilterPath
    }

    $config | ConvertTo-Json | Set-Content -Path $script:DefaultConfigPath -Encoding UTF8

    return Get-R1Config
}

function Resolve-R1Ticker {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Ticker
    )

    $output = & dotnet run --project $script:EtlProjectPath lookup $Ticker 2>&1
    $text = ($output | Out-String).Trim()

    if ($LASTEXITCODE -ne 0) {
        return [pscustomobject]@{
            Ticker = $Ticker
            CIK = $null
            Source = 'RuleOne.ETL'
            RawOutput = $text
            Success = $false
        }
    }

    $match = [regex]::Match($text, 'Resolved\s+([^\s]+)\s+->\s+([^\s]+)')
    if ($match.Success) {
        return [pscustomobject]@{
            Ticker = $match.Groups[1].Value
            CIK = $match.Groups[2].Value
            Source = 'RuleOne.ETL'
        }
    }

    return [pscustomobject]@{
        Ticker = $Ticker
        CIK = $null
        Source = 'RuleOne.ETL'
        RawOutput = $text
        Success = $false
    }
}

function Import-R1Filings {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CIK,
        [ValidateSet('10-K', '10-Q')]
        [string]$FormType,
        [string]$ConceptFilterPath,
        [string]$DatabasePath
    )

    $config = Get-R1Config
    if (-not $FormType) {
        $FormType = $config.FormType
    }

    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }
    $args = @($CIK, $FormType)

    if ($ConceptFilterPath) {
        $args += @('--concept-filter', $ConceptFilterPath)
    }

    Push-Location $script:RepoRoot
    try {
        $output = & dotnet run --project $script:EtlProjectPath @args 2>&1
        $text = ($output | Out-String).Trim()
        if ($LASTEXITCODE -ne 0) {
            throw $text
        }

        return [pscustomobject]@{
            CIK = $CIK
            FormType = $FormType
            DatabasePath = $effectiveDatabasePath
            ConceptFilterPath = $ConceptFilterPath
            ExitCode = $LASTEXITCODE
            RawOutput = $text
        }
    }
    finally {
        Pop-Location
    }
}

function Get-R1Facts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CIK,
        [string]$DatabasePath
    )

    $config = Get-R1Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $output = & dotnet run --project $script:EtlProjectPath query $CIK 2>&1
    $text = ($output | Out-String).Trim()

    if ($LASTEXITCODE -ne 0) {
        throw $text
    }

    return [pscustomobject]@{
        CIK = $CIK
        DatabasePath = $effectiveDatabasePath
        RawOutput = $text
    }
}

function Get-R1ConceptFacts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Concept,
        [string]$DatabasePath
    )

    $config = Get-R1Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $output = & dotnet run --project $script:EtlProjectPath concept $Concept 2>&1
    $text = ($output | Out-String).Trim()

    if ($LASTEXITCODE -ne 0) {
        throw $text
    }

    return [pscustomobject]@{
        Concept = $Concept
        DatabasePath = $effectiveDatabasePath
        RawOutput = $text
    }
}

function Get-R1Securities {
    [CmdletBinding()]
    param(
        [string]$DatabasePath
    )

    $config = Get-R1Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $output = & dotnet run --project $script:EtlProjectPath list 2>&1
    $text = ($output | Out-String).Trim()

    if ($LASTEXITCODE -ne 0) {
        throw $text
    }

    $rows = @()
    foreach ($line in $text -split "`r?`n") {
        if ($line -match '^(?<CIK>\S+)\s*\|\s*(?<CompanyName>[^|]+)\s*\|\s*(?<LastFilingDate>[^|]+)\s*\|\s*(?<FactCount>\d+)\s*facts$') {
            $rows += [pscustomobject]@{
                CIK = $Matches.CIK
                CompanyName = $Matches.CompanyName.Trim()
                LastFilingDate = $Matches.LastFilingDate.Trim()
                FactCount = [int]$Matches.FactCount
                DatabasePath = $effectiveDatabasePath
            }
        }
    }

    return $rows
}

Export-ModuleMember -Function @(
    'Resolve-R1Ticker',
    'Import-R1Filings',
    'Get-R1Facts',
    'Get-R1ConceptFacts',
    'Get-R1Config',
    'Set-R1Config',
    'Get-R1Securities'
)
