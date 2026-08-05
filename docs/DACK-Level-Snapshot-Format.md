# DACK Level Snapshot and Package Format

- **Status:** Normative persistence contract; Version 1 remains provisional until the first complete package round-trip
- **Canonical editable format:** `.dacklevel`
- **Related engineering plan:** [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md)
- **State authority:** [ADR-0012](adr/ADR-0012-snapshot-analysis-clone-state-separation.md)
- **Card/dependency authority:** [ADR-0014](adr/ADR-0014-card-definition-instance-and-dependency-contract.md)

## Purpose

`Snapshot` is the author-facing word for freezing an immutable native-pixel baseline at the moment it feels right. The Level—not the Snapshot—owns the creator's Cards, placed instances, rules, routes, accepted corrections, and mutation policy. The current mutable branch is the Working Clone; a saved damaged/remixed branch is a Variant.

The creator can tune the source document, window, desktop region, image, or text map in its native app; DACK then captures a stable clone, reads what it can in the background, and packages the resulting image, geometry, OCR labels, placed tools, rules, assets, and mutation state into a level file.

This turns the current proof-of-concept loop into an intentional workflow:

1. Tune the source until the page/screen has the right visual shape.
2. Capture a Snapshot.
3. Optionally author an Intake Recipe, then let DACK produce a versioned Analysis revision containing text, regions, background color, icons, pillboxes, and possible bonus anchors.
4. Optionally run Word Sense/OCR in the background.
5. Place ladders, ramps, elevators, triggers, enemies, routes, pinball parts, score panels, and other toolkit objects.
6. Test and deform the clone through play.
7. Save the pristine Snapshot, the deformed variant, or both.
8. Export a shareable pack.

The key promise: the original source remains untouched. DACK always works on a clone.

## Normative Version, Identity, and Coordinate Contract

This section is the authoritative contract for saved levels. Prototype save code may temporarily implement only part of it, but new persistence work should converge here rather than create another level format.

### Canonical container and version

- `.dacklevel` is the one canonical editable/interchange level format. The public saved form is a ZIP-compatible package with a constrained internal layout. The editor may use an expanded transactional work/cache directory, but that directory is an implementation detail, not a second format. The current `rad-test.dacklevel.json` file is a migration fixture.
- `.dackpack` is a ZIP-compatible publishing/campaign container that embeds canonical `.dacklevel` packages or their normalized contents; it does not redefine level semantics.
- Every root manifest must contain `format`, `formatVersion`, and a stable level `id`.
- `formatVersion` versions the serialized schema. `dackVersion` records which application build wrote it; these fields must not be used interchangeably.
- Readers must migrate older known versions deliberately. A newer unsupported major format must produce a clear compatibility error rather than being guessed at or partially loaded.
- Save/migrate operations preserve the last good file until the replacement has validated and completed atomically.
- Save assembles and validates a temporary package on the same target volume, then atomically replaces/renames the previous package and retains a bounded recovery backup. Lightweight autosave writes a recipe/command recovery record and references immutable content-addressed Snapshot blobs rather than rebuilding a large ZIP on every edit.
- Package readers reject absolute paths, `..` traversal, links/reparse escapes, duplicate canonical paths, excessive entry counts, per-entry/total expansion beyond declared limits, and decompression-ratio/archive-bomb abuse before materializing files.

### Stable identity

- Levels, snapshots, geometry regions, words, objects, actors, triggers, routes, cards, assets, effects, and mutations receive stable IDs at creation.
- IDs are immutable within the lifetime of an authored item and remain unchanged when lists are reordered, files are re-saved, or display names change.
- References use IDs, never array positions, display labels, pixel hashes alone, or source filenames alone.
- A duplicated item receives a new ID and may record `derivedFromId`; it must not reuse the source item's identity.
- Snapshot hashes verify content, while snapshot IDs preserve authored identity. They serve different purposes.
- Algorithm-derived geometry should also record the detector/algorithm version so a re-scan can be distinguished from the geometry the creator approved.

### Native snapshot coordinate space

