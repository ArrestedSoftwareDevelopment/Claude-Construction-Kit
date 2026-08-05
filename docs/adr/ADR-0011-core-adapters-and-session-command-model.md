# ADR-0011: Core/Adapter Boundaries and a Single-Writer Session Command Model

- **Status:** Accepted
- **Date:** 2026-08-05
- **Decision owners:** DACK project
- **Related:** Primary Design Document §13; `DACK-Optimization-and-Refactoring-Plan.md`; ADR-0009; ADR-0010

## Context

The RAD proved the product by letting ideas compound quickly. Its largest classes now also reveal the cost of that success: the root controller coordinates UI, simulation, assets, persistence, source interpretation, and transitions; the playfield owns rendering as well as environmental queries and mutations; and the sprite catalog performs both development-time discovery and runtime assembly.

Live capture and a second window would multiply this coupling if they were attached directly to those classes. DACK also needs background OCR, image analysis, asset compilation, thumbnail work, and future capture deltas without allowing stale results to alter a newer source or touching Godot objects off the main thread.

The project needs an architectural rule stronger than “split large files.” The rule must preserve the prototype while making state ownership, dependencies, threading, undo, persistence, and failure behavior explicit.

## Decision

### 1. Use a core-and-adapters dependency direction

DACK will converge on these logical boundaries. They may begin as folders/namespaces and become assemblies only when the seam is stable.

```text
                           Dack.Godot Host
                scenes, drawing, input, window views
                                |
             +------------------+------------------+
             |                                     |
        Dack.Editor                           Dack.Runtime
  workspaces, selection, cards,         simulation, toolkits,
  inspector schemas, undo UX            actors, damage, goals
             |                                     |
             +------------------+------------------+
                                |
                           Dack.Application
                   session, commands, transactions,
                    jobs, events, use-case services
                                |
          +---------------------+----------------------+
          |                     |                      |
     Dack.Analysis         Dack.Assets          Dack.Persistence
  pixels -> immutable    catalog/compiler       DTOs, migrations,
  environment records    and provenance         atomic packages
          |                     |                      |
          +---------------------+----------------------+
                                |
                             Dack.Core
                 IDs, units, records, policies, math,
                   interfaces, deterministic rules

        Dack.Platform.Windows and future ImporterHost are adapters
        that enter through explicit Application/Core contracts.
```

Dependency rules:

1. `Dack.Core` has no Godot, Windows capture, UI, file-dialog, or importer-host dependency.
2. Analysis and asset compilation consume plain buffers/records and emit immutable, versioned products. They do not change the active level.
3. Runtime does not call capture, OCR, raw-vault discovery, UI widgets, or serializers from a simulation tick.
4. Editor views and Godot nodes render state and issue commands. They are not the authoritative store for level/session values.
5. Windows, Godot, OCR providers, file systems, and future importer subprocesses are adapters selected at composition time.
6. Toolkit modules depend on shared contracts and register descriptors/systems; shared services never depend on a concrete toolkit.

This is a modular monolith, not a distributed system. No network service, plugin process, native extension, or separate repository is implied unless its trust or deployment boundary independently requires one.

### 2. One session is the authoritative writer

`DackSession` owns durable working state: active source and Snapshot identity, working-clone version, mutation log, level/card/object state, active family/preset, simulation/authoring/surface/safety state, selection, dirty state, and save identity.

Durable changes occur through named commands such as:

- `SetAuthoringMode`, `SetSimulationState`, `OpenWorkspace`;
- `PlaceCardInstance`, `SetProperty`, `ApplyInstanceOverride`, `RevertOverride`, `ForkCard`;
- `CommitMutationBatch`, `ResetWorkingClone`, `PromoteVariant`;
- `AcceptAnalysisCorrection`, `ApplyRefreshCandidate`, `DiscardRefreshCandidate`;
- `SaveLevel`, `LoadLevel`, `BindAssetProfile`.

A command validates against a known session revision and produces one transaction containing:

- new state or a bounded state delta;
- domain events for views/runtime caches;
- an undo record when the action is creator-reversible;
- dirty-state and persistence implications;
- diagnostics explaining rejection or partial application.

Views may optimistically preview drags or numeric changes, but the committed result returns through the command path. Runtime-only high-frequency state may live in `SimulationWorld`; crossing into authored state, a checkpoint, a saved run, or a clone mutation requires an explicit transaction.

### 3. Separate commands, events, and queries

