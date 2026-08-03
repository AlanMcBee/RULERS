function Import-Filings {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CIK,
        [ValidateSet('10-K', '10-Q')]
        [string]$FormType,
        [string]$ConceptFilterPath,
        [string]$DatabasePath
    )

    $config = Get-Config
    if (-not $FormType) {
        $FormType = $config.FormType
    }

    $effectiveDatabasePath = if ($DatabasePath) { $DatabasePath } else { $config.DatabasePath }
    $args = @($CIK, $FormType)

    if ($ConceptFilterPath) {
        $args += @('--concept-filter', $ConceptFilterPath)
    }

    Push-Location $script:RepoRoot
    try {
        $output = & dotnet run --project $script:EtlProjectPath @args 2>&1
        $text = ($output | Out-String).Trim()
        if ($LASTEXITCODE -ne 0) {
            throw $text
        }

        return [pscustomobject]@{
            CIK = $CIK
            FormType = $FormType
            DatabasePath = $effectiveDatabasePath
            ConceptFilterPath = $ConceptFilterPath
            ExitCode = $LASTEXITCODE
            RawOutput = $text
        }
    }
    finally {
        Pop-Location
    }
}