- All persisted playfield geometry uses `snapshot-pixels`: origin at the frozen image's top-left, positive X to the right, positive Y downward.
- One saved coordinate unit equals one pixel in the Snapshot image. A point `[604, 318]` addresses that exact native-resolution pixel location.
- `pixelSize` defines the bounds of the coordinate space. Stored rectangles, endpoints, paths, collision masks, mutations, OCR boxes, actor positions, and authored handles all share it.
- `dpiScale` records how the source/display was captured. It is metadata for reconstruction and physical-size reasoning; it must not silently multiply or divide saved coordinates.
- Runtime window fitting, monitor placement, zoom, camera movement, letterboxing, and editor magnification are view transforms only. They never rewrite canonical level coordinates.
- Input is converted from display coordinates through the inverse view transform before selection, dragging, collision editing, or placement.
- Imported sprite-local coordinates remain local to their asset/card. The placed actor or object supplies the transform into `snapshot-pixels`.
- If a capture is intentionally resampled, it becomes a new Snapshot with a new `pixelSize`, ID, and explicit transform/rebinding record. DACK must not label a stretched image as the original Snapshot.

This 1:1 rule protects the central visual promise: document text stays at native clarity while gameplay, collision, OCR, and mutation agree on exactly where every pixel lives.

## Snapshot vs. Source

A Snapshot Baseline is not the original file. It is DACK's frozen, playable pixel representation of the source at a specific time, resolution, capture rectangle, and approved normalization. `IntakeRecipe` and `AnalysisRevision` are separately versioned because creator guides and detector algorithms can evolve without rewriting those pixels.

For sharing, DACK should support three source policies:

- **Frame-only source policy:** packages the frozen playable baseline and the separate Level/Analysis records needed to play it, without including the originating document. This is the safest default and works even when the source was a proprietary document, browser page, app window, or desktop.
- **Scrubbed source clone:** optionally includes a copy of an approved open or supported source file after metadata scrubbing and preview. This is useful for editable remix packs, but it is never required for play.
- **External source reference:** stores provenance only, such as "captured from Word window" or a creator note. This may help the author rebuild locally, but shared play still uses the Snapshot image.

Hub publishing scrubs supported metadata 100% as a mandatory policy, with no creator override. If a source clone is included, it must be a DACK-created clone, never the original, and the export UI must make clear exactly what is being shared.

## Editable Level vs. Published Pack

Use two related containers:

- `.dacklevel` for the canonical editable creator project or single level.
- `.dackpack` for a distributable playset/campaign bundle.

Both have constrained ZIP-compatible public containers. Expanded directories are allowed only as editor workspaces/developer fixtures and must round-trip through the same validator.

Suggested editable layout:

```text
MyLevel.dacklevel/
  manifest.json
  snapshots/
    snapshot-0001/
      baseline.json
      image.png
      intake-recipe.json
      analysis/
        revision-0001.json
        words-cache.json
        masks/
  level/
    corrections.json
    initial-region-overrides.json
    mutation-policy.json
  placed/
    objects.json
    actors.json
    triggers.json
    routes.json
  rules/
    toolkit.json
    events.json
    ai.json
  assets/
    manifest.json
    sprites/
    effects/
    audio/
  sources/
    source-manifest.json
    clones/
  provenance/
    licenses.json
    attribution.txt
  variants/
    variant-0001/
      variant.json
      flattened-cache/
  recovery/
    checkpoint.json        # optional run checkpoint; not authored level truth
```

`initial-region-overrides.json` is authored start-state policy, not a serialized copy of the mutable `RegionRuntimeState`. A named Variant stores an explicit mutation/region-state branch; transient current-run state belongs only in an optional checkpoint.

Suggested published layout:

```text
MyPlayset.dackpack/
  manifest.json
  player/
  levels/
    level01.dacklevel/
    level02.dacklevel/
  shared-assets/
  provenance/
    licenses.json
    attribution.txt
```

The package should be playable from the baseline plus canonical Level data and approved embedded assets. Derived Analysis/OCR/flattened-tile caches may be embedded for instant play but are marked rebuildable. Included source clones are an optional remix/editing feature.

## Level Cards and Multi-Level Design

The card model should extend all the way up.

In DACK, a card is any reusable authored unit. That includes tiny ingredients like sprites and sounds, mid-sized composites like enemies and spawners, and large structures like levels, worlds, and campaigns.

Useful hierarchy:

