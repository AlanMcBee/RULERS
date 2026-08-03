# PowerShell Module

The repository now includes a lightweight PowerShell module for the ETL workflow.

## Importing the module

```powershell
Import-Module .\modules\RuleOne\RuleOne.psd1
```

These commands are intended for PowerShell 7+ and will be exposed with the module prefix from the manifest.

## Example commands

```powershell
Resolve-R1Ticker -Ticker AAPL
Import-R1Filings -CIK 0000320193 -FormType 10-K
Get-R1Facts -CIK 0000320193
Get-R1ConceptFacts -Concept Revenues
Get-R1Securities
```

## Notes

The module is intentionally thin and returns structured PowerShell objects such as PSCustomObject. It wraps the existing ETL CLI rather than reimplementing SEC parsing logic.
