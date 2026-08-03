Set-StrictMode -Version Latest

$script:ModuleRoot = Split-Path -Parent $PSCommandPath
$script:RepoRoot = Split-Path -Parent (Split-Path -Parent $script:ModuleRoot)
$script:EtlProjectPath = Join-Path $script:RepoRoot 'src/RuleOne.ETL'
$script:DefaultConfigPath = Join-Path $script:ModuleRoot 'RuleOne.config.json'

$scriptFiles = @(
    'Invoke.ps1',
    'Config.ps1',
    'Ticker.ps1',
    'Filings.ps1',
    'Facts.ps1',
    'Securities.ps1'
)

foreach ($scriptFile in $scriptFiles) {
    $path = Join-Path $PSScriptRoot 'Scripts' $scriptFile
    . $path
}

Export-ModuleMember -Function @(
    'Resolve-Ticker',
    'Import-Filings',
    'Get-Facts',
    'Get-ConceptFacts',
    'Get-Config',
    'Set-Config',
    'Get-Securities'
)
