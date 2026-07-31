# DACK Optimization and Refactoring Plan

**Status:** Active engineering plan  
**Baseline:** RAD prototype, July 2026  
**Applies to:** `dack/` Godot 4.7.1 .NET prototype and the transition to the product editor/runtime

## 1. Why This Plan Exists

DACK has passed the “is the idea real?” stage. The RAD now demonstrates captured text as terrain and targets, shared clone deformation, Platformer/Brickbat/Pinball/Overhead play families, draggable construction objects, actor/enemy import, animation labeling, OCR-assisted word effects, combat, sound, save/load, cards, shelves, and the beginnings of the Sprite Studio.

That success changes the engineering problem. Adding every new idea directly to the current controller will make the prototype slower to change, harder to diagnose, and increasingly likely to regress. The next milestone is not the largest possible feature count. It is a stable construction-kit spine that lets new game families arrive cheaply.

The plan therefore runs on two tracks:

1. **Shore up the product surface:** make the current editor readable, bounded, predictable, and easy to return to from play.
2. **Extract shared engine services:** preserve the proven behavior while moving capture/text analysis, environment queries, actors, toolkits, assets, persistence, effects, and UI out of monolithic classes.

Refactoring is authorized when it protects a proven loop or removes a measured bottleneck. It is not an excuse for a ground-up rewrite.

## 2. Current Prototype Baseline

### Proven

- Native-resolution captured-page rendering without scaling the document.
- Text detection at letter, word, and line/platform granularity.
- Clone-only erasure, background-color replacement, and persistent cross-playset deformation.
- Platformer movement, gravity, ladders, crawl surfaces, ramps, slides, conveyors, elevators, start/goal markers, enemies, radar, projectiles, damage, and score.
- Brickbat letter/word targets, three-ball reserve rules, literary word ticker, multiball, laser, effects, sound, and draggable HUD.
- First pinball ball/flipper/plunger/text-clearing loop.
- Overhead movement and a broad actor library seed.
- Draggable/scalable actors and draggable endpoint-based world objects.
- Contextual Cockpit tabs, Player/Enemy/Projectile/Object pages, Inspector, and editor/play separation.
- Live-linked sprite pad, large Sprite Studio proof, editable animation labels/sequences, save/load manifests, strobe, ping-pong, and per-source animation defaults.
- Source-specific sprite import experiments: fixed grid, fixed rectangles, component extraction, blob detection, and individual-file sequences.
- Lazy, local OCR behind a service boundary, with gameplay-prioritized requests and geometry-only fallbacks.
- JSON level save/load for the current RAD state.

### Still Prototype-Grade

- UI construction, application state, simulation, persistence, and toolkit rules are concentrated in `Main.cs`.
- Rendering, mutable image work, text-region queries, world-object editing, collision helpers, and environment interpretation are concentrated in `PlayfieldSurface.cs`.
- Asset catalog, source-specific imports, image cleanup, frame detection, frame assembly, and curated actor factories are concentrated in `SpriteAnimationSet.cs`.
- Many panels are built imperatively in code and refreshed by polling, making layout defects and stale controls more likely.
- The current `SetEditorMode(false)` path forces Platformer, so ordinary Play/Cockpit navigation from Brickbat, Pinball, or Overhead can change the active playset. This is a P0 correctness defect and the clearest evidence that shell state must be explicit.
- Text-region queries repeatedly map, allocate, and sometimes inspect pixels instead of querying a stable indexed model.
- The captured-page importer performs several independent whole-image passes for related detections.
- Image mutations update the full texture even when only a small dirty rectangle changed.
- OCR work has a useful queue, but no session cancellation, bounded lifetime, persisted cache, or embedded provider yet.
- Level persistence is a RAD manifest nested inside `Main.cs`, not yet the versioned Snapshot/package contract.
- There is no automated test project or repeatable performance scene suite.

These limitations are normal for a successful RAD. They are now explicit exit criteria rather than invisible debt.

## 3. Engineering Rules

