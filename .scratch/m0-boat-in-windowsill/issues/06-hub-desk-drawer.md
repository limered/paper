# 05 — Hub + Desk view + drawer + start fold from UI

Status: ready-for-afk

## Parent

`docs/GDD.md` → Core loop, design decisions Q8 (no WASD; hub is a menu), Q9 (template/paper selection lives in desk drawer), Q11 (fixed three-quarter desk camera).

## What to build

The minimal player-facing shell that gets the player from "game launched" to "boat session started" without a debug keybind:

- A **Hub scene** with three destinations: **Desk**, **Diorama** (placeholder — opens #06's stub), **Settings** (placeholder, dead button is fine). No 3D walking; just a fixed-camera menu, can be UI buttons over a still illustration or over a simple 3D framing.
- A **Desk scene** with a fixed three-quarter camera framing a wooden desk surface. A drawer element (3D mesh or 2D panel — either is acceptable in M0) that opens on click.
- Inside the drawer: the boat template (the only one) and the two paper colors (the default + the locked one; the locked one is greyed out until #08 unlocks it). Selecting a paper + template + "begin" closes the drawer and starts a `TemplateSession` from #04.
- A "back to hub" affordance from Desk and Diorama.

Camera placement in `main.tscn` was previously noted as needing fixing — handle it as part of this slice (the desk camera here is a new fixed-three-quarter camera, so the old placement concern dissolves into "set up the desk scene's camera correctly").

## Acceptance criteria

- [ ] New scene(s) for Hub and Desk, wired together with scene transitions or scene swaps.
- [ ] Hub presents three destinations; Desk and Diorama buttons navigate; Settings can be a no-op.
- [ ] Desk view shows a fixed three-quarter camera over a desk surface.
- [ ] Drawer opens on click, shows boat + paper choices, and a "begin" button.
- [ ] "Begin" starts a `TemplateSession` for the boat with the chosen paper; the ghost-click flow from #04 takes over.
- [ ] Locked paper is visually distinct (greyed out) and not selectable until #08.
- [ ] Back-to-hub works from Desk.
- [ ] All existing tests still pass.
- [ ] Manual editor check: launch game → hub → desk → drawer → pick boat → ghost-click through the boat.

Note for the commit: this slice touches `.tscn` files. If the user has the Godot editor open, the scene changes may need re-applying via the editor — mention this in the commit message.

## Blocked by

- #04 — needs ghost-click sessions to be triggerable from a non-debug path.
