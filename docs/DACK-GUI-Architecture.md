# DACK GUI Architecture: Collapsible Construction Cockpit

- **Status:** Active product architecture
- **Baseline:** RAD prototype, reviewed 2026-08-05
- **Authority:** Shell state, workspace ownership, compositing layers, responsive behavior, and shared UI rules
- **Related:** [DACK UI Redesign Proposal](DACK-UI-Redesign-Proposal.md), [DACK Sprite Studio Mini-App](DACK-Sprite-Studio-Mini-App.md), [DACK Top-Level Menu Plan](DACK-Top-Level-Menu-Plan.md), [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md), and [ADR-0010](adr/ADR-0010-session-preserving-ui-navigation.md)

## Purpose

DACK is not one game with menus. It is a desktop/world transformer with multiple construction kits. The GUI therefore needs to scale from a nearly invisible play overlay to a serious editor without burying the playfield under chrome.

Top-level game-type/menu planning lives in [`DACK-Top-Level-Menu-Plan.md`](DACK-Top-Level-Menu-Plan.md). The key decision there is to organize primary game menus by view/control family—Side View, Overhead, Ball/Table, Paddle/Clearing, Grid/Text, Route/Flow, Ambient/Desktop Toybox—then place named presets underneath.

The guiding metaphor is:

**A magic transparency sheet over your desktop.**

The user is not editing Word, Excel, GIMP, Krita, TextPad, OpenOffice, or Windows itself. They are editing DACK's playable clone and its transparent gameplay layer: collision, actors, tools, triggers, semantics, effects, and rules.

## Core UX Rule

Every editable object should answer three questions:

1. **What source object is this bound to?** A word, line, icon, window edge, cell, path, color region, or manually placed point.
2. **What gameplay object did it become?** Platform, bumper, pellet, ladder, trigger, checkpoint, enemy, tunnel, power word, target, obstacle, or invisible logic.
3. **Can I detach/edit/toggle it?** Keep as text, convert to graphic, show hybrid, move it, stretch it, bind it elsewhere, or delete only the DACK object.

Detection proposes. The editor disposes.

## Current Baseline

The RAD already proves the basic product shape: fullscreen play, an Esc-toggleable Cockpit, transitional contextual game-family pages, Player/Enemy/Projectile/Object shelves, a persistent Inspector, draggable cards and objects, editor/play separation, a large Sprite Studio proof, and a two-monitor probe. Their working behaviors are the baseline to preserve; the target navigation maps those family pages into stable task workspaces as the shell is made more legible and efficient.

The remaining UI debt is structural rather than conceptual:

- some pages can still overflow or place controls beyond the usable monitor area;
- the root controller still constructs and refreshes too much UI imperatively;
- mode, window, selection, cursor, and input ownership need one authoritative session model;
- repeated button/panel construction needs shared descriptors and components;
- tabs, shelves, Inspector sections, and long animation lists need predictable independent scrolling;
- low-contrast labels and controls must be treated as defects;
- transitions must preserve the source, working clone, mutations, active playset, selection, and editor location.

The implementation and optimization sequence is owned by the [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md). This document defines how the resulting UI must behave.

### Launch surface

At cold launch, DACK intentionally presents almost nothing: the desktop remains underneath, while a transparent, gently floating DACK logo sits above it with calm choices for **Try a Bundled Demo**, **Open Recent**, or **Capture / Use a Source**. It captures nothing merely because it launched. A concise hint reads `Esc — Open DACK   |   Ctrl+Alt+B — Boss / Hide Immediately`. The toolbar, HUD, Cockpit, and ordinary editor chrome remain hidden. This establishes DACK as a secondary layer over the user's desktop rather than another dominant application window.

`Ctrl+Alt+B` is reserved for the immediate safety path. `Esc` or an explicit launch action reveals the ordinary DACK workspace; `F1` opens Help. Onboarding names `CLONE ONLY`, explains that visible pixels may still be private, and asks the creator to test Boss once. The logo is presentation-only: it never captures or mutates source content, and it disappears atomically when Boss mode takes over.

## State and Ownership Invariants

These rules are normative. No toolkit or editor page may invent a competing version of them.

