# Architectural Decision Records

Short, dated notes capturing _why_ a non-obvious architectural choice was made. ADRs explain rationale and trade-offs that aren't recoverable from the code alone.

## When to write one

- A decision rules out an alternative that a future contributor (human or agent) might otherwise reach for
- A constraint exists for a non-obvious reason (perf, library quirk, scope boundary)
- A pattern is being introduced that should be applied consistently across the codebase

Don't write ADRs for routine tactical choices — those belong in code comments or PR descriptions.

## Format

- Filename: `NNNN-short-slug.md`, zero-padded, monotonically incrementing (e.g. `0001-replay-changememory-on-render.md`)
- Use [`0000-adr-template.md`](./0000-adr-template.md) as the starting point
- ADRs are append-only; supersede an old decision by writing a new ADR that references it (don't edit the old one beyond adding a `Superseded by ADR-NNNN` line)

## Status values

- `Proposed` — under discussion
- `Accepted` — in force
- `Superseded by ADR-NNNN` — no longer in force; see the referenced ADR
- `Rejected` — considered and not adopted (still recorded so the conversation isn't repeated)
