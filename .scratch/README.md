# .scratch — Local Markdown Issue Tracker (archived)

> **Migrated to GitHub Issues on 2026-08-07.** Active issues now live at https://github.com/limered/paper/issues. This directory is kept as read-only history.

Issues and PRDs for this repo used to live here as markdown files instead of in an external tracker. `docs/agents/issue-tracker.md` now describes the GitHub workflow.

## Historical layout

- One feature per directory: `.scratch/<feature-slug>/`
- The PRD (if any) is `.scratch/<feature-slug>/PRD.md`
- Implementation issues are `.scratch/<feature-slug>/issues/<NN>-<slug>.md`, numbered from `01`
- Triage state was recorded as a `Status:` line near the top of each issue file (see `docs/agents/triage-labels.md` for the role strings)
- Comments and conversation history appended to the bottom of the file under a `## Comments` heading

## What used to go here

- Anything that would otherwise be a GitHub issue, ticket, or "todo" beyond a single working session
- PRDs produced by `/to-prd`
- Issue breakdowns produced by `/to-issues`

## What doesn't

Items here are historical working artefacts. When something becomes stable design rationale, promote it to `docs/adr/` (architectural decisions) or `docs/GDD.md` (design decisions).
