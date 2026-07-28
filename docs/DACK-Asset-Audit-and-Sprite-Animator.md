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
| The Game Creator's Pack | 44 | 161 MB | WAV, MP3, PNG | Rights confirmed by project owner; good first sprite importer/animation-editor use case |
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

### 5. The Game Creator's Pack is a useful importer proving ground

The project owner has confirmed rights for The Game Creator's Pack. Its `Graphic Pack` folder is a small, useful stress test rather than a huge runtime burden:

| File | Dimensions | Importer implication |
| --- | ---: | --- |
| `Player_DarkOutline.png` | 571x42 | horizontal strip with many variably spaced frames; blob/manual grouping needed |
| `Player_LightOutline.png` | 571x42 | same as above, palette/style variant |
| `Platformer_SpriteSheet.png` | 217x154 | mixed character/enemy/tile sheet; needs blob detection plus human clip grouping |
| `Shooter_SpriteSheet.png` | 256x128 | dense small sprite/effect sheet |
| `Shooter_SpriteSheet_C64.png` | 179x96 | constrained C64-flavored variant |
| `Shooter_SpriteSheet_NoPaletteSwaps.png` | 188x65 | reduced palette-swap sheet |
| `Potions.png` | 67x33 | loose object sheet |
| `Shooter_Boss_Sprite.png` | 64x64 | single sprite |
| `Shooter_Expansion_2.png` | 64x64 | small mixed sprite/effect sheet |
| `The Retro Puzzle Pack - Sample.png` | 101x77 | small puzzle/tile sample |

This argues that the first importer should not assume square frames or perfect grids. It should support:

- fixed grid slicing when the sheet is regular;
- automatic non-transparent blob detection for loose strips/sheets;
- manual frame rectangles for anything the detector gets wrong;
- clip grouping after detection, because detection finds pictures, not meaning.

`tools/prep_game_creator_graphics.py` is the local prototype for this path. It writes frame candidates and a manifest to ignored quarantine, giving us real data for the future animation editor without admitting the pack into runtime assets yet.

### 6. Unknown-license packs stay design-only

`Knight`, `Props`, `Sprites`, `MountainDuskGodot`, `all_64c`, and `all_spr` need explicit provenance before any curated runtime admission. They may still inform editor/animator design locally.

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

## Character Picker / Action Label Flow

The outer creator-facing workflow should be a Character Picker, not a raw file browser. A creator should be able to:

1. pick an existing character;
2. preview its action labels at the current playfield scale;
3. swap it into the player/enemy slot;
4. edit/fork it into a new character;
5. fine-tune the action labels and frame groups.

Starter action vocabulary:

- idle;
- run;
- turn;
- jump-up;
- jump-apex;
- fall;
- land;
- crawl;
- climb;
- climb-up;
- climb-down;
- dig;
- dig-up;
- dig-down;
- slide;
- shoot;
- run-shoot;
- jump-shoot;
- shoot-up;
- shoot-down;
- bounce / stomp-rebound;
- power-up / buff;
- hurt;
- death;
- special/custom.

Control note: vertical intent and jump intent must stay separate. Up is reserved for climbing, crawling, entering upward routes, and later digging/interaction tools. Jump should default to Space so platforming, climbing, and digging can coexist without fighting over the same key.

Imported packs will often need human labeling. Blob/grid detection can find frame candidates, but it cannot know which frames mean `turn`, `land`, or `shoot` without a creator pass. The first test picker can therefore ship with rough labels and an explicit “needs labeling” state. DACK should make that labeling feel like tuning a toy: click an action, scrub frames, drag frame cards into order, preview in-place, and save the resulting `.dackanim.json`.

The smallest useful version is a strip editor: show the detected frame strip, choose an action, then set start/end frame endpoints. DACK can preview the selected range immediately on the live actor. That gives us the essential labeling loop before we build the prettier timeline:

