# DACK Optimization and Refactoring Plan

**Status:** Active engineering plan  
**Baseline:** RAD prototype, re-audited 2026-08-05
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
- Shared File/Transport menus, F6 Build/Play, F7 Freeze/Resume, explicit UI-state axes, and family-preserving navigation.
- Common nine-section family pages, descriptor-backed cards/shelves/slots, repeated drag placement, project-local forks, and instance-level actor/world-object editing through a shared docked/floating Inspector.
- A provisional geometry-only Playfield Profile and top-three source-to-game recommendations in Understand.
- Live-linked sprite pad, large Sprite Studio proof, editable animation labels/sequences, save/load manifests, strobe, ping-pong, and per-source animation defaults.
- Source-specific sprite import experiments: fixed grid, fixed rectangles, component extraction, blob detection, and individual-file sequences.
- Lazy, local OCR behind a service boundary, with gameplay-prioritized requests and geometry-only fallbacks.
- JSON level save/load for the current RAD state.

### Still Prototype-Grade

- UI construction, application state, simulation, persistence, and toolkit rules are concentrated in `Main.cs`.
- Rendering, mutable image work, text-region queries, world-object editing, collision helpers, and environment interpretation are concentrated in `PlayfieldSurface.cs`.
- Asset catalog, source-specific imports, image cleanup, frame detection, frame assembly, and curated actor factories are concentrated in `SpriteAnimationSet.cs`.
- Many panels are built imperatively in code and refreshed by polling, making layout defects and stale controls more likely.
- The former `SetEditorMode(false)` forced-Platformer side effect was removed during the August shell pass. It remains a permanent transition-regression fixture because it exposed the need for explicit session/surface/simulation ownership.
- Text-region queries repeatedly map, allocate, and sometimes inspect pixels instead of querying a stable indexed model.
- The captured-page importer performs several independent whole-image passes for related detections.
- Image mutations update the full texture even when only a small dirty rectangle changed.
- OCR work has a useful queue, but no session cancellation, bounded lifetime, persisted cache, or embedded provider yet.
- Level persistence is a RAD manifest nested inside `Main.cs`, not yet the versioned Snapshot/package contract.
- A mixed-content sprite sheet can still be admitted as one character (for example, a snake and a green blob combined into one profile); the importer has no reliable ambiguity gate or creator-review artifact yet.
- Maze/grid generation, path finding, reusable point/curve paths, parabola authoring, and motion-profile/inertia editing do not yet have shared engine services; they are still scattered toolkit ideas.
- The Dragon shadow can be mirrored/back-projected incorrectly, and Sprite Studio can fail to show the selected sprite even when the profile/frame data exists; editor and runtime currently lack one verified shadow/preview path.
- Palette handling is still centered on a small default strip, and repeated character shelves consume more space than a shared two-level picker.
- There is no automated test project or repeatable performance scene suite.

The current size audit reinforces the ownership problem: `Main.cs` is about 8,300 lines, `PlayfieldSurface.cs` about 2,100, and `SpriteAnimationSet.cs` about 1,900. File size alone is not a defect, but these three classes still cross application, editor, simulation, analysis, persistence, and asset-compilation boundaries. The smaller `DackUiState`, `FamilyPageShell`, card/shelf/slot components, shared Inspector, and `PlayfieldProfiler` are useful strangler seams; productization should route ownership through them rather than replace them wholesale.

These limitations are normal for a successful RAD. They are now explicit exit criteria rather than invisible debt.

## 3. Engineering Rules

1. **Preserve the playable loop during extraction.** Each refactor step ends with the project building and the Platformer, Brickbat, Pinball, and editor/play smoke paths still usable.
2. **Measure before and after.** A suspected hot path earns priority through frame timing, query counts, allocation counts, or a repeatable hitch—not file size alone.
3. **No full-image work in the frame loop.** Capture analysis, OCR preparation, color maps, and connected-component work run on import/background jobs or explicit mutation regions.
4. **Derived data is cached and versioned.** Text boxes, local backgrounds, spatial buckets, OCR labels, and import manifests belong to separable analysis/content-addressed or asset-compile caches and invalidate by baseline/recipe/content hash plus provider/algorithm version.
5. **Stable IDs replace position-as-identity.** Text objects, actors, cards, placed objects, assets, and mutations need stable identifiers so save/load and incremental updates do not depend on list order.
6. **One authoritative session state.** UI controls render state and issue commands; they do not each own competing copies of gameplay configuration.
7. **Toolkits declare differences.** A toolkit provides verbs, shelf entries, rules, preflight mutations, HUD widgets, and win/lose logic through a common contract.
8. **The immutable source remains separate.** Source pixels, working clone pixels, collision state, semantic labels, and runtime mutations are distinct layers.
9. **Degrade spectacle before gameplay.** Under load, reduce particles, shadows, glow, animation rate, OCR urgency, and distant AI ticks before reducing input/collision fidelity.
10. **Optimize for office PCs.** The baseline is an ordinary keyboard/mouse Windows machine at native document resolution, not a controller-first gaming rig.
11. **One simulation, many views.** A second monitor binds another view/input scope to the same session, world, environment, assets, audio, and mutation state.
12. **Godot objects stay on the main thread.** Workers consume immutable buffers/DTOs and return versioned results through a bounded main-thread commit queue.
13. **Reject stale work by identity.** Every OCR, analysis, refresh, save-stage, thumbnail, and asset-compile result carries session/source/content/algorithm identity and cancellation.
14. **User gestures define transactions.** A drag or slider scrub previews continuously but commits one undo command. Runtime mutation history is separate from creator Undo/Redo.
15. **Caches are replaceable; creator intent is not.** Derived masks and thumbnails may rebuild. Stable IDs, accepted corrections, source bindings, card overrides, and authored rules must survive algorithm/version changes.

