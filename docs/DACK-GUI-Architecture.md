# DACK GUI Architecture: Collapsible Construction Cockpit

Related: [DACK Sprite Studio Mini-App](DACK-Sprite-Studio-Mini-App.md).

## Purpose

DACK is not one game with menus. It is a desktop/world transformer with multiple construction kits. The GUI therefore needs to scale from a nearly invisible play overlay to a serious editor without burying the playfield under chrome.

Top-level game-type/menu planning now lives in [`DACK-Top-Level-Menu-Plan.md`](DACK-Top-Level-Menu-Plan.md). The key decision there is to organize primary game menus by view/control family—Side View, Overhead, Ball/Table, Paddle/Clearing, Grid/Text, Route/Flow, Ambient/Desktop Toybox—then place named presets underneath.

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
- View Family: Side View / Overhead / Ball-Table / Paddle-Clearing / Grid-Text / Route-Flow / Ambient.
- Preset: changes by family, e.g. Platformer, Brickbat, Pinball, Combat/Tanks, RPG, Snake/Maze, Racing, Tower Defense.
- Clone: Reset / Save Variant / Compare Source.
- Word Sense: Off / Lazy Local / Full Page Prep, plus status.
- Safety: Boss Key hint and clone-only indicator.
- Close gadget: visible `×` to hide the ordinary Cockpit; separate from Boss Key.

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

Only the selected toolkit/family page should be expanded by default. Other toolkit pages fold away to keep the playfield and inspector usable. The Inspector and Understand panels remain broadly available because selected-object attributes, source bindings, invisible logic, and detection layers cut across every game type.

This prevents feature growth from turning into a pile of unrelated windows.

## Character Page vs. Sprite Sidebar

The live sprite sidebar remains the quick in-context pad: select an actor, tweak pixels or frame labels, see the playfield update. It should stay small, hidable, and toy-like.

As actor setup grows, DACK needs a larger Character page. That page should collect the heavier work: imported frame source, animation labels, timing, origins/baselines, attachment points, AI/rule cards, projectile slots, sounds, effects, and text-interaction options. `Idle` and `Climb` should be treated as core labels alongside run, jump, shoot, hurt, and death.

This prevents the sidebar from becoming a cramped Aseprite clone and gives creators a natural place to tune enemies, players, guards, climbers, flyers, projectiles, and future RPG/overhead actors.

Placed toolkit objects should follow the same principle: the playfield gives direct manipulation with A/B handles, while the Inspector gives precise nudges. Ramps, slides, conveyors, elevators, pinball parts, gates, and future line objects should be rotatable; line tools rotate by their endpoints and by Inspector rotate nudges. Ladders are the exception: they should remain vertical climb volumes, with width tuned against the player character rather than treated as angled ropes.

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

### Cross-Cutting Editor Tools

DACK should grow a visible motion/path editor family:

- **Parabola editor first:** cheap and broadly useful. Define start/end, apex/height, gravity/flight time, preview ghost positions, and bind the result to jumps, thrown objects, arcing shots, enemy hops, bounce/stomp rebounds, pickups, and power-up travel.
- **Bezier/spline editor next:** richer handles for patrol paths, flying enemies, swinging vines/ropes, racing curves, pinball ramps/wireforms, camera moves, particle ribbons, and authored enemy entrances.

The parabola editor should be near-term because it is simple, legible, and directly connected to the platformer/projectile work already underway. Beziers should wait until the UI can support curve handles cleanly.

### Phase A: Replace RAD Toolbar with Shell Skeleton

- Input Router: Esc menu toggle, Boss Key separation, cursor policy.
- UI Shell Controller: Play / Build / Understand states.
- Layer Manager: playfield, actors, effects, HUD, overlay, cockpit, safety.
- HUD Manager: whitespace placement and fade-on-approach.

### Shared shadow rendering

Every visible gameplay object should be able to cast a cheap composited shadow onto the cloned document/page. The goal is not physical realism; it is grounding. Objects should feel like they sit on the paper, desktop, or app surface rather than floating as ordinary UI.

First rule:

- Sprites draw a projected duplicate of their current frame before the real frame.
- The duplicate is squashed vertically, slightly rotated/offset, tinted grayscale/black, and made semi-transparent.
- This creates a single-function "paper shadow" for players, enemies, animated targets, imported characters, and later pickups/projectiles.

Second rule:

- Vector/toolkit objects use the same concept with shape-specific helpers: line shadows for ramps/conveyors/flippers, ellipse shadows for bumpers/balls, soft rect shadows for panels and icons.
- A later Renderer/Theme service should expose shadow parameters: `shadowEnabled`, `shadowOpacity`, `shadowOffset`, `shadowSquash`, `shadowBlurStyle`, and `shadowFollowsTheme`.
- Dark mode and Boss Key can reduce or disable shadows if they harm legibility or office-safe presentation.

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

## System theme and dark mode

DACK should respect the user's system theme without letting the game layer damage document legibility.

Policy:

- Default UI chrome should follow system light/dark mode when the platform exposes it.
- The captured document clone remains visually faithful by default. DACK should not globally invert, recolor, or "dark-mode" the source clone unless the creator chooses a stylized table/theme mode.
- Game overlays adapt instead:
  - light system theme: quieter glow, darker outlines, lower underlay opacity;
  - dark system theme: slightly stronger glow, brighter HUD text, but still capped to avoid arcade glare;
  - high-contrast/accessibility mode: prefer crisp outlines and reduced strobe over neon haze.
- ANSI/BBS underlays should have independent `underlayOpacity`, `glowStrength`, `scanlineStrength`, and `textProtection` controls.
- Text protection wins over theme. If an underlay, effect, or HUD region makes document text hard to read, DACK should dim/mask/relocate the overlay rather than alter the document.
- Boss Key should always force the quietest office-safe presentation regardless of system theme.

Implementation notes:

- Keep a small Theme Service rather than scattering color constants everywhere.
- Store user preference as `System`, `Light`, `Dark`, `Quiet Office`, `Arcade Neon`, `Terminal/BBS`, or `Debug`.
- At startup, read the OS/system theme if Godot exposes it cleanly on the platform; otherwise default to `System` with light-safe values.
- Make effects request theme colors/intensity from the service, so Brickbat explosions, Pinball underlays, HUDs, and editor handles can adapt together.

## Design North Star

DACK should feel like the user has opened a familiar document or desktop, then discovered a transparent toybox layer where everything can become a game object.

The GUI succeeds when:

- Play mode feels uncluttered.
- Build mode feels powerful.
- Understand mode makes engine guesses visible.
- Manual authorship outranks automation.
- Any toolkit can apply to any screen.
- Complexity lives in shelves and layers, not in permanent windows.
