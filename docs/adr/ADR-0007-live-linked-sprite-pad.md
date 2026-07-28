# ADR-0007: Live-Linked Constrained Sprite Pad

- **Status:** Accepted
- **Date:** 2026-07-26
- **Decision owners:** DACK project
- **Related:** Desktop Arena Design Document §§10.4, 11.1–11.3, 15, 18

- **Follow-up:** `docs/DACK-Asset-Audit-and-Sprite-Animator.md`

## Context

DACK needs an art-editing path that supports its construction-kit principle: creation should be as immediate and playful as gameplay. A reduced copy of a general pixel-art editor would create substantial scope while remaining inferior to dedicated tools.

The Stage 1/2 art ladder already favors readable stick figures and small sprites. C64-era sprite constraints—tiny fixed dimensions, deliberately small palettes, and strongly legible silhouettes—fit that direction. The important product opportunity is not nostalgia or hardware emulation; it is editing the selected actor in context and seeing each pixel change on the actual playfield immediately.

Aseprite remains valuable for serious animation and asset production, but its export/import workflow is necessarily a round trip. Its source and official builds are distributed under the Aseprite EULA, which restricts redistribution. DACK must remain an independent implementation and may not embed or redistribute Aseprite.

## Decision

DACK will make a **live-linked constrained sprite pad** the primary in-app sprite-authoring tool.

1. The pad lives in the editor sidebar and binds to the currently selected entity.
2. Every pixel edit updates the editor/playtest sprite on the next render update.
3. Initial canvas profiles are C64-like 24×21 and DACK 32×32 for new work, plus a constrained 64×64 compatibility profile for the RAD stick-figure sheets. They are product presets, not exact hardware emulation.
4. Each profile uses a small explicit palette with one transparent entry and nearest-neighbor zoom.
5. The initial toolset is pencil, eraser, fill, line, picker, mirror, palette slots, transparent preview, undo/redo, clear, and duplicate.
6. The UI always distinguishes editing a shared entity-type sprite from a per-instance sprite fork. "Edit this one" clones before mutation.
7. Sprite pixels and collision geometry remain separate. Art edits never silently reshape physics.
8. Layers, animation timelines, frame tags, masks, scripting, and broad image operations are excluded from the first pad.
9. Aseprite is the advanced external path for animation, layers, timing, tags, and sprite-sheet packing. DACK may import/refresh exported PNG and JSON through an optional bridge.
10. DACK will not copy or redistribute Aseprite code, binaries, UI assets, or protected implementation. Any reusable third-party module requires explicit compatible licensing and recorded provenance.

## Consequences

### Positive

- Art editing follows the same instant-feedback philosophy as live physics and AI tuning.
- Fixed scope makes a polished in-app tool achievable during the proof of concept.
- Small profiles reinforce DACK's stick-figure aesthetic and make creator output visually coherent.
- Casual creators can modify actors without leaving the level editor.
- Advanced artists retain a capable external animation workflow.

### Tradeoffs

- The first pad will not support complex animation production.
- Shared-versus-instance asset semantics require prominent UI and reliable cloning.
- Live texture updates need a fast, undoable asset pipeline and must avoid unnecessary disk churn.
- Mixing canvas profiles may complicate scaling and asset consistency; this remains a product decision.

## Validation

The RAD proof of concept succeeds when a creator can:

1. select a placed stick figure;
2. change several pixels in the sidebar;
3. see the change immediately on all intentionally bound actors;
4. fork one instance and edit it without changing siblings;
5. undo the edit;
6. save/reload the project with identical sprite, palette, transparency, and binding behavior.