1. **Preserve the playable loop during extraction.** Each refactor step ends with the project building and the Platformer, Brickbat, Pinball, and editor/play smoke paths still usable.
2. **Measure before and after.** A suspected hot path earns priority through frame timing, query counts, allocation counts, or a repeatable hitch—not file size alone.
3. **No full-image work in the frame loop.** Capture analysis, OCR preparation, color maps, and connected-component work run on import/background jobs or explicit mutation regions.
4. **Derived data is cached and versioned.** Text boxes, local backgrounds, spatial buckets, OCR labels, and import manifests belong to the Snapshot/asset cache and invalidate by source hash plus algorithm version.
5. **Stable IDs replace position-as-identity.** Text objects, actors, cards, placed objects, assets, and mutations need stable identifiers so save/load and incremental updates do not depend on list order.
6. **One authoritative session state.** UI controls render state and issue commands; they do not each own competing copies of gameplay configuration.
7. **Toolkits declare differences.** A toolkit provides verbs, shelf entries, rules, preflight mutations, HUD widgets, and win/lose logic through a common contract.
8. **The immutable source remains separate.** Source pixels, working clone pixels, collision state, semantic labels, and runtime mutations are distinct layers.
9. **Degrade spectacle before gameplay.** Under load, reduce particles, shadows, glow, animation rate, OCR urgency, and distant AI ticks before reducing input/collision fidelity.
10. **Optimize for office PCs.** The baseline is an ordinary keyboard/mouse Windows machine at native document resolution, not a controller-first gaming rig.

## 4. Provisional Performance Budgets

These are starting budgets to make “fast enough” testable. Profile data may revise them, but changes should be recorded.

| Area | Target | Floor / guardrail |
| --- | --- | --- |
| Active play | 60 FPS at 1920×1080 on the agreed baseline PC | Never remain below 30 FPS during an ordinary level |
| Main-thread frame time | 16.7 ms budget | Warn at 22 ms sustained; capture a trace at 33 ms |
| Input-to-visible-response | under 50 ms for editor handles and controls | No blocking OCR/import work on input |
| Play/Edit/Cockpit transition | visibly immediate; under 150 ms preferred | Must not reset or copy the full source unnecessarily |
| Boss Key response | under 100 ms to hide/neutralize and mute | Safety path is never delayed by save/import/OCR |
| Document mutation | under one frame for small letter/word hits | Larger blasts may queue dirty regions, never rescan the whole page |
| OCR | background only, bounded concurrency | Zero frame-loop waits; cancellation on source/session change |
| Save | atomic and recoverable | Never partially overwrite the last good level |

The benchmark matrix should include:

- a clean 1080p office screenshot;
- a dense document with at least 2,500 detected text objects;
- a heavily deformed clone;
- 1, 10, and 50 active actors;
- one and three Brickbat balls with maximum allowed effects;
- mixed ladders/conveyors/elevators/enemies/projectiles;
- the largest admitted sprite sheet and a deliberately awkward importer test sheet;
- a two-monitor layout with different DPI scales when Live Desktop work begins.

## 5. Instrumentation Before Optimization

Add a small developer-only performance overlay and an exportable diagnostic snapshot. It should report:

- FPS, average/p95 frame time, physics/update time, draw time, and UI time;
- active actors, projectiles, effects, balls, and placed objects;
- environmental queries per frame and candidate counts per query;
- text objects by granularity and active/deleted counts;
- dirty image rectangles and uploaded texture pixels;
- OCR queued/active/completed/failed counts;
- asset cache hits/misses and import duration;
- managed allocations or coarse garbage-collection counts where practical;
- current source resolution, monitor, DPI scale, and playfield transform.

Create deterministic test seeds for random effects, Brickbat launch angles, enemy timers, and procedural imports. A bug report should be able to include the seed, level, source hash, and performance snapshot.

## 6. Highest-Value Optimization Work

The July 2026 static audit found concrete multipliers:

- asking for active letter/word regions rebuilds mapped arrays and tests current pixels inside every candidate region;
- Platformer repeats broad surface/text queries for the player, each grounded enemy, and each projectile/OCR prediction;
- Brickbat chooses nearby OCR words every rendered frame;
- Pinball may perform six 120 Hz ball substeps per rendered frame and can enumerate all active letters during each text-plow step;
- a multi-letter hit can run local-background/flood/speck work and upload the full 1679×1012 texture several times in one event;
- local-background estimation currently allocates color buckets during pixel-level erasure;
- placed-object helpers repeatedly allocate arrays/lists by kind;
- actor animation clocks/redraws and some HUD/Cockpit text/layout work have more than one owner.

These are algorithm/data-flow problems. Native C++ or a new physics engine would not fix them.

### 6.1 Stop rebuilding UI state every frame

Current polling keeps the Cockpit fitted and refreshes status/HUD text in the main process loop. Move to event-driven updates:

- refresh labels when their underlying values change;
- refit layouts on resize, monitor change, or panel open;
- update countdowns at a modest UI tick rate when they truly need animation;
- keep gameplay animation in the frame loop, not editor form reconstruction.

**Acceptance:** opening a complex Cockpit page does not add meaningful idle frame cost, and controls never drift offscreen after resize.

### 6.2 Build one document-analysis product

The current importer derives platforms, letters/bricks, words, lines, and anchors through separate scans. Replace this with a staged analysis pipeline:

1. generate luminance, local-background, ink, and connected-component maps once;
2. group components into glyphs, words, lines, regions, icons/pillboxes, and background zones;
3. assign stable IDs and confidence/authority;
4. build spatial buckets;
5. cache the result in the Snapshot.

Toolkit-specific shapes should be views over that shared analysis, not independent detections.

**Acceptance:** importing one source performs one primary pixel-analysis pass; switching playsets does not repeat it.

### 6.3 Add a spatial environment index

Actors, balls, projectiles, OCR prioritizers, and HUD placement currently iterate broad lists. Add a uniform grid or quadtree-like index over:

- active text objects;
- placed solid/motion objects;
- actors/projectiles;
- triggers and invisible logic;
- HUD avoidance regions.

Queries should ask for nearby candidates by bounds/ray, then apply exact tests.

**Acceptance:** collision/document queries allocate no managed memory after warm-up, inspect a small local candidate set (target at most 32 for ordinary probes), and scale with nearby content rather than total page content.

### 6.4 Make mutation incremental

Maintain an active/deleted state per derived text object and a dirty-rectangle queue for pixel changes. Erasure should:

- sample/carry the background model already associated with the region;
- mutate only the affected object/padded blast region;
- mark intersecting semantic/collision entries dirty or inactive;
- upload the changed texture region when Godot permits, or coalesce several small changes into one bounded update;
- append a reversible mutation record.

**Acceptance:** deleting one letter does not scan every word or upload/rebuild unrelated page state; any explosion/laser/Brickbat cluster/pinball plow commits at most one texture update per simulation frame; Reset restores pixels and active-region state exactly.

### 6.5 Compile sprite imports once

Runtime spawning should load a curated manifest, not rediscover how a sheet is sliced. The importer may use fixed grids, explicit rectangles, component seeds, or a draft detector, but it should write a reviewable compiled result:

- source hash and importer-profile version;
- frame rectangles/origins/display boxes;
- transparency/recolor policy;
- action labels and ordered frame sequences;
- warnings, excluded components, and preview contact sheet;
- provenance/export state.

**Acceptance:** spawning another copy of an admitted actor performs no blob scan, no per-pixel recolor, and no per-frame texture creation.

### 6.6 Bound asynchronous work

OCR, live capture analysis, asset import, and future source diffs need:

- cancellation tokens tied to the active source/session;
- bounded queues with priority and deduplication;
- an explicit OCR pending cap (initial target: 32) and predictor cadence around 4–10 Hz rather than every rendered frame;
- failure backoff;
- thread-safe result publication on the main thread;
- persisted results keyed by source/region hash;
- explicit low-power/paused modes.

**Acceptance:** switching/resetting a source cancels pending work and releases samples promptly (target within 250 ms), no region is cropped more than once per attempt, and pressing the Boss Key leaves no runaway background jobs, temporary-file buildup, or stale results applied to a new source.

### 6.7 Pool transient gameplay objects

