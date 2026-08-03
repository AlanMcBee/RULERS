# PowerShell Module

The repository includes a lightweight PowerShell module that wraps the
[`RuleOne.ETL` CLI](CLI_RUNBOOK.md).

## Importing the module

```powershell
Import-Module .\modules\RuleOne\RuleOne.psd1
```

These commands require PowerShell 7.4+ and are exposed with the `R1` command
prefix applied via the module manifest's `DefaultCommandPrefix` (the
functions themselves are defined without any prefix — see
[ADR-0003](../adr/ADR-0003-powershell-module-design.md)).

## Example commands

```powershell
Resolve-R1Ticker -Ticker AAPL
Import-R1Filings -CIK 0000320193 -FormType 10-K
Get-R1Facts -CIK 0000320193
Get-R1ConceptFacts -Concept Revenues
Get-R1Securities
```

`Resolve-R1Ticker` returns `$null` when the ticker cannot be found in SEC
data; unexpected failures (e.g. the SEC request itself failing) throw.

## Configuration

`Get-R1Config` / `Set-R1Config` read and write
`modules\RuleOne\RuleOne.config.json`. Settings not explicitly passed to
`Set-R1Config` are preserved across calls.

| Setting             | Description                                                                 |
|---------------------|-------------------------------------------------------------------------------|
| `DatabasePath`       | Default SQLite database path used when a command's `-DatabasePath` is omitted. |
| `ConceptFilterPath`  | Default concept allow/deny JSON file path (see [CLI_RUNBOOK.md](CLI_RUNBOOK.md)). |
| `SecContact`         | Contact string (name/email or URL) sent in SEC requests' `User-Agent`/`Contact` headers, per [SEC's developer guidance](https://www.sec.gov/developer). Passed through to the ETL process automatically. |

There is no `FormType` setting — `Import-R1Filings` requires `-FormType` on
every call, since a single global default cannot represent the fact that
both `10-K` and `10-Q` filings are needed.

```powershell
Set-R1Config -SecContact 'Jane Analyst jane@example.com'
Get-R1Config
```

## Notes

The module is intentionally thin and returns structured PowerShell objects
such as PSCustomObject. It wraps the existing ETL CLI rather than
reimplementing SEC parsing logic, invoking a pre-built `RuleOne.ETL.dll`
(building it once automatically if no build output exists yet) instead of
`dotnet run`, for faster repeated interactive use.