- select source strip/sheet;
- detect candidate frames;
- choose action label;
- set start/end endpoints;
- preview;
- adjust;
- save draft labels.

Frame display should use compact renumberable sequences. The first useful layout is rows of eight frames, each thumbnail labeled with its detected frame number. A creator-facing `number from` control lets the same detected sequence display as zero-based importer indices, one-based human labels, or another preferred sequence base without changing the underlying data. Highlighted ranges make it clear which frames currently belong to idle, run, jump, and later turn/land/shoot.

Labels themselves must be editable and expandable. The creator should not be trapped in our starter vocabulary. The strip editor needs:

- editable action names;
- editable start/end frame numbers;
- add-label support;
- a per-label ping-pong/reverse toggle, so a short range like three jump frames can preview/play as a five-frame forward-then-back motion;
- visual range highlighting for every label;
- save/export to `.dackanim.json`, including both creator-visible numbering and internal zero-based detected frame indices;
- later: delete/reorder labels, duplicate ranges, and bind labels explicitly to engine motion states.

Current RAD test:

- `STICKMAN` uses the admitted OctoPyte stick figure clips.
- `TGC PLAYER` uses The Game Creator's Pack `Player_DarkOutline.png` strip through local blob-detected frame loading.
- The TGC mapping starts rough, but the prototype exposes a visual 8-column frame strip plus editable label names, numeric endpoints, and ping-pong toggles. Recognized labels like idle/run/jump-up/jump-down drive the current player animation; extra labels such as run-shoot, jump-shoot, climb-up/down, dig-up/down, shoot-up/down, bounce, and death can be added and highlighted ahead of full engine binding.
- `SAVE TGC LABELS` writes `dack/assets/quarantine/game-creators-pack-graphics-prep/tgc-player.dackanim.json`, a local-only manifest that records the frame rectangles, displayed frame numbers, internal playback indices, editable label names, and ping-pong flags. This is the debugging lens for numbering/range mistakes.

Swinging vines/ropes should wait for the visible spline/Bezier tool family. A straight-line placeholder would teach the wrong feel. The desired version is an authored curve with draggable handles, visible swing arc, optional attach/detach points, and animation labels for grab, swing, release, and land.

Power-up animations can often be effect composites rather than separate sprite sheets. The cheap useful version is: play the actor's idle or current-state animation, then layer reusable DACK effects over it—rings, glows, color cycling, sparks, outline pulses, rotating text sigils, or Jeff-Minter-style neon bursts. This lets a creator define `Power Up`, `Shielded`, `Hasted`, `Poisoned`, `Charged`, or `Invincible` without requiring new hand-drawn frames for every character.

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

### Stage 2A: Blob-detected draft manifest

For irregular strips and mixed sheets, DACK should create a draft import manifest before the creator does any manual cleanup:

```json
{
  "source": "Player_DarkOutline.png",
  "slicing": "blob-detect",
  "frames": [
    { "rect": [0, 2, 28, 40], "origin": [14, 39], "tags": [] }
  ],
  "clips": {
    "unassigned": { "frames": [0], "fps": 8, "loop": true }
  }
}
```

The animation editor then lets the creator rename clips, reorder frames, set FPS, define origins, and bind clips to actor states. This gives us a safe import runway for packs that are visually obvious to a human but not machine-regular.

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
10. Use The Game Creator's Pack Graphic Pack as the first irregular-sheet importer test: blob-detect frames, generate a manifest, then hand-group clips.
11. Do not admit unknown-license packs until provenance is explicit.

## Product Feeling

DACK's art workflow should feel like:

- quick edits happen directly on the selected actor;
- serious animation can come from external tools;
- imported sheets become understandable clips;
- every asset carries its license/provenance;
- creators can always see whether they are editing a shared sprite, an instance fork, or a runtime-only clone.

The sprite pad is the toy. The animator is the librarian and stage manager. Both are needed.