```text
Ingredient Cards
  sprite, animation labels, sound, effect, projectile, AI, physics, text rule

Object Cards
  player, enemy, pickup, obstacle, flipper, tower, semantic word-object

Logic Cards
  checkpoint, hidden switch, enemy spawn point, route, wave, win/loss rule

Level Card
  Snapshot/Analysis references, accepted corrections, placed instances, rules, routes, Variant policy, music/soundscape, scoring

World / Chapter Card
  ordered or mapped set of level cards, shared theme, shared enemy pool, shared progression rules

Playset / Campaign Card
  title, mode family, world map, level order, shared assets, unlocks, scoring, publishing policy
```

This means the creator can build upward naturally:

1. Build a `Blue Guard Enemy Card`.
2. Drop it onto an `Enemy Spawn Point Card`.
3. Use that spawner inside a `Memo Climber Level Card`.
4. Put several level cards into an `Office Tower World Card`.
5. Publish the whole thing as a `Corporate Dungeon Playset Card`.

Level Cards should be draggable/selectable in the same spirit as smaller cards. A multi-level builder can present them as a shelf, flowchart, map, list, or board depending on game type:

- Side-view platformers: linear route, branching map, vertical tower, elevator bank.
- Brickbat: sequence of document boards, challenge pack, puzzle set.
- Pinball: table set, missions, wizard-mode board, score attack ladder.
- Overhead/RPG: rooms, floors, overworld nodes, dungeon levels.
- Racing: track list, cup, route variants.
- Tower Defense / Escort: wave stages, convoy routes, escalation schedule.

The creator-facing rule is the same as for enemies:

> Build something from cards; when it works, save the result as a bigger card.

For implementation, `.dacklevel` is the serialized Level Card package. A `.dackpack` is the validated distribution container that may include one Playset/Campaign Card, one or more Level Cards, a lightweight Player, shared dependencies, provenance, and publishing policy; the container is not itself a Card Definition. A later `.dackworld` or world section inside `.dackpack` can group levels without forcing every project to become a campaign.

### PageSequence: one document, many levels

When the source is a multi-page Word, Writer, PDF, or browser document, DACK may create a `PageSequence`. The sequence preserves document order and owns shared assets, progression, transition, and persistence rules; each page remains an ordinary Level Card that references its immutable Snapshot Baseline/selected Analysis Revision and owns its placed objects, routes, rules, corrections, and Variant policy.

```json
{
  "documentId": "doc-001",
  "sourceVersion": "capture-0007",
  "pages": [
    { "pageId": "page-001", "ordinal": 1, "snapshotId": "snapshot-001", "nativeSize": [1920, 1080] },
    { "pageId": "page-002", "ordinal": 2, "snapshotId": "snapshot-002", "nativeSize": [1920, 1080] }
  ],
  "transitionPolicy": { "default": "goal-or-portal", "allowEdgeExit": false },
  "persistencePolicy": { "player": "sequence", "score": "sequence", "mutations": "page" }
}
```

The default runtime shows one page at native resolution at a time. A page may also define a camera viewport for horizontal or vertical scrolling; scrolling changes the view transform, not Snapshot coordinates or source pixels. `Next Page`, `Previous Page`, goals, portals, and edge exits are all transition types. Page analysis and lazy OCR can preload the next page in the background.

Stable page IDs are matched across scroll recaptures or re-snapshots using source hashes, page/layout signatures, text geometry, and ordinal hints. Uncertain matches are flagged for creator review. A page-local mutation (such as a Brickbat erase) stays on that page by default; a sequence-global rule (score, inventory, unlock, or shared objective) is explicit. The original document is never edited, and a published package remains playable from frozen page Snapshots alone.

## Manifest Fields

`manifest.json` should answer the broad questions quickly:

```json
{
  "format": "dacklevel",
  "formatVersion": 1,
  "id": "level-office-climber-001",
  "title": "Memo Climber",
  "dackVersion": "0.1-rad",
  "createdAt": "2026-07-28T00:00:00-04:00",
  "modifiedAt": "2026-07-28T00:00:00-04:00",
  "author": {
    "displayName": "Creator"
  },
  "toolkitDefault": "platformer",
  "activeSnapshot": "snapshot-0001",
  "sourcePolicy": "frame-only",
  "publishPolicy": {
    "metadataScrubbed": true,
    "includesSourceClone": false
  }
}
```