## 4. Provisional Performance Budgets

These are starting budgets to make “fast enough” testable. Profile data may revise them, but changes should be recorded.

| Area | Target | Floor / guardrail |
| --- | --- | --- |
| Baseline machine | Windows 11; 4 physical/8 logical cores; 16 GB RAM; integrated-GPU class; 1920×1080 at 60 Hz | Record exact CPU/GPU/driver/power mode with every published result |
| Active play | 60 FPS; p95 frame ≤16.7 ms and p99 ≤25 ms after warm-up | Never remain below 30 FPS during an ordinary supported level |
| Main-thread frame time | 16.7 ms budget | Warn at 22 ms sustained; automatically capture a trace/counters at 33 ms |
| Hot-loop allocation | zero managed allocation after warm-up for simulation/environment probes | No page-wide LINQ/array construction per actor, ball, or projectile |
| Input-to-visible-response | under 50 ms for editor handles and controls | No blocking OCR/import work on input |
| Intake guide manipulation | 60 FPS visual guide response; expensive regional re-analysis begins only after a 150–300 ms configurable debounce | One active candidate per region/revision; superseded work cancels or is discarded |
| Play/Edit/Cockpit transition | p95 under 150 ms | Must not reset, recapture, reanalyze, or copy the full source |
| Boss Key response | under 100 ms to hide/neutralize and mute | Safety path is never delayed by save/import/OCR |
| Document mutation | collision/active state commits in the current simulation transaction; pixels visible by the next frame | One coalesced texture upload per rendered frame; never rescan the page |
| 1080p capture/intake | preview within 1 s; coarse geometry within 750 ms; full non-OCR analysis within 2 s | Progressive/cancelable; creator can work from last coherent result |
| 4K/mixed-DPI stress | coherent non-OCR analysis within 5 s | Tile/bound memory and avoid duplicate full-frame buffers |
| OCR/background jobs | bounded concurrency and queues; cancellation acknowledged within 250 ms | Zero frame-loop waits; stale result never applies to a new source |
| Manual-refresh idle | median process CPU below 1% over ten minutes on the baseline machine, with no captures/full analysis and no continuously animated hidden UI | Foreground office work and Boss/Desktop parking suspend nonessential previews/jobs |
| Second presentation view | identical simulation/query counts to one-view run; hidden view adds below 1% median CPU after settling | Visible view adds rendering only; it never advances physics, audio, RNG, OCR, or mutation twice |
| Active session visual/analysis working set | initial guardrail ≤256 MB at 1080p and ≤768 MB at 4K, including baseline, clone, analysis, masks, indices, staging, and resident GPU tiles | Instrument each owner separately; spill inactive page/derived caches before paging the process |
| Save | atomic; metadata/recipe save p95 under 500 ms excluding an explicit new large image copy | Never partially overwrite the last good level; retain rolling recovery |

The benchmark matrix should include:

- a clean 1080p office screenshot;
- a dense document with at least 2,500 detected text objects;
- a heavily deformed clone;
- 1, 10, and 50 active actors;
- one and three Brickbat balls with maximum allowed effects;
- mixed ladders/conveyors/elevators/enemies/projectiles;
- the largest admitted sprite sheet and a deliberately awkward importer test sheet;
- a two-monitor layout with different DPI scales when Live Desktop work begins.
- 1366×768, 1920×1080, and 3840×2160 editor layouts at 100%, 125%, 150%, and 200% scale, including one mixed-DPI pair;
- idle/foreground-office and background-DACK power behavior, not only a maximized benchmark run.

The first memory figures are engineering guardrails, not promises. Record peak committed memory, owned pixel buffers by purpose, texture memory, Snapshot/page cache count, and eviction. A native 4K RGBA frame is roughly 32 MB before clones, masks, mip/tile summaries, textures, and refresh candidates; accidental copies can consume the budget faster than the algorithms themselves.

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
5. publish and cache an immutable `AnalysisRevision` keyed by Snapshot Baseline hash, Intake Recipe hash, and detector/schema version.

