# DACK GUI Architecture: Collapsible Construction Cockpit

## Purpose

DACK is not one game with menus. It is a desktop/world transformer with multiple construction kits. The GUI therefore needs to scale from a nearly invisible play overlay to a serious editor without burying the playfield under chrome.

The guiding metaphor is:

**A magic transparency sheet over your desktop.**

The user is not editing Word, Excel, GIMP, Krita, TextPad, OpenOffice, or Windows itself. They are editing DACK's playable clone and its transparent gameplay layer: collision, actors, tools, triggers, semantics, effects, and rules.

## Core UX Rule

Every editable object should answer three questions:

1. **What source object is this bound to?** A word, line, icon, window edge, cell, path, color region, or manually placed point.
2. **What gameplay object did it become?** Platform, bumper, pellet, ladder, trigger, checkpoint, enemy, tunnel, power word, target, obstacle, or invisible logic.
3. **Can I detach/edit/toggle it?** Keep as text, convert to graphic, show hybrid, move it, stretch it, bind it elsewhere, or delete only the DACK object.

Detection proposes. The editor disposes.

## Three Working Moods

### 1. Play

The playfield is sacred.

- Fullscreen real estate.
- No fake window.
- Cursor hidden during active play.
- Minimal HUD placed in whitespace.
- HUD fades or slides away when a ball, player, projectile, or selection handle approaches.
- Esc toggles the normal DACK cockpit.
- Boss Key remains separate, instant, and safety-oriented.

Play mode should feel like the document or desktop has become alive, not like a level is trapped inside a UI panel.

### 2. Build

Build mode reveals the construction cockpit.

The cockpit is semi-transparent, collapsible, and organized around the live playfield:

- **Top strip:** source, play/edit state, toolkit, reset clone, Word Sense status, Boss/Safety indicator.
- **Left shelf:** draggable assets, toolkit parts, and creator tools.
- **Right inspector:** selected object properties, bindings, toggles, rules, and numeric controls.
- **Bottom tray:** event log, mutation history, word ticker, OCR/Word Sense discoveries, test-play notes.
- **Center:** the playfield remains visible and interactive, with handles and overlays.

The cockpit should appear with Esc and collapse back to pure play with Esc or a single "Test/Play" action.

### 3. Understand

DACK needs an explicit "what does the engine think?" mode. This is the antidote to magical confusion.

The Understanding Overlay should be toggleable like layers:

- Source clone.
- Text pixels.
- Letter boxes.
- Word boxes.
- Line/platform regions.
- Background/empty-space regions.
- Window/icon/UI objects.
- Detected color regions.
- Visible collision.
- Invisible collision.
- Mutable/deleted terrain.
- Word Sense labels.
- Semantic word-objects.
- Triggers/checkpoints.
- Enemy routes and AI heatmaps.
- HUD avoidance zones.
- Live Desktop boundaries.

If creators can see the engine's interpretation, they can correct it.

## The Cockpit Layout

### Top Strip

The top strip should be compact and persistent only while the cockpit is open.

Suggested controls:

- Source: Desktop / Monitor / Window / Region / Image / Text Grid.
- Mode: Play / Build / Understand.
- Toolkit: Platformer / Brickbat / Pinball / Snake-Maze / RPG / Racing / Tower Defense / Action / Casual.
- Clone: Reset / Save Variant / Compare Source.
- Word Sense: Off / Lazy Local / Full Page Prep, plus status.
- Safety: Boss Key hint and clone-only indicator.

### Left Shelf

The shelf is where construction-kit identity lives. The shell stays consistent; the shelf changes by toolkit.

Global categories:

- Actors.
- Terrain.
- Motion Tools.
- Hazards.
- Pickups.
- Triggers.
- Effects.
- Text/Semantic Tools.
- Invisible Logic.
- Toolkit Parts.

Toolkit examples:

- **Brickbat:** paddles, balls, target recipes, lasers, power-ups, scoring, persistence.
- **Platformer:** platforms, ladders, ramps, slides, conveyors, elevators, checkpoints, enemies, projectiles, dig tools.
- **Pinball:** flippers, plunger lanes, bumpers, rollovers, gates, ramps, drains, inserts, jackpots, nudges.
- **Snake/Maze:** maze walls, pellets, tunnels, enemy homes, wrap edges, word goals, avoid words, power states, route heatmaps.
- **RPG/Roguelike:** glyph legend, walls, doors, keys, monsters, items, stairs, fog, traps, inventory rules.
- **Racing:** tracks, start/finish, checkpoints, boosts, hazards, shortcuts, ghost paths.
- **Tower Defense:** routes, towers, spawns, objectives, upgrade rules, wave schedules.

### Right Inspector

Selecting anything should reveal:

- Name/type.
- Source binding.
- Presentation: text / graphic / hybrid / invisible.
- Geometry and handles.
- Behavior preset.
- Parameters.
- Events/conditions/actions.
- Persistence policy.
- Accessibility/safety notes where relevant.

The inspector should not be the only way to edit. Direct handles come first; numeric fields are the fallback.

### Bottom Tray

The bottom tray is for time and meaning:

- Recent destroyed/read words.
- Found-poem ticker.
- OCR/Word Sense discoveries.
- Mutation log.
- Event grid preview.
- Test-play messages.
- Errors/warnings.
- Performance hints.

This is where "the document is talking back" can become legible without crowding the playfield.

