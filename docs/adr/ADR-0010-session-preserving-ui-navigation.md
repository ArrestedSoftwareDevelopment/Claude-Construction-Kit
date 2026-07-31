# ADR-0010: Session-Preserving UI Navigation and Layer Ownership

- **Status:** Accepted
- **Date:** 2026-07-30
- **Decision owners:** DACK project
- **Related:** Desktop Arena Design Document §§13, 15, 17–18; `DACK-GUI-Architecture.md`

## Context

The RAD proved many editor surfaces quickly: a quick strip, Cockpit, contextual playset pages, Inspector, Sprite Studio/sidebar, HUDs, play/edit state, Boss overlay, and early multi-monitor controls. Their state is currently coordinated through several booleans and visibility side effects.

That coupling has produced a concrete correctness problem: the current “enter Play” path can force Platformer, so ordinary Cockpit navigation from Brickbat, Pinball, or Overhead may change the active game. Similar ambiguity exists around cursor visibility, Sprite Studio return context, subordinate-window closure, editor-only anchors, and which layer draws above effects.

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
- Static source rendering, gameplay animation, HUD, and editor UI can invalidate independently.
- Transition tests can verify preservation without rendering the whole editor.

### Tradeoffs

- The root controller needs incremental extraction into session, shell, input, selection, layer, and window-layout services.
- Existing controls that directly change fields/visibility need commands or state bindings.
- Some convenient RAD side effects must be removed even if they currently make one page appear to “just work.”

## Validation

Automated or repeatable transition tests must prove:

1. Brickbat → Cockpit → Play remains Brickbat with identical mutation state.
2. Pinball/Overhead follow the same invariant.
3. Sprite Studio closes back to the correct actor and originating page.
4. Play hides editor-only objects; Build restores them.
5. Esc never resets the source or level.
6. Boss mode works from Canvas, Cockpit, and Sprite Studio and restores each state correctly.
7. gameplay-critical objects remain visible during maximum allowed effects.
