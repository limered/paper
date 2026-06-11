# Valleyfold — Game Design Document

A cosy origami game where the player folds a square paper, guided step-by-step, and presents finished models in pre-built dioramas.

This document records design decisions only. Implementation details belong in the codebase; setup belongs in `AGENTS.md`.

## Pitch

The player sits at a wooden desk, opens a drawer, picks a sheet of paper and a folding template, and folds an origami model by clicking highlighted ghost creases. Finished models go into a collection and can be placed inside themed 3D dioramas (paper-tree shelf, windowsill, etc.) for display. Progression unlocks new templates, paper styles and dioramas. There is no scoring, no failure, no timer.

## Pillars

1. **Beauty** — paper shaders, several paper colors, dioramas read as illustrations. Finished models are *real* 3D geometry, not pre-rendered images.
2. **Creation** — the player makes each model fold by fold. Guidance is presented as ghost crease lines on the paper.
3. **Advancement & Completion** — unlocks come from completing models. Each new template, each finished model's paper, each new diorama is a small reward.

## Core loop

Hub → Desk view → pick template + paper from drawer → click ghost creases one at a time until the model is complete → model is added to the collection → from any Diorama view, place collected models freely in the scene.

There is no walking. The hub is a small menu of fixed destinations: **Desk**, **Diorama(s)**, **Settings**.

## Design decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | **Reuse the existing fold engine; constrain player input to prescribed folds.** Internally each step calls `VertexToVertexFold` / `VertexToEdgeFold` / `EdgeToEdgeFold` with pre-baked endpoints from the template. The engine is still WIP — see *Engine roadmap*. | Preserves the real-geometry promise of Pillar 1. Avoids the "pre-baked animation" trap. |
| 2 | **v1 fold vocabulary is tier 1 only** — flat valley/mountain folds on already-flat paper. No reverse, squash, petal or sink folds. Bird base and crane are deferred indefinitely. | Tier 2/3 require multi-layer engine work that doesn't exist yet. Tier 1 still yields a viable model catalogue. |
| 3 | **Starting template set** (tier 1, square paper): hat, boat, fortune teller, samurai helmet, dog face, tulip. **First implemented model: boat.** | Concrete catalogue scoped to engine capability. |
| 4 | **Fold gesture: click the ghost crease.** One click on the highlighted crease plays the canonical fold animation. | Cosy, minimal-skill, smallest input surface for a WIP engine. Drag-corner-to-corner is the planned post-MVP upgrade. |
| 5 | **Guidance: one ghost crease at a time.** Only the next required fold is visible. Wrong clicks do nothing. No undo system in v1. | Zero failure surface. Removes the need for a working undo system. The "all creases visible, strict order" variant is a planned expert mode. |
| 6 | **Advancement = completion spine + collection flavor.** Completing a template unlocks the next template in a small branching tree, *and* permanently adds that template's paper to the drawer. No currency, no scoring, no dailies. | Honors Pillar 3 without introducing judgment systems that fight Pillar 1's cosy tone. |
| 7 | **Display = themed dioramas with free 3D placement within them.** Every completed model is kept (removable). Dioramas are pre-built scenes (e.g. paper tree on a shelf) viewed with an orbit camera; the player places models freely inside the scene. No free placement at room scale. | Matches the "different scenes" language in the original pitch while keeping object-manipulation scope bounded to one diorama at a time. |
| 8 | **No WASD / no character controller in MVP.** Hub is a fixed-camera menu of destinations. | Cuts a whole subsystem (controller, collisions, walkable room art) for a game whose loop happens at a desk and inside dioramas. Walking can return as a post-launch upgrade. |
| 9 | **Template & paper selection lives inside the Desk view's drawer.** Entering the desk view shows the wooden surface and a drawer that slides out on click. Pick template + paper, close drawer, fold begins. | Preserves the tactile "drawer under the desk" image from the pitch despite no walking. |
| 10 | **On completion, the model goes to the collection — placement happens later in a diorama view.** Short celebratory beat on the desk (camera push + chime) then it's available in a "place from collection" UI inside any diorama. | Supports a folding session (fold several boats in different papers) without forcing a placement interruption. |
| 11 | **Folding camera: fixed three-quarter angle.** Desk view is framed like an illustration. Diorama views use a free-floating orbit camera. | Tier-1 folds read fine from a fixed angle, removes camera-control input ambiguity at the desk, and the orbit cost is paid only where it matters (dioramas). |