1. **One session owns durable working state.** The active source/Snapshot, working clone, mutation history, active view family and preset, level, selection, and editor navigation state belong to one DACK session. Panels render that state and issue commands; they do not own private copies.
2. **Play, Build, and Understand are mutually exclusive primary modes.** Build and Understand may pause or preview the simulation, but they do not run as hidden second modes underneath Play.
3. **Esc navigates workspace ownership; it never changes content.** Esc dismisses the topmost transient editor, then closes Sprite Studio to its owning Cockpit location, then closes the Cockpit, or opens the Cockpit when pure Play owns the screen. Esc never resets, re-snapshots, changes playset, discards mutations, or silently starts a new game.
4. **Closing restores the prior authority.** If the Cockpit was opened from Play, closing it resumes Play. If it was opened during a Build or Understand session, closing a sub-workspace returns to that session and does not auto-play.
5. **Boss is a separate global safety path.** The Boss Key immediately hides or neutralizes every DACK window, releases input, and mutes DACK without waiting for save, OCR, import, or layout work. It does not reset or discard the session. Returning restores the same source, clone, mutations, playset, mode, and selection.
6. **Sprite Studio is an owned workspace.** Opening Studio hides the ordinary Cockpit surface and transfers editor focus to the full-screen Studio workspace. Studio's close gadget or Esc returns to the exact calling tab/card/selection. Closing the main editor also closes or safely returns from Studio; no orphan editor window remains.
7. **Cursor policy follows the owner.** Build, Understand, Cockpit, and Sprite Studio show the pointer. Active Play uses the toolkit's pointer policy and hides it by default. Boss always releases the pointer to Windows. Text entry suppresses gameplay bindings while keeping the Boss Key available.
8. **Mode and playset changes preserve work.** Switching modes, Cockpit tabs, view families, presets, monitors, or Studio does not replace the source or clear the mutated clone. Only explicit commands such as Reset Working Clone, Refresh Source/Re-snapshot, Load Level, or New Source may do that, with an appropriate dirty-work confirmation.
9. **One surface owns input at a time.** Safety/Boss outranks Studio; Studio outranks Cockpit; transient dialogs outrank their owner; pure Play owns input only when no editor surface is active. Pointer and keyboard events must not leak through an active editor into the simulation.
10. **Selection survives context changes when valid.** Returning from test play, Understand, Studio, or another tab restores the prior selection, active tab, Inspector section, and scroll position. If the selected object no longer exists, the UI says so and falls back predictably.
11. **Play/Build has a dedicated one-key toggle.** The default `F6` binding switches Build ↔ Play and back without opening a page, changing the playset, resetting the source, or discarding mutations. It is configurable, suppressed while text is being edited, and must restore the previous tab/selection/Inspector state on return.

## Three Working Modes

### 1. Play

The playfield is sacred.

- Fullscreen real estate.
- No fake window.
- Cursor hidden during active play.
- Minimal HUD placed in whitespace.
- HUD fades or slides away when a ball, player, projectile, or selection handle approaches.
- Esc toggles the normal DACK cockpit.
- `F6` toggles Build/Play as the fast test loop; the visible mode badge changes immediately and the transition must not wait for OCR/import work.
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

The cockpit should appear with Esc and collapse back to pure play with Esc or a single "Test/Play" action. Test/Play preserves the current selection, active tab, scroll positions, source, and mutation state for the return trip.

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

## Layer Ownership and Compositing Order

Logical data layers and visual draw layers must be explicit. ADR-0010's ten roots remain the normative ownership/render enum. The more detailed order below names sublayers inside those roots—for example ANSI/Working Clone inside Source Clone and underlays inside World/Mutable Terrain. A toolkit can contribute content to a layer, but it cannot silently reorder or take ownership of the shell.

From back to front:

