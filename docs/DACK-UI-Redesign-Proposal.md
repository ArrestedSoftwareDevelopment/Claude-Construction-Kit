# DACK UI Redesign Proposal: Unified Session Shell

- **Status:** Proposed redesign for review
- **Date:** 2026-08-04
- **Scope:** Main menu, Cockpit, family pages, cards, shelves, Inspector, modals, window ownership, play/edit transitions, and future two-monitor layout
- **Related:** [DACK GUI Architecture](DACK-GUI-Architecture.md), [DACK Top-Level Menu Plan](DACK-Top-Level-Menu-Plan.md), [DACK Sprite Studio Mini-App](DACK-Sprite-Studio-Mini-App.md), [ADR-0010](adr/ADR-0010-session-preserving-ui-navigation.md), [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md)

## Executive summary

DACK has reached the point where adding another button is no longer a harmless prototype shortcut. The engine now has multiple game families with a common player/enemy/object vocabulary, draggable cards, contextual shelves, a native-resolution document clone, OCR/Word Sense, Sprite Studio, and a future live-desktop/two-monitor path.

The redesign should make DACK feel like one coherent construction environment rather than a sequence of game-specific panels.

The central proposal is:

> DACK has one persistent session, one playfield, one editor shell, and a controlled stack of owned workspaces. The File menu owns lifecycle. Game-family pages share one page contract. Cards describe reusable things. Shelves create and place them. The Inspector edits the selected thing. Modals handle short, interrupting tasks only.

This keeps the existing game-family taxonomy but changes how the taxonomy, controls, windows, and session state are presented.

## Core decisions

### One session, several views

The durable session owns:

- source provider and source identity;
- immutable capture/reference;
- native-resolution working clone;
- Snapshot metadata and source hash;
- mutation history and undo/redo;
- active game family and preset;
- level objects, cards, rules, and bindings;
- OCR/Word Sense cache;
- selection and originating page;
- dirty state;
- save/load identity.

The interface owns:

- which surface currently has input;
- modal stack;
- active tab and expanded groups;
- panel widths and visibility;
- focus and keyboard navigation;
- single- or two-monitor arrangement;
- theme and density preferences.

Panels render and edit session state. They do not maintain private copies.

### Four visible operating conditions

The creator-facing vocabulary remains Play, Build, and Understand, but simulation and surface ownership are separate axes.

| Axis | Values | Meaning |
| --- | --- | --- |
| Simulation | Running, Frozen, Stopped | Whether gameplay advances |
| Authoring mood | Play, Build, Understand | What the creator is doing |
| Owned surface | Canvas, Cockpit, Sprite Studio, Modal | Which workspace receives input |
| Safety | Normal, Boss | Whether DACK is visible and interactive |

Recommended combinations:

- Pure play: Running + Play + Canvas
- Freeze and edit: Frozen + Build + Cockpit
- Inspect interpretation: Frozen + Understand + Cockpit
- Sprite editing: Frozen + Build + Sprite Studio
- Short confirmation or picker: any non-Boss state plus Modal
- Return to desktop: no visible DACK surface, preserving the session
- Boss Key: Boss, hiding or neutralizing all DACK surfaces and releasing input

The context strip must state these conditions plainly.

### One return stack

The return stack is:

Pure playfield -> Esc -> Cockpit

Cockpit -> open Sprite Studio -> Sprite Studio

Cockpit -> open short task -> Modal

Sprite Studio or Modal -> close/Esc -> prior Cockpit context

Cockpit -> close gadget/Esc -> prior playfield

Any ordinary surface -> Boss Key -> hidden safety state -> dismiss -> exact prior state

Rules:

- Esc closes the topmost ordinary surface; it never resets a level.
- A visible close gadget performs the same ordinary return as Esc.
- Boss is not a close gadget, save command, or quit command.
- Returning restores tab, card, selection, frame, Inspector section, scroll position, and simulation state.
- Opening the Cockpit from play freezes simulation by default.

## Outside influences

These are reference patterns, not templates to copy.

