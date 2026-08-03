function Get-Config {
    [CmdletBinding()]
    param()

    if (-not (Test-Path $script:DefaultConfigPath)) {
        return [pscustomobject]@{
            DatabasePath = Join-Path $script:RepoRoot 'ruleone.db'
            FormType = '10-K'
            ConceptFilterPath = $null
            SecContact = $null
        }
    }

    try {
        $config = Get-Content -Path $script:DefaultConfigPath -Raw | ConvertFrom-Json
        return [pscustomobject]@{
            DatabasePath = $config.DatabasePath
            FormType = $config.FormType
            ConceptFilterPath = $config.ConceptFilterPath
            SecContact = $config.SecContact
        }
    }
    catch {
        Write-Information "Unable to read config at $script:DefaultConfigPath. Using defaults."
        return [pscustomobject]@{
            DatabasePath = Join-Path $script:RepoRoot 'ruleone.db'
            FormType = '10-K'
            ConceptFilterPath = $null
            SecContact = $null
        }
    }
}

function Set-Config {
    [CmdletBinding()]
    param(
        [string]$DatabasePath,
        [ValidateSet('10-K', '10-Q')]
        [string]$FormType,
        [string]$ConceptFilterPath,
        [string]$SecContact
    )

    $existingConfig = Get-Config

    $config = [ordered]@{
        DatabasePath = if ($PSBoundParameters.ContainsKey('DatabasePath')) { $DatabasePath } else { $existingConfig.DatabasePath }
        FormType = if ($PSBoundParameters.ContainsKey('FormType')) { $FormType } else { $existingConfig.FormType }
        ConceptFilterPath = if ($PSBoundParameters.ContainsKey('ConceptFilterPath')) { $ConceptFilterPath } else { $existingConfig.ConceptFilterPath }
        SecContact = if ($PSBoundParameters.ContainsKey('SecContact')) { $SecContact } else { $existingConfig.SecContact }
    }

    $config | ConvertTo-Json | Set-Content -Path $script:DefaultConfigPath -Encoding UTF8

    return Get-Config
}
