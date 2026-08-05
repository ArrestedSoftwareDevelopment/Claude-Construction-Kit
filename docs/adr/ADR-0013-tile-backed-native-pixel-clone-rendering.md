# ADR-0013: Tile-Backed Native-Pixel Clone Rendering

- **Status:** Accepted in principle; tile size/upload path must be measured
- **Date:** 2026-08-05
- **Decision owners:** DACK project
- **Related:** ADR-0009; ADR-0012; `DACK-Optimization-and-Refactoring-Plan.md`

## Context

DACK must preserve native document pixels while allowing a one-letter hit, a laser, an explosion, a long multi-page source, or a live dirty rectangle to update only the affected area. The RAD currently owns one large mutable image/texture and can upload or re-evaluate far more pixels than a small mutation requires.

One 3840×2160 RGBA buffer is about 32 MiB before the immutable baseline, mutable clone, capture/refresh staging, analysis maps, GPU texture, and caches. Long documents and stitched monitors can also exceed comfortable single-texture dimensions. A dirty-rectangle contract over one monolithic texture is therefore not sufficient as the durable design.

## Decision

The Snapshot baseline, Working Clone renderer, and analysis spatial index will share a tile address space.

```text
SnapshotPx
  -> TileKey(pageOrSurfaceId, tileX, tileY)
  -> immutable baseline tile
  -> optional mutable clone tile/version
  -> analysis bucket and region references
  -> dirty/upload state
  -> optional resident Godot texture
```

Rules:

1. Canonical geometry remains continuous `snapshot-pixels`; tiling is storage/rendering/indexing, not visible gameplay geometry.
2. Rectangles use a documented half-open convention `[left, top, right, bottom)`. Pixels are addressed by integer `(x, y)` indices; geometric sampling treats their centers consistently as `(x + 0.5, y + 0.5)`. Adjacent tiles may not double-own or omit boundary pixels.
3. The immutable baseline tile is byte-stable. A mutable tile is copy-on-write from its baseline or selected Variant.
4. A mutation service clips one committed mutation batch to affected tiles, updates Region Runtime State in the same transaction, then publishes dirty tile/version records.
5. At most one coalesced upload commit occurs per visible tile per render frame. Multiple hits merge before upload.
6. Only visible/preloaded tiles require GPU residency. Inactive page/long-document tiles can evict rebuildable GPU/render caches without losing authored state.
7. Texture filtering is disabled at authoritative 1:1 presentation. Fit/overview is a labeled view transform and must not rewrite source pixels or collision coordinates.
8. Tile size is a profiled setting, initially comparing 256×256 and 512×512. File/format semantics do not depend on the selected runtime tile size.
9. Analysis may use its own multiscale summaries, but region IDs and query bounds remain in Snapshot coordinates and resolve through the shared tile index.
10. Flattened Variant tiles are disposable accelerators unless a package explicitly embeds them; the mutation/region-state record remains the semantic authority.

### Upload implementation strategy

The first product implementation uses ordinary Godot textures per visible tile and CPU-authoritative clone pixels. This keeps analysis, mutation, save, and visual verification on one normalized buffer path. A Windows shared GPU texture or GDExtension path is considered only after measurement proves CPU staging/tile upload misses the live-capture budget.

## Consequences

### Positive

- Letter/word damage has bounded pixel and upload cost.
- Long documents, page sequences, stitched monitors, and camera views no longer require one giant resident texture.
- Memory ownership, eviction, mutation diagnostics, and dirty-area budgets become measurable.
- Capture dirty rectangles, analysis buckets, environment queries, and renderer invalidation share a useful spatial unit.
- Static source tiles do not redraw because actors, effects, or HUDs animate.

### Tradeoffs

- The renderer must avoid seams, filter bleed, incorrect clipping, and excessive draw calls.
- Effects or rotated source blocks spanning tiles need a composition rule and bounds expansion.
- Save/export may need to stream or compose tiles into a conventional image for interoperability.
- Small documents incur more objects/resources than a monolithic texture; batching and residency policy must be measured.

## Validation

1. A synthetic checker/grid rendered across tile boundaries has no gap, duplicate pixel, filter bleed, or collision discontinuity at 1:1.
2. Deleting one glyph dirties/uploads only its intersecting tiles and changes collision in the same transaction.
3. Reset reproduces the baseline byte-for-byte across all affected tiles.
4. Pan/zoom over a long document respects the memory residency cap and does not hitch on ordinary prefetch.
5. 1080p, 4K, mixed-DPI, and multi-page benchmark fixtures report CPU/GPU tile memory, dirty pixels, upload count, and visible residency.
6. A hidden second view suspends its rendering cost; a visible second view does not create another mutable clone.
