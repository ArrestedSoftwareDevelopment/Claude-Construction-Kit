# DACK Document Geometry and Pagination Plan

- **Status:** Planned design contract
- **Scope:** document-native transforms, attached gameplay geometry, and multi-page level flow
- **Related:** [DACK Live Capture and Understanding Plan](DACK-Live-Capture-and-Understanding-Plan.md), [DACK Level Snapshot and Package Format](DACK-Level-Snapshot-Format.md), [DACK GUI Architecture](DACK-GUI-Architecture.md)

## Why this matters

DACK should be able to make a paragraph behave like a Donkey Kong platform without pretending that the source document was a game map. The creator needs to rotate, slant, or otherwise deform a **clone-bound block of text**, attach ladders and routes to it, and still retain an exact mapping back to the source pixels and words.

The same model should make a long Word, OpenOffice, PDF, or browser document feel like a Bubble Bobble-style sequence: each page is a level, page order is preserved, and the document remains recognizable rather than being stretched into one blurry world.

The source is always read-only. Every transform, mutation, OCR result, and gameplay attachment belongs to the DACK clone or Snapshot.

## Design rules

1. **Transform the clone, not the original.** A rotated paragraph is a DACK variant with a mutation record; it never edits Word or the source file.
2. **Keep native pixels authoritative.** Rotation may create a larger bounding box, but DACK does not resample the source to fit a convenient viewport. The editor pans, clips, or uses unused space.
3. **Preserve a reversible source mapping.** Every transformed region keeps its source rectangle, local transform, inverse transform, stable region ID, and optional word/glyph IDs. Collision, erasure, OCR, selection, and rebinding use the same mapping.
4. **Separate appearance from gameplay surface.** A block can remain readable text, become a rasterized clone, or use a hybrid display while its collision surface is a line, oriented rectangle, glyph mask, or creator-authored outline.
5. **Attachments inherit deliberately.** A ladder, checkpoint, conveyor, spawn rail, or goal may follow a block transform, remain in world space, or be detached. The Inspector must show which choice is active.

## Rotatable document blocks

### Authoring unit

The primary unit is a `TextRegionGroup`: a paragraph, heading, line band, selected words, table row, or manually boxed region. It stores:

```text
TextRegionGroup
  id, sourceRegionIds, pageId
  localBounds, pivot
  Transform2D: position, rotation, scale, optional shear
  displayMode: text-preserving | hybrid | raster-clone
  surfaceMode: glyph-mask | oriented-block | baseline | authored-outline
  backgroundPolicy, antialiasPolicy, erasurePolicy
  attachmentPolicy: inherit | world-space | detached
```

The first implementation should support translation, arbitrary rotation, and uniform scale. Shear and non-uniform scale are useful for dramatic ramps but should be an explicit advanced control because they can damage legibility.

The editor shows a pivot, local X/Y axes, oriented bounds, source anchors, and a before/after ghost. Numeric angle entry and pixel nudging supplement direct handles. Rotation is recorded in degrees (or radians in the runtime API) and is never baked into a new source image unless the creator explicitly saves a variant.

### Donkey Kong-style platform recipe

The preset can be built entirely from document text:

1. Select a paragraph or line band.
2. Rotate it to a chosen slope, usually 10-35 degrees, and choose `baseline` or `oriented-block` collision.
3. Add a ladder card with two endpoint anchors. Ladders remain vertical climb volumes, with bounded width, even when they connect slanted platforms.
4. Add a `RollingSpawnRoute` to the upper platform. It follows the transformed local edge or an authored polyline/spline and points downhill by default.
5. Add a goal, checkpoint, gap, and optional rolling hazard. All can be duplicated and placed at arbitrary source-relative anchors.

The first runtime can approximate a slanted paragraph with an oriented capsule/box and a sampled edge. Later it can use glyph masks and local support probes for accurate feet-on-text behavior. A character standing on the block receives the block's local tangent and normal; gravity remains world-down unless the preset explicitly enables surface-relative gravity.

### Rolling enemy spawn points

`RollingSpawnRoute` is a reusable logic card, not a special Donkey Kong hard-code:

- route binding: transformed paragraph edge, authored polyline, Bezier, or parabola;
- direction: forward, reverse, downhill, ping-pong, or random;
- spawn count, interval, max active, speed, acceleration, and launch offset;
- actor card, projectile/effect card, radar range, collision response, and edge behavior;
- trigger: level start, timer, checkpoint, player proximity, word/icon event, or hidden switch.

Counts and intervals use small bounded values in the editor. The runtime enforces the active cap so a designer cannot accidentally create a CPU-heavy horde. Spawned actors inherit the route's current transform but remain ordinary draggable actor instances in Build mode.

## Coordinate and collision contract

All saved geometry remains in `snapshot-pixels`. A transformed block introduces one additional local space:

```text
source pixels -> region local pixels -> block Transform2D -> snapshot pixels -> view/monitor
```

Selection and collision convert the pointer or actor through the inverse transform. Erasing a letter first resolves the hit into source-local coordinates, then applies the mutation to the clone and invalidates only the affected transformed region. OCR boxes remain source-bound; their displayed boxes are transformed views.

Collision modes should be explicit:

