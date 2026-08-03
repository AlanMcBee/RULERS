function Get-Config {
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

function Set-Config {
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

    return Get-Config
}