1. **Immutable source:** original capture/import reference, never edited.
2. **ANSI/table base:** optional full-color ANSI-rendered image or generated board skin, rasterized from terminal cells at its authored/native aspect and used as a visual base.
3. **Working clone:** the native-resolution playable image plus clone-only pixel mutations. In Pinball Board Skin mode, the environment background mask may blend or yield to the ANSI base while source text/ink remains native and legible.
4. **Environment interpretation:** text, word, line, icon, background, collision, and semantic records. Normally not drawn; Understand visualizes them.
5. **Underlays and grounded decoration:** optional paper shadows, trails, and non-interactive atmosphere.
6. **World objects:** platforms, ladders, conveyors, flippers, bumpers, triggers, pickups, and other placed construction parts.
7. **Transient effects:** explosions, letter shrapnel, particles, ribbons, and score bursts.
8. **Gameplay-critical actors:** players, enemies, balls, projectiles, objectives, and critical indicators remain legible above spectacle.
9. **HUD:** score, lives, word ticker, missions, and status; it uses whitespace and approach fading rather than becoming collision.
10. **Build overlays:** selection, handles, invisible objects, guides, paths, and drag previews.
11. **Understand overlays:** engine interpretation, confidence, authority, collision, routes, mutations, and diagnostics.
12. **Cockpit or Sprite Studio:** the active editor workspace that owns interaction.
13. **Safety:** Boss/clone-only/privacy state, always topmost and independent.

The renderer may batch adjacent layers for speed, but their ownership and visible ordering must remain equivalent. Effects quality may degrade under load; gameplay-critical actors, input feedback, collision, and safety UI may not.

### ANSI base / playfield composition policy

The ANSI base is a presentation layer, not a collision layer. A toolkit chooses one source-background policy:

- **Opaque source:** show the ANSI base only in margins or explicitly transparent source regions.
- **Blend:** retain the source image while mixing the ANSI base beneath it at a controlled opacity.
- **Background mask:** use the cached `EnvironmentMap` background zones to let the ANSI image show through while preserving native text, icons, and promoted gameplay regions. This is the default for Pinball Board Skin mode.

All three policies preserve native source coordinates and avoid scaling the document to fit the ANSI canvas. If background confidence is too low, DACK falls back to Blend and warns the creator instead of guessing a destructive mask.

## The Cockpit Layout

### Tabbed Switchboard

The target Cockpit uses stable task workspaces plus an orthogonal family/preset switcher instead of showing every construction column or turning each family into another app.

Intent:

- Stable workspace tabs are **Overview, Player, Actors, World, Logic, Effects, Assets, Understand**. One displays its page; inactive pages reduce to compact tab names.
- The family/preset switcher independently selects Side View/Platformer, Paddle/Brickbat, Ball/Pinball, Overhead, and later Grid/Text, Route/Flow, or Ambient.
- A family contributes shelf groups, property schemas, overlays, rules, diagnostics, and HUD declarations inside stable workspaces; it does not create a second tab grammar.
- Inspector is the exception: it stays docked beside the active tab so selected-object details remain visible while the creator changes toolsets.
- Switching the active family/preset filters the workspace without resetting the source, clone, mutations, or placed objects.
- File/Open/Save/Snapshot/Reset and Run/Freeze/Stop live once in the shared menu/transport shell; no family page owns another lifecycle implementation.
- Tabs, groups, cards, and actions should come from descriptors/registries so labels, capitalization, visibility, tooltips, shortcuts, and enabled states remain consistent.
- Character and sprite selection uses one compact two-level picker everywhere: **top-level role/family pull-down → individual asset pull-down**. Player, Enemy, Spawn, Builder, Projectile, and effect pages reuse the same picker instead of repeating large sprite shelves.
- Inapplicable controls collapse or disappear with a discoverable explanation; they do not occupy permanent dead columns.
- Each tab remembers selection, expanded groups, and scroll position.
- Long tab bodies, shelves, Inspectors, animation lists, and logs scroll inside their own bounds. The whole editor must not grow past the usable viewport.

The implemented `FamilyPageShell` nine-section family pages are a useful RAD scaffold and map into these workspaces. They are not authority to preserve duplicate lifecycle controls or a permanent family-first top level. This preserves the "big cockpit" feeling while keeping screen real estate under control: the user flips task pages in one instrument panel while the family changes its parts.

### Responsive, Readability, and Keyboard Rules

