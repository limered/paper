# 07 — Collection + completion beat + placement in diorama

Status: ready-for-afk

## Parent

`docs/GDD.md` → Engine roadmap point 6 ("collection / save") *(minus persistence — see note below)* and design decisions Q10 (completion → collection → place later) and Q7 (free placement within diorama).

## What to build

Connect the dots so a completed boat actually shows up in the windowsill:

- A **`Collection`** in-memory data structure: a list of completed model instances, each carrying template id, paper id, and a placement transform (nullable until placed). For M0 this lives in memory only — **persistence is explicitly deferred to MVP** per GDD's "Save format" open question.
- A **completion beat** when `TemplateCompletedEvent` (from #04) fires on the Desk: short camera push toward the finished model + a chime (placeholder audio is acceptable, or silence with a TODO). After the beat, the model is added to the `Collection` and the desk resets to "drawer closed, ready for another session".
- A **"place from collection" UI** in the Diorama view (#06): a panel listing unplaced models from the collection. Selecting one enters a placement mode where the model follows the cursor against the windowsill's surface (raycast onto the sill); click to place; the placement transform is stored on the collection entry.
- Placed models render in the windowsill using the same per-face transform approach as the desk renderer — i.e. the boat in the windowsill is the real folded mesh, not a screenshot.
- Placed models can be picked up and re-placed (click-and-drag or click-pick-up + click-place; either is acceptable).

## Acceptance criteria

- [ ] `Collection` data structure with add/list/update-placement operations and unit tests.
- [ ] `TemplateCompletedEvent` triggers a completion beat on the desk and adds an entry to the collection.
- [ ] After the beat, the desk view is ready to start another session.
- [ ] Diorama view has a "place from collection" UI listing unplaced entries.
- [ ] Selecting a collection entry enters placement mode; raycast onto the sill positions the model preview; click commits the placement.
- [ ] Placed models render as real folded geometry, not placeholders.
- [ ] Placed models can be re-positioned.
- [ ] In-memory only; no save file written. Re-launching the game loses the collection (acceptable for M0; ticket persistence under MVP).
- [ ] All existing tests still pass.
- [ ] Manual editor check: fold boat → completion beat → go to windowsill → place boat → boat sits on sill → re-position works.

## Blocked by

- #05 — needs the desk view and template session to fire `TemplateCompletedEvent`.
- #06 — needs the diorama scene to place into.