Toolkit-specific shapes should be views over that shared analysis, not independent detections.

**Acceptance:** importing one source performs one primary pixel-analysis pass; switching playsets does not repeat it.

#### 6.2.1 Complete text-understanding and erasure overhaul

The current black-pixel/rectangle assumptions are no longer sufficient. A useful source may contain anti-aliased body text, light sub-headings, colored labels, icons, pillboxes, gutters, margins, and large regions of near-white background. The same source can also be damaged over time by Brickbat, Pinball, projectiles, explosions, and platformer gaps. Discovery, OCR, collision, and erasure must therefore become one versioned pipeline rather than separate heuristics.

The named development fixtures and their expected behaviors are recorded in [DACK Document-Analysis Fixture Matrix](DACK-Document-Analysis-Fixture-Matrix.md). The folder includes sparse desktop icons, Git-style nested panels, spreadsheet and Gantt grids, ASCII text maps, and a temporal browser capture; these are the baseline corpus for rectangle/icon discovery rather than optional examples.

Deliver it in five explicit layers:

1. **Appearance model:** derive luminance, chroma, local background, ink likelihood, anti-alias fringe, and connected-component maps at native resolution. Do not require true black or assume that white is the background. Store confidence and the sampled background model with each region.
2. **Region graph:** group components into glyphs, words, lines, paragraphs, headings, icons, pillboxes, cells, panels, gutters, and background zones. Keep candidate regions even when they are not yet understood; collision and geometry-only play must work from those candidates.
3. **Meaning service:** queue small, prioritized OCR regions near the next likely interaction. Bind LibTesseract or another local provider to stable region IDs, cache by source/region hash plus provider version, cancel stale work, and retain geometry-only fallbacks. OCR must never be required to start or continue play.
4. **Mutation service:** make the region mask used for collision the same mask used for letter/word erasure, scoring, and effects. Expand a hit by a small configurable fringe, include adjacent anti-alias pixels, fill from the cached local background model, mark only intersecting regions dirty, and record a reversible mutation. Exact word mode must deactivate one selected word, not every overlapping candidate.
5. **Verification and correction:** show a creator-facing Understand overlay with accepted/rejected regions, OCR confidence, background samples, and the current mutation mask. Golden fixtures must cover light text, anti-aliasing, icons, pillboxes, sub-headings, dense paragraphs, already-erased pages, and repeated word shapes.

The output is one immutable `AnalysisRevision` resolved through a shared `EnvironmentMap` view consumed by Platformer, Brickbat, Pinball, Snake/Maze, RPG, and future route tools. It is acceptable for OCR to arrive late; it is not acceptable for a later playset to use a different definition of the same text object.

**Acceptance:** the supplied difficult document remains legible at 1:1; light and anti-aliased text is discoverable; collision and erasure agree; a one-letter/one-word hit changes only its bounded dirty region; a blast cannot leave a visible fringe outside its configured tolerance; OCR may be disabled without breaking geometry play; and the same stable region survives a playset switch and reload.

### 6.2.2 Shared geometry and motion authoring services

The following creator tools are a planned shared layer, not separate one-off features in each genre page:

| Tool | Shared contract | First consumers |
| --- | --- | --- |
| Maze Generator | Seeded rectangular or hexagonal topology, wall/floor masks, entry/exit constraints, difficulty/loop controls, and a preview diff against the source clone | RPG/Roguelike, Snake/Maze, Minefield, tower/escort routes |
| Path Finder | Grid- and graph-based route queries over text, placed solids, gates, and hazards; A*/BFS baseline with optional flow-field output; explainable blocked/cost overlays | Enemies, escorts, racing, tower defense/offense, Snake/Maze |
| Grid Overlay | Non-destructive rectangular and hexagonal snapping/inspection overlay with cell IDs, coordinate transforms, and per-cell source/creator/runtime state | Grid/Text, RPG, Snake/Maze, spreadsheets, cellular experiments |
| Point/Path/Curve Generator | Named points, polylines, Bezier curves, tangents, loop/branch controls, arc-length sampling, and draggable handles | Patrols, conveyors, elevators, ropes/vines, racing, projectile paths |
| Parabola Editor | Start/end/peak or angle/velocity controls, gravity preview, landing prediction, sampled collision path, and export to a reusable motion profile | Jump arcs, artillery, pinball launch previews, thrown objects, Lunar Lander tuning |
| Inertia Settings | Acceleration, drag, braking, reverse time, angular inertia, max speed, and control response with ground/vehicle/air/space presets | Platformer, driving, aircraft, spacecraft, tanks, overhead combat |

All six tools use the same native-resolution coordinate contract and emit serializable creator geometry. A visible overlay is optional presentation; the underlying grid/path/curve remains usable when the overlay is hidden. Generated geometry is versioned, seeded where applicable, undoable, and queryable by `EnvironmentMap` and `SimulationWorld`.

