# ADR-0001: Repository Documentation System

- Status: Accepted
- Date: 2026-08-02
- Deciders: @AlanMcBee
- Technical Area: interface | operations
- Supersedes: none
- Superseded by: none

## Context

RuleOne needs durable planning and architecture traceability as implementation resumes after a long pause. Session-only plans are not sufficient for long-running execution, and future expansion requires transparent decisions that can be reviewed over time.

## Decision

Adopt a root `docs/` system as the repository source of truth for planning and architecture communication, with these subdomains:

1. `docs/progress/` for active plans and execution logs
2. `docs/adr/` for architecture decisions
3. `docs/architecture/` for technical system descriptions
4. `docs/usage/` for runbooks and operator guidance

Documentation updates are required in the same change set when parser/schema/analytics decisions materially change.

## Consequences

Positive:
- Improves project continuity across long intervals.
- Makes architecture evolution auditable.
- Reduces onboarding cost for future collaborators.

Negative:
- Adds documentation maintenance overhead.
- Requires discipline to keep docs in sync with code.

## Alternatives Considered

1. Keep plans only in chat/session memory.
- Rejected: low durability and weak collaboration value.

2. Keep only a single root planning markdown file.
- Rejected: insufficient separation for ADRs, usage, and architecture artifacts.

## Follow-up Actions

- [ ] Add initial system overview and data flow docs.
- [ ] Add CLI runbook and JSON run spec reference.
- [ ] Add ADRs for parser strategy and schema evolution decisions.
