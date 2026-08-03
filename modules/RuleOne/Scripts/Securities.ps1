<#
.SYNOPSIS
    Lists securities currently stored in the database.
.EXAMPLE
    Get-R1Securities
#>
function Get-Securities {
    [CmdletBinding()]
    param(
        [string]$DatabasePath
    )

    $config = Get-Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $text = Invoke-EtlCommand -Arguments @('list')

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
