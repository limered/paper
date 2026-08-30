# CONTEXT — Valleyfold

Domain glossary for the project. Skills like `improve-codebase-architecture`, `diagnose`, and `tdd` read this before exploring the codebase to pick up the right vocabulary.

## Source of truth (for now)

The authoritative design vocabulary currently lives in [`docs/GDD.md`](docs/GDD.md) — the Game Design Document. Until terms are extracted into a proper glossary here, treat the GDD as the canonical source for domain language (folds, templates, dioramas, pillars, milestones, etc.).

Engineering-level vocabulary (Frame, Frame3D, EventBus, IFold, ChangeMemory, etc.) is described in [`AGENTS.md`](AGENTS.md) under "Architecture quirks an agent will miss".

## Glossary

_To be filled in as terms get pinned down. Use `/grill-with-docs` to add entries when ambiguity surfaces._

### UI

**Folding HUD**:
The in-desk control surface for fold mode (valley/mountain/unfold/refold) and reset. Currently implemented as the `ingame.tscn` sidepane; accepted as the real player-facing UI for M0, to be styled "cute and chill" rather than removed.
_Avoid_: debug panel, dev UI

**Hub**:
The top-level destination menu. M0 treatment: a still illustration background with buttons for Desk, Diorama, and Settings. No 3D walking or character controller.
_Avoid_: main menu, lobby

**Desk drawer**:
The UI panel in the Desk view where the player selects template and paper. M0 treatment: a styled 2D panel (drawer-front look with wood texture, rounded corners, soft shadow) that slides open/closed. The actual 3D desk surface stays visible behind it. Paper choice controls are functional text buttons in #6; their visual styling as color swatches is issue #9's scope.
_Avoid_: drawer, template picker

**Desk session lifecycle**:
A template session begins when the player picks template + paper in the drawer and presses Begin. If the current paper is not a valid starting square (e.g., after a previous session), Begin first resets the paper automatically, then starts the session. The player is not required to press Reset manually between sessions.
_Avoid_: manual reset loop

**Hub transition**:
The animated change between Hub, Desk, and Diorama scenes. M0 treatment: directional wipe/slide per destination (e.g., Desk slides up from below, Diorama wipes in from the side). Replaces the current instant `ChangeSceneToFile` cuts.
_Avoid_: scene change, fade

### Audio

**UI audio**:
Sound feedback for buttons, drawer, and session start. Explicitly out of scope for #6 and M0; deferred to the MVP audio direction pass per `docs/GDD.md` open questions.
_Avoid_: SFX, sound effects
