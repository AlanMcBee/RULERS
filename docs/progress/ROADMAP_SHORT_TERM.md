# RuleOne Short-Term Roadmap

Scope: 1-2 week execution horizon, optimized for single-analyst productivity while preserving expansion-safe architecture.

## Objectives

1. Replace placeholder SEC fact parsing with real Inline XBRL extraction.
2. Establish idempotent ingestion and provenance-aware persistence.
3. Deliver parameterized analytics foundations for 5-10 year screening.
4. Keep UX low-res and deterministic via CLI + JSON + static outputs.

## Milestones

- [ ] M0: Baseline and constraints captured in docs.
- [ ] M1: Real parser path in ETL (no mock fact generation).
- [ ] M2: Concept selection policy configurable via JSON.
- [ ] M3: Idempotent persistence and provenance fields in storage.
- [ ] M4: Robust SEC fetch retries/backoff/rate compliance.
- [ ] M5: Analytics parameter precedence model defined (global/sector/industry/company).
- [ ] M6: JSON run spec shape defined for deterministic screening runs.
- [ ] M7: Verification gates passing (build, tests, ingestion realism, idempotency, reproducibility).

## Workstreams

## Workstream A: Ingestion Pipeline

- Filing discovery and retrieval strategy
- Inline XBRL parsing and normalization
- Error taxonomy and retry policy

## Workstream B: Data Model and Persistence

- Schema evolution from single-table convenience toward analysis-ready shape
- Compatibility strategy for legacy queries
- Provenance and dedupe constraints

## Workstream C: Analytics Engine v1

- Metric calculators vs rule layers separation
- Scope-aware parameter bundles and precedence
- Model version tagging for reproducibility

## Workstream D: CLI and Outputs

- JSON-driven run specs
- CSV/JSON/Markdown result emitters
- Static chart/table pipeline for notebooks/reports

## Notes

- Non-goals for this horizon: rich UI, authentication implementation, hosted multi-tenant service.
- Architecture rule: engine remains headless and deterministic.
- Data rule: raw filing facts remain immutable; analyst judgments are layered artifacts.
