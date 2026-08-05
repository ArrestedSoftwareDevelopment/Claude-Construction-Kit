# ADR-0009: Shared Snapshot Analysis and Environmental Map

- **Status:** Accepted
- **Date:** 2026-07-30
- **Decision owners:** DACK project
- **Implementation companion:** [DACK Live Capture and Understanding Plan](../DACK-Live-Capture-and-Understanding-Plan.md)
- **Supersedes implementation details in:** ADR-0008
- **Ownership refinement:** [ADR-0012](ADR-0012-snapshot-analysis-clone-state-separation.md) preserves this shared-map decision while separating immutable analysis evidence from mutable region/run state.
- **Related:** Desktop Arena Design Document §§5.1–5.2.1, 7.6, 13, 15, 17–18

## Context

ADR-0008 proved the essential product idea: visible text in a captured document can become Platformer terrain before OCR or native file parsing. The prototype then reused captured letters and words for Brickbat, Pinball, projectiles, explosions, OCR prioritization, HUD placement, and cross-playset deformation.

The first dark-horizontal-band detector is too narrow for the product. Real screens contain anti-aliased and colored text, light subheaders, icons, pillboxes, borders, shaded regions, mixed backgrounds, dark mode, and creator-painted objects. Re-running separate scans or checking current pixels for every gameplay query also becomes prohibitively expensive on dense pages.

## Decision

DACK will create one cached **Analysis Revision** (called “Snapshot Analysis” in the original prototype vocabulary) and expose it through one shared **Environmental Map** for every captured/static source.

1. Snapshot sources render at native 1:1 pixels by default. Spare display area is nonphysical UI/HUD margin unless explicitly authored.
2. A primary analysis pass builds reusable luminance/chroma, local-background, ink/component, and region evidence from a managed pixel buffer.
3. Text/glyphs, words, lines, background/whitespace regions, icons, pillboxes, UI rectangles, and other candidates become stable records with IDs, bounds or masks, type, authority, confidence, source evidence, and background model.
4. UIA, OCR, manual painting, and later structured importers enrich or correct those same records; they do not create incompatible parallel worlds.
5. Source-derived analysis, accepted creator corrections, and runtime mutation state remain separate products that the Environmental Map resolves into one query view.
6. Immutable region evidence has no current gameplay state. A separate `RegionRuntimeState`, keyed by stable region ID, carries active/deleted/damaged state; erasure updates that overlay immediately and records a reversible mutation against the Working Clone.
7. Spatial indexing lets actors, balls, projectiles, AI, HUD placement, and OCR prioritizers query nearby candidates rather than scan the full page.
8. Collision and visual erasure reference the same region identities so a destroyed letter cannot remain as an invisible platform.
9. Pixel mutation is batched into bounded dirty regions. No full-image scan or blocking OCR/import work belongs in the gameplay frame loop.
10. Understand mode exposes the regions, evidence, authority, current state, and creator overrides. Manual authorship outranks automatic detection.

OCR remains optional. It attaches meaning to geometry but is not required for collision, mutation, save/load, or ordinary play.

## Consequences

### Positive

- All playsets share one consistent understanding of the document.
- Colored text, local backgrounds, icons, pillboxes, and dark-mode sources can evolve without toolkit-specific detectors.
- Cross-playset deformation is natural because every toolkit sees the same active region state.
- Dense documents become tractable through cached records and local spatial queries.
- Analysis caches can avoid re-analysis when the baseline/recipe hashes and algorithm version are unchanged.
- Creator corrections are visible, durable, and reusable.

### Tradeoffs

- The analysis format must version its algorithm and region records independently of the Snapshot Baseline.
- Stable IDs, reconciliation, split/merge behavior, and recapture diffs require deliberate design.
- Local-background and anti-alias classification will still make mistakes; Understand mode and manual correction remain required.
- Live Desktop mode needs bounded incremental updates and debounce policies rather than rebuilding the entire map on every changed pixel.

## Validation

The decision is validated when:

1. one native-resolution page supplies letters, words, lines, icons/pillboxes, whitespace, and platform surfaces from one cached analysis;
2. colored/light text and mixed backgrounds remain detectable;
3. deleting a region removes both its pixels and collision in every active playset immediately;
4. an actor/ball/projectile query examines a small local candidate set rather than all page regions;
5. Reset restores both the original clone pixels and region-active state exactly;
6. creator corrections survive save/load and appear distinctly in Understand mode.
