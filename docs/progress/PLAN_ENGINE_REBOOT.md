# Plan: RuleOne Engine Reboot + Repository Docs System

## Intent

Short-term priority is to make RuleOne usable by one analyst for fast 5-10 year screening across US public companies, while preserving a clean path to multi-analyst and multi-interface expansion.

Recommended approach:
1. Harden ingestion first (real XBRL facts and repeatable batch updates).
2. Establish a versioned analytics engine with parameterized models at sector/industry/company/time scopes.
3. Keep UX low-res through CLI, JSON specs, and static outputs.

## Phases

## Phase 0: Reconfirm Baseline and Constraints

1. Validate implemented capabilities vs documented capabilities.
2. Capture near-term non-goals: no rich UI, no auth implementation, no hosted multi-tenant runtime.

## Phase 0.5: Documentation Foundation

1. Keep docs as source of truth for progress and architecture decisions.
2. Use ADRs for material architecture changes.
3. Require docs updates in the same change set as parser/schema/analytics changes.

## Phase 1: Data Ingestion Foundation (Blocking)

1. Replace placeholder fact extraction with real Inline XBRL parsing.
2. Add concept selection policy (curated Rule #1 set plus JSON allow/deny controls).
3. Add idempotent ingestion semantics and provenance fields.
4. Add resilient SEC acquisition policy (rate compliance, retries, backoff).

## Phase 2: Storage and Query Shape

1. Evolve schema toward analysis-ready shape while preserving compatibility.
2. Add dimensions for sector, industry, company, form, fiscal period, and concept aliases.
3. Add materialization/snapshot strategy for repeated 5y/10y scans.

## Phase 3: Analytics Engine v1

1. Separate metric calculators, normalization rules, and screening rules.
2. Implement scope-aware parameter precedence: global, sector, industry, company.
3. Add model versioning for reproducible runs.
4. Prioritize long-horizon stability and resilience metrics.

## Phase 4: Analyst Workflow (Low-Res UX)

1. Define JSON run specs for universe, horizon, filters, and outputs.
2. Emit deterministic ranked outputs (CSV/JSON/Markdown) and static visuals.
3. Add manual annotation sidecar artifacts without mutating raw facts.

## Phase 5: Expansion-Safe Hooks

1. Keep engine interfaces adapter-friendly (CLI now, notebook/API later).
2. Introduce ownership/sharing metadata at model level.
3. Define model/profile/annotation import-export contracts.

## Verification Gates

1. Build/test: restore, build, test pass in Release.
2. Ingestion realism: representative CIKs produce non-mock persisted facts.
3. Idempotency: rerun ingestion without duplicate logical facts.
4. Reproducibility: two profile runs remain deterministic.
5. Scope overrides: precedence rules pass fixtures.
6. Docs quality: links valid, ADR sequence valid, usage docs match CLI behavior.

## Core Rules

1. Architecture rule: keep engine headless and deterministic.
2. Data rule: preserve immutable raw filing facts; layer analyst judgments separately.