**Acceptance:** a creator can generate a seeded maze, inspect or edit its grid, ask an actor for a route, drag a curve or parabola handle while seeing the preview update immediately, and save/reload the result without baking it into the source image. No toolkit is allowed to invent a second path, grid, or inertia representation.

### 6.2.3 Make source refresh explicit

The active playfield must not be invalidated every time the underlying desktop or document changes. Use the canonical ADR-0012 products rather than a three-bucket shortcut:

- ephemeral immutable `SourceFrame`, admitted immutable `SnapshotBaseline`, independent versioned `IntakeRecipe`, and immutable replaceable `AnalysisRevision`;
- authored `LevelDefinition` plus mutable tiled `WorkingClone`/`RegionRuntimeState` and reversible mutation log;
- transient `RefreshCandidate` aggregate that is invisible to gameplay until approved and never publishes its pixels without the matching analysis revision.

Initial capture runs one full analysis. Editing, Play, playset switching, OCR naming, and clone mutations reuse the stable Snapshot Baseline and selected cached `AnalysisRevision`; they do not recapture the source. A lightweight OS/window signal may set `Refresh available`, but only the creator's `Refresh Source` command can capture and analyze a candidate. Apply/Rebind/Discard is one transaction; rejected or superseded candidates cancel their OCR and analysis work.

**Acceptance:** ten minutes of idle Editing/Playing performs no source capture or full-image analysis; switching playsets performs no capture; applying a refresh creates a new Snapshot and preserves the previous one; discarding a candidate leaves the active clone byte-for-byte unchanged.

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

Runtime spawning should load a curated manifest, not rediscover how a sheet is sliced. The importer may use fixed grids, an interactively calibrated grid, explicit rectangles, component seeds, or a draft detector, but it should write a reviewable compiled result. The calibrated-grid tool is an internal development path: it lets a creator repair a difficult sheet once, then hands deterministic geometry to the same compiler used by runtime.

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
| `DackSession` | active product IDs/revisions and one coherent Source/Baseline/Recipe/Analysis/Level/Clone/Region/Run/Variant aggregate, family/preset, selection, dirty/recovery state | Godot controls, duplicated view models, or rendering |
| `CommandDispatcher` / `CommandHistory` | validate session revision, commit authored transactions, coalesce gestures, undo/redo, diagnostics | rendering widgets or simulation polling |
| `JobScheduler` | bounded priority lanes, deduplication, cancellation, stale-result rejection, main-thread publication | unbounded worker creation or direct Godot-node access |
| `InputRouter` | Esc, Boss Key, play/edit mappings, text-entry suppression | toolkit simulation |
| `UiShellController` | window/panel ownership, tabs, responsive layout, fade/collapse | level rules |
| `SelectionService` | selected card/object/actor and edit commands | inspector widgets |
| `LevelContentsModel` | virtualized searchable hierarchy, visibility/lock/authority, multi-selection | asset discovery or direct mutation |
| `InspectorSchemaRegistry` | property descriptors, validation, grouping, undoable setters | object-specific hardcoded panels |
| `ToolkitRegistry` | toolkit descriptors, shelves, verbs, rules, preflight, HUD declarations | global capture/input/effects |
| `SimulationWorld` | fixed-step actors, projectiles, damage, triggers, win/lose events | editor layout |
| `EnvironmentMap` | indexed resolved query view over Analysis, accepted corrections, creator geometry, and Region Runtime State | source decoding, detector evidence mutation, or clone pixel uploads |
| `SourceProvider` | source capabilities and coherent `SourceFrame` acquisition, bounds, DPI/color/rotation metadata, update policy | Snapshot admission, analysis, or gameplay mutations |
| `DocumentAnalysisService` | baseline + Intake Recipe + algorithm version → immutable Analysis Revision and profile evidence | toolkit-specific scoring or runtime active state |
| `MutationService` | transactional Working Clone edits, Region Runtime State updates, runtime mutation history, Variant derivation | original source, Analysis evidence, or creator command undo/redo |
| `CloneRenderer` | tile residency, dirty-tile uploads, 1:1 presentation, view transforms | semantic mutation/scoring rules |
| `CardCatalog` | canonical Card definitions, typed Slots, forks, dependency/version resolution, resolved hashes, and placeholders | placed Instances or competing actor/asset definition stores |
| `AssetCatalog` | stable asset IDs, provenance, compiled manifests, and Card-addressable asset records | raw-vault discovery at runtime or Card composition authority |
| `SpriteImportCompiler` | source-specific slicing/cleanup/diagnostics | actor behavior |
| `ActorProfileRepository` | actor-specific projection/adapter over resolved Cards: animation, boxes, movement/AI/attack/effect/sound bindings | independent Card truth or live actor instances |
| `HudManager` | placement, whitespace/avoidance, approach fading | toolkit score rules |
| `EffectService` / `AudioService` | named reusable feedback profiles and load shedding | hit detection |
| `LevelRepository` | versioned load/save, migrations, atomic files, Snapshot/package assembly | live simulation |
| `GeometryToolService` | grids, maze/path/curve/parabola authoring and serializable geometry | family-specific scoring/win rules |
| `PlayfieldRecommendationService` | descriptor-driven compatibility/confidence/explanations and draft construction recipes | silently selecting a family or placing objects |
| `PackageValidator` / `Sanitizer` | dependency resolution, path/archive limits, provenance, privacy scrub, publish preview | originals or raw-vault admission |

