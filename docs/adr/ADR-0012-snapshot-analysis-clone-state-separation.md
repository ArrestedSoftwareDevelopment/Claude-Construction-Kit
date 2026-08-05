# ADR-0012: Source, Snapshot, Analysis, Level, Clone, and Run-State Separation

- **Status:** Accepted
- **Date:** 2026-08-05
- **Decision owners:** DACK project
- **Related:** ADR-0009; ADR-0011; `DACK-Level-Snapshot-Format.md`; `DACK-Live-Capture-and-Understanding-Plan.md`

## Context

Prototype documentation used “Snapshot” for several useful but different things: a captured image, analysis results, placed objects/rules, the damaged clone, and sometimes a whole saved level. That ambiguity makes immutability, refresh, OCR enrichment, undo, packaging, and cache invalidation impossible to specify consistently.

The Intake Workbench adds another requirement: a creator can draw grids, regions, edges, or seed points before a detector has emitted any region to attach them to. Gameplay likewise needs mutable active/deleted/damaged state without editing the immutable analysis that explains where a region came from.

## Decision

DACK uses these distinct products:

| Product | Owner and mutability | Contains | Excludes |
| --- | --- | --- | --- |
| `SourceDescriptor` | Session/platform; replaceable | local identity, permission, capabilities, app/window/monitor hints | captured pixels, gameplay |
| `SourceFrame` | Capture backend; immutable; often ephemeral | one coherent acquisition, exact pixels, sequence/time/color/coordinate metadata | analysis, creator edits |
| `SnapshotBaseline` | Snapshot repository; immutable | admitted native pixels, capture metadata, content hash, stable Snapshot ID | placed objects, active damage, current OCR progress |
| `IntakeRecipe` | Level authoring; versioned/undoable | independent guides, grids, polygons, edges, seeds, exclusions, labels, source transform | detector-owned regions |
| `AnalysisRevision` | Analysis cache; immutable/replaceable | derived regions/masks/backgrounds/profile for baseline hash + recipe hash + algorithm version | active/deleted/damaged runtime state |
| `LevelDefinition` | Persistence/application; authored | accepted corrections, Card definitions/references, instances/overrides, rules, routes, source bindings, mutation policy | transient score/positions/effects |
| `WorkingClone` | Mutation/renderer; mutable branch | visual composition derived from baseline plus committed mutation batches | original source authority |
| `RegionRuntimeState` | Environment/mutation service; mutable branch | active/deleted/damaged/presentation state keyed by region ID | detector evidence |
| `RunState` | Simulation; transient unless explicitly checkpointed | actor positions, score, lives, cooldowns, RNG state, transient effects | authoring data by default |
| `Variant` | Level authoring; immutable named revision once saved | mutation sequence, region-state delta, optional flattened tile/image cache | mutation of its parent baseline |
| `Session` | Application; authoritative working aggregate | open level, chosen revisions, clone/run state, UI/selection/history/jobs | a second copy per window |
| `Pack` | Package validator/publisher | validated levels, approved pinned/embedded assets, provenance, sanitized clones | raw local vault, originals, disposable caches unless deliberately embedded |

### Identity and revision rules

1. A new capture creates a new `SourceFrame`; admitting it creates a new `SnapshotBaseline` ID even when it resembles the old one.
2. Editing `IntakeRecipe` creates a recipe revision. Debounced analysis produces a candidate `AnalysisRevision`; explicit acceptance selects it for the level.
3. An `AnalysisRevision` cache key includes Snapshot content hash, recipe hash, detector schema/version, and relevant color/normalization profile.
4. Lazy OCR is an enrichment cache bound to an Analysis region ID plus provider/version/language. It may publish incrementally without mutating the Analysis geometry.
5. Current collision is resolved from immutable analysis + accepted corrections + creator objects + `RegionRuntimeState`; it is not serialized as if it were source geometry.
6. A refresh candidate has its own baseline, recipe mapping, analysis, and explicit lineage proposal. Applying it creates/selects new revisions; rejecting it cannot alter the active session.
7. `Promote Variant` creates a derived baseline/level revision with lineage. No operation overwrites an existing immutable baseline or named variant.
8. Creator-authored IDs and decisions persist. Detector regions may receive new IDs across a new analysis; a tested lineage/rebinding table relates old and new records.

### Persistence rules

- Canonical authored data: baseline references/embedded pixels, Intake Recipe, selected Analysis revision metadata, accepted corrections, Cards/instances/rules/routes/bindings, variants, and provenance.
- Disposable derived data: detector maps, OCR crops, thumbnails, spatial indices, flattened variant tiles, and render caches. A pack may embed validated caches for instant play, but the format marks them rebuildable.
- Checkpoint/run save is a separate optional record and never silently becomes the authored level default.

## Consequences

- “Snapshot” stops being a synonym for “everything currently open.”
- The Workbench can render guides immediately and reanalyze asynchronously without manufacturing detector regions first.
- OCR and improved analysis versions can be replaced without losing creator intent.
- Brickbat/Pinball/platformer mutations agree through Region Runtime State while Reset remains exact.
- The package format needs explicit directories/records for authored data versus caches and run/checkpoint state.
- Some current RAD save fields require migration into the correct product.

## Validation

1. Discarding a refresh or analysis candidate leaves the active baseline, clone, collision, level, and selection unchanged.
2. Reset reconstructs Working Clone and Region Runtime State exactly from the chosen baseline/variant policy.
3. Editing a grid guide before analysis is possible, undoable, saveable, and does not mutate source pixels.
4. Disabling/deleting all derived caches leaves a level reconstructable from canonical authored data.
5. A saved runtime checkpoint can be deleted without changing the authored level.
6. Promoting a variant records lineage and creates a new identity; the parent remains byte-stable.
