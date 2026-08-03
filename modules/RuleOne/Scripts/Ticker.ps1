function Resolve-Ticker {
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
