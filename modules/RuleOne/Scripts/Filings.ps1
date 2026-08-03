<#
.SYNOPSIS
    Fetches and stores SEC filings for a company.
.DESCRIPTION
    Runs the RuleOne ETL fetch command for the given CIK and form type,
    optionally applying a concept allow/deny filter, and stores results
    in the configured database.
.EXAMPLE
    Import-R1Filings -CIK 0000789019 -FormType 10-K
.EXAMPLE
    Import-R1Filings -CIK 0000789019 -FormType 10-Q -ConceptFilterPath .\concepts.json
#>
function Import-Filings {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CIK,
        [Parameter(Mandatory = $true)]
        [ValidateSet('10-K', '10-Q')]
        [string]$FormType,
        [string]$ConceptFilterPath,
        [string]$DatabasePath
    )

    $config = Get-Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $arguments = @($CIK, $FormType)
    if ($ConceptFilterPath) {
        $arguments += @('--concept-filter', $ConceptFilterPath)
    }

    $text = Invoke-EtlCommand -Arguments $arguments

    return [pscustomobject]@{
        CIK = $CIK
        FormType = $FormType
        DatabasePath = $effectiveDatabasePath
        ConceptFilterPath = $ConceptFilterPath
        RawOutput = $text
    }
}
