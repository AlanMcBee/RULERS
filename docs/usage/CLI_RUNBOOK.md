# CLI Runbook

This runbook describes current command behavior for `RuleOne.ETL`. For a
PowerShell-friendly wrapper with structured objects and durable
configuration, see [POWERSHELL_MODULE.md](POWERSHELL_MODULE.md).

## Prerequisites

1. .NET 8 SDK installed
2. Dependencies restored (`dotnet restore`)
3. Project built (`dotnet build`)
4. Optional: set the `RULEONE_SEC_CONTACT` environment variable (or the
   PowerShell module's `SecContact` config setting) to a contact string per
   [SEC's developer guidance](https://www.sec.gov/developer). Defaults to a
   project URL if unset.

## Commands

## Fetch SEC Filings by CIK and Form

```powershell
dotnet run --project src/RuleOne.ETL <CIK> <10-K|10-Q>
```

Example:

```powershell
dotnet run --project src/RuleOne.ETL 0000789019 10-K
```

Behavior:
1. Initializes/opens `ruleone.db` in current working directory.
2. Fetches SEC company submissions metadata and `companyfacts` JSON.
3. Parses and stores normalized facts for the selected form.

## Fetch With Concept Filter Override

```powershell
dotnet run --project src/RuleOne.ETL <CIK> <10-K|10-Q> --concept-filter <path>
```

Example:

```powershell
dotnet run --project src/RuleOne.ETL 0000789019 10-K --concept-filter .\concept-filter.json
```

Concept filter file format:

```json
{
	"allow": ["Revenues", "NetIncomeLoss", "StockholdersEquity"],
	"deny": ["Assets"]
}
```

Rules:
1. If `allow` is set, only listed concepts are ingested.
2. Any concept in `deny` is always excluded.
3. If no file is provided, a built-in Rule #1 oriented allowlist is used.

## Query Facts by CIK

```powershell
dotnet run --project src/RuleOne.ETL query <CIK>
```

Example:

```powershell
dotnet run --project src/RuleOne.ETL query 0000789019
```

## Query Facts by Concept

```powershell
dotnet run --project src/RuleOne.ETL concept <CONCEPT>
```

Example:

```powershell
dotnet run --project src/RuleOne.ETL concept Revenues
```

## Notes and Limitations

1. Parsing is based on SEC `companyfacts` for Phase 1 speed and repeatability.
2. Output is intentionally low-res and CLI-centric in this phase.
3. Future runbooks will include JSON run specs and batch orchestration.

## Troubleshooting

1. If SEC requests fail, confirm network access and User-Agent compliance.
2. If no facts appear, validate parser status and concept matching.
3. If database is empty, rerun fetch before query commands.
