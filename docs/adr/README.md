# Architecture Decision Records (ADRs)

This folder contains durable records of architecture decisions.

## ADR Lifecycle Status

- Proposed: candidate decision under discussion
- Accepted: approved and active
- Superseded: replaced by a newer ADR
- Deprecated: no longer recommended for new work

## Required ADR Sections

1. Status
2. Context
3. Decision
4. Consequences
5. Alternatives Considered
6. Follow-up Actions

Use `ADR_TEMPLATE.md` as the base template.

## Numbering

- File format: `ADR-XXXX-short-title.md`
- IDs are zero-padded and monotonically increasing.
- Never reuse an ADR ID.

## Linking Rules

- If a decision replaces a previous one, include `Supersedes` and `Superseded by` links.
- Reference affected docs and code paths.
