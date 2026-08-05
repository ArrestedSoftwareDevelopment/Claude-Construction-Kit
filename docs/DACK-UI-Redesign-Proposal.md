# DACK UI Redesign Proposal: Unified Session Shell

- **Status:** Accepted unified shell specification; implementation in progress
- **Date:** Accepted 2026-08-05; implementation baseline reviewed 2026-08-05
- **Scope:** Main menu, Cockpit, stable task workspaces, family/preset contributions, cards, shelves, Inspector, modals, window ownership, play/edit transitions, and future two-monitor layout
- **Related:** [DACK GUI Architecture](DACK-GUI-Architecture.md), [DACK Top-Level Menu Plan](DACK-Top-Level-Menu-Plan.md), [DACK Sprite Studio Mini-App](DACK-Sprite-Studio-Mini-App.md), [ADR-0010](adr/ADR-0010-session-preserving-ui-navigation.md), [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md)

## Executive summary

DACK has reached the point where adding another button is no longer a harmless prototype shortcut. The engine now has multiple game families with a common player/enemy/object vocabulary, draggable cards, contextual shelves, a native-resolution document clone, OCR/Word Sense, Sprite Studio, and a future live-desktop/two-monitor path.

The redesign should make DACK feel like one coherent construction environment rather than a sequence of game-specific panels.

The central proposal is:

> DACK has one persistent session, one playfield, one editor shell, and a controlled stack of owned workspaces. The File menu owns lifecycle. Stable task workspaces keep their locations while the chosen family/preset contributes relevant Cards, shelves, properties, rules, and overlays. The Inspector edits the selected thing. Modals handle short, interrupting tasks only.

This keeps the existing game-family taxonomy but changes how the taxonomy, controls, windows, and session state are presented.

## Core decisions

### One session, several views

The durable session owns:

- Source Descriptor/provider identity and admitted immutable Snapshot Baseline selection;
- versioned Intake Recipe and selected immutable Analysis Revision;
- Level Definition: accepted corrections, Card definitions/references, placed Instances/overrides, rules, routes, bindings, source policy, active family, and preset;
- native-resolution Working Clone, Region Runtime State, runtime mutation log, and selected Variant policy;
- transient Run State and the one simulation owner;
- creator command/undo history, kept distinct from runtime mutations;
- versioned optional OCR/Word Sense cache bound to Analysis region IDs;
- selection and originating workspace;
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