Godot scenes and resources should define stable visual composition. C# controllers should bind state and commands rather than construct the entire product UI imperatively. The dependency direction and thread rules are normative in [ADR-0011](adr/ADR-0011-core-adapters-and-session-command-model.md); the state products are normative in [ADR-0012](adr/ADR-0012-snapshot-analysis-clone-state-separation.md); tile-backed clone rendering is normative in [ADR-0013](adr/ADR-0013-tile-backed-native-pixel-clone-rendering.md); Card resolution and physics/clock ownership are normative in [ADR-0014](adr/ADR-0014-card-definition-instance-and-dependency-contract.md) and [ADR-0015](adr/ADR-0015-simulation-clock-and-physics-authority.md).

### 7.1 Canonical runtime products

```text
SourceDescriptor -> SourceFrame -> SnapshotBaseline
                                      + IntakeRecipe
                                      -> AnalysisRevision

LevelDefinition references the baseline/analysis and owns authored corrections,
Cards, instances, rules, routes, bindings, and policies.

WorkingClone + RegionRuntimeState are the mutable environmental branch.
RunState is transient simulation. Variant is a named authored mutation branch.
Session is the one open aggregate; Pack is a validated distribution artifact.
```

Do not store placed actors/rules/current damage inside an immutable Snapshot. Do not store creator grids/seeds only inside detector regions. Do not mutate an Analysis revision to represent a destroyed letter. Cache and canonical data must remain separable enough that deleting all derived caches cannot delete creator intent.

## 8. UI Shoring Plan

### P0: Keep every control reachable

- Fit all full-screen editor pages to the active monitor’s usable rectangle and DPI.
- Give long shelf, inspector, and animation-label areas their own scroll containers.
- Reserve a fixed header row with title, current selection, mode, and close gadget.
- Clamp popovers/inspectors inside the usable viewport.
- Replace the current mojibake UI strings (`Ã—`, `â€¢`, `Â°`, and similar) with verified UTF-8 text or simple safe glyphs, then add a startup/UI-string smoke check so close gadgets and status text cannot regress.
- Closing the main editor closes or returns ownership of Sprite Studio and transient subpages.
- Esc follows one stack: dismiss transient edit → close Sprite Studio → close Cockpit → open Cockpit. It never resets the level.
- Reserve `F1` for Help and `Ctrl+P` for conventional Print semantics; use `Ctrl+Shift+P` for Command Search. `Ctrl+Alt+B` is Boss/Safety only, not ordinary show/hide.
- Right-click quick Inspector/context commands also exist through `Shift+F10`/Menu key and the shared Inspector command.

### P0: Make Play and Build unmistakable

- Build mode: pointer visible, handles/invisible logic visible, simulation paused unless previewed.
- Play mode: editor-only objects and handles hidden, pointer policy owned by the toolkit, game input active.
- The top status must show `BUILD`, `PLAY`, or `UNDERSTAND` in plain language.
- Entering play collapses the editor automatically; returning restores the prior tab, selection, scroll position, and deformed clone.
- Entering Play preserves the active playset/preset. No UI navigation command is allowed to select Platformer, recapture a source, reset a run, or clear mutations as a side effect.

### P0: Repair sprite visibility and shadow correctness

- Make Sprite Studio's selected-label preview and editable-frame preview share one explicit texture/frame binding; a loaded profile with valid frames must never render an empty stage.
- Route editor and runtime shadows through the same `ShadowRenderer` transform. The shadow must use the current frame, facing, flip, origin, and scale; the Dragon's backwards shadow is a regression fixture.
- Default the shared page-light vector to a modest relative back/left offset, with an optional facing-relative mode and per-profile override.
- Add a visual smoke test that opens Dragon in Studio, previews Idle/Run/Fly, toggles facing, and compares sprite/shadow orientation and offset in editor and Play.

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

The descriptor registry also owns the shared two-level character picker and the single `F6` Play/Build toggle, so these behaviors do not diverge across Player, Enemy, Spawn, Builder, Projectile, and effect pages.

This removes repeated button-building code and makes capitalization, spacing, button width, and disabled states consistent.

### P1: Add Level Contents and transactional editing