For `.dackpack`, the manifest also lists levels, shared assets, campaign order, and runtime requirements.

## Snapshot Baseline Record

Each baseline stores how the immutable pixels were admitted and what coordinate system all level data uses. OCR status, Analysis revisions, placed content, and active damage do not belong in this record.

```json
{
  "id": "snapshot-0001",
  "createdAt": "2026-07-28T00:00:00-04:00",
  "sourceFamily": "rich-text-document",
  "sourceAppHint": "Word/OpenOffice/TextPad/browser/etc",
  "captureMode": "window",
  "pixelSize": { "width": 1920, "height": 1080 },
  "dpiScale": 1.0,
  "coordinateSpace": "snapshot-pixels",
  "imagePath": "image.png",
  "snapshotImageHash": "sha256:...",
  "pixelFormat": "rgba8-srgb",
  "colorNormalizationProfile": "sdr-srgb-v1"
}
```

Snapshot Baselines are immutable once admitted. If the creator recaptures the source, DACK writes a new baseline and offers explicit guide/region lineage and placed-object rebinding. The Level manifest selects its Intake Recipe and Analysis revision; changing that selection does not imply the baseline bytes changed.

## Analysis / Geometry Record

An `AnalysisRevision` stores the engine's immutable understanding of the frozen image for one baseline hash + Intake Recipe hash + detector version. It stores derived geometry and proposed policies, not current mutable state.

Important categories:

- `textLetters`: per-letter collision/erase regions.
- `textWords`: word-level regions, with optional links to child letters.
- `textLines`: line/paragraph/subheading bands for platforming and traversal.
- `backgroundRegions`: sampled empty regions suitable for HUDs, score panels, shelves, or safe spawn zones.
- `iconRegions`: desktop icons, app icons, bullets, pillboxes, UI chips, thumbnails, and other non-text objects.
- `fixedBoundaries`: windows, taskbars, panels, gutters, rulers, sidebars, margins, and other environmental boundaries.
- `bonusAnchors`: suggested places for power-ups, score inserts, pinball lights, or semantic triggers.
- `collisionCandidates`: source-derived masks/proposed policies. The current playable collision view is resolved at runtime from Analysis + accepted corrections + creator objects + Region Runtime State.

The current proof of concept already points to this split: Brickbat erases text from the mutable clone and updates Region Runtime State; Platformer treats those inactive regions as holes, while Reset reconstructs the selected baseline/Variant exactly. Neither action edits the Analysis revision.

## Word Sense / OCR Record

`words.json` stores OCR results as optional upgrades, not as required geometry.

```json
{
  "words": [
    {
      "id": "word-0042",
      "rect": [604, 318, 72, 21],
      "text": "LADDER",
      "confidence": 0.91,
      "provider": "tesseract/libtesseract",
      "sourceRegion": "textWord-0042",
      "semanticTags": ["tool-candidate", "climbable"],
      "acceptedByCreator": false
    }
  ]
}
```

This lets a level ship with already-read words. End users do not need OCR installed for the authored level to play with word bonuses, word-goals, literary score tickers, or semantic hazards. OCR remains useful when the player captures new local material or the creator re-snapshots a changed source.

Design rule: OCR can add meaning, but the level must still play without it.

## Placed Objects

`placed/objects.json` stores creator-authored toolkit objects layered over the Snapshot.

```json
{
  "objects": [
    {
      "id": "ladder-001",
      "kind": "ladder",
      "start": [480, 710],
      "end": [480, 430],
      "thickness": 18,
      "orientationConstraint": "vertical",
      "presentation": "hybrid",
      "collisionProfile": "climbable",
      "sourceBinding": {
        "mode": "detached",
        "wordId": "word-0042"
      }
    },
    {
      "id": "elevator-001",
      "kind": "elevator",
      "start": [900, 820],
      "end": [900, 520],
      "speed": 64,
      "phase": 0.0,
      "collisionProfile": "moving-platform"
    }
  ]
}
```

The same schema should cover platformer ladders, ramps, slides, conveyors, elevators, checkpoints, invisible triggers, enemy spawns, spline routes, pinball rails, flippers, bumpers, racing start lines, tower-defense paths, and RPG room markers.

