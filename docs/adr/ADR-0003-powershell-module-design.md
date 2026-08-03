# ADR-0003: PowerShell Module Design

- Status: Accepted
- Date: 2026-08-02
- Deciders: @AlanMcBee
- Technical Area: tooling | developer experience
- Supersedes: none
- Superseded by: none

## Context

`RuleOne.ETL` is a CLI-first tool. Interactive and scripted analyst workflows
benefit from a PowerShell surface that returns structured objects
(`PSCustomObject`) instead of parsing CLI text output by hand, while still
treating the ETL CLI as the single source of truth for SEC fetching,
parsing, and storage logic.

## Decision

### Thin wrapper, not a reimplementation

The module in `modules/RuleOne` is a thin wrapper: every exported function
invokes the `RuleOne.ETL` process and shapes its output into
`PSCustomObject` results. No SEC parsing or business logic is duplicated in
PowerShell.

### Command naming and prefixing

Functions are defined with plain, unprefixed noun names (e.g.
`Resolve-Ticker`, `Get-Config`). The `R1` prefix is applied exclusively via
the module manifest's `DefaultCommandPrefix = 'R1'` property in
`RuleOne.psd1`. Function definitions and `Export-ModuleMember` calls must
never bake `RuleOne`, `Rule1`, or `R1` into the function name itself — this
keeps the manifest as the single place that controls the public naming
surface and allows prefix changes without touching implementation files.

### File layout

- `RuleOne.psm1` is the root module. It sets shared `$script:` scope
  variables (`ModuleRoot`, `RepoRoot`, `EtlProjectPath`, `DefaultConfigPath`)
  and dot-sources each file under `Scripts/`.
- Each `Scripts/*.ps1` file defines one or more related public functions
  using plain names.
- `Scripts/Invoke.ps1` defines private helpers (`Invoke-EtlCommand`,
  `Resolve-EtlExecutablePath`) shared by all other script files; these are
  not exported.

### ETL process invocation

`Invoke-EtlCommand` resolves a pre-built `RuleOne.ETL.dll` under
`src/RuleOne.ETL/bin/{Debug,Release}/net8.0/`, building it once (and caching
the resolved path for the session) if no build output is found, then invokes
it via `dotnet <dll path> <args>`. This avoids the overhead of `dotnet run`
recompiling/re-resolving on every call, which matters for a wrapper used
repeatedly in interactive sessions.

### Configuration

`Get-Config`/`Set-Config` persist settings to `RuleOne.config.json` next to
the module. `Set-Config` merges explicitly-passed parameters with existing
values (via `$PSBoundParameters.ContainsKey(...)`) so unspecified settings
are preserved across calls, and supports `-WhatIf`/`-Confirm` via
`SupportsShouldProcess` since it mutates state on disk.

`SecContact` is read from config and propagated to the ETL process as the
`RULEONE_SEC_CONTACT` environment variable for the duration of the call,
reconciling the previously-separate env-var-only and config-only mechanisms.

There is no `FormType` config setting: a single global default cannot
represent that both `10-K` and `10-Q` filings are legitimate targets, so
`Import-Filings` (`Import-R1Filings`) requires `-FormType` on every call.

### Error handling convention

Functions follow one convention throughout the module:
- A query or command that can legitimately return "no data" (e.g. a ticker
  that does not resolve to a CIK) returns `$null` or an empty result — never
  a `Success`/`Error` wrapper object.
- An unexpected failure (non-zero exit code from the ETL process, an SEC
  request that itself fails) throws.

Callers within the module, and consumers of the module, must anticipate this
convention rather than checking a `Success` property.

## Consequences

Positive:
- Prefix changes only require editing the manifest, not every function body.
- Structured objects support standard PowerShell pipelines
  (`Where-Object`, `Sort-Object`, `Export-Csv`, etc.).
- Pre-built exe invocation makes repeated interactive use noticeably faster
  than `dotnet run`.
- One error-handling convention removes ambiguity about how to check for
  "not found" vs. "failed".

Negative:
- The pre-built exe path must be kept in sync with target framework/output
  changes in `RuleOne.ETL.fsproj`; a build-output layout change requires a
  matching update to `Resolve-EtlExecutablePath`.
- Auto-building on first use adds an unpredictable one-time delay to the
  first command in a session if no build output exists.

## Alternatives Considered

1. Bake the `R1` prefix directly into function names (e.g.
   `Resolve-R1Ticker` as the actual function name).
   - Rejected: couples naming policy to every function definition; renaming
     the prefix later would require touching every file.
2. Always invoke `dotnet run --project ...`.
   - Rejected: recompiles/re-resolves on every call, which is slow for
     repeated interactive use; a pre-built exe with one-time auto-build is
     faster after the first call.
3. Represent "ticker not found" as a `PSCustomObject` with a `Success`
   property.
   - Rejected in favor of returning `$null`, consistent with the module-wide
     error-handling convention distinguishing legitimate empty results from
     unexpected failures.

## Follow-up Actions

- [ ] Revisit the pre-built exe resolution strategy if `RuleOne.ETL.fsproj`
      changes its target framework or output directory structure.