- Add a virtualized searchable Level Contents/Outliner for Actors, World, Logic, HUD, source-bound objects, editor-only objects, and unresolved records.
- Expose visible, editor-locked, authority/source, selection, and multi-select state without requiring a precise canvas click.
- A drag/resize/rotation or slider scrub previews at pointer rate but commits one coalesced command on release; Cancel restores its start value.
- Inspector edits target the placed instance by default. Editing a project-owned definition is explicit, names affected instances, and offers Apply/Reset/Fork.
- Runtime mutation history remains separate from creator `Ctrl+Z`; converting a run mutation into authored terrain is an explicit Promote/Save Variant command.
- Add lightweight recipe autosave and rolling recovery records. Immutable Snapshot tiles are referenced by hash instead of recopied on each recovery pass.

### P1: Establish a small design system

Create shared tokens/components for:

- compact button, primary action, toggle/chip, card, shelf group, property row, warning, status badge, and close gadget;
- spacing, minimum hit size, font size/contrast, border, focus, selected, disabled, and error states;
- light/dark/high-contrast/Quiet Office/Arcade Neon/Terminal themes.

Palette selectors are part of the design system: expose named constrained profiles (C64, DOS/ANSI, DACK 32/64, Game Boy, monochrome, full-color) and a creator custom-palette path without expanding every card into a permanent swatch wall.

Buttons size to content unless a primary action explicitly deserves full width. Faint dark-gray labels and low-contrast toggles are defects, not theme flavor.

Accessibility profiles are part of the design system: Reduced Motion, No Flash/Strobe, screen-shake reduction/off, pointer-hide override, scalable editor text, color-independent states, and visual equivalents for important sound cues. Strobe is disabled by default. Keyboard-only, Narrator/NVDA, Magnifier, Windows contrast themes, and mixed-DPI checks belong in the shell smoke suite.

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

Split the work into two measured spikes before integration: (1) Godot multi-window presentation over one existing Snapshot/session; (2) Windows monitor/window/region capture and loss/recovery behavior. The presentation spike tests negative desktop coordinates, mixed DPI, focus, Alt-Tab, monitor hot-unplug, hidden-window suspension, close ownership, and atomic Boss teardown. The capture spike tests consent, self-capture exclusion, cursor policy, rotation/color normalization, protected/inaccessible content, resize/close, device/access loss, and refresh candidates.

### P2: Activity Center and background-work UX

Capture, analysis, OCR, import, save, thumbnail, and compilation jobs report to one nonmodal Activity Center. Each activity shows source/session identity, progress when meaningful, current stage, Cancel, outcome, and diagnostics. Play may begin from the last coherent partial product. When another office application is foreground or DACK is parked/Bossed, nonessential work yields or suspends according to policy rather than continuing invisibly at full power.

### P2: Optional dynamic-light spectacle

Keep the shared projected-paper shadow as the baseline path. As an opt-in rendering tier, allow one scene light to drive shadow direction, length, skew, softness, and tint for all eligible objects. Cache the light/object relationship, update it only when the light or object transform changes, and shed back to the cheap shadow when frame time or office-safe presentation requires it. Validate this first in a Pinball showcase scene; it is not a prerequisite for ordinary document playfields.

## 9. Refactoring Sequence

The `R` labels are remaining architecture gates, not a claim that visible product work happened in that order. August work has already implemented parts of the shell, cards, Inspector, and profiler inside the monolith. A workstream is not complete until four distinct states are recorded:

| Workstream | Visible in RAD | Extracted behind target boundary | Automated coverage | Performance/persistence qualified |
| --- | --- | --- | --- | --- |
| UI axes, F6/F7, File/Transport, family shell | Yes | Partial (`DackUiState`/`FamilyPageShell`; root still owns transitions/UI) | No | No |
| Cards, shelves, slots, forks, repeated placement | Yes | Partial descriptors/components | No | Partial RAD save only |
| Actor/world Inspector | Yes | Shared presentation but not schema/command complete | No | Partial RAD save only |
| Playfield recommendations | Provisional geometry-only | Initial `PlayfieldProfiler`; hardcoded family scoring | No | No |
| Source analysis/environment/mutation | Proven across playsets | No shared immutable Analysis revision/index/tile renderer | No | No |
| Sprite imports/actor defaults | Proven for many sources | No compiled catalog boundary | Manual only | No |
| Level/Snapshot/package persistence | RAD JSON save/load | No canonical DTO/repository/package boundary | No | No |

Work may proceed in thin vertical slices, but R0 observability/fixtures, the canonical state graph, and the session/command boundary gate any serious live-source integration. A small multi-window presentation spike may run alongside analysis work only if it uses one session and testable transforms.

### R0 — Safety Net and Baselines

- Add smoke-test checklists and deterministic sample levels for Platformer, Brickbat, Pinball, Overhead, animation import, and save/load.
- Add build validation plus unit tests for pure geometry, frame-sequence expansion, source transforms, serialization, and migration.
- Add the developer performance overlay and four benchmark scenes.
- Record current timings before changing architecture.

**Exit:** regressions and performance changes can be detected repeatably.

### R1 — State, Input, and UI Shell Extraction

