<#
.SYNOPSIS
    Queries stored SEC facts for a company by CIK.
.EXAMPLE
    Get-R1Facts -CIK 0000789019
#>
function Get-Facts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CIK,
        [string]$DatabasePath
    )

    $config = Get-Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $text = Invoke-EtlCommand -Arguments @('query', $CIK)

    return [pscustomobject]@{
        CIK = $CIK
        DatabasePath = $effectiveDatabasePath
        RawOutput = $text
    }
}

<#
.SYNOPSIS
    Queries stored SEC facts across companies by concept name.
.EXAMPLE
    Get-R1ConceptFacts -Concept Revenues
#>
function Get-ConceptFacts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Concept,
        [string]$DatabasePath
    )

    $config = Get-Config
    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }

    $text = Invoke-EtlCommand -Arguments @('concept', $Concept)

    return [pscustomobject]@{
        Concept = $Concept
        DatabasePath = $effectiveDatabasePath
        RawOutput = $text
    }
}
