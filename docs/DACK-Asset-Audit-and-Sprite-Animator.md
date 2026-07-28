# DACK Asset Audit and Sprite Editor / Animator Plan

## Purpose

This note audits the current asset folders and turns the findings into a practical plan for DACK's sprite editor, animation module, and asset shelf.

The core distinction:

- `raw base assets/` is a local, ignored asset vault for evaluation.
- `dack/assets/quarantine/` is ignored license-pending material.
- `dack/assets/third_party/` is the curated runtime area for assets with recorded provenance.
- DACK-created procedural/editor assets are the safest redistributable defaults.

## Current Repository Asset State

### Runtime-admitted assets

These are present under `dack/assets/third_party/` and recorded in `dack/assets/ASSET_PROVENANCE.md`.

| Asset | Status | License | Current use |
| --- | --- | --- | --- |
| 8-Bit Dungeon Tile Set Free | Approved subset admitted | CC0 1.0 | Brick/platform/window prototype scenery |
| Stickman Pack thin sheets | Approved subset admitted | CC BY 4.0, paid download terms noted | Platformer idle/run/jump animation and sprite-pad seed |

### Quarantine

`dack/assets/quarantine/` contains local-only material. It is ignored by Git and must not be included in exports, builds, hub packages, or commits unless promoted through provenance review.

### Raw asset vault

`raw base assets/` is ignored by Git and contains a large evaluation collection. It is useful for design, taxonomy, and workflow testing, but not automatically usable in the runtime.

Top-level raw packs observed:

| Pack | Files | Approx size | Dominant types | Immediate value |
| --- | ---: | ---: | --- | --- |
| 8-Bit-Dungeon-Tiles | 278 | 18 MB | EPS, PNG, SVG | Already partially admitted; useful RPG/platformer/toolkit icons |
| all_64c | 615 | <1 MB | `.64c` | Retro/source research; needs format understanding |
| all_spr | 47 | <1 MB | `.spr` | Retro sprite research; needs format understanding |
| explosion pack 1 | 263 | 1.6 MB | PNG, GIF, PDF | Candidate effects reference, license present |
| explosion tutorial files | 16 | tiny | PNG, GIF, ASE | Candidate effect animation learning set, license present |
| Knight | 14 | tiny | PNG | Candidate character/motion reference; license unknown from local audit |
| Legacy Collection | 5,055 | 33 MB | PNG, GIF, PSD, ASE | Huge animation/effects/reference trove; license PDFs present |
| MountainDuskGodot | 384 | 9 MB | Godot import/cache files, PNG | Likely imported sample project; needs cleanup/intent review |
| PinBall_By_VerzatileDev | 64 | ~1 GB | PNG, TXT | Pinball kit reference; very large sheets; CC BY 4.0 + no standalone redistribution |
| Props | 4 | tiny | PNG | Candidate shelf props; license unknown from local audit |
| Sprites | 231 | 1.8 MB | PNG, PSD | Candidate character animation reference; license unknown from local audit |
| StickmanPack-V0.1 | 12 | tiny | PNG, GIF | Approved source for current stickman subset |
| StickmanPack-V0.2 | 8 | tiny | PNG | Better current stickman animation source; should be promoted after provenance update |
| The Game Creator's Pack | 44 | 161 MB | WAV, MP3, PNG | Candidate audio/UI/game kit reference; license unknown from local audit |
| Warped shooting fx files | 276 | 16 MB | PNG, ASEPRITE, GIF, PDF, MP3 | Candidate projectile/effects animation source, license present |

## Important Observations

### 1. The raw vault is animation-heavy

Observed raw animation/source counts:

- PNG: 4,015
- GIF: 439
- PSD: 177
- `.ase`: 107
- `.aseprite`: 56

This argues for an asset catalog and animator module, not just a pixel editor. DACK should be able to inspect/import sprite sheets and animation clips while keeping the live-linked pad small and constrained.

### 2. Stickman is ideal for the RAD animation ladder

Current admitted stickman dimensions:

| File | Dimensions | Meaning |
| --- | ---: | --- |
| `thin-idle-sheet.png` | 384×64 | 6 frames of 64×64 |
| `thin-run-sheet.png` | 576×64 | 9 frames of 64×64 |
| `thin-jump-sheet.png` | 128×128 | likely 2×2 or composite jump sheet |
| `thin-jump-up.png` | 64×64 | single jump-up frame |
| `thin-jump-down.png` | 64×64 | single jump-down frame |

StickmanPack V0.2 adds useful fuller coverage:

| File | Dimensions | Meaning |
| --- | ---: | --- |
| `Full.png` | 384×384 | combined sheet |
| `Run.png` | 576×64 | 9 frames |
| `Idle/Thin.png` | 384×64 | 6 frames |
| `Jump/Jump.png` | 128×128 | jump sheet |
| `Death/Death.png` | 192×192 | death animation sheet |
| `Punch/Punch.png` | 256×192 | punch animation sheet |

Recommendation: promote the V0.2 stickman files into `third_party/stickman-pack-v0.2/` after updating provenance. They are small, legible, and directly useful for idle/run/jump/punch/death animation states.

### 3. Pinball assets are too large for naive runtime admission

The largest VerzatileDev pinball PNGs are 3937×3937 and roughly 118 MB each. They are excellent for taxonomy and visual design, but should not be dragged whole into the Godot runtime shelf.

Recommendation:

- Keep the raw pack in the ignored vault.
- Curate small runtime slices only: flipper, bumper, plunger, lane marker, insert, rollover, drain, ball, gate, ramp segment.
- Downsample/slice into a DACK-specific kit sheet.
- Preserve attribution and no-standalone-redistribution restrictions.
- Treat full-size files as local reference, not distributable shelf assets.

### 4. Effects packs suggest an animation importer path

Explosion and shooting FX packs include PNG sequences, GIFs, `.ase`, and `.aseprite`. These are ideal for validating:

- frame-sequence import;
- GIF-to-clip preview;
- Aseprite/ASEPRITE metadata handling;
- effect clip timing;
- origin/pivot markers;
- additive/glow palette presets;
- reusable effects deck integration.

Do not admit broadly until license/provenance is written pack-by-pack.

### 5. Unknown-license packs stay design-only

`Knight`, `Props`, `Sprites`, `The Game Creator's Pack`, `MountainDuskGodot`, `all_64c`, and `all_spr` need explicit provenance before any curated runtime admission. They may still inform editor/animator design locally.

## Asset Governance Rules

1. **Raw is not runtime.** Nothing in `raw base assets/` ships or enters hub packages by default.
2. **Quarantine is never distributable.** Quarantined files are local-only until promoted.
3. **Promotion requires provenance.** Every promoted asset needs source, creator, license, admitted file list, intended use, and attribution string where applicable.
4. **Prefer curated subsets.** Admit the smallest useful runtime slice, not whole packs.
5. **Separate source files from runtime files.** PSD/ASE/ASEPRITE can be local source references; runtime should prefer PNG sheets/atlases plus DACK metadata.
6. **Preserve transform history.** If DACK slices/downsamples/recolors an asset, record the source and transformation.
7. **No standalone redistribution traps.** Packs with no-standalone-resale terms should be embedded only as functional game/editor assets, not exposed as raw downloadable asset libraries.

## Sprite Editor / Animator Boundary

DACK should have two cooperating art tools:

### Live-linked sprite pad

Primary purpose: fast in-context edits.

Scope:

- 24×21, 32×32, and 64×64 profiles.
- Small palette.
- Transparent color.
- Pencil, eraser, fill, line, picker, mirror.
- Undo/redo.
- Shared-type vs per-instance fork.
- Immediate update on the playfield.

This is the construction-kit toy: quick, playful, and bound to the selected actor.

### Sprite animator/catalog module

Primary purpose: manage sheets, clips, timing, origins, and external art.

Scope:

- Import sprite sheet.
- Slice by grid, manual rectangles, or metadata.
- Define animation clips: idle, run, jump, crawl, shoot, punch, climb, slide, hurt, death, custom.
- Set frames-per-second and loop mode.
- Set per-frame duration overrides.
- Set origin/pivot.
- Set visual bounds.
- Set collision/hitbox/attachment points separately from art.
- Preview against the real playfield scale.
- Assign clip to motion state or toolkit behavior.
- Refresh from external PNG/JSON export.
- Record source/provenance.

