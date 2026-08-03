<#
.SYNOPSIS
    Resolves a stock ticker to its SEC Central Index Key (CIK).
.DESCRIPTION
    Looks up the ticker against SEC's company tickers feed. Returns $null when
    the ticker is not found; throws when the SEC request itself fails.
.EXAMPLE
    Resolve-R1Ticker -Ticker AAPL
#>
function Resolve-Ticker {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Ticker
    )

    $text = Invoke-EtlCommand -Arguments @('lookup', $Ticker)

    $match = [regex]::Match($text, 'Resolved\s+([^\s]+)\s+->\s+([^\s]+)')
    if ($match.Success) {
        return [pscustomobject]@{
            Ticker = $match.Groups[1].Value
            CIK = $match.Groups[2].Value
            Source = 'RuleOne.ETL'
        }
    }

    # Ticker not found in SEC data - a legitimate empty result, not a failure.
    return $null
}
