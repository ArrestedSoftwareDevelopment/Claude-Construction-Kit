# ADR-0010: Session-Preserving UI Navigation, One Shared Session, and Multiple Views

- **Status:** Accepted
- **Date:** 2026-07-30
- **Decision owners:** DACK project
- **Related:** Desktop Arena Design Document §§13, 15, 17–18; `DACK-GUI-Architecture.md`

## Context

The RAD proved many editor surfaces quickly: a quick strip, Cockpit, contextual playset pages, Inspector, Sprite Studio/sidebar, HUDs, play/edit state, Boss overlay, and early multi-monitor controls. Much of their state is still coordinated through root-controller fields and visibility side effects.

That coupling produced a concrete historical correctness problem: an early “enter Play” path could force Platformer, so ordinary Cockpit navigation from Brickbat, Pinball, or Overhead changed the active game. The current RAD F6 path is wired and preserves the selected playset; the defect is fixed, but it remains a required regression test while session ownership is still concentrated in the root controller. Similar architectural risk remains around cursor visibility, Sprite Studio return context, subordinate-window closure, editor-only anchors, and which layer draws above effects.

UI navigation must never alter the creator’s game as an accidental side effect.

## Decision

DACK will model UI/session state through independent axes:

- **Mood:** `Play`, `Build`, or `Understand`.
- **Surface:** `Canvas`, `Cockpit`, `SpriteStudio`, or a transient dialog.
- **Active playset/preset:** independent of Mood and Surface.
- **Source/Snapshot:** independent of navigation.
- **Selection:** selected actor/object/card/source region plus originating page.
- **Window layout:** single-window or coordinated editor/playfield monitors.
- **Safety override:** Boss/Safety temporarily hides/neutralizes all surfaces while preserving prior state.

Navigation invariants:

1. Entering Play preserves the active playset/preset, source, Snapshot, placed objects, mutations, score rules, and level configuration.
2. Esc closes the deepest ordinary surface first: transient edit → Sprite Studio → Cockpit → Canvas. From Canvas it opens the Cockpit.
3. Esc never changes playset, restores the source, resets a run, clears mutations, discards unsaved edits, or quits.
4. Sprite Studio returns to its originating Cockpit page, actor/card selection, and scroll context.
5. Play collapses editor surfaces and hides handles/editor-only objects; Build restores them and the prior selection.
6. Pure Play owns gameplay input and hides the cursor when appropriate. Build, Understand, Cockpit, and Sprite Studio show it. Quick-strip visibility does not decide input authority.
7. The Boss Key atomically hides/neutralizes every DACK window, mutes audio, and releases input, then restores the exact prior ordinary state when dismissed.
8. Closing a subordinate editor/preview does not close the level. Closing the owning main editor closes or resolves its subordinate surfaces safely.
9. Two-monitor mode has one authoritative session/simulation and multiple bound views, never two copies of level state.

### Shared-session and view ownership

Single-window and multi-monitor layouts use the same ownership model:

- One authoritative session owns the active playset, source/Snapshot identity, working-clone state, placed level data, runtime state, selection, dirty state, persistence operations, and undoable commands.
- One simulation clock advances one gameplay world. A second window is a view of that world, not another running scene tree, controller, random stream, physics world, or copy of the level.
- Views may own only view-local state: monitor, window rectangle, zoom/camera framing, panel arrangement, transient focus/hover, and presentation preferences that do not alter gameplay.
- Editor, playfield, preview, and Understand views read immutable published session snapshots and submit commands to the same session authority. They must not mutate duplicated local models and reconcile them later.
- Input authority is explicit. At most one view owns gameplay input and pointer capture at a time; editor shortcuts and the Boss Key are routed through the shared shell.
- Opening, closing, moving, or losing a secondary view cannot pause, reset, fork, or replace the simulation unless the creator issues an explicit session command.
- Rendering cadence may differ by view, but simulation cadence and outcomes do not. A hidden or slower editor view must not slow or advance the game independently.

DACK will use explicit layer roots, back to front:

1. Source Clone.
2. Mutable Terrain.
3. World Objects.
4. Actors.
5. Spectacle Effects.
6. Gameplay-Critical Objects.
7. HUD.
8. Editor Handles / Understanding.
9. Cockpit / Sprite Studio.
10. Boss / Safety.

Players, balls, projectiles, targeting cursors, and active edit handles remain readable above nonessential effects. Boss/Safety is always topmost.

## Consequences

### Positive

- Opening a menu cannot silently change the game.
- Esc, close gadgets, cursor, input, anchors, and subordinate editors become predictable.
- Single- and two-monitor layouts can share the same state model.
- Editor and playfield monitors cannot drift into contradictory level or simulation states.
- Static source rendering, gameplay animation, HUD, and editor UI can invalidate independently.
- Transition tests can verify preservation without rendering the whole editor.

### Tradeoffs

- The root controller needs incremental extraction into session, shell, input, selection, layer, and window-layout services.
- Existing controls that directly change fields/visibility need commands or state bindings.
- View-local and session-owned fields must be separated before independent windows can be reliable.
- Some convenient RAD side effects must be removed even if they currently make one page appear to “just work.”

## Current implementation status

- **Working now:** F6 toggles Build/Play, and the transition no longer forces Platformer. The active playset and working-clone mutations are preserved by the current path.
- **Partially proven:** monitor enumeration and a move-window primitive exist; ordinary Cockpit/Sprite Studio/Boss transitions exercise parts of the state model.
- **Not yet complete:** the authoritative session, command routing, layer ownership, and window-layout responsibilities are still being extracted from the root controller. Coordinated editor/playfield windows are not implemented. Therefore “one simulation, multiple views” is the mandatory architecture for that work, not a claim that dual-monitor mode already ships.

## Validation

Automated or repeatable transition tests must prove:

1. Brickbat → Cockpit → Play remains Brickbat with identical mutation state.
2. Pinball/Overhead follow the same invariant.
3. Sprite Studio closes back to the correct actor and originating page.
4. Play hides editor-only objects; Build restores them.
5. Esc never resets the source or level.
6. Boss mode works from Canvas, Cockpit, and Sprite Studio and restores each state correctly.
7. gameplay-critical objects remain visible during maximum allowed effects.
8. F6 from every playset toggles Build/Play twice without changing playset, Snapshot, score/run state, object identities, or clone mutations.
9. Opening a second view shows the same selected level and simulation tick; closing/reopening it cannot duplicate actors, physics steps, projectiles, random events, audio, or input.
10. Different view refresh rates, zooms, monitor positions, and hidden states do not change simulation outcomes.