- `solid-block`: fast oriented rectangle/capsule for early play;
- `text-mask`: glyph and antialias-aware mask for precise clearing and gaps;
- `baseline`: one-way support along the transformed line band;
- `climb-surface`: ladder/text climb rules with line-spacing and width thresholds;
- `visual-only`: visible art with no collision.

## Multi-page documents as levels

### Page sequence model

A source provider may expose a `PageSequence` even when DACK only has screenshots. Each page descriptor contains:

```text
PageSequence
  documentId, sourceVersion, order
  pages[]: pageId, ordinal, nativeSize, sourceHash, thumbnail, captureEvidence
  transitionPolicy, sharedRules, sharedAssets, persistencePolicy
```

Each page is an ordinary Level Card with its own Snapshot, environment map, OCR cache, placed objects, routes, mutations, and rules. The sequence owns the shared catalog and progression rules. Page IDs are stable across recapture when visual/layout matching is confident; uncertain matches are shown for creator approval.

### Bubble Bobble-style flow

The default page play mode is one page visible at a time at native resolution:

- `Next Page`, `Previous Page`, a goal, edge exit, or explicit portal advances the sequence;
- a page can be a full-screen room, a viewport into a larger page, or a vertical/horizontal scroll scene;
- scrolling moves the camera/viewport, never the source pixels; text is clipped, not stretched;
- the next page is analyzed and lightly preloaded in the background while the current page runs;
- page transitions can preserve player, score, inventory, lives, intensity, and unlocked cards according to the sequence policy.

The editor provides a page thumbnail strip and a `Open as Level` action. Creators can reorder pages, duplicate a page into a variant, assign a transition, and place page-local or sequence-global goals. A document's natural order is the default; branching maps are a later World Card feature.

### Capture and reconciliation

For Word/Writer/PDF/browser sources, the capture service should:

1. capture the window or selected region at native resolution;
2. detect page boundaries from whitespace, repeated margins, scroll position, scrollbar/page landmarks, and optional UI Automation evidence;
3. create or update page descriptors without requiring OCR;
4. run geometry and lazy OCR per page, prioritizing the current page and nearby transition targets;
5. reconcile a new scroll capture with known pages using source hash, layout signatures, text geometry, and ordinal hints;
6. cancel stale analysis when the source version changes and never mix results from different captures.

If a native document importer is unavailable, the screenshot path still works: the creator can mark page breaks, capture each page, or use a scroll recorder. A later structured importer may improve headings, paragraphs, layers, and reflow awareness without changing the page-level runtime contract.

### Persistence choices

- **Page-local mutation:** erasing a word or deforming a paragraph affects only that page.
- **Sequence mutation:** a rule can intentionally carry a change across pages, such as a score-wide unlock or a destroyed shared asset.
- **Reset sequence:** restore all pages from immutable Snapshot records.
- **Save variant:** preserve the current multi-page state as a named variant without replacing the pristine sequence.

The package remains playable from frozen page Snapshots alone. Source clones are optional, scrubbed DACK clones, and never the original document.

## Editor experience

The Build page adds two compact tools:

- **Transform Block:** select a text region, rotate/slant it, choose display/collision policy, and manage attachments.
- **Page Navigator:** thumbnail/order strip, current page card, transition target, page-local versus sequence-global scope, and `Re-snapshot` status.

Understand mode can reveal source bounds, transformed bounds, local axes, page IDs, OCR confidence, and collision masks. Play mode hides handles, spawn markers, and editor-only anchors while retaining their behavior. `F6` continues to toggle Build/Play; `Esc` returns to the Cockpit without changing the current page or mutation state.

## Delivery plan

### G0 - transformed text proof

Implement a clone-only `Transform2D` for one paragraph, oriented bounds, inverse hit testing, native-resolution rendering, save/load, and reset.

### G1 - attached geometry

Bind ladders, goals, checkpoints, gaps, and rolling spawn routes to transformed edges. Add downhill route sampling and collision regression fixtures.

### G2 - static page sequence

Add `PageSequence` and per-page Snapshot cards. Support a two-page fixture, thumbnails, ordered transitions, page-local mutations, and cross-page save/load.

### G3 - live scroll capture

Add Word/Writer/PDF/browser page-boundary detection, scroll recording, page reconciliation, incremental geometry analysis, and background OCR cancellation.

### G4 - multi-page playsets

Ship Bubble Bobble-style page traversal, optional camera scroll, sequence-global progression, page variants, and a creator-facing transition editor.

## Acceptance fixtures

- Rotate one paragraph by 15, 30, 45, and 90 degrees; text remains legible at native pixels and collision/erasure hit the same source letters.
- Attach a vertical ladder and downhill spawn rail to a slanted block; save/load preserves transforms, stable IDs, direction, and endpoints.
- Place two copies of the same enemy/spawn card on one page without pile-up; each instance remains independently draggable.
- Capture two or more Word pages, play page 1, transition to page 2, and return without losing score, chosen policy, or page-local mutations.
- Scroll a document and recapture it; stable pages reconcile, moved pages are flagged, and stale OCR never appears on the new page.
- No test writes to the source document, and no viewport fit operation changes Snapshot coordinates or text raster quality.

