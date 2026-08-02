# ADR-0002: SEC Company Facts Parser Strategy for Phase 1

- Status: Accepted
- Date: 2026-08-02
- Deciders: @AlanMcBee
- Technical Area: ingestion | storage
- Supersedes: none
- Superseded by: none

## Context

The ETL path previously used placeholder/mock fact generation. Immediate priority is to establish real, repeatable SEC-backed ingestion for long-horizon screening while minimizing short-term complexity and preserving a path to deeper filing-specific parsing later.

## Decision

Use SEC `companyfacts` JSON as the Phase 1 source for real facts. Parse and normalize facts from the endpoint, filter by requested form (`10-K` or `10-Q`), and persist provenance fields required for reproducibility.

Also introduce concept selection policy:
1. Default curated Rule #1 concept allowlist.
2. Optional JSON allow/deny override via CLI `--concept-filter`.

## Consequences

Positive:
- Removes mock ingestion and provides real SEC-backed facts immediately.
- Provides stable and testable JSON parsing path.
- Supports controlled ingestion scope via configurable concept filtering.
- Captures provenance for analysis traceability.

Negative:
- `companyfacts` may not expose every filing nuance needed for advanced modeling.
- Full inline XBRL document parsing remains future work.

## Alternatives Considered

1. Parse inline XBRL documents directly in Phase 1.
- Rejected for now: higher implementation complexity and slower time-to-value.

2. Continue with placeholder parser until broader redesign.
- Rejected: blocks meaningful analyst workflow and validation.

## Follow-up Actions

- [ ] Add SEC retry/backoff policy and explicit rate-limit handling.
- [ ] Add dedupe constraints keyed by logical fact identity.
- [ ] Evaluate selective inline XBRL parsing for concepts not well represented in `companyfacts`.
