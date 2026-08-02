# RuleOne System Overview

## Purpose

RuleOne is a .NET/F# financial analysis engine for long-horizon Rule #1 style screening.

## Current Bounded Contexts

1. Ingestion (`RuleOne.ETL`)
- Retrieves SEC submission metadata.
- Discovers filing candidates.
- Parses financial facts (currently partial and under active implementation).
- Persists normalized facts to SQLite.

2. Analytics (`RuleOne.Analytics`)
- Provides metric calculators (CAGR, ROIC, EPS, MOS).
- Will evolve into layered metric + rule engine with scope-aware parameters.

3. Consumer Surfaces
- CLI commands for ingest and queries.
- Notebook for exploration and static visualizations.
- Future adapters may include API and automation pipelines.

## Architectural Principles

1. Headless core engine: business logic must not depend on a specific UI.
2. Deterministic runs: same inputs and config produce same outputs.
3. Immutable raw facts: qualitative overlays and analyst judgments are separate artifacts.
4. Expansion-safe boundaries: keep adapter interfaces stable across future surfaces.

## Current Data Flow

1. CLI command triggers ETL.
2. ETL fetches SEC data.
3. ETL parses and normalizes facts.
4. Facts are stored in SQLite.
5. Query/analysis layers read facts and compute metrics.
6. Results are emitted to console/notebook/static reports.

## Active Risks

1. Placeholder parsing path is still present and must be replaced.
2. Analytics need richer long-horizon stability metrics.
3. Storage shape must evolve for scalable universe-wide screening.
