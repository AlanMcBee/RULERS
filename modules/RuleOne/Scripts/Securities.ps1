function Get-Securities {
    [CmdletBinding()]
    param(
        [string]$DatabasePath
    )

    $config = Get-Config
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