- Introduce `DackSession`, explicit `AppMode`, and command/event flow.
- Add transition tests that preserve the now-fixed forced-Platformer regression and every family/source/mutation combination.
- Extract `InputRouter`, `UiShellController`, `SelectionService`, and `HudManager`.
- Move tab/shelf/button definitions into registries/descriptors.
- Register one two-level character/asset picker (role/family → individual asset) and one configurable `F6` Build/Play command.
- Replace frame-loop UI polling with signals/events.
- Preserve existing visual behavior while splitting `Main.cs`.

**Exit:** `Main` composes services/scenes; it no longer owns every panel, save schema, actor rule, and toolkit simulation.

### R2 — Snapshot, Analysis, and Environment Map

- Implement `SnapshotImageSource` behind `SourceProvider`.
- Implement `SnapshotBaseline`, independent versioned `IntakeRecipe`, and immutable `AnalysisRevision` per ADR-0012.
- Deliver the first rectangular Intake Recipe grid (origin, cell size, rows/columns, rotation, snapping, undo, save/load) with immediate guide drawing and debounced/cancelable regional analysis; hex/semantic guides follow.
- Build one cached document-analysis result with stable region IDs, adaptive background/ink/anti-alias maps, and explicit icon/pillbox/background candidates.
- Replace the current separate discovery/OCR/erasure heuristics with the staged text-understanding and mutation pipeline in §6.2.1; preserve geometry-only play when OCR is unavailable.
- Introduce `EnvironmentMap` layers and spatial queries.
- Move text collisions, current/deleted state, background regions, and mutation events out of `PlayfieldSurface`.
- Add dirty-rectangle mutation and reversible deltas.
- Replace the monolithic clone texture with the tile-backed renderer in ADR-0013 and measure 256/512-pixel tiles before freezing the runtime default.

**Exit:** all current toolkits consume the same indexed environment and can distinguish source geometry from current mutable geometry.

### R3 — Asset Catalog and Sprite Import Compiler

- Define versioned asset/import/profile manifests.
- Move curated factories and source rules out of `SpriteAnimationSet`.
- Add the internal calibrated-grid sheet tool: live origin/cell/gutter handles, accepted-cell exclusions, baseline preview, and profile save/reload.
- Add reusable visual-card manifests for Pinball board skins, logos, backglass, rails, bumpers, inserts, aprons, and typography, plus Brickbat ANSI target tables, target-wall frames, paddle/ball skins, and bonus banners; keep visual bindings separate from collision/target-part cards.
- Generate diagnostic previews and explicit accepted frame lists.
- Cache compiled textures/atlases and reuse them across actor instances.
- Promote creator-tested animation defaults without hardcoding new switch branches in `Main`.

**Exit:** an admitted character is data plus compiled assets; adding one does not require editing the root controller.

### R4 — Shared Simulation and Toolkit Contracts

- Extract actor movement, perception/radar, damage, weapons, projectiles, spawns, and goals into reusable systems.
- Move all gameplay advancement to the single fixed session clock in ADR-0015 (60 Hz semantic baseline; measured Pinball physics-rate option), with interpolation and bounded catch-up rather than frame-driven toolkit loops.
- Separate gameplay/decorative RNG, define deterministic event priority, and budget AI/rules/path/contact/mutation work per tick.
- Add shared `GeometryToolService` capabilities: rectangular/hex grids, seeded maze generation, path finding, point/polyline/Bezier paths, parabola previews, and serialized inertia/motion profiles.
- Define toolkit descriptors/modules for Platformer, Brickbat, Pinball, and Overhead.
- Move toolkit-specific preflight, HUD, scoring, win/lose, and shelf declarations behind those modules.
- Add transient-object pools and effects quality tiers if profiling justifies them.

**Exit:** a new playset composes shared verbs and registers its differences rather than adding another branch throughout `Main`; one/two views and 30/60/144 Hz rendering produce the same fixed-input gameplay trace within declared physics tolerances.

### R5 — Level/Snapshot Contract

- Replace nested RAD save classes with versioned DTOs in a persistence assembly/folder.
- Add atomic save, backups, migrations, unknown-field tolerance, asset IDs, Snapshot hashes, OCR cache, mutations, and actor profiles.
- Keep the current RAD level loadable through a migration adapter.
- Define authored level data, derived caches, optional checkpoint/run state, autosave/recovery, card/asset version pins, safe archive extraction, and package privacy scrub as separate records.

**Exit:** save files survive refactors and can become `.dacklevel`/`.dackpack` without embedding implementation details.

### R6 — Live Desktop and Two-Monitor Product Spike

