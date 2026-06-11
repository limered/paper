# .scratch — Local Markdown Issue Tracker

Issues and PRDs for this repo live here as markdown files instead of in an external tracker. This is the canonical location referenced by `docs/agents/issue-tracker.md`.

## Layout

- One feature per directory: `.scratch/<feature-slug>/`
- The PRD (if any) is `.scratch/<feature-slug>/PRD.md`
- Implementation issues are `.scratch/<feature-slug>/issues/<NN>-<slug>.md`, numbered from `01`
- Triage state goes on a `Status:` line near the top of each issue file. Label vocabulary is in [`docs/agents/triage-labels.md`](../docs/agents/triage-labels.md)
- Comments and conversation history append to the bottom of the file under a `## Comments` heading

## Example

```
.scratch/
└── m1-single-layer-3d-fold/
    ├── PRD.md
    └── issues/
        ├── 01-parameterise-fold-angle.md
        ├── 02-tween-fold-progress.md
        └── 03-block-input-during-animation.md
```

## What goes here

- Anything that would otherwise be a GitHub issue, ticket, or "todo" beyond a single working session
- PRDs produced by `/to-prd`
- Issue breakdowns produced by `/to-issues`

## What doesn't

Items here are working artefacts, not finished documentation. When something becomes stable design rationale, promote it to `docs/adr/` (architectural decisions) or `docs/GDD.md` (design decisions). Once an issue is closed, leave the file in place as history — don't delete it.