Projectiles, explosions, letter shards, score captions, and other short-lived effects should use bounded pools once profiling shows allocation churn.

**Acceptance:** a maximum-effects Brickbat or combat burst does not create repeated garbage-collection hitches; gameplay-critical balls/projectiles remain visible above spectacle.

## 7. Target Product Boundaries

The current classes should be separated by responsibility, not merely split into arbitrary smaller files.

| Boundary | Owns | Does not own |
| --- | --- | --- |
| `DackSession` | active source, Snapshot, playset, edit/play state, selection, dirty state | Godot controls or rendering |
| `InputRouter` | Esc, Boss Key, play/edit mappings, text-entry suppression | toolkit simulation |
| `UiShellController` | window/panel ownership, tabs, responsive layout, fade/collapse | level rules |
| `SelectionService` | selected card/object/actor and edit commands | inspector widgets |
| `InspectorSchemaRegistry` | property descriptors, validation, grouping, undoable setters | object-specific hardcoded panels |
| `ToolkitRegistry` | toolkit descriptors, shelves, verbs, rules, preflight, HUD declarations | global capture/input/effects |
| `SimulationWorld` | fixed-step actors, projectiles, damage, triggers, win/lose events | editor layout |
| `EnvironmentMap` | stable regions, layers, spatial queries, source/current geometry | source file decoding |
| `SourceProvider` | Snapshot image/live frames, bounds, DPI, update policy | gameplay mutations |
| `DocumentAnalysisService` | pixels → regions/backgrounds/text/icon guesses | toolkit-specific scoring |
| `MutationService` | clone edits, active-state updates, undo/redo, variants | original source |
| `AssetCatalog` | stable asset IDs, provenance, cards, compiled manifests | raw-vault discovery at runtime |
| `SpriteImportCompiler` | source-specific slicing/cleanup/diagnostics | actor behavior |
| `ActorProfileRepository` | animation, boxes, movement/AI/attack/effect/sound bindings | live actor instances |
| `HudManager` | placement, whitespace/avoidance, approach fading | toolkit score rules |
| `EffectService` / `AudioService` | named reusable feedback profiles and load shedding | hit detection |
| `LevelRepository` | versioned load/save, migrations, atomic files, Snapshot/package assembly | live simulation |

Godot scenes and resources should define stable visual composition. C# controllers should bind state and commands rather than construct the entire product UI imperatively.

## 8. UI Shoring Plan

### P0: Keep every control reachable

- Fit all full-screen editor pages to the active monitor’s usable rectangle and DPI.
- Give long shelf, inspector, and animation-label areas their own scroll containers.
- Reserve a fixed header row with title, current selection, mode, and close gadget.
- Clamp popovers/inspectors inside the usable viewport.
- Replace the current mojibake UI strings (`Ã—`, `â€¢`, `Â°`, and similar) with verified UTF-8 text or simple safe glyphs, then add a startup/UI-string smoke check so close gadgets and status text cannot regress.
- Closing the main editor closes or returns ownership of Sprite Studio and transient subpages.
- Esc follows one stack: dismiss transient edit → close Sprite Studio → close Cockpit → open Cockpit. It never resets the level.

### P0: Make Play and Build unmistakable

- Build mode: pointer visible, handles/invisible logic visible, simulation paused unless previewed.
- Play mode: editor-only objects and handles hidden, pointer policy owned by the toolkit, game input active.
- The top status must show `BUILD`, `PLAY`, or `UNDERSTAND` in plain language.
- Entering play collapses the editor automatically; returning restores the prior tab, selection, scroll position, and deformed clone.
- Entering Play preserves the active playset/preset. No UI navigation command is allowed to select Platformer, recapture a source, reset a run, or clear mutations as a side effect.

### P0: Make layer ownership explicit

Use named layer roots in this order:

1. Source Clone.
2. Mutable Terrain.
3. World Objects.
4. Actors.
5. Spectacle Effects.
6. Gameplay-Critical Objects (player, balls, projectiles, targeting cursors).
7. HUD.
8. Editor Handles / Understanding.
9. Cockpit / Sprite Studio.
10. Boss / Safety.

