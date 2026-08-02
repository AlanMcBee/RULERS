# RuleOne Documentation

This folder is the source of truth for planning, architectural decisions, and operational guidance.

## Structure

- `progress/`: active planning and execution tracking
- `adr/`: Architecture Decision Records
- `architecture/`: system descriptions and technical design
- `usage/`: runbooks and user/operator guides

## How to Use This Folder

1. Update `progress/ROADMAP_SHORT_TERM.md` when milestone status changes.
2. Append dated notes to `progress/WORK_LOG.md` during implementation.
3. Add or update ADRs when material architecture decisions change.
4. Keep architecture and usage docs aligned with code changes in the same PR.

## Documentation Quality Gates

- Links resolve and file names are consistent.
- ADR numbering is sequential.
- Usage docs match actual command behavior.
- Parser/schema/analytics changes include docs updates.

## Naming Conventions

- ADRs: `ADR-XXXX-short-title.md` (zero-padded numeric ID)
- Progress docs: uppercase words with underscores
- Architecture and usage docs: uppercase words with underscores
