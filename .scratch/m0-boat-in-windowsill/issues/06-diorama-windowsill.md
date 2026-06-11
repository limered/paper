# 06 — Diorama view + orbit camera + windowsill stub

Status: ready-for-afk

## Parent

`docs/GDD.md` → Engine roadmap point 7 ("diorama orbit camera + placement UI") and design decisions Q7 (themed dioramas with free placement within) and Q11 (orbit camera for dioramas).

## What to build

A standalone Diorama scene wired into the Hub from #05:

- A **windowsill** 3D scene — a window frame, a sill, simple lighting. Visual quality is post-M0; the geometry can be primitive cubes with a placeholder material. The point is to have a bounded volume the player can orbit around and (in #07) place models inside.
- A **free orbit camera**: mouse-drag to orbit around a fixed pivot at the windowsill's centre; scroll to zoom; right-drag (or similar) to pan. Standard "inspect-a-model" controls.
- A "back to hub" affordance.
- **No placement UI yet** — that's #07. This slice's success criterion is "you can navigate from hub to windowsill, orbit around it, and go back".

This slice can be developed in parallel with #03/#04/#05 once #01 lands, because it touches no engine code.

## Acceptance criteria

- [ ] New Diorama scene reachable from the hub.
- [ ] Windowsill geometry visible (primitive shapes are fine).
- [ ] Orbit camera with drag-to-orbit and scroll-to-zoom controls.
- [ ] Back-to-hub button works.
- [ ] All existing tests still pass.
- [ ] Manual editor check: hub → diorama → orbit around → back to hub.

Note for the commit: this slice touches `.tscn` files. If the user has the Godot editor open, the scene changes may need re-applying via the editor — mention this in the commit message.

## Blocked by

- #01 — only because the engine refactor should land first to avoid merge conflicts on `PaperRenderer`. Otherwise content-independent of #02–#05.
