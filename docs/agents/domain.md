# Domain Docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Layout (single-context)

This repo uses a single-context layout:

```
/
├── CONTEXT.md            ← domain glossary
├── docs/
│   ├── GDD.md            ← Game Design Document (current source of design vocabulary)
│   └── adr/              ← architectural decision records
└── valleyfold/, Testing/ ← source
```

There is no `CONTEXT-MAP.md` and no per-module `CONTEXT.md` files. If the codebase later splits into multiple bounded contexts, switch to the multi-context layout described below.

## Before exploring, read these

- **`CONTEXT.md`** at the repo root.
- **`docs/GDD.md`** — the Game Design Document. Currently the most fleshed-out source of domain vocabulary (pillars, fold tiers, milestones, templates, dioramas).
- **`docs/adr/`** — read ADRs that touch the area you're about to work in.
- **`AGENTS.md`** — engineering-level architecture quirks (Statics, EventBus, two-model Frame/Frame3D split, IFold, ChangeMemory).

If any of these files don't exist or are sparse, **proceed silently**. Don't flag their absence; don't suggest creating them upfront. The producer skill (`/grill-with-docs`) creates entries lazily when terms or decisions actually get resolved.

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in `CONTEXT.md` or `docs/GDD.md`. Don't drift to synonyms the docs explicitly avoid.

If the concept you need isn't in the glossary yet, that's a signal — either you're inventing language the project doesn't use (reconsider) or there's a real gap (note it for `/grill-with-docs`).

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding:

> _Contradicts ADR-0007 (face-stacking via face-graph traversal) — but worth reopening because…_

## Future: multi-context layout (not currently used)

If the codebase grows multiple bounded contexts (e.g. engine vs. game vs. editor tooling), introduce a `CONTEXT-MAP.md` at the root pointing at per-context `CONTEXT.md` files, and put context-specific ADRs under `src/<context>/docs/adr/`:

```
/
├── CONTEXT-MAP.md
├── docs/adr/                          ← system-wide decisions
└── src/
    ├── engine/
    │   ├── CONTEXT.md
    │   └── docs/adr/
    └── game/
        ├── CONTEXT.md
        └── docs/adr/
```
