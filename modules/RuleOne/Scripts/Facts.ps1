function Get-Facts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CIK,
        [string]$DatabasePath
    )

    $config = Get-Config
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

function Get-ConceptFacts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Concept,
        [string]$DatabasePath
    )

    $config = Get-Config
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