## Roadmap

Three sequential milestones, each shippable in concept:

### M-1 — Single-layer 3D fold (engine prerequisite)

Not a player milestone. Proves the engine can rotate paper geometry around a crease axis. No game systems yet.

- A `VertexToVertexFold` applied to the unfolded square actually rotates half the paper by `FoldAngle` over time.
- `Frame3D` per-face transform reflects the fold history.
- Visible in the existing scene as a single diagonal fold animating from 0° to 180°.

### M0 — Boat in the windowsill

Proof of loop. Not a game yet, but proves the player-facing systems hang together.

- 1 template: **boat**.
- 1 diorama: **windowsill**.
- 2 paper colors: default + 1 unlocked by completing the boat.
- Full loop: Desk view → drawer → pick boat + paper → click ghosts → completion beat → boat appears in collection → Diorama view → place boat in windowsill.
- Requires multi-layer fold support to land in the engine first.

### MVP — First shippable version

- 3 templates: boat, hat, tulip.
- 2 dioramas: windowsill + shelf with paper tree.
- 4 paper colors.
- Unlock tree wired up; "place from collection" UI in dioramas; settings.
- ~30–60 min of content end-to-end.

### v1 — Launch scope

- 6 templates: hat, boat, fortune teller, samurai helmet, dog face, tulip.
- 3 dioramas.
- 8 paper colors.
- All tier-1 content shipped.

### Post-v1 (deferred, not promised)

- Drag-corner-to-corner gesture as an alternative input mode.
- Expert mode: all creases visible, strict order, no auto-highlight.
- Tier 2 folds (reverse folds) → new model catalogue (jumping frog, fish base).
- Tier 3 folds (squash/petal/sink) → bird base, crane.
- Real walkable room with WASD.
- Free placement at room scale (Unpacking-style).

## Engine roadmap (what blocks each milestone)

The engine performs real 3D fold rotations today: `PaperRenderer.OnPaperFolded` walks `ChangeMemory`, identifies vertices on the picked-vertex side of each fold line, and calls `ReflectedAroundLine` (a true `Vector3.Rotated(axis, angle)`) to fold them. The math works. What's missing:

1. The rotation angle is hardcoded to `Math.PI` (always fully flat). `Edge.FoldAngle` exists and is unused in rendering.
2. There is no animation — folds snap to the final position instantly.
3. The "vertices on the same side of the fold line" check is purely 3D-positional. It has no concept of face layers, so it cannot handle folding *already-folded* paper correctly.
4. Only `ChangeType.ValleyFold` exists; no mountain.

Blocking work, in order:

1. **M-1: animation + parameterised angle.** Tween a per-fold progress 0 → target angle over time; render with the interpolated angle; block new folds while one is animating. Per architecture decision Q12, keep the existing replay-all-changes loop. Required by M-1.
2. **M0: face-stacking / layer order.** Replace the "same-side-in-3D" rule with a face-graph traversal: starting from the face containing the picked vertex, walk the adjacency graph without crossing the new crease to determine which faces participate. Per-face transforms accumulate fold history. This is the rendering refactor deferred from Q12. Required by M0 (boat needs at least one fold of already-folded paper).
3. **M0: mountain folds.** Add `ChangeType.MountainFold` (or sign on the angle). Required by M0 if any boat step is a mountain.
4. **M0: template format.** A serialisable list of prescribed folds (fold type + endpoints + assignment + target `FoldAngle`) plus ghost-line metadata. Required by M0.
5. **M0: ghost-crease renderer.** The per-step highlighted line on the current paper surface, click-targetable. Required by M0.
6. **M0: collection / save.** Per-template list of completion instances (paper choice, timestamp) and their diorama placement transforms. Required by M0.
7. **M0: diorama orbit camera + placement UI.** Required by M0.
8. **MVP: paper shader iteration.** Pillar 1 quality bar.

Undo (`ChangeMemory`'s unfold path) is **not** on the critical path for the game — decision 5 removes it from the v1 requirement set. The change log itself is, however, retained because the renderer relies on it.

## Open questions (deferred, not blocking)

- Paper shader direction (texture/normal/translucency style) — defer until M-1 is in.
- Audio direction — defer until MVP.
- Save format and slot model — defer until M0 forces the choice.
- Settings content — defer until MVP.
- Platform / input device assumptions beyond mouse+keyboard — defer.
- Accessibility (colorblind paper choices, text size, motion) — revisit at MVP.
