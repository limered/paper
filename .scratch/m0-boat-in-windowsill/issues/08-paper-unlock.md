# 08 — Paper colors + unlock on completion

Status: ready-for-afk

## Parent

`docs/GDD.md` → Design decision Q6 ("completing a template permanently adds that template's paper to the drawer") and Roadmap M0 ("2 paper colors: default + 1 unlocked by completing the boat").

## What to build

The minimum unlock spine:

- A **`PaperRegistry`** with two papers: a default (always unlocked) and a second one bound to the boat (initially locked). Each paper has an id, a display name, and a visual style — for M0 the visual style can be just a tint color applied to the paper material; the proper paper-shader iteration is a later milestone.
- An **unlock state** stored in memory (persistence deferred to MVP, same as #07's collection): the set of unlocked paper ids, initially `{ default }`.
- When `TemplateCompletedEvent` fires for the boat with the default paper, the boat's bound paper id is added to the unlocked set, and the drawer UI (#05) updates: the previously-greyed paper becomes selectable.
- The selected paper id propagates through the `TemplateSession` so the rendered paper and the eventual collection entry use the chosen tint, and the placed model in the diorama shows that tint.

## Acceptance criteria

- [ ] `PaperRegistry` with two paper entries: default + boat's unlock.
- [ ] Unlock state in memory, default-only at startup.
- [ ] Completing the boat (any paper) unlocks the boat's bound paper.
- [ ] Drawer reflects the current unlock state: locked papers greyed out and not selectable; unlocked papers selectable.
- [ ] Chosen paper id flows: drawer → `TemplateSession` → rendered desk model → collection entry → placed model in diorama.
- [ ] Two visually distinct tints — easy to tell the two boats apart on the windowsill.
- [ ] In-memory only; no persistence (per GDD; ticket persistence under MVP).
- [ ] All existing tests still pass.
- [ ] Manual editor check: first boat is default-tinted; after completion, drawer shows the new paper unlocked; folding a second boat with the new paper produces a differently-tinted boat that places into the windowsill alongside the first.

## Blocked by

- #07 — needs the completion event flow and collection entries that carry paper id all the way to the diorama.