- First complete the Godot multi-window presentation spike over the existing Snapshot session; separately complete the Windows capture-backend spike; then integrate them.
- Add capability-declaring monitor/window/region providers, using Windows Graphics Capture for consent-oriented window/display selection and Desktop Duplication where monitor dirty/move/cursor metadata or performance justifies it.
- Normalize all coordinates through explicit source, Snapshot, playfield, window, monitor, and DPI transforms.
- Debounce boundary changes and update only affected environment regions.
- Run the editor and playfield as coordinated windows over one session.
- Exercise Boss Key, focus release, monitor removal, minimized source, and different-DPI cases.
- Exercise negative monitor coordinates, rotation, color normalization/HDR-to-SDR policy, DACK self-capture exclusion, protected content, source close/replacement, access/device loss, Alt-Tab/taskbar behavior, and hidden-window render suspension.

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
- text-surface policies (ignore, solid-platform, solid-block, climbable, crawlable, hybrid), line-spacing classification, gap tolerance, and actor capability gating;
- stable region IDs and spatial-index queries;
- animation label parsing, dashed actions, reverse/ping-pong/strobe expansion;
- shadow transform invariants (frame/facing/flip/origin/scale), palette-profile validation, and two-level picker selection state;
- importer manifests and profile validation;
- damage, ball reserve, cooldown, range, and goal rules;
- level migrations and source/mutation separation.

### Golden-data tests

- Light, anti-aliased, colored, icon, pillbox, sub-heading, and already-erased source fixtures â†’ expected region masks and background classifications.
- One-letter, one-word, blast, laser, and overlapping-word mutations â†’ expected bounded dirty rectangles and active/deleted IDs.
- Mixed-content sprite sheets, including the snake/green-blob failure case â†’ expected separated candidates, ambiguity warning, and explicit creator acceptance before runtime admission.
- Seeded maze/grid/path/Bezier/parabola generation â†’ stable geometry hashes and route results.

- known screenshot → expected text/word/line/icon region summaries;
- known sprite sheet → expected frame rectangles/order/origins;
- known level → stable save/load semantic equivalence;
- known mutation sequence → expected active object IDs and image-region hash.

### Runtime smoke tests

- source opens at 1:1 and unused monitor space is non-physical;
- Esc/editor/play/Boss paths always recover;
- `F6` toggles Build/Play and returns to the prior tab/selection without resetting the session;
- selection/drag/resize/Inspector edits survive test play;
- repeated shelf placement creates multiple same-type actors/objects at separated initial positions and preserves them through save/load;
- Platformer can start, reach a goal, die by named cause, and reload;
- Brickbat respects three-ball reserves, letter/word deletion, and cooldowns;
- Pinball launches, flips, clears text, and exits cleanly;
- actors remain animated in every playset where they are visible;
- closing the main editor closes or safely returns from Sprite Studio.
- one edit drag or slider scrub produces one undo transaction, Cancel restores its pre-preview value, and runtime mutation history never contaminates creator Undo;
- Snapshot, Intake Recipe, Analysis Revision, Level, Working Clone, Region Runtime State, Run State, and Variant survive save/load without ownership leakage;
- tile seams are visually absent at 1:1, erasure and collision share the same mask, and dirty uploads remain bounded to affected tiles;
- stale capture/analysis/OCR jobs cannot publish over a newer session or revision;
- a second presentation window does not create a second simulation tick, and monitor removal/DPI changes return to a recoverable single-window state;
- keyboard-only and `Shift+F10` paths reach every durable editor action, Level Contents exposes invisible/overlapped items, and Reduced Motion/No Flash are honored by every effect family;

## 12. Optimization Exit Criteria

The stabilization/refactor plateau is complete when:

- no ordinary editor page or Inspector can render offscreen at supported window sizes;
- Sprite Studio previews the selected valid frame/animation, including the Dragon smoke fixture;
- editor/runtime shadows agree on orientation and use the shared back/left default offset;
- the Cockpit adds negligible idle cost when open;
- dense text queries use an index and do not scan every text object for each actor/projectile/ball;
- small mutations update bounded regions and immediately change collision state;
- text discovery, OCR labels, collision, and erasure use one stable region/mask model across playsets;
- admitted sprites load from compiled manifests without runtime detection;
- mixed-content sheets cannot silently become a single actor profile;
- grid, maze, path, curve, parabola, and inertia data round-trip through the level contract;
- background work is cancelable, bounded, observable, and never blocks play;
- Platformer, Brickbat, Pinball, and Overhead consume shared source/environment/session services;
- the current RAD level format migrates into a versioned Snapshot-aware save;
- benchmark scenes meet the agreed 60 FPS target or record an explicit, evidence-backed exception;
- a new actor, shelf object, Inspector property, or toolkit action can be added mostly through data/registration rather than another root-controller branch.
- Player, Actors, World, Logic, Effects, and asset-binding surfaces use the shared catalog/two-level picker projection, and expanded palette profiles load without permanent shelf sprawl.
- p95 and p99 frame/input/job latency, resident CPU/GPU memory, idle cost, and capture/analysis timings are recorded on the published baseline hardware at 1080p and 4K, including a mixed-DPI two-monitor case.

At that point, Live Desktop work can proceed without multiplying the prototype’s current coupling.
