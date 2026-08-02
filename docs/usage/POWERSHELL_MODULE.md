# PowerShell Module

The repository now includes a lightweight PowerShell module for the ETL workflow.

## Importing the module

```powershell
Import-Module .\modules\RuleOne\RuleOne.psd1
```

## Example commands

```powershell
Resolve-RuleOneTicker -Ticker AAPL
Import-RuleOneFilings -CIK 0000320193 -FormType 10-K
Get-RuleOneFacts -CIK 0000320193
Get-RuleOneConceptFacts -Concept Revenues
```

## Notes

The module is intentionally thin and returns structured PowerShell objects such as PSCustomObject. It wraps the existing ETL CLI rather than reimplementing SEC parsing logic.