- Fit full-screen workspaces to the active monitor's usable rectangle and DPI. Never assume 1920×1080, one monitor, or one scale factor.
- Keep the title/mode/selection/close row reachable. Clamp popovers, menus, and the Inspector inside the viewport.
- Wide layout: shelf, playfield/preview, and Inspector may sit side by side. Medium layout: keep the active page plus Inspector drawer/tab. Narrow layout: show one content pane at a time with persistent workspace tabs and a clear Inspector toggle.
- Preserve document pixels at native resolution. Use unused screen space for tools; scroll or pan the editor rather than scaling the source clone into illegibility.
- Buttons size to content unless explicitly designated as a primary full-width action. Hit targets remain comfortably clickable even when labels are short.
- Body text and control labels must meet strong light/dark contrast; faint gray-on-gray labels are defects. Selected, focused, disabled, warning, and error states must differ by more than color alone.
- `Tab`/`Shift+Tab` traverse controls in reading order; arrow keys change tabs, lists, frame selections, and numeric steps where expected; `Enter`/`Space` activate the focused control.
- Every mouse action that changes durable state needs a keyboard path. Visible focus rings are mandatory, and focus returns to the invoking control when a transient surface closes.
- Right-click context/quick Inspector commands are reachable with `Shift+F10`/Menu key and the persistent Inspector command; a floating Inspector is modeless, draggable, pinnable/dockable, and viewport-clamped.
- Tooltips supplement concise labels; they do not carry essential instructions that keyboard users cannot reach.
- The two-level picker keeps the selected card's thumbnail, name, source/provenance badge, and a small preview beside the pull-downs; search, recent, and favorites are available inside the second list.
- A virtualized Level Contents view exposes overlapping, invisible, editor-only, source-bound, and unresolved objects through a searchable keyboard/screen-reader-friendly hierarchy with visibility, `editorLocked`, authority, and multi-select.
- Reduced Motion, No Flash/Strobe, screen-shake Off, pointer-hide override, scalable editor text, sound captions/visual equivalents, and color-independent state are shared accessibility policies, not toolkit options.

### UI Efficiency Rules

- Update controls from session events/signals. Do not rebuild panels, refit full-screen pages, or rewrite unchanged labels every frame.
- Create expensive workspace contribution panels and catalogs lazily, then reuse them. Hidden workspaces/groups pause previews, thumbnail animation, diagnostics, and polling.
- Virtualize or page very large asset/card lists and cache thumbnails, text measurements, imported previews, and Inspector schemas.
- Refit layout only on workspace open, resize, monitor/DPI change, theme change, or meaningful content change.
- OCR, sprite compilation, source analysis, save, and thumbnail generation must not block input. Their status appears asynchronously in the owning page and stale results are discarded by session/source identity.
- One nonmodal Activity Center exposes capture, analysis, OCR, import, save, thumbnail, and compilation work with identity, stage, outcome, diagnostics, and Cancel. Boss/Desktop parks nonessential work.
- Direct manipulation previews at input rate but commits one command per drag/resize/rotation/slider gesture; runtime mutation history is separate from creator Undo/Redo and lightweight recovery.
- Under load, reduce decorative preview rate, effects, glow, shadows, and distant animation before reducing pointer feedback, input, collision, or safety responsiveness.
- Instrument page-open time, idle Cockpit cost, layout passes, active preview count, and list/card counts so efficiency work follows evidence.

### Kenney UI theme seed

The CC0 Kenney UI Pack is an approved seed for DACK's creator-facing chrome. The complete local bundle copy and its CC0 record have been audited; use that canonical copy rather than importing the duplicate standalone pack. Admission still means a curated subset through one shared Godot theme, not hundreds of per-button texture assignments. Exact shipping status remains governed by `dack/assets/ASSET_PROVENANCE.md`.