- **Commands** request a change and have one owner.
- **Events** announce a committed fact and may have many subscribers.
- **Queries** read immutable snapshots or indexed views and may not mutate as a side effect.

An event is not a second command bus. Subscribers may update a view/cache, but they may not silently make unrelated durable changes. A follow-up domain change is another named command so undo, replay, diagnostics, and tests remain intelligible.

### 4. Make loop/thread ownership explicit

- The Godot main thread owns scene-tree nodes, textures, controls, input dispatch, window surfaces, and final presentation commits.
- A fixed simulation clock owns gameplay advancement. Rendering may interpolate; editor and HUD refreshes are event-driven or use a slower UI cadence.
- Worker jobs may own immutable pixel buffers, OCR crops, analysis records, hashes, serialization staging, thumbnail generation, and asset compilation. They may not hold or modify live Godot objects.
- Each asynchronous job carries `sessionId`, `sourceVersion`, input/content hash, algorithm/provider version, and cancellation token.
- Results publish through a bounded main-thread commit queue. A result whose identity no longer matches the active request is discarded, not “best-effort” applied.
- Queues have caps, priority, deduplication, cancellation, and back-pressure. The system keeps the last coherent product when producers outrun consumers.

### 5. Treat analysis, mutations, and persistence as versioned products

`SourceFrame`, `SnapshotBaseline`, `IntakeRecipe`, `AnalysisRevision`, resolved `EnvironmentMap`, `WorkingClone`/`RegionRuntimeState`, and `RefreshCandidate` are different products with different owners as formalized by ADR-0012. Immutable baseline/analysis products are never edited in place. A mutation batch changes the clone and current region-state overlay atomically, using stable region IDs and bounded dirty rectangles.

Persisted DTOs do not serialize Godot nodes, controller fields, delegates, or cache implementation details. Caches are keyed by source/content hash plus algorithm/provider version and can be discarded and rebuilt. Creator decisions and stable IDs are authored data and cannot be discarded as cache.

### 6. Keep the two-monitor design as multiple views over one composition root

The second Godot window receives a view model, input scope, and render surface from the same session/simulation. It does not instantiate a second root controller, asset catalog, environment map, mutation log, audio engine, or simulation. Boss/Safety issues one high-priority application command whose platform adapter hides/neutralizes every owned window and releases input without waiting for ordinary work.

## Consequences

### Positive

- Live capture and two-monitor work extend existing contracts instead of forming a second engine.
- Pure geometry, card resolution, mutations, simulation rules, migrations, and transitions become unit-testable without a rendered Godot scene.
- Undo/redo, definition-versus-instance behavior, dirty state, and save/load share one change vocabulary.
- Stale OCR/capture/import results cannot corrupt a newer source.
- Performance instrumentation can attribute time and allocations to analysis, simulation, rendering, UI, and background queues.
- The future Player can reuse Core/Application/Runtime/Persistence while omitting creator workspaces.

### Tradeoffs

- Some simple RAD field assignments become explicit commands and descriptors.
- Preview interactions need a deliberate preview-versus-commit boundary.
- The session/application layer risks becoming another monolith unless commands remain use-case-sized and domain services retain their own data structures.
- Assembly extraction too early would slow iteration; dependency tests and namespaces should prove seams before project proliferation.

## Incremental Adoption

1. Add characterization tests and timing before moving behavior.
2. Use the existing `DackUiState`, cards, shelves, and profiler as seams; do not replace them merely to rename them.
3. Introduce command/state interfaces beside current fields, migrate one complete path, then delete the duplicate ownership.
4. Extract pure region queries/mutation plans before changing rendering.
5. Compile one admitted actor through the new asset contract before migrating the full catalog.
6. Keep `Main` as the composition root until it mostly wires services and views; a smaller file is an outcome, not the acceptance criterion.

## Validation

The decision is satisfied when repeatable tests prove:

1. UI navigation and a second window cannot change playset/source/mutations except through commands.
2. A stale analysis/OCR/import result is rejected by identity/version.
3. Simulation and environment hot queries allocate no managed memory after warm-up on benchmark levels.
4. One mutation transaction updates pixels, collision/active region state, score/effects events, undo, and dirty state consistently.
5. Save/load uses versioned DTOs and can run without a live Godot scene tree.
6. A toolkit and admitted actor can be registered without adding a cross-cutting branch to the root controller.
7. Boss/Safety preempts background work and neutralizes every owned window inside its performance budget.