Gameplay-critical objects must remain readable above nonessential effects. Boss/Safety is always topmost. Static document rendering should not redraw merely because an animation or HUD changed.

### P1: Replace control sprawl with descriptors

Define tabs, shelf groups, cards, inspector properties, and toolkit actions as data. The same descriptor should supply:

- label, concise tooltip, icon/thumbnail;
- category and applicable toolkit/mode;
- enabled/visible conditions;
- action/command;
- provenance/export badge where applicable;
- keyboard access and help text.

This removes repeated button-building code and makes capitalization, spacing, button width, and disabled states consistent.

### P1: Establish a small design system

Create shared tokens/components for:

- compact button, primary action, toggle/chip, card, shelf group, property row, warning, status badge, and close gadget;
- spacing, minimum hit size, font size/contrast, border, focus, selected, disabled, and error states;
- light/dark/high-contrast/Quiet Office/Arcade Neon/Terminal themes.

Buttons size to content unless a primary action explicitly deserves full width. Faint dark-gray labels and low-contrast toggles are defects, not theme flavor.

### P1: Make the Inspector schema-driven

The Inspector remains visible beside the active tab, but shows only properties relevant to the selection. Direct handles remain primary; fields provide precision. Common properties use common widgets everywhere:

- name/type/card;
- visible/editor-only, opacity, tint, shadow;
- position/size/rotation/layer;
- collision/material/text policy;
- movement/range/direction;
- health/damage/radar/AI;
- source binding and persistence;
- toolkit-specific section.

Edits become undoable commands, enabling multi-select and copy/paste later.

### P2: Separate windows intentionally

The target two-monitor model is:

- one playfield/runtime window that can occupy the source monitor at native coordinates;
- one editor window with shelves, Inspector, cards, Sprite Studio, logs, and performance tools;
- a single session/selection model shared between them;
- predictable focus, input capture, and Boss Key teardown across both.

Do not clone application state into two independent controllers.

## 9. Refactoring Sequence

### R0 — Safety Net and Baselines

- Add smoke-test checklists and deterministic sample levels for Platformer, Brickbat, Pinball, Overhead, animation import, and save/load.
- Add build validation plus unit tests for pure geometry, frame-sequence expansion, source transforms, serialization, and migration.
- Add the developer performance overlay and four benchmark scenes.
- Record current timings before changing architecture.

**Exit:** regressions and performance changes can be detected repeatably.

### R1 — State, Input, and UI Shell Extraction

- Introduce `DackSession`, explicit `AppMode`, and command/event flow.
- Add transition tests first, then remove the forced-Platformer side effect from Play/Build navigation.
- Extract `InputRouter`, `UiShellController`, `SelectionService`, and `HudManager`.
- Move tab/shelf/button definitions into registries/descriptors.
- Replace frame-loop UI polling with signals/events.
- Preserve existing visual behavior while splitting `Main.cs`.

**Exit:** `Main` composes services/scenes; it no longer owns every panel, save schema, actor rule, and toolkit simulation.

### R2 — Snapshot, Analysis, and Environment Map

- Implement `SnapshotImageSource` behind `SourceProvider`.
- Build one cached document-analysis result with stable region IDs.
- Introduce `EnvironmentMap` layers and spatial queries.
- Move text collisions, current/deleted state, background regions, and mutation events out of `PlayfieldSurface`.
- Add dirty-rectangle mutation and reversible deltas.

**Exit:** all current toolkits consume the same indexed environment and can distinguish source geometry from current mutable geometry.

### R3 — Asset Catalog and Sprite Import Compiler

- Define versioned asset/import/profile manifests.
- Move curated factories and source rules out of `SpriteAnimationSet`.
- Generate diagnostic previews and explicit accepted frame lists.
- Cache compiled textures/atlases and reuse them across actor instances.
- Promote creator-tested animation defaults without hardcoding new switch branches in `Main`.

**Exit:** an admitted character is data plus compiled assets; adding one does not require editing the root controller.

### R4 — Shared Simulation and Toolkit Contracts

