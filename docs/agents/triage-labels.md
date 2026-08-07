# Triage Labels

The skills speak in terms of five canonical triage roles. This file maps those roles to the actual label strings used in this repo's issue tracker (local markdown — see `issue-tracker.md`).

| Canonical role     | Label in this repo | Meaning                                  |
| ------------------ | ------------------ | ---------------------------------------- |
| `needs-triage`     | `needs-triage`     | Maintainer needs to evaluate this issue  |
| `needs-info`       | `needs-info`       | Waiting on reporter for more information |
| `ready-for-agent`  | `ready-for-afk`    | Fully specified, ready for an AFK agent  |
| `ready-for-human`  | `ready-for-human`  | Requires human implementation            |
| `wontfix`          | `wontfix`          | Will not be actioned                     |
| `done`             | `done`             | Implemented and verified                 |

When a skill mentions a canonical role (e.g. "apply the AFK-ready triage label"), use the corresponding label string from this table.

Since the tracker is local markdown, "applying a label" means writing it on the `Status:` line near the top of the issue file:

```markdown
# 03 — Animate fold over time

Status: ready-for-afk

...
```

Edit the right-hand column if the vocabulary shifts.