### Godot: viewport plus contextual docks

Godot presents a dominant viewport, contextual toolbars, docks, and an explicit distraction-free mode that hides docks when the view needs room. DACK should use the same hierarchy: the document/desktop clone is the viewport, while shelves and Inspector are authoring chrome. [Godot editor overview](https://docs.godotengine.org/en/stable/getting_started/introduction/first_look_at_the_editor.html)

Borrow a sacred center canvas, contextual toolbars, an explicit distraction-free state, and docks that hide without destroying state. Avoid permanent dock walls and panels that silently become gameplay.

### Blender: task workspaces made of areas

Blender's workspaces are named layouts composed of editor areas for tasks such as modeling, animation, and scripting. DACK should use a smaller version of this idea: purpose-built workspaces observing one session. [Blender Workspaces](https://docs.blender.org/manual/en/4.0/interface/window_system/workspaces.html)

Borrow task-oriented workspace names, saved layout preferences, large previews, and optional detachable areas for advanced users. Avoid exposing arbitrary layout complexity before the common layout is stable.

### VS Code: command search and profiles

VS Code makes a large command vocabulary discoverable through a searchable Command Palette and remembers coherent settings through profiles. DACK should add lightweight Command Search for actions such as “Add enemy spawn,” “Freeze simulation,” “Open Sprite Studio,” and “Save Snapshot.” [VS Code user interface](https://code.visualstudio.com/docs/editing/userinterface) and [VS Code profiles](https://code.visualstudio.com/docs/configure/profiles)

Borrow fuzzy command search, recent commands, visible shortcuts, and saved layout/theme/input profiles. Keep safety-critical commands visible.

## Unified shell anatomy

When the editor shell is visible:

| Region | Responsibility |
| --- | --- |
| Menu bar | File, Edit, View, Build, Assets, Window, Help |
| Context strip | Source, Snapshot, family/preset, mode, simulation state, dirty state, selection, close |
| Workspace tabs | Overview, Player, Actors, World, Logic, Effects, Assets, Understand |
| Center | Native-resolution playfield or focused editor preview |
| Shelf rail | Searchable, draggable cards filtered by current family |
| Inspector | Contextual properties of the selected card/object |
| Bottom tray | Status, mutation history, OCR state, messages, optional timeline |

The shell is hidden during pure Play. The playfield owns the screen and the cursor follows toolkit policy.

### File menu

File must own lifecycle rather than repeating Save/Load buttons on every family page.

- New Session
- Open Level
- Open Recent
- Save Level
- Save Level As
- Save Snapshot
- Open Snapshot
- Re-snapshot Source
- Reset Working Clone
- Compare With Source
- Export .dacklevel
- Export .dackpack
- Close Editor
- Exit DACK

These commands distinguish saving a level recipe, saving a Snapshot, exporting a package, and returning to the desktop while leaving the session alive. The original source is never overwritten.

### Edit menu

- Undo, Redo
- Cut, Copy, Paste
- Duplicate
- Delete Selected
- Select All In Current Group
- Rename Selected
- Fork Shared Card
- Apply To Selected
- Clear Selection

### View menu

- Show/Hide Cockpit
- Distraction-Free Playfield
- Show Shelf
- Show Inspector
- Show Bottom Status Tray
- Show Build Handles
- Show Invisible Objects
- Show Text/Word/Line Regions
- Show Collision
- Show Routes and Splines
- Show AI Perception
- Show HUD Avoidance Zones
- Native Resolution, Fit Viewport, Pan/Zoom
- Light, Dark, High Contrast, Terminal, Arcade Neon
- Reset Layout

### Build menu

- Play, Stop
- Freeze, Resume
- Build Mode
- Understand Mode
- Refresh Analysis
- Rebuild Environment Map
- Markers & Logic
- Add Player, Enemy, Projectile, Object, Effect
- Open Sprite Studio
- Open Asset Catalog

### Assets menu

- Player Characters
- Enemies
- Projectiles
- Explosions and Effects
- Objects and Pickups
- Sounds
- Fonts and Typography
- ANSI/ASCII Art
- Imported Sources
- Project-Created Cards
- Asset Provenance
- Import / Calibrate Sheet

### Window menu

- Cockpit
- Inspector
- Shelf
- Sprite Studio
- Command Search
- Single Monitor
- Two Monitor: Work + Clone
- Two Monitor: Editor + Preview
- Two Monitor: Overview + Detail
- Move DACK To Next Monitor
- Save Layout
- Load Layout

### Help menu

- Keyboard Shortcuts
- Game-Family Guide
- Card and Shelf Guide
- Source/Snapshot Safety
- OCR and Word Sense Status
- Asset Credits and Licenses
- Diagnostics
- Open Logs Folder
- About DACK

Boss remains a visible safety indicator and global shortcut, not an ordinary File or Window command.

### Context strip

The strip below the menu bar is an orientation/status surface, not another menu:

Source: Screenshot | Snapshot: Working Clone* | Side View / Platformer

BUILD / FROZEN | Player: Stickman 2.0 | 3 unsaved changes | [Run] [Freeze] [Close]

It shows source, Snapshot, family/preset, simulation state, selection, dirty state, Run, Freeze, close, and Boss/safety status.

## Navigation and page model

### Two levels of navigation

1. Workspace tabs identify the broad task.
2. A compact family switcher identifies the active game/control family.

Recommended workspace tabs:

- Overview
- Player
- Actors
- World
- Logic
- Effects
- Assets
- Understand

Family switcher:

- Side View
- Overhead
- Ball / Table
- Paddle / Clearing
- Grid / Text
- Route / Flow
- Ambient

The family changes the cards and shelf groups available inside the stable workspace tabs.

### Common family-page contract

Every implemented family exposes the same nine sections, with irrelevant groups collapsed:

1. Overview and transport: preset, Run, Freeze, Stop, Save, Load, Reset Clone
2. Player: player card, controls, spawn, scale, movement/physics
3. Actors: enemies, NPCs, spawns, AI, perception, damage
4. World: platforms, table, walls, routes, objects, terrain
5. Weapons and effects: projectiles, explosions, sounds, power-ups, text shrapnel
6. Markers and logic: start, checkpoint, goal, switches, triggers, protected objects
7. Text and source: text policy, OCR, word goals, erasure, background, icons
8. Scoring and rules: lives, health, win/lose, reserves, cooldowns, intensity
9. Understand and test: interpretation overlays, diagnostics, playtest notes

Moving from Platformer to Brickbat or Pinball should not require learning a new menu language.

### Family-specific shelves

#### Side View

Text floors, climb surfaces, ladders, ramps, slides, conveyors, elevators, holes, safety floor, gravity, start/checkpoint/goal, grounded enemies, flyers, projectiles, fall/death, and text destruction.

#### Overhead

Movement model, tanks/cars/flyers/spaceships/RPG actors, patrol/defend/track-player/flee/flock/horde, radar and line of sight, cover, inertia, localized gravity, projectiles, objectives, safe zones, and spawns.

#### Ball / Table

Generated table shell, ball/plunger, flippers, bumpers, gates, rails, drains, gravity, friction, elasticity, nudge/tilt, text passthrough versus bounce, inserts, jackpots, multiball, scores, and ANSI/backglass art.

#### Paddle / Clearing

Paddle and ball, letter/word/line/icon target grain, erasure/persistence, laser, power-ups, scoring, OCR ticker, target-wall skins, and destructible enemies.

#### Grid / Text

Rectangular/hex grids, maze generator, path finder, glyph legend, text/graphic/hybrid policy, roguelike, Snake, Minefield, Life, word goals, fog, tunnels, cells, walls, exits, and inventory.

#### Route / Flow

Points, polylines, Bezier curves, parabolas, racing, Frogger, escort, tower defense/offense, waves, lanes, checkpoints, protected goals, timing, inertia, towers, escorts, blockers, boosts, slows, and upgrades.

#### Ambient

Tiny workers, office wildlife, low/medium/high intensity, safe zones, desktop boundaries, ambient triggers, live-document events, gentle goals, and Boss behavior.

## Cards and shelves

### Card types

- Player Card
- Enemy/NPC Card
- Projectile Card
- Explosion/Effect Card
- Object Card
- Marker/Logic Card
- Text Rule Card
- Physics Card
- Sound Card
- Style/Theme Card
- Level Card
- World/Chapter Card
- Playset Card

### Shared card anatomy

| Card area | Contents |
| --- | --- |
| Header | thumbnail, name, role, family, status |
| Provenance | source, license/export badge, creator |
| Bindings | sprite, animation, rules, sounds, effects |
| Tags | grounded, shooter, text-aware, visible, editor-only |
| Actions | Drag, Edit, Duplicate, Fork, overflow menu |

Drag rules:

- Dropping a card on the playfield creates an instance at the drop point.
- Repeated drops create repeated instances.
- Dropping a card on a compatible slot binds it.
- Dropping on an existing object offers Apply or Fork.
- Shared cards are read-only until Fork.
- Level-local overrides are visibly marked.
- Drag previews show size, anchor, collision footprint, and editor-only status.

### Character assembly

Player/Enemy cards contain Sprite and Animation, Movement/Physics, AI or Input, Projectile, Explosion/Impact, Sounds, Text Interaction, Damage/Health, and Spawn/Goal/Marker links.

A ground shooter can be assembled by dragging a character, grounded movement, patrol/track-player behavior, projectile, explosion, sound, and text-aware rule. The composite becomes a reusable Enemy Card.

### Shelf structure

Shelf:

- Search
- Recent
- Favorites
- Current Family: Player, Enemies, World, Weapons, Logic, Effects
- All Cards
- Project-Created
- Quarantined / Reference

The shelf is a searchable, draggable catalog, not a permanent wall of buttons. Use list/grid views, recent/favorites, content-sized controls, provenance badges, and independent scrolling. Do not load or animate every thumbnail at once.

## Inspector

The Inspector remains beside every workspace tab and is owned by the current selection.

Universal sections:

1. Identity: name, type, card, instance/fork status
2. Transform: position, size, rotation, scale, anchor, layer
3. Appearance: tint, opacity, shadow, visibility, presentation mode
4. Source binding: word, letter, line, icon, window, region, manual
5. Collision and physics: shape, solid/passthrough, gravity, friction, elasticity, range
6. Movement: speed, direction, endpoints, path, inertia, acceleration, reverse
7. Behavior: AI, radar, target, defend, patrol, flock/horde, triggers
8. Damage and scoring: health, shots-to-kill, contact damage, score, lives, cooldowns
9. Weapons and effects: projectile, impact, explosion, sound, text shrapnel
10. Text interaction: ignore, support, climb, crawl, destroy, harvest, protect, seek, avoid
11. Marker/logic: start, checkpoint, goal, hidden switch, spawn, objective
12. Provenance: source, license, export status, attribution, fork history

Only relevant sections expand automatically. The creator can always open any section manually.

## Modals and floating windows

### Use a modal for

- short confirmations;
- file and level selection;
- source refresh diff;
- asset picker/search;
- Apply or Fork choices;
- import calibration;
- shortcut reference;
- diagnostics and license inspection.

### Use a workspace for

- Sprite Studio;
- full character/actor editing;
- large animation timelines;
- source analysis and Understand;
- two-monitor preview;
- long card or level editing.

Every modal has a title, visible close gadget, focus trap, Cancel and primary action, consequences for destructive actions, Esc-to-cancel, focus restoration, viewport clamping, and keyboard navigation. A modal never owns a private copy of session state.

## File, Snapshot, and level lifecycle

Open Level loads the level recipe, Snapshot reference, mutations, cards, rules, and OCR cache. Missing assets become visible unresolved-card warnings.

Save Level saves the recipe and editor state without replacing the source.

Save Snapshot freezes the selected source/clone image and analysis baseline. The creator can continue editing the level afterward.

Re-snapshot Source:

1. Capture the requested desktop/window/region.
2. Compare it with the current Snapshot.
3. Show changed geometry, background, text, icon, and window regions.
4. Let the creator apply, rebind, or discard changes.
5. Preserve level-local objects unless their source binding is invalid.

Reset Working Clone restores the Snapshot and warns that pixel mutations/deleted terrain will be lost. It does not delete the level recipe or cards.

Export Pack builds a sanitized clone package with provenance, source policy, OCR cache, level data, and credits. It never edits the original source.

## Play, freeze, edit, and desktop

### Run

Hide editor-only anchors and handles, apply the toolkit cursor policy, run the simulation, and show only minimal HUD/safety affordances.

### Freeze

Stop simulation time immediately, leave the playfield visible, show handles and selection in Build, allow edits, and preserve actor positions/effects unless Reset Run is explicitly chosen.

### Build

Show the pointer, editor-only objects, invisible logic, selected-object handles, Inspector, and shelves. Freeze simulation by default.

### Understand

Show source interpretation layers: text, words, lines, icons, background, collision, routes, AI perception, confidence, and mutations. Allow correction without requiring algorithm knowledge.

### Return to desktop

Hide DACK surfaces, release input to Windows, preserve the session, and return immediately. This is a normal visibility action, distinct from Boss.

### Boss

Hide or neutralize every DACK window, mute DACK, release input, preserve exact prior state, and restore it when dismissed.

## Keyboard and pointer contract

| Shortcut | Action |
| --- | --- |
| Esc | Close topmost ordinary surface; from pure Play, open Cockpit |
| F6 | Toggle Build and Play |
| F7 | Freeze/Resume simulation |
| Ctrl+Alt+B | Boss Key / immediate desktop safety |
| Ctrl+N | New Session |
| Ctrl+O | Open Level |
| Ctrl+S | Save Level |
| Ctrl+Shift+S | Save Level As |
| Ctrl+Z / Ctrl+Y | Undo / Redo |
| Ctrl+P | Command Search |
| Tab / Shift+Tab | Move focus |
| Arrow keys | Navigate tabs, cards, frames, and bounded values |
| Enter / Space | Activate focused control when text entry is not active |

Space remains available to a toolkit such as Platformer for jump when focus is on the playfield. F7 is deliberately used for Freeze.

Pointer policy:

- visible in Build, Understand, Cockpit, Sprite Studio, and modals;
- hidden during active Play where the toolkit requests it;
- released immediately by Return to Desktop and Boss;
- never captured by a hidden or inactive panel.

## Theme and visual unification

Shared themes:

- Office Light
- Office Dark
- High Contrast
- Arcade Neon
- Terminal

Themes define surfaces, text/muted text, focus/selection/warning/error/success, cards, modals, shelf density, and close gadgets. Families may supply accents, but cannot redefine the control grammar.

Rules:

- strong contrast in every theme;
- selection/focus/disabled/warning/error differ by more than color;
- document pixels remain native-resolution;
- UI scale follows monitor DPI without scaling the source clone;
- controls size to content by default;
- close, mode, selection, and status remain reachable;
- typography uses a readable body font and optional licensed display fonts for effects;
- asset provenance determines whether a font or graphic is runtime, local-only, reference-only, or license-pending.

## Two-monitor layout

### Work + Clone

Monitor A keeps the live document/desktop usable. Monitor B shows the DACK clone and playfield.

### Editor + Preview

Monitor A shows Cockpit, shelves, cards, Inspector, and Sprite Studio. Monitor B shows a native-resolution playfield preview.

### Overview + Detail

Monitor A shows the full playfield. Monitor B shows a focused selection, Inspector, animation preview, or local playtest.

All layouts share one session, simulation, selection, mutation history, Boss Key, input router, and explicit monitor/DPI transforms. Detachable windows are views, not duplicated state.

## Implementation boundaries

| Component | Responsibility |
| --- | --- |
| DackSession | source, Snapshot, clone, level, playset, selection, dirty state |
| InputRouter | Esc, Boss, mode keys, text-entry suppression, focus ownership |
| WorkspaceShell | menu, tabs, shelves, Inspector, child-surface ownership |
| CommandRegistry | commands, labels, shortcuts, conditions |
| WorkspaceRegistry | page descriptors and family applicability |
| SelectionService | selected card/object/source region and origin |
| CardCatalog | definitions, forks, provenance, search, recent/favorites |
| InspectorSchemaRegistry | common and family-specific properties |
| WindowLayoutService | single/two-monitor surfaces and DPI |
| SimulationController | Running/Frozen/Stopped and test transitions |
| ModalService | focus-trapped short tasks |
| HudManager | score/status placement and whitespace avoidance |

The first refactor does not need every boundary in a new assembly. It does need ownership boundaries so Main.cs stops being the source of truth for every page, button, simulation rule, and window.

## Migration sequence

### Phase 0: Shell safety

- Add explicit session, surface, simulation, and safety state.
- Centralize Esc, F6, F7, Boss, close gadgets, cursor policy, and input ownership.
- Remove playset-changing side effects from Play/Build navigation.
- Ensure closing the main editor resolves Sprite Studio and modals.

### Phase 1: File menu and transport

- Add the menu bar and File lifecycle commands.
- Move Load/Save/Snapshot/Reset/Export out of family strips.
- Add common Run, Freeze, Stop, and mode badges.
- Add one status/notification tray.

### Phase 2: Common family shell

- Build the nine-section page template.
- Bind Side View, Paddle/Clearing, Ball/Table, and Overhead.
- Collapse irrelevant groups rather than creating different UI grammars.

### Phase 3: Cards and shelves

- Replace repeated button rows with card descriptors.
- Add the universal two-level character picker.
- Support repeated drag placement, duplicate, fork, and Apply To Selected.
- Surface provenance/export state on every card.

### Phase 4: Inspector and modal manager

- Move object properties into schema-driven Inspector sections.
- Create shared modal close/Cancel/Apply behavior.
- Add source refresh diff and import calibration as proper modals.

### Phase 5: Understand and live desktop

- Make interpretation overlays a first-class workspace.
- Introduce Return to Desktop.
- Bind source/capture updates through the same session state.

### Phase 6: Two-monitor spike

- Move the editor surface to a second coordinated Godot window.
- Test focus, DPI, source monitor changes, Boss, and monitor removal.
- Keep playfield and editor on one authoritative session.

## Acceptance criteria

The redesign is successful when:

- every implemented family has the same transport, Save/Load, Player, Actors, World, Effects, Logic, Text, Rules, and Understand locations;
- common commands appear once in File/Edit/View/Build/Assets/Window;
- no family page needs a unique Save, Load, Play, or Close implementation;
- source, Snapshot, clone mutations, selected card, and current family survive navigation;
- Esc and close gadgets follow the return stack;
- F6 and F7 never change family or reset the level;
- Return to Desktop releases input without destroying the session;
- Boss hides all surfaces immediately and restores them exactly;
- a modal never becomes an accidental second editor;
- Sprite Studio returns to its caller and never remains orphaned;
- repeated same-type cards can be placed at different positions;
- Inspector never goes offscreen and irrelevant sections are collapsed;
- source pixels remain native-resolution and readable;
- themes preserve one control grammar;
- single- and two-monitor layouts show the same selection and simulation state;
- adding a card or family action is primarily a registration/data change.

## Recommendation

Adopt this as the shell-level redesign proposal. Keep the existing GUI Architecture and Top-Level Menu Plan as supporting specifications for state invariants and game-family coverage.

Do not add more permanent top-level game buttons until the common family-page shell exists. Make the File menu, session state, and Run/Freeze/Desktop transitions the next refactoring milestone.

The immediate target path is:

File -> Open Level -> Side View -> Platformer -> Build -> Freeze -> drag cards -> F6 Play -> F7 Freeze -> Esc Cockpit -> File Save

The same path must work for Brickbat, Pinball, and Overhead without changing its vocabulary or losing the creator's work.