Toolkit starter geometry uses the same schema with `starterGenerated: true` and a toolkit/preset source. For Pinball this records the generated table shell, so **Clear Starter Shell** can remove only those objects and save/load can preserve the creator's decision not to regenerate them.

Endpoint editing is central. A placed object can be moved as a body or reshaped by dragging endpoints, and its saved record should reflect that directly.

## Generated Geometry and Motion Profiles

Generated authoring data is creator geometry, not a rewrite of the captured source. It belongs in the level package so a maze, route, or motion profile can be edited, reset, and reused across playsets:

```json
{
  "geometry": [
    {
      "id": "maze-001",
      "kind": "maze",
      "topology": "hex",
      "seed": 18421,
      "bounds": [120, 160, 980, 680],
      "entry": "cell-0003",
      "exit": "cell-0412",
      "source": "creator-generated"
    },
    {
      "id": "patrol-001",
      "kind": "bezier-path",
      "points": [[80, 640], [320, 420], [760, 500], [1100, 300]],
      "closed": false,
      "sampling": { "mode": "arc-length", "step": 8 }
    },
    {
      "id": "jump-profile-001",
      "kind": "parabola",
      "start": [420, 700],
      "end": [760, 500],
      "gravity": 980,
      "inertia": { "acceleration": 2200, "drag": 0.08, "braking": 1800 }
    }
  ]
}
```

Rectangular and hexagonal grids share one cell/neighbor contract. Path Finder consumes either grid cells or continuous route geometry and reports costs/blockers for editor overlays and AI. Curves and parabolas store handles or their source parameters, not only a baked point list, so creators can continue tuning them. A generated maze or path may use text-derived obstacles, placed objects, or both; the underlying source remains immutable.

## Mutations and Variants

The Level's authored mutation branch records intentional changes to the Working Clone and matching Region Runtime State:

- erased letters/words;
- laser cuts;
- projectile damage;
- pixel-level destruction;
- paragraph slants/warps;
- placed text-to-graphic transformations;
- pinball dents, lit inserts, or dropped targets;
- gameplay-created holes, bridges, and hazards.

For performance, DACK can store flattened variant tiles/image beside the mutation log as a disposable cache:

```text
variants/variant-0001/
  variant.json
  flattened-cache/
    tile-0003-0007.png
```

Creators should be able to choose:

- **Reset Working Clone:** discard current play damage and reconstruct from the selected baseline/Variant policy.
- **Save as Variant:** preserve the damaged clone as a new level state.
- **Promote Variant as New Baseline:** derive a new immutable baseline/level revision with lineage for further editing; never overwrite the parent Snapshot.

This formalizes the emergent feature where one game type deforms the page and another game type inherits it. Authoring mutations/Variants are distinct from transient `RunState`; saving a checkpoint does not silently rewrite the designed level.

## Snapshot Lifecycle in the UI

Snapshot lifecycle is global session behavior, not a family-owned page or duplicate shelf. The accepted shell routes it consistently:

- **File / Source:** Capture Snapshot and Re-snapshot/Refresh Source;
- **Build / Variant:** Reset Working Clone, Save Variant, and the deliberately weighty **Promote Variant as New Baseline** command;
- **Understand:** analysis layers, correction, Word Sense start/status, and source/analysis comparison;
- **File / Export:** Export Pack and the optional scrubbed **Include Source Clone** choice with an exact-content preview.

The context/status strip may show concise source/Snapshot age, dirty state, analysis/OCR progress, active Variant, and Hub-safe state, but it does not create another command implementation.

`Re-snapshot` is an explicit refresh transaction, not an automatic live update. DACK captures and analyzes a temporary candidate, shows a diff, and applies it only after the creator chooses `Apply as New Snapshot` or `Rebind and Apply`. Until then, the active Snapshot, clone, mutations, collision map, and gameplay remain unchanged. `Discard` removes the candidate without affecting the level.

The snapshot state should be visible but not fussy: `Draft`, `Frozen`, `Word Sense reading`, `42 words known`, `Damaged variant unsaved`, `Hub-safe`.

## Re-snapshot and Rebinding

Creators will tune source documents after testing. DACK should make this normal.

When a new Snapshot is captured from a changed source:

