# 01 — Make Frame3D auto-sync with Frame on read

Status: needs-triage

## Parent

None — standalone tech-debt cleanup. Surfaced by the OOB fixed in commit `7e748dc` (`Fix Frame3D.ImportMetadataFromFrame OOB after fold completes`).

## What's wrong

`Frame` and `Frame3D` are two parallel models kept in sync by `PaperRenderer.RebuildFrame3D` running every frame in `_Process`. Mutations to `Frame.Vertices` happen elsewhere (input handlers, fold appliers, edge splits) and are NOT immediately reflected in `Frame3D.Vertices`.

For one tick after every fold this divergence is observable:

- `_Input`: fold completes; `EdgeCommands.AddVertexToEdge` grows `Frame.Vertices`.
- `_Process` (same frame, undefined node order): if any consumer reads `Frame3D` before `PaperRenderer._Process` runs, it sees stale state — `Frame3D.Vertices.Count < Frame.Vertices.Count`.

The recent OOB in `Frame3D.ImportMetadataFromFrame` was one symptom (`StartPointSelectionState._Process` ran first, walked off the end of `Frame3D.Vertices`). Commit `7e748dc` patched it with a defensive `Math.Min` and a comment explaining the race. That's a band-aid: every future consumer of `Frame3D` has to either be defensive or rely on getting lucky with node order.

Other suspect call sites that read `Frame3D` from `_Process` paths:

- `valleyfold/src/Interaction/SelectionStates/StartPointSelectionState.cs:47` — iterates `Frame3D.Vertices` for snapping.
- `valleyfold/src/Interaction/SelectionStates/EndPointSelectionState.cs:50,72` — reads `Frame3D.Vertices[id]` for the picked vertex and preview line.
- `valleyfold/src/Folding/FoldInteractionApplier.cs` (legacy `ApplyVertexFold`) — reads `Frame3D.Vertices` during the crossing scan; safe in `_Input` because the fold is the thing growing it, but worth auditing.

None of these would crash on the same tick they themselves trigger the mutation, but any cross-system read inside `_Process` is a latent crash.

## What to build

Make `Frame3D` self-syncing so consumers can't read stale state. Two viable shapes — pick one in the implementation and document why in the commit message:

- **Option A — lazy rebuild on read.** Track a `_dirty` flag on `Frame3D` (or compare `Frame.Vertices.Count` against `Vertices.Count` on entry to public readers). If divergent, call `ImportFromFrame` before serving the read. Consumers stay ignorant.
- **Option B — dirty-bump from mutators.** Have every code path that mutates `Frame.Vertices` (currently `EdgeCommands.AddVertexToEdge`, plus any future ops added by ChangeMemory replay) mark `Frame3D` dirty. `PaperRenderer` and any pre-renderer consumer rebuild on demand from the flag.

Both require care: `Frame3D` carries per-vertex state (`Coord` rotated by replayed folds, `IsSelected`) that a naive rebuild would clobber. The rebuild path must still go through the existing replay loop in `RebuildFrame3D` (folds + layer updates), not just `ImportFromFrame`.

A simpler intermediate step worth considering first: have `ThreeDeePaperSelector._Process` explicitly request `PaperRenderer.RebuildFrame3D` (or an equivalent sync method) before running any selection state, removing the node-order assumption entirely. That's smaller and may be enough.

## Acceptance criteria

- [ ] After any fold that grows `Frame.Vertices`, the next `_Process` tick on *any* node sees `Frame3D.Vertices.Count == Frame.Vertices.Count` regardless of node order.
- [ ] The defensive `Math.Min` clamp in `Frame3D.ImportMetadataFromFrame` (commit `7e748dc`) is no longer load-bearing — either removed or kept as a redundant safety net with a comment saying so.
- [ ] Regression test `Frame3DTests.ImportMetadataFromFrame.DoesNotThrow_WhenFrameHasMoreVerticesThanFrame3D` still passes (the contract — tolerate or prevent — must remain).
- [ ] No new test regressions; all existing tests still pass.
- [ ] If Option A is chosen, the lazy-rebuild path must NOT clobber per-vertex state (rotation, layers, selection) carried over from the previous frame's replay.

## Blocked by

None.

## Notes

This is the architectural follow-up flagged in the `/diagnose` post-mortem on `7e748dc`. It's not urgent — the band-aid holds — but every new `Frame3D` consumer adds another latent crash if the underlying invariant isn't fixed.