- Use neutral Grey for ordinary controls, Blue for selection/active state, and Green for Apply/Play/success. The local Red family contains only two arrow sprites, so destructive or stop/cancel actions keep DACK's high-contrast red style token instead of pretending the pack supplies a complete red control state.
- Prefer content-sized rectangular controls, compact square icon buttons, checkboxes, arrows, slider parts, and close gadgets. The pack does not override the rule that buttons should not stretch across the screen by default.
- Pair Default and Double assets as 1x/2x DPI variants. Use explicit texture/content margins or `StyleBoxTexture` nine-slicing so labels can grow without distorting corners.
- Retain DACK's high-contrast text tokens and focus rings. A colorful skin cannot make disabled, selected, focused, or keyboard states ambiguous.
- Keep the normal system-readable font for body labels and forms. `Kenney Future` may be an optional arcade/display heading; it is not the default small-text font.
- Theme sprites belong to editor chrome and HUD cards. They do not become document geometry, playfield art, collision, or captured-source content unless a creator deliberately places an exported UI card as an object.
- Admit only the reviewed subset used by the theme and load it once. Hidden pages should not create their own texture copies.

### Top Strip

The top strip should be compact and persistent only while the cockpit is open.

Suggested controls:

- Source: Desktop / Monitor / Window / Region / Image / Text Grid.
- Mode: Play / Build / Understand.
- View Family: Side View / Overhead / Ball-Table / Paddle-Clearing / Grid-Text / Route-Flow / Ambient.
- Preset: changes by family, e.g. Platformer, Brickbat, Pinball, Combat/Tanks, RPG, Snake/Maze, Racing, Tower Defense.
- Clone: Reset / Save Variant / Compare Source.
- Source refresh: explicit `Refresh Source` action, candidate diff, Apply/Rebind/Discard; never an automatic capture while editing or playing.
- Word Sense: Off / Lazy Local / Full Page Prep, plus status.
- Safety: Boss Key hint and clone-only indicator.
- Close gadget: visible `×` to hide the ordinary Cockpit; separate from Boss Key.

Button sizing rule:

- Buttons should not stretch to fill the available width by default.
- Buttons should size to their label/content and sit in compact rows.
- Cards, shelves, panels, text fields, previews, and sliders may expand when their job benefits from it.
- A full-width button should be an explicit exception for a large primary action, not the normal control style.

### Left Shelf

The shelf is where construction-kit identity lives. The shell stays consistent; the shelf changes by toolkit.

Repeated placement is intentional: every shelf click or card drop creates a new instance with its own identity. The same enemy, ladder, bumper, pickup, or projectile may appear many times. Initial editor placement uses a randomized, overlap-avoiding candidate within the native playfield, then remains fully draggable and saveable; no asset button is consumed after the first placement.

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

Only the selected task workspace and relevant family-contributed groups should be expanded by default. Other groups fold away to keep the playfield and Inspector usable. The Inspector and Understand workspace remain broadly available because selected-object attributes, source bindings, invisible logic, and detection layers cut across every game type.

This prevents feature growth from turning into a pile of unrelated windows.

## Sprite Studio vs. Live Sprite Pad

The live sprite sidebar remains the quick in-context pad: select an actor, tweak pixels or frame labels, see the playfield update. It should stay small, hidable, and toy-like.

Serious actor setup belongs to the owned full-screen **Sprite Studio** workspace. Its character-composition hub collects imported frame source, animation labels, timing, origins/baselines, attachment points, AI/rule Cards, projectile Slots, sounds, effects, and text-interaction options. `Idle` and `Climb` are core labels alongside run, jump, shoot, hurt, and death.

This prevents the sidebar from becoming a cramped Aseprite clone and gives creators a natural place to tune enemies, players, guards, climbers, flyers, projectiles, and future RPG/overhead actors.

Composition should think in Cards. Small Cards are ingredients; composed Cards are finished reusable objects. A creator assembles sprite, animation, behavior, projectile, sound, effect, and text-rule Cards into a richer Enemy/Player/Spawner/Object Card through Sprite Studio or the explicit definition editor, then drags that finished Card into levels or other compatible Slots.

Player retains its own stable task workspace. It owns protagonist selection, player-Card placement/swapping, control/movement defaults, gun/no-gun, size/text ratio, and player-specific animation hooks. Ordinary selected-instance wiring stays in the Inspector; reusable actor composition opens the explicit definition/Sprite Studio workspace rather than a competing permanent Builder tab.