## Toolkit Shelves, Not Toolkit UIs

Each toolkit should feel specialized without becoming a separate application.

The stable shell handles:

- Source selection.
- Mode switching.
- HUD behavior.
- Inspector.
- Layer toggles.
- Save/reset.
- Word Sense.
- Boss/Safety.

The toolkit contributes:

- Shelf parts.
- Overlay handles.
- Rules panels.
- Meters.
- Presets.
- Test-play affordances.

This prevents feature growth from turning into a pile of unrelated windows.

## Source Binding and Manual Authorship

Auto-detection and Word Sense are assistants, not authorities.

Example: Word Sense discovers `TARPIT`.

The creator can:

- Accept it as a hazard.
- Ignore it.
- Turn it into a tarpit graphic.
- Keep it as readable text.
- Show hybrid text plus tar bubbles.
- Move the hazard elsewhere.
- Stretch/rotate the hazard.
- Detach behavior from the word.
- Rebind behavior to another word.
- Use it as a score target instead of a hazard.

The same pattern applies to `LADDER`, `BRIDGE`, `KEY`, `DOOR`, `CHECKPOINT`, `BOOST`, `POISON`, `JACKPOT`, and manually placed objects.

## Word Sense in the GUI

Word Sense is a local/offline meaning layer over the geometry layer.

Recommended setting:

`Word Sense: Off / Lazy Local / Full Page Prep`

Default:

`Lazy Local`

Per-playset usage toggles:

- Effects.
- Bonuses.
- Semantic objects.
- Goals.
- AI behavior.
- All.

Status examples:

- `Word Sense Off`
- `Local reader unavailable`
- `Reading nearby words`
- `23 words known`
- `Full page prep paused`

The UI should make the fallback guarantee clear: the game still works when Word Sense is off, late, or unavailable.

## Understanding Overlay as a Product Feature

The Understanding Overlay is not just debug UI. It is how non-programmers learn to trust and edit the engine.

Recommended controls:

- Show all.
- Hide all.
- Show source.
- Show playable objects.
- Show engine guesses.
- Show manual edits.
- Show Word Sense.
- Show invisible logic.
- Show mutations.

Objects should be color-coded by authority:

- **Source-derived:** detected from capture/UIA/OCR/import.
- **Suggested:** proposed by DACK but not accepted.
- **Creator-authored:** manually placed or accepted/edited.
- **Runtime-mutated:** changed by gameplay.
- **Invisible logic:** triggers, checkpoints, zones, route hints.

## HUD Rules

HUD is useful, but it must never become an accidental obstacle.

Rules:

- Prefer whitespace placement.
- Stay outside the playfield if there is unused screen space.
- Fade or slide away when approached.
- Let creators choose always visible / fade on approach / hidden during play.
- Let game-critical objects redraw above effects and HUD.
- Keep Boss/Safety UI exempt from ordinary fading.

Brickbat's score/word ticker is the current proof. Pinball score reels, Snake/Maze word-goal trackers, RPG inventory, and Tower Defense wave meters should use the same HUD service.

## Drag-Create-Edit Loop

The shelf interaction should be one fluid gesture:

1. Drag a part from the shelf.
2. Preview snaps/alignment on the playfield.
3. Release to place.
4. Handles appear immediately.
5. Inspector opens with the object selected.
6. One-click test play preserves the current edit state.

Examples:

- Drag a ladder word-object, then stretch its endpoints.
- Drag a pinball flipper, then edit its sweep arc.
- Drag a Snake/Maze tunnel, then connect two endpoints.
- Drag a Brickbat target recipe, then choose letter/word/heading/icon/color.
- Drag a Tower Defense route, then show enemy path heatmap.

## Phased Refactor Path

### Phase A: Replace RAD Toolbar with Shell Skeleton

- Input Router: Esc menu toggle, Boss Key separation, cursor policy.
- UI Shell Controller: Play / Build / Understand states.
- Layer Manager: playfield, actors, effects, HUD, overlay, cockpit, safety.
- HUD Manager: whitespace placement and fade-on-approach.

### Phase B: Add Shelf and Inspector

- Global shelf categories.
- Toolkit shelf registry.
- Drag preview.
- Direct handles.
- Selection model.
- Property inspector.

### Phase C: Add Understanding Overlay

- Text/word/line overlays.
- Collision overlays.
- Word Sense overlays.
- Mutation overlays.
- Invisible logic overlays.
- Source/manual/runtime authority coloring.

### Phase D: Toolkit-Specific Cockpits

- Brickbat builder overlay.
- Platformer movement/terrain overlay.
- Pinball shelf/handles.
- Snake/Maze word-goal and route overlay.
- RPG glyph map overlay.

### Phase E: Polish and User Trust

- Named UI presets: Quiet Office, Arcade Neon, Terminal/BBS, Debug.
- First-run tutorial.
- Performance toggles.
- Word Sense install/status helper.
- Clone-only warnings and metadata-scrub status.

## Design North Star

DACK should feel like the user has opened a familiar document or desktop, then discovered a transparent toybox layer where everything can become a game object.

The GUI succeeds when:

- Play mode feels uncluttered.
- Build mode feels powerful.
- Understand mode makes engine guesses visible.
- Manual authorship outranks automation.
- Any toolkit can apply to any screen.
- Complexity lives in shelves and layers, not in permanent windows.
