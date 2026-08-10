# ADR-0004: Polyglot Notebook Language Strategy (F# and PowerShell)

- Status: Accepted
- Date: 2026-08-09
- Deciders: @AlanMcBee
- Technical Area: analytics | interface
- Supersedes: none
- Superseded by: none

## Context

`samples/FinancialAnalysis` was originally an all-F# `.ipynb` notebook (Polyglot
Notebooks / .NET Interactive), using `Microsoft.Data.Sqlite` directly and
`Plotly.NET` for charts.

Polyglot Notebooks is no longer maintained, which forced a migration. The
notebook was moved to the [Verso Notebook](https://marketplace.visualstudio.com/items?itemName=Datafication.verso-notebook)
extension and, during that migration, rewritten entirely in PowerShell to
sidestep .NET Interactive kernel issues. That all-PowerShell version works,
but re-implements SQLite access and chart generation by hand instead of
reusing the F# domain logic already built in `RuleOne.ETL` (`Database.fs`)
and `RuleOne.Analytics` (`Library.fs`).

Verso notebooks are polyglot: every code cell declares its own language
(`fsharp`, `powershell`, `csharp`, `sql`, etc.), and all cells in a notebook
share a single variable store, so a value produced in one language's cell is
directly usable in a cell written in another language. This removes the
main reason to pick a single notebook-wide language and reopens the
question of which language should be used for which parts of the notebook.

## Decision

Use both languages in `samples/FinancialAnalysis.verso`, each where it is
strongest, rather than standardizing on one:

- **F# cells** for exploratory and data-shaping work:
  - Querying and transforming SEC facts, including calling directly into
    `RuleOne.ETL.Database` and `RuleOne.Analytics.FinancialMetrics` (via
    `#r` references to the built assemblies) instead of re-deriving that
    logic in another language.
  - Any step that benefits from F#'s type inference, records/discriminated
    unions, and option types for handling irregular or partially-missing
    SEC data.
- **PowerShell cells** for CLI-style orchestration of already-built
  operations:
  - Invoking the `RuleOne` PowerShell module (ADR-0003) — e.g.
    `Import-R1Filings`, `Get-R1Securities`, `Get-R1Config` — for anything
    that module already wraps.
  - Environment/setup concerns: locating build output, writing files,
    opening generated reports, and other host/OS-level operations.

Rule of thumb: if the step is exploring or shaping data using logic that
lives in (or belongs in) the F# projects, write it in F#. If the step is
driving a well-established operation exposed as a PowerShell command, write
it in PowerShell. Pass data between them through Verso's shared variable
store rather than re-fetching or re-parsing the same data twice.

## Consequences

Positive:
- Notebook cells reuse the same F# domain logic (`Database.fs`,
  `Library.fs`) as the CLI, instead of maintaining a second,
  PowerShell-only reimplementation of SQLite querying and CAGR math.
- PowerShell cells stay thin orchestration, consistent with the ADR-0003
  wrapper philosophy, instead of growing ad hoc data-processing logic.
- Readers can tell which language to reach for based on what the cell is
  doing, rather than guessing from notebook-wide convention.

Negative:
- Contributors need to be comfortable reading both F# and PowerShell in the
  same document.
- Cross-language variable sharing in Verso, while direct, still needs a
  type annotation or shape check when a value crosses from F# into
  PowerShell (or vice versa), since PowerShell has no static type system.
- The existing all-PowerShell cells in `FinancialAnalysis.verso` (SQLite
  access via `Add-Type`, hand-built Plotly HTML, manual CAGR) need to be
  reworked to call into F# where this ADR says F# should own the logic.

## Alternatives Considered

1. All-PowerShell notebook (the state immediately before this ADR).
   - Rejected: duplicates SQLite access and CAGR logic that already exists
     in `RuleOne.ETL` and `RuleOne.Analytics`, and loses F#'s type-safe
     handling of the underlying data.
2. All-F# notebook (the original state).
   - Rejected: loses the already-built, tested `RuleOne` PowerShell module
     surface for orchestration-style operations, and re-adds the .NET
     Interactive kernel dependency this migration was meant to remove.
3. Two separate notebooks, one per language.
   - Rejected: splits a single analysis workflow across files and forfeits
     Verso's shared variable store, the main advantage of a polyglot
     notebook.

## Follow-up Actions

- [ ] Rework `samples/FinancialAnalysis.verso` so SQLite querying and CAGR
      calculations use F# cells calling `RuleOne.ETL.Database` /
      `RuleOne.Analytics.FinancialMetrics`, with PowerShell cells limited to
      orchestration (module calls, file/report handling).
- [ ] Update `samples/README.md`, `README.md`, `EXAMPLES.md`, and
      `IMPLEMENTATION_SUMMARY.md` references from `FinancialAnalysis.ipynb`
      to `FinancialAnalysis.verso` and describe the Verso Notebook
      prerequisite instead of Polyglot Notebooks/.NET Interactive.