Placed toolkit objects should follow the same principle: the playfield gives direct manipulation with A/B handles, while the Inspector gives precise nudges. Ramps, slides, conveyors, elevators, pinball parts, gates, and future line objects should be rotatable; line tools rotate by their endpoints and by Inspector rotate nudges. Ladders are the exception: they should remain vertical climb volumes, with width tuned against the player character rather than treated as angled ropes.

Document-native geometry adds two compact tools to the same workspace:

- **Transform Block:** select a paragraph, heading, line band, table row, or word group; rotate/slant the DACK clone; choose text-preserving, hybrid, or raster display; choose glyph-mask, oriented-block, baseline, climb-surface, or visual-only collision; and decide whether attached ladders, goals, routes, and spawn points inherit the transform or stay in world space.
- **Page Navigator:** browse a multi-page Word/Writer/PDF/browser source as ordered Level Cards, open a page for editing, set the transition target, and distinguish page-local mutations from sequence-global progression. Thumbnails and page status stay in their own scroll region so they cannot push the Inspector offscreen.

Understand mode exposes local axes, transformed bounds, source/page IDs, OCR confidence, and collision masks. Play mode hides transform handles, spawn markers, and invisible anchors while retaining their behavior. Page transitions preserve the active session, selection policy, and chosen persistence rules; they do not resample the source clone.

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

## Cross-Cutting Editor Tools and Visual Services

### Cross-Cutting Editor Tools

DACK should grow a visible motion/path editor family:

- **Parabola editor first:** cheap and broadly useful. Define start/end, apex/height, gravity/flight time, preview ghost positions, and bind the result to jumps, thrown objects, arcing shots, enemy hops, bounce/stomp rebounds, pickups, and power-up travel.
- **Bezier/spline editor next:** richer handles for patrol paths, flying enemies, swinging vines/ropes, racing curves, pinball ramps/wireforms, camera moves, particle ribbons, and authored enemy entrances.

The parabola editor should be near-term because it is simple, legible, and directly connected to the platformer/projectile work already underway. Beziers should wait until the UI can support curve handles cleanly.

### Shared shadow rendering

Every visible gameplay object should be able to cast a cheap composited shadow onto the cloned document/page. The goal is not physical realism; it is grounding. Objects should feel like they sit on the paper, desktop, or app surface rather than floating as ordinary UI.

First rule:

- Sprites draw a projected duplicate of their current frame before the real frame.
- The duplicate uses the exact current frame, facing, horizontal flip, origin, and scale before it is squashed vertically, slightly rotated/offset, tinted grayscale/black, and made semi-transparent. Editor and runtime must call the same shadow transform; a Dragon shadow that reads backwards in Studio is a correctness bug, not an art preference.
- This creates a single-function "paper shadow" for players, enemies, animated targets, imported characters, and later pickups/projectiles.

Second rule:

- Vector/toolkit objects use the same concept with shape-specific helpers: line shadows for ramps/conveyors/flippers, ellipse shadows for bumpers/balls, soft rect shadows for panels and icons.
- Default lighting should cast a modest page shadow back/left (screen-space) with a relative offset, while an optional `behind-facing` mode derives the cast direction from the actor's facing. A Renderer/Theme service should expose shadow parameters: `shadowEnabled`, `shadowOpacity`, `shadowOffset`, `shadowSquash`, `shadowRotation`, `shadowSpace`, `shadowBlurStyle`, and `shadowFollowsTheme`.
- Optional spectacle tier: a scene may define one shared `LightSource` with position, radius, height, color, and intensity. The renderer can derive each object's shadow direction, length, skew, and softness from that source. This is deliberately not the default: it is more expensive, must fall back to the cheap paper shadow under load, and should be reserved for showcase scenes such as Pinball or a dramatic boss encounter.
- Dark mode and Boss Key can reduce or disable shadows if they harm legibility or office-safe presentation.

### Implementation sequencing

This architecture no longer carries a competing phase list. Extraction of the session, input router, UI shell, selection, HUD, environment layers, toolkit descriptors, and performance work is sequenced and measured in the [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md).

UI work is considered complete only when it preserves the invariants above, keeps every control reachable, avoids idle per-frame reconstruction, restores context across Play/Build/Understand/Studio transitions, and passes the shell smoke tests defined by that plan.

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
