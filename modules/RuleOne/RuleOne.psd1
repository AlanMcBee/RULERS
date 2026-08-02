@{
    RootModule = 'RuleOne.psm1'
    ModuleVersion = '0.1.0'
    GUID = '3a6f8d69-33d9-49c0-8f0d-d8d1d7ad3e72'
    Author = 'RuleOne'
    CompanyName = 'RuleOne'
    Copyright = '(c) RuleOne. All rights reserved.'
    Description = 'PowerShell wrapper for the RuleOne ETL CLI.'
    PowerShellVersion = '5.1'
    FunctionsToExport = @(
        'Resolve-RuleOneTicker',
        'Import-RuleOneFilings',
        'Get-RuleOneFacts',
        'Get-RuleOneConceptFacts',
        'Get-RuleOneConfig',
        'Set-RuleOneConfig'
    )
    CmdletsToExport = @()
    VariablesToExport = '*'
    AliasesToExport = @()
    PrivateData = @{
        PSData = @{
            Tags = @('RuleOne', 'SEC', 'Finance')
            LicenseUri = 'https://github.com/AlanMcBee/RULERS/blob/main/LICENSE'
            ProjectUri = 'https://github.com/AlanMcBee/RULERS'
            ReleaseNotes = 'Initial PowerShell wrapper module for the RuleOne ETL CLI.'
        }
    }
}
