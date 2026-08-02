# CLI Runbook

This runbook describes current command behavior for `RuleOne.ETL`.

## Prerequisites

1. .NET 8 SDK installed
2. Dependencies restored (`dotnet restore`)
3. Project built (`dotnet build`)

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
2. Fetches submission metadata from SEC EDGAR.
3. Parses and stores facts for selected filings.

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

1. Current parsing includes placeholder behavior that is under replacement.
2. Output is intentionally low-res and CLI-centric in this phase.
3. Future runbooks will include JSON run specs and batch orchestration.

## Troubleshooting

1. If SEC requests fail, confirm network access and User-Agent compliance.
2. If no facts appear, validate parser status and concept matching.
3. If database is empty, rerun fetch before query commands.
