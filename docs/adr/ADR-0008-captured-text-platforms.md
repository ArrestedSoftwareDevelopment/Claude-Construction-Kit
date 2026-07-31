# ADR-0008: Captured Text as Baseline Platform Terrain

- **Status:** Accepted for the initial RAD; generalized by ADR-0009
- **Date:** 2026-07-26
- **Decision owners:** DACK project
- **Related:** Desktop Arena Design Document §§5.2, 6.1, 18

## Context

DACK's platformer kit should make an ordinary document playable without requiring the creator to place ladders, ramps, elevators, or collision overlays first. Those authored objects are important editor features, but the first proof must validate the stronger idea: if text is visible in the cloned playfield, the actor can treat it as terrain.

The initial RAD used a captured screenshot as a read-only background clone. It did not mutate the screenshot or the original application/document. A simple image-processing pass identified dark horizontal text bands and exposed them as standable surfaces before OCR, UI Automation text ranges, or native document import existed.

ADR-0009 preserves this decision’s product promise—visible captured text is immediately playable—but generalizes the implementation from dark bands to one contrast/background-aware, stable, shared environmental model.

## Decision

DACK will treat captured text bands as the baseline platformer terrain for snapshot playfields.

1. The import module scans the cloned framegrab for dark horizontal text bands.
2. Detected bands become basic standable platform surfaces.
3. This pass is OCR-free and semantic-light; it exists to make the visible words playable.
4. Ladders, ramps, slides, conveyors, elevators, checkpoints, triggers, and patrol paths remain editor-authored world objects layered on top.
5. All detected geometry is considered provisional and must be editable in the level editor.
6. The original screenshot/source is never modified.

## Consequences

- A captured page can produce a playable platformer immediately.
- Character scale and motion speed must be tuned for text-sized terrain, not tile-sized terrain.
- The detector will produce false positives and missed lines; editor correction is a required product feature, not a patch later.
- Future UIA/OCR/native-document passes can replace or enrich this detection, but they should emit the same region/platform vocabulary.