VS Code makes a large command vocabulary discoverable through a searchable Command Palette and remembers coherent settings through profiles. DACK should add lightweight Command Search for actions such as “Add enemy spawn,” “Freeze simulation,” “Open Sprite Studio,” and “Capture Snapshot.” [VS Code user interface](https://code.visualstudio.com/docs/editing/userinterface) and [VS Code profiles](https://code.visualstudio.com/docs/configure/profiles)

Borrow fuzzy command search, recent commands, visible shortcuts, and saved layout/theme/input profiles. Keep safety-critical commands visible.

### GameMaker and Unity: definition, instance, and override

GameMaker explicitly separates an Object template from the instances dragged into a Room; Unity's prefab Inspector marks instance overrides and exposes Apply/Revert. DACK should make the same distinction visible without adopting engine jargon: Card Definition, placed Instance, local Override, Reset to Card, Apply to Definition, and Fork Card. [GameMaker objects versus instances](https://manual.gamemaker.io/beta/en/Additional_Information/Objects_vs_Instances.htm), [GameMaker Room Editor](https://manual.gamemaker.io/monthly/en/The_Asset_Editors/Rooms.htm), and [Unity prefab-instance Inspector](https://docs.unity3d.com/ja/current/Manual/prefab-instance-inspector-reference.html)

### Unreal and Figma: contextual properties, bulk work, and compatible slots

Unreal's Details panel follows the selected actor, supports search/favorites, and can edit shared properties across a selection; its Property Matrix handles high-volume comparison. Figma consolidates component properties and compatible instance swaps in the right panel. DACK should use one selection-driven Inspector, a later Level Contents/property-matrix view, and Card Slots that offer only compatible replacements. [Unreal Details Panel](https://dev.epicgames.com/documentation/en-us/unreal-engine/level-editor-details-panel-in-unreal-engine), [Unreal Property Matrix](https://dev.epicgames.com/documentation/en-us/unreal-engine/property-matrix-in-unreal-engine), and [Figma component properties](https://help.figma.com/hc/en-us/articles/5579474826519-Explore-component-properties)

### Windows: keyboard parity, responsive commands, and accessibility

Windows guidance keeps primary commands in consistent locations, moves lower-priority commands into responsive overflow, and requires logical tab order, arrow navigation within groups, visible focus, access keys, and non-pointer routes to contextual commands. DACK's target user is already at a keyboard, so these are core workflow requirements rather than compliance polish. [Command bars](https://learn.microsoft.com/en-us/windows/apps/design/controls/command-bar), [keyboard interactions](https://learn.microsoft.com/en-us/windows/apps/develop/input/keyboard-interactions), [contextual commanding](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/collection-commanding), and [accessibility overview](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/accessibility-overview)

The resulting interface should feel less like a general-purpose engine than these references. Their mature conventions reduce surprise; DACK's own identity comes from the native document canvas, Understand mode, clone safety, source binding, playful Cards, and immediate F6 loop.

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
- Capture / Use Source
- Use Existing Snapshot Baseline
- Refresh Source / Re-snapshot
- Reset Working Clone
- Compare With Source
- Export Playset (.dackpack)
- Close Editor
- Exit DACK

These commands distinguish saving the canonical `.dacklevel`, admitting or selecting an immutable Snapshot Baseline, exporting a validated playset package, and returning to the desktop while leaving the session alive. The original source is never overwritten.

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

Source: Screenshot | Snapshot: snap-0007 | Clone: Modified* | Side View / Platformer

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

### Stable workspace contract and current family scaffold

The target shell exposes the same stable workspaces in every family:

1. Overview — preset summary, readiness, recommendations, recent changes, test notes
2. Player — player Card, controls, spawn, scale, movement/physics
3. Actors — enemies, NPCs, spawns, AI, perception, damage
4. World — platforms, table, walls, routes, objects, terrain, markers
5. Logic — triggers, goals, text rules, scoring, win/lose, intensity, event sheet
6. Effects — projectiles, explosions, sounds, power-ups, text shrapnel
7. Assets — catalogs, project Cards, provenance, import/calibration entry
8. Understand — source interpretation, corrections, profiles, diagnostics

The family/preset switcher changes what these stable workspaces contribute; it does not replace the workspace tabs. Save/Open/Snapshot/Reset and Run/Freeze/Stop exist once in the shared menu/transport shell.

The implemented RAD `FamilyPageShell` currently presents nine matching collapsible sections inside each active family page. This is a useful compatibility scaffold and maps directly into the stable workspaces, but it is not authority for duplicating lifecycle commands or creating a permanent family-specific navigation grammar. Moving from Platformer to Brickbat or Pinball must not require learning another shell.

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

Not every configurable scalar is a Card. Speed, opacity, radar range, gravity scale, and similar values normally remain typed properties. A Card is a reusable asset, component/ruleset, or composite with identity, provenance, compatibility, and meaningful reuse.

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
- Dropping on an existing object creates a compatible Instance override; shared reuse requires the explicit Apply to Definition or Fork Card action.
- Built-in/third-party Cards are immutable. Project-owned Card Definitions are editable through an explicit definition command; placed instances are locally editable by default.
- Level-local overrides are visibly marked.
- Drag previews show size, anchor, collision footprint, and editor-only status.

Every inherited property/slot exposes the same concise actions:

- **Reset to Card** — remove this instance's override;
- **Apply to Definition** — update a project-owned definition after showing how many instances inherit the change;
- **Fork Card** — create a new reusable definition and optionally rebind this instance;
- **Open Definition** — move sustained definition composition/animation work to Sprite Studio or the appropriate workspace.

Published packs pin or embed exact resolved Card/asset versions. Incompatible changes surface as unresolved/repairable placeholders; they never silently discard overrides.

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

The shelf is a searchable, draggable catalog, not a permanent wall of buttons. Use list/grid views, recent/favorites, content-sized controls, provenance badges, and independent scrolling. Do not load or animate every thumbnail at once. The compact two-level picker is a filtered projection of this same catalog for replacing a Slot; it does not maintain another asset list.

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

Only relevant sections expand automatically. The creator can always open any section manually. The Inspector marks inherited, overridden, unresolved, runtime-only, and creator-authored values by icon/text as well as color. Search and pinned/favorite properties arrive when the schema is stable.

The Inspector is not the only selection route. A virtualized **Level Contents** view lists Actors, World, Logic, HUD, source-bound objects, invisible/editor-only objects, and unresolved records with selection, visibility, `editorLocked`, authority, and multi-select/batch editing. Overlapping canvas objects support “select next under pointer.”

## Modals and floating windows

### Use a modal for

- short confirmations;
- file and level selection;
- destructive/reset/publish consequences;
- a short Apply-to-Card or Fork confirmation when impact must be acknowledged;
- shortcut reference;
- compact license inspection.

### Use a workspace for

- Sprite Studio;
- full character/actor editing;
- large animation timelines;
- source analysis and Understand;
- Intake Workbench, source-refresh diff/rebinding, and import calibration;
- two-monitor preview;
- long card or level editing.

Every modal has a title, visible close gadget, focus trap, Cancel and primary action, consequences for destructive actions, Esc-to-cancel, focus restoration, viewport clamping, and keyboard navigation. A modal never owns a private copy of session state.

### Use a modeless palette/flyout for

- the floating Inspector beside the selection;
- search/filter/favorite lists;
- color/palette selection;
- a short context command list;
- Activity Center and nonblocking diagnostics.

Right-click selects and opens the quick Inspector/context surface; `Shift+F10`/Menu key and the Inspector command are equivalent. The floating Inspector is movable, pinnable/dockable, and viewport-clamped while canvas selection remains usable. The target docked and floating presenters share schema, view-model, commands, and selection—not physical reparenting of one live control tree, which remains only a RAD shortcut.

## File, Snapshot, and level lifecycle

Open Level loads the Level Definition, Snapshot Baseline/selected Analysis references, Intake Recipe, accepted corrections, Cards/instances/overrides, rules/routes, selected Variant/Working Clone policy, and optional caches. Missing assets become visible unresolved-card warnings.

Save Level saves the recipe and editor state without replacing the source.

Capture Snapshot admits a new immutable native-pixel baseline from the selected source and records separate Intake/Analysis references. It does not silently absorb placed actors, current score, or transient run state. The creator can continue editing the Level afterward.

Refresh Source / Re-snapshot:

1. Capture the requested desktop/window/region.
2. Compare it with the current Snapshot.
3. Show changed geometry, background, text, icon, and window regions.
4. Let the creator apply, rebind, or discard changes.
5. Preserve level-local objects unless their source binding is invalid.

Reset Working Clone reconstructs the mutable clone from the selected Snapshot Baseline/Variant policy and warns that uncommitted pixel mutations or deleted terrain will be lost. It does not delete the Level Definition or Cards.

Export Pack builds a sanitized clone package with provenance, source policy, OCR cache, level data, and credits. It never edits the original source.

Lightweight autosave writes the validated Level recipe plus a coalesced command-recovery journal and references immutable Snapshot blobs by ID/hash. It does not rebuild/copy the full image package after every drag. On restart, recovery is offered as an explicit newer draft beside the last manual save.

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
| Ctrl+Shift+P | Command Search (`Ctrl+P` remains available for conventional Print semantics) |
| F1 | Help / shortcut and mode guidance |
| Shift+F10 / Menu key | Context commands / quick Inspector for the current selection |
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

## Accessibility and motion safety

The office-keyboard target makes accessibility and efficiency the same design problem.

- All primary scenarios—capture/open, Understand defaults/correction, Card placement/binding, property editing, F6 test, save/export, and Boss—must be completable by keyboard.
- Every Godot `Control` receives a meaningful accessible name/description/role/value through the platform-supported accessibility path. Custom canvas objects are also represented in Level Contents so a screen-reader or keyboard user is not forced to locate pixels visually.
- Drag operations have numeric/nudge alternatives. Tab order follows visual reading order; arrow keys stay within compound controls; focus is never invisible and returns to the invoker after dismissal.
- State is expressed with text/icon/shape or pattern as well as color. Editor typography scales independently of the authoritative Snapshot pixels. Office Light/Dark are not substitutes for a tested Windows contrast theme.
- **Reduced Motion** removes nonessential rotation, parallax, camera motion, animated thumbnails, and large travel while retaining immediate feedback.
- **No Flash/Strobe** disables strobe, rapid luminance/color alternation, and photosensitive effect variants. It is the default for unreviewed imported effects; death/effect strobe must be deliberately enabled by a creator and remains subject to the player safety override.
- Screen shake has a cap and Off setting. Important sound events have captions/visual equivalents; muting audio does not remove necessary state.
- Pointer hiding has an accessibility override and gameplay bindings are remappable without changing the Boss/Safety route.

Release smoke tests cover keyboard-only, Narrator and NVDA where practical, Magnifier, Windows contrast themes, 100/125/150/200% DPI, reduced motion, no flash, and audio-muted play. [Windows accessibility overview](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/accessibility-overview)

## Two-monitor layout

### Work + Clone

Monitor A keeps the live document/desktop usable. Monitor B shows the DACK clone and playfield.

### Editor + Preview

Monitor A shows Cockpit, shelves, cards, Inspector, and Sprite Studio. Monitor B shows a native-resolution playfield preview.

### Overview + Detail

Monitor A shows the full playfield. Monitor B shows a focused selection, Inspector, animation preview, or local playtest.

All layouts share one session, simulation, selection, mutation history, Boss Key, input router, and explicit monitor/DPI transforms. Detachable windows are views, not duplicated state.

Godot already supplies native `Window` surfaces, screen placement, focus/visibility signals, and ordinary fullscreen with multi-window support. That makes creation of a second window straightforward; it does not make the product behavior free. DACK must own every window's close request, surface/input scope, view model, and safe restoration. Exclusive fullscreen is not the default multi-window path. [Godot `Window`](https://docs.godotengine.org/en/stable/classes/class_window.html)

The spike passes only when it covers:

- one simulation tick and one audio/session/environment owner regardless of view count;
- negative desktop coordinates and 100/125/150/200% mixed-DPI placement;
- moving either DACK window across monitors and monitor hot-unplug/replug;
- Editor/Preview focus, text-entry suppression, Alt-Tab/taskbar behavior, cursor capture/release, and close ownership;
- hidden/minimized preview suspension so a parked second viewport does not keep rendering at full cost;
- source monitor/window close/replacement and a clear fallback to the last coherent Snapshot;
- DACK self-capture exclusion where supported, with no security guarantee implied;
- Boss neutralizing every owned window, audio, and input inside the safety budget, then restoring the exact layout/state.

Run the Godot multi-window presentation spike over an existing static Snapshot separately from the Windows capture-backend spike. Integrate only after both can be diagnosed independently.

## Implementation boundaries

| Component | Responsibility |
| --- | --- |
| DackSession | selected Source/Baseline/Recipe/Analysis/Level/Variant products, Working Clone/region/run state, family/preset, selection, dirty/recovery identity |
| InputRouter | Esc, Boss, mode keys, text-entry suppression, focus ownership |
| WorkspaceShell | menu, tabs, shelves, Inspector, child-surface ownership |
| CommandRegistry | commands, labels, shortcuts, conditions |
| CommandDispatcher / History | revision-checked commits, coalesced gestures, undo/redo, dirty/recovery events |
| WorkspaceRegistry | stable task-workspace descriptors plus family/preset contribution applicability |
| SelectionService | selected card/object/source region and origin |
| LevelContentsModel | virtualized searchable object/logic/source hierarchy, locks, visibility, multi-selection |
| CardCatalog | canonical definitions, stable Slots, forks, provenance, dependency/version resolution, unresolved placeholders, search, recent/favorites |
| InspectorSchemaRegistry | common and family-specific properties |
| WindowLayoutService | single/two-monitor surfaces and DPI |
| SimulationController | Running/Frozen/Stopped and test transitions |
| ModalService | focus-trapped short tasks |
| HudManager | score/status placement and whitespace avoidance |
| JobScheduler / ActivityCenter | bounded/cancelable work plus nonmodal progress and diagnostics |

The first refactor does not need every boundary in a new assembly. It does need ownership boundaries so Main.cs stops being the source of truth for every page, button, simulation rule, and window.

## Migration status and remaining gates

Visible behavior and architectural completion are tracked separately. A phase is not complete merely because its controls exist inside `Main`.

| Phase | August 2026 visible RAD | Remaining product gate |
| --- | --- | --- |
| **0 — Shell safety** | `DackUiState`, family-preserving F6, F7, Esc/Boss/Studio transitions, pause propagation | Extract session/input/surface commands; transition tests; atomic multi-window Boss; accessibility focus ownership |
| **1 — File and transport** | Shared command row plus File/Transport/View menus, dirty confirmation/status, Snapshot history, Desktop parking | Versioned repository/atomic save/recovery; canonical Snapshot vocabulary; command registry; no family lifecycle ownership |
| **2 — Common family scaffold** | `FamilyPageShell` with nine matching sections for Side View, Paddle, Ball/Table, Overhead | Map scaffold into stable task workspaces; remove future-placeholder prose from ordinary pages; lazy/event-driven creation; responsive qualification |
| **3 — Cards and shelves** | Shared definitions/shelves/slots, search/categories/recent/favorites/forks, repeated placement, provenance badges | Immutable vs project-owned Cards, version/dependency pins, override badges/actions, all component catalogs, persistence and cycle validation |
| **4 — Inspector and editing** | Actor/world selection, docked/floating RAD form, live AI/bindings/appearance edits, duplicate/fork, RAD save/load | Schema/view-model presenters (no control-tree reparenting), Level Contents, multi-select, command coalescing/Undo, `editorLocked`, modeless/context/keyboard parity |
| **5 — Understand/intake** | Provisional geometry-only Playfield Profile and top-three recommendations | Snapshot/Intake/Analysis separation, rectangular Workbench guide first, confidence vs compatibility, reversible Preview/Apply, uncertain-region queue, tile-backed clone |
| **6 — Two-monitor/live** | Earlier monitor probes only | Separate Godot multi-window and Windows capture spikes; then one-session integration with mixed DPI, device/monitor loss, self-capture, focus, power, and Boss tests |

The current Inspector's physical reparenting and `Try` recommendation actions are useful RAD seams, not final contracts. Docked/floating forms converge on one schema/selection/command view model. `Try` becomes a noncommitted Preview followed by explicit Apply. Source refresh review and import calibration become sustained Understand/Intake workspaces, not blocking modals.

## Acceptance criteria

The redesign is successful when:

- every implemented family contributes to the same Player, Actors, World, Logic, Effects, Assets, and Understand workspaces while global transport/Save/Load remain singular;
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
- docked/floating Inspectors share schema, commands, and selection without requiring one physical control tree;
- Level Contents can select/lock/edit overlapping and invisible objects without a precise canvas click;
- a drag or slider scrub creates one undo transaction, autosave/recovery never recopies immutable pixels unnecessarily, and runtime mutations remain a separate history;
- source pixels remain native-resolution and readable;
- themes preserve one control grammar;
- keyboard-only, accessible names/alternate object tree, high contrast, Reduced Motion, and No Flash paths pass their smoke tests;
- single- and two-monitor layouts show the same selection and simulation state;
- adding a card or family action is primarily a registration/data change.

## Ratified direction

This document is the accepted shell-level specification. [GUI Architecture](DACK-GUI-Architecture.md) owns mode/interaction/compositing behavior; [Top-Level Menu Plan](DACK-Top-Level-Menu-Plan.md) owns family/preset taxonomy only; the [Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md) owns implementation order and measurable gates.

The reference creator path remains:

```text
File -> Open/Capture -> Understand/Profile -> choose Family/Preset
     -> Build -> drag/bind Cards -> F6 Play -> F7 Freeze
     -> Esc returns to the same context -> File Save
```

That path must work for Side View/Platformer, Paddle/Brickbat, Ball/Table/Pinball, and Overhead without changing its vocabulary or losing work. Current File/Transport, F6/F7, common family sections, Cards/Shelves/Slots, floating Inspector, and provisional recommendations prove the direction; the migration table above deliberately distinguishes that proof from extracted, tested, performance-qualified product architecture.