This module should not turn the live pad into Aseprite. It sits beside the pad as the organizer/importer/previewer.

## Recommended DACK Animation Data Model

```text
SpriteAsset
  id
  name
  provenanceId
  sourcePath
  runtimeTexture
  transparentColorPolicy
  canvasProfile
  clips[]

AnimationClip
  id
  name
  motionState
  framesPerSecond
  loopMode
  frames[]

AnimationFrame
  sourceRect
  durationOverride
  origin
  visualBounds
  collisionProfileRef
  hitboxRefs[]
  attachmentPoints[]
```

Key rule: **sprite pixels and gameplay geometry remain separate.** Editing art never silently changes collision.

## Animator UI Proposal

The animator belongs in the right inspector / bottom tray, not as a permanent large window.

Recommended panes:

- **Asset browser:** approved, quarantined, project-created, recently used.
- **Clip list:** idle/run/jump/etc. with small loop previews.
- **Frame strip:** reorder, duplicate, delete, adjust duration.
- **Preview stage:** selected actor at current playfield scale, with onion-skin and checkerboard options.
- **Binding panel:** motion state, behavior state, projectile/effect hook.
- **Geometry panel:** origin, bounds, collision profile, hitboxes, attachment points.
- **Source/provenance panel:** creator, license, source pack, attribution, runtime-admitted status.

The live pixel pad should appear as an edit mode for an individual frame or small sprite profile.

## Import Pipeline

### Stage 1: Current RAD path

- Hardcoded stickman sheets.
- Near-white transparency.
- Uniform square frame slicing.
- Idle/run/jump states.
- 32×32 live pad seed from first frame.

### Stage 2: Sheet manifest path

Add `.dackanim.json` beside curated sheets:

```json
{
  "id": "stickman-thin",
  "texture": "thin-run-sheet.png",
  "frameSize": [64, 64],
  "transparentColor": "#FFFFFF",
  "clips": {
    "run": { "frames": [0,1,2,3,4,5,6,7,8], "fps": 12, "loop": true }
  }
}
```

This gets rid of hardcoded assumptions in `SpriteAnimationSet`.

### Stage 3: External editor refresh

- Aseprite bridge imports PNG + JSON exported by Aseprite.
- DACK does not embed or redistribute Aseprite.
- Refresh preserves DACK bindings, origins, hitboxes, and behavior assignments where possible.

### Stage 4: Source-format research

- `.ase`/`.aseprite` direct reading is a research item only if licensing and implementation are clean.
- PSD import remains out of scope until the broader importer sandbox exists.
- GIF import can be useful for simple effect previews but should normalize to sprite clips.

## Immediate Recommendations

1. Keep the current admitted runtime set small.
2. Promote StickmanPack V0.2 as the next curated animation source after provenance update.
3. Create a `dack/assets/project/` or `dack/assets/dack_builtin/` area for DACK-created sprites/effects.
4. Add `.dackanim.json` manifests for stickman v0.1/v0.2.
5. Replace hardcoded `SpriteAnimationSet.TryLoadStickman()` sheet assumptions with manifest loading.
6. Add an Asset Catalog service that separates approved / quarantined / raw-local / project-created assets.
7. Add a minimal Animator inspector: clip list, frame strip, FPS, loop, origin, preview.
8. Keep the live pad as the in-context frame/sprite editor, not a full animation suite.
9. Curate only small pinball parts from the huge VerzatileDev sheets when the pinball shelf begins.
10. Do not admit unknown-license packs until provenance is explicit.

## Product Feeling

DACK's art workflow should feel like:

- quick edits happen directly on the selected actor;
- serious animation can come from external tools;
- imported sheets become understandable clips;
- every asset carries its license/provenance;
- creators can always see whether they are editing a shared sprite, an instance fork, or a runtime-only clone.

The sprite pad is the toy. The animator is the librarian and stage manager. Both are needed.
