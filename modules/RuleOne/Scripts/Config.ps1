<#
.SYNOPSIS
    Gets the current RuleOne module configuration.
.DESCRIPTION
    Reads persisted settings (DatabasePath, ConceptFilterPath, SecContact) from
    RuleOne.config.json, falling back to defaults if the file is missing or invalid.
.EXAMPLE
    Get-R1Config
#>
function Get-Config {
    [CmdletBinding()]
    param()

    if (-not (Test-Path $script:DefaultConfigPath)) {
        return [pscustomobject]@{
            DatabasePath = Join-Path $script:RepoRoot 'ruleone.db'
            ConceptFilterPath = $null
            SecContact = $null
        }
    }

    try {
        $config = Get-Content -Path $script:DefaultConfigPath -Raw | ConvertFrom-Json
        return [pscustomobject]@{
            DatabasePath = $config.DatabasePath
            ConceptFilterPath = $config.ConceptFilterPath
            SecContact = $config.SecContact
        }
    }
    catch {
        Write-Information "Unable to read config at $script:DefaultConfigPath. Using defaults."
        return [pscustomobject]@{
            DatabasePath = Join-Path $script:RepoRoot 'ruleone.db'
            ConceptFilterPath = $null
            SecContact = $null
        }
    }
}

<#
.SYNOPSIS
    Updates persisted RuleOne module configuration.
.DESCRIPTION
    Merges the specified settings into RuleOne.config.json, preserving any
    settings not explicitly passed on the command line.
.EXAMPLE
    Set-R1Config -SecContact 'jane@example.com'
.EXAMPLE
    Set-R1Config -DatabasePath 'C:\data\ruleone.db' -ConceptFilterPath 'C:\config\concepts.json'
#>
function Set-Config {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        [string]$DatabasePath,
        [string]$ConceptFilterPath,
        [string]$SecContact
    )

    $existingConfig = Get-Config

    $config = [ordered]@{
        DatabasePath = if ($PSBoundParameters.ContainsKey('DatabasePath')) { $DatabasePath } else { $existingConfig.DatabasePath }
        ConceptFilterPath = if ($PSBoundParameters.ContainsKey('ConceptFilterPath')) { $ConceptFilterPath } else { $existingConfig.ConceptFilterPath }
        SecContact = if ($PSBoundParameters.ContainsKey('SecContact')) { $SecContact } else { $existingConfig.SecContact }
    }

    if ($PSCmdlet.ShouldProcess($script:DefaultConfigPath, 'Update RuleOne configuration')) {
        $config | ConvertTo-Json | Set-Content -Path $script:DefaultConfigPath -Encoding UTF8
    }

    return Get-Config
}