1. Keep the old Snapshot.
2. Detect the new geometry.
3. Match old and new regions by position, text shape, OCR label, line/paragraph similarity, and nearby anchors.
4. Preserve placed objects that map cleanly.
5. Flag uncertain bindings in the Inspector.
6. Never silently throw away a creator's ladder, route, trigger, enemy, or pinball mechanism.

This is the level-editor cousin of Live Document Mode: stable, deliberate, and creator-controlled.

## Recovery, Dependency, and Publishing Contract

### Autosave and crash recovery

- Autosave records the last validated Level DTO revision plus a bounded command/recovery journal; it references immutable Snapshot/content-addressed blobs instead of copying them.
- A drag/slider transaction enters recovery only when committed. Partially previewed values are discarded after a crash.
- Manual Save validates and atomically replaces the canonical `.dacklevel`; recovery never overwrites it silently.
- Cloud-synced or unavailable targets are treated as fallible: save to a same-volume temporary file, flush/validate, replace, and report conflicts without deleting the last local recovery.

### Card and asset dependencies

- Published packs pin or embed exact Card/asset versions and stable component-slot IDs. Dependency cycles are rejected with a readable chain.
- Built-in/third-party immutable Cards, project-owned editable Cards, and placed instance override patches are distinct. Unresolved or incompatible Cards load as visible disabled placeholders retaining their raw optional data.
- The Player resolves validated compiled assets only. A frozen-pack Player does not need raw-vault scanning, creator importers, capture, or OCR unless that capability is explicitly part of the pack/runtime profile.

### Publish sanitization and archive safety

- Hub publishing rebuilds from an allow-list of canonical records and admitted assets; it never zips the editor work directory wholesale.
- Scrubbing removes image/text-container metadata, source filenames/absolute paths, account/user names, window titles, machine/monitor identifiers, temporary paths, local recent-history, and nonessential provider diagnostics. Visible pixels/OCR text remain sensitive content and are shown in mandatory preview.
- Internal content hashes that could fingerprint a private source are omitted or replaced by pack-local integrity IDs where they are not required for runtime verification.
- The validator enforces canonical paths, archive expansion/entry/size limits, asset/license eligibility, executable-content rejection, and complete provenance before the package receives a Hub-safe status.

## Near-Term RAD Implementation

The first useful public-format proof does not need every campaign/publishing feature, but it must preserve the ownership model rather than bake the RAD fields into a misleading “Snapshot.”

Minimum viable `.dacklevel` package round trip:

- save one immutable captured baseline image and baseline manifest;
- save an independent Intake Recipe and selected Analysis Revision containing detected text/background geometry;
- save optional OCR labels from `LazyOcrService` as a versioned replaceable cache;
- save the Level Definition: accepted corrections, placed objects/actors, active family/preset settings, rules, Cards/overrides, and source bindings;
- save a named Variant or mutation/state delta when the creator deliberately preserves Brickbat/Platformer damage;
- validate, atomically replace, then load the same products back into the prototype scene without changing their ownership.

Once this round-trips, DACK has a real level format instead of a demo screenshot.

Current RAD `dacklevel` pass:

- The prototype writes `dack/levels/rad-test.dacklevel.json`.
- This is a deliberately small local test slot and migration fixture, not a competing format or the final package directory structure.
- It saves placed world objects, Start/Checkpoint/Goal markers, object attributes, visible actors/enemies, actor names, coarse animation source IDs, text scale, actor scale, playset mode, platformer mode, and gameplay toggles.
- Loading a level returns to Editor Mode by default. Entering Play Mode is an explicit test action that honors the Start Point, hides editor-only markers, clears transient shots, resets the player, and allows enemy collision/projectile rules to run.
- It does not yet package the frozen Snapshot Baseline, Intake Recipe, Analysis Revision/OCR cache, or named Variant data. That migration is the R2/R5 persistence work in the Optimization Plan, not an extension of the RAD JSON schema by accretion.
- Actor animation source IDs are currently pragmatic (`stickman-v0.1`, `tgc-player`, `sunny-dragon-fly`) and must become stable, versioned library asset IDs before hub publishing.
- Projectile/explosion assignments should also become stable IDs. The RAD catalog lives at `dack/assets/project/effects/projectile-effect-profiles.json`; future `.dacklevel` actor/weapon records should reference profile IDs such as `explosion-b-fireball-impact` rather than hardcoded textures.