- Extract actor movement, perception/radar, damage, weapons, projectiles, spawns, and goals into reusable systems.
- Use a fixed simulation step where construction-kit predictability benefits.
- Define toolkit descriptors/modules for Platformer, Brickbat, Pinball, and Overhead.
- Move toolkit-specific preflight, HUD, scoring, win/lose, and shelf declarations behind those modules.
- Add transient-object pools and effects quality tiers if profiling justifies them.

**Exit:** a new playset composes shared verbs and registers its differences rather than adding another branch throughout `Main`.

### R5 — Level/Snapshot Contract

- Replace nested RAD save classes with versioned DTOs in a persistence assembly/folder.
- Add atomic save, backups, migrations, unknown-field tolerance, asset IDs, Snapshot hashes, OCR cache, mutations, and actor profiles.
- Keep the current RAD level loadable through a migration adapter.

**Exit:** save files survive refactors and can become `.dacklevel`/`.dackpack` without embedding implementation details.

### R6 — Live Desktop and Two-Monitor Product Spike

- Add `LiveDesktopSource`/window/region providers.
- Normalize all coordinates through explicit source, Snapshot, playfield, window, monitor, and DPI transforms.
- Debounce boundary changes and update only affected environment regions.
- Run the editor and playfield as coordinated windows over one session.
- Exercise Boss Key, focus release, monitor removal, minimized source, and different-DPI cases.

**Exit:** the live path reuses the product spine rather than creating a second screenshot-specific engine.

## 10. What Not to Refactor Yet

- Do not replace Godot physics wholesale. Use Godot 2D physics and focused kinematic helpers until a measured construction-kit predictability problem demands more.
- Do not build the future importer subprocess ecosystem during engine stabilization. Compile curated image assets locally first.
- Do not add Office add-ins or native `.docx`/`.psd` parsing before Live Desktop and Snapshot workflows are solid.
- Do not convert every C# UI into scenes in one pass. Extract one stable shell/page family at a time.
- Do not rewrite working visual effects merely because their current code is procedural. First separate invocation/profile data from drawing and measure it.
- Do not optimize raw asset-vault scanning for end users; raw vaults are developer intake, not a runtime feature.

## 11. Testing Strategy

### Pure unit tests

- geometry transforms and DPI mapping;
- AABB/line/ramp/ladder/support calculations;
- stable region IDs and spatial-index queries;
- animation label parsing, dashed actions, reverse/ping-pong/strobe expansion;
- importer manifests and profile validation;
- damage, ball reserve, cooldown, range, and goal rules;
- level migrations and source/mutation separation.

### Golden-data tests

- known screenshot → expected text/word/line/icon region summaries;
- known sprite sheet → expected frame rectangles/order/origins;
- known level → stable save/load semantic equivalence;
- known mutation sequence → expected active object IDs and image-region hash.

### Runtime smoke tests

- source opens at 1:1 and unused monitor space is non-physical;
- Esc/editor/play/Boss paths always recover;
- selection/drag/resize/Inspector edits survive test play;
- Platformer can start, reach a goal, die by named cause, and reload;
- Brickbat respects three-ball reserves, letter/word deletion, and cooldowns;
- Pinball launches, flips, clears text, and exits cleanly;
- actors remain animated in every playset where they are visible;
- closing the main editor closes or safely returns from Sprite Studio.

## 12. Optimization Exit Criteria

The stabilization/refactor plateau is complete when:

- no ordinary editor page or Inspector can render offscreen at supported window sizes;
- the Cockpit adds negligible idle cost when open;
- dense text queries use an index and do not scan every text object for each actor/projectile/ball;
- small mutations update bounded regions and immediately change collision state;
- admitted sprites load from compiled manifests without runtime detection;
- background work is cancelable, bounded, observable, and never blocks play;
- Platformer, Brickbat, Pinball, and Overhead consume shared source/environment/session services;
- the current RAD level format migrates into a versioned Snapshot-aware save;
- benchmark scenes meet the agreed 60 FPS target or record an explicit, evidence-backed exception;
- a new actor, shelf object, Inspector property, or toolkit action can be added mostly through data/registration rather than another root-controller branch.

At that point, Live Desktop work can proceed without multiplying the prototype’s current coupling.
