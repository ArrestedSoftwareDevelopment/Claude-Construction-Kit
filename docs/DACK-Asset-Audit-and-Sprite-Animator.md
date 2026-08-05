# DACK Asset Audit and Sprite Editor / Animator Plan

> Document status: Active architecture and optimization plan
>
> Last reviewed: 2026-08-03
>
> Implementation status: RAD features exist; release packaging and compiled
> importer manifests are planned
>
> Authority: `dack/assets/ASSET_PROVENANCE.md` is the source of truth for
> distribution eligibility

## Purpose

This note audits the current asset folders and turns the findings into a practical plan for DACK's sprite editor, animation module, and asset shelf.

The core distinction is a distribution tier, not merely a directory:

- **RAW-LOCAL:** ignored originals and license-pending reference material.
- **REPO/DEV-TEST:** assets deliberately available to the RAD and private
  development builds but excluded from public builds and hub exports.
- **PUBLIC-BUILD:** reviewed, allowlisted assets that may ship with DACK.
- **HUB-EXPORT:** PUBLIC-BUILD assets also cleared for inclusion in a creator's
  scrubbed, cloned level/game package.

`raw base assets/` and `dack/assets/quarantine/` normally hold RAW-LOCAL
material. `dack/assets/project/` currently contains both DACK-created material
and REPO/DEV-TEST third-party copies. `dack/assets/third_party/` is intended for
reviewed assets, but directory placement never overrides the provenance record.
DACK-created procedural/editor assets remain the safest redistributable
defaults.

## Current Repository Asset State

### Distribution classification

The complete decision record lives in `dack/assets/ASSET_PROVENANCE.md`.

| Asset | Distribution tier | Evidence posture | Current use |
| --- | --- | --- | --- |
| DACK procedural fallback art | PUBLIC-BUILD / HUB-EXPORT | Project-created | Safe default and live-pad seed |
| 8-Bit Dungeon Tile Set Free | PUBLIC-BUILD / HUB-EXPORT | CC0 record and curated subset recorded | Dungeon Runner, scenery, and starter playset |
| Stickman Pack thin sheets | PUBLIC-BUILD / HUB-EXPORT | CC BY 4.0 source and credit recorded | Platformer animation and sprite-pad seed |
| Creative Commons Sounds test deck | REPO/DEV-TEST | `CC0` inferred from local folder names; exact file sources missing | Combat and Brickbat sound wiring |
| The Game Creator's Pack runtime copies | REPO/DEV-TEST | Owner reports rights; exact redistribution record missing | Importer, actors, and shelf tests |
| Legacy Collection runtime copies | REPO/DEV-TEST | Local license PDFs exist; subset review incomplete | Actors and effects tests |
| Explosion Pack runtime copies | REPO/DEV-TEST | Local material exists; exact source/license/file mapping incomplete | Projectile and explosion profiles |
| Knight transparent strips | REPO/DEV-TEST | Supplied locally; exact rights/source/license/credit record missing | `knight-player` card, 96-frame named-strip and melee/roll/shield test |
| VerzatileDev Pinball pack | RAW-LOCAL | Local license record present; no curated promotion | Pinball taxonomy and scaling research |

### Quarantine

`dack/assets/quarantine/` contains RAW-LOCAL material and generated draft
catalogs. It is ignored by Git and must not be included in a repository commit,
build, level package, or hub export unless a specific subset is promoted through
provenance review.

### Raw asset vault

`raw base assets/` is ignored by Git and contains a large evaluation
collection. It is useful for design, taxonomy, and workflow testing, but is
never an input to release packaging.

Top-level raw packs observed:

| Pack | Files | Approx size | Dominant types | Immediate value |
| --- | ---: | ---: | --- | --- |
| 8-Bit-Dungeon-Tiles | 278 | 18 MB | EPS, PNG, SVG | PNG subset admitted; useful RPG/platformer/toolkit icons, Lode Runner-style climb player, and dungeon playset shelf |
| all_64c | 615 | <1 MB | `.64c` | Retro/source research; needs format understanding |
| all_spr | 47 | <1 MB | `.spr` | Retro sprite research; needs format understanding |
| explosion pack 1 | 263 | 1.6 MB | PNG, GIF, PDF | REPO/DEV-TEST effects source; exact public provenance and file map pending |
| kenney_ui-pack | 1,315 | about 1.35 MiB | PNG, SVG, OGG, TTF, URL | Complete CC0 source; exact duplicate exists inside the All-in-One bundle, whose copy adds ten atlas files and is canonical |
| Kenney Game Assets All-in-One 3.6.0 | 88,346 | about 0.95 GiB | PNG, SVG, OGG, 3D formats, XML, fonts | Complete CC0 bundle source; audio-first and non-isometric curation plan lives in `DACK-Kenney-All-in-One-Intake.md` |
| explosion tutorial files | 16 | tiny | PNG, GIF, ASE | Candidate effect animation learning set, license present |
| Knight | 14 | tiny | PNG | Seven transparent strips promoted to REPO/DEV-TEST as `knight-player`; exact creator/source/license still required before public packaging |
| Legacy Collection | 5,055 | 33 MB | PNG, GIF, PSD, ASE | Huge animation/effects/reference trove; license PDFs present |
| MountainDuskGodot | 384 | 9 MB | Godot import/cache files, PNG | Likely imported sample project; needs cleanup/intent review |
| PinBall_By_VerzatileDev | 64 | ~1 GB | PNG, TXT | Pinball kit reference; very large sheets; CC BY 4.0 + no standalone redistribution |
| Props | 4 | tiny | PNG | Candidate shelf props; license unknown from local audit |
| Sprites | 231 | 1.8 MB | PNG, PSD | Candidate character animation reference; license unknown from local audit |
| StickmanPack-V0.1 | 12 | tiny | PNG, GIF | Approved source for current stickman subset |
| StickmanPack-V0.2 | 8 | tiny | PNG | Admitted Stickman animation source with melee/death coverage |
| The Game Creator's Pack | 44 | 161 MB | WAV, MP3, PNG | Owner reports usage rights; REPO/DEV-TEST importer/animation use pending exact redistribution record |
| Warped shooting fx files | 276 | 16 MB | PNG, ASEPRITE, GIF, PDF, MP3 | Candidate projectile/effects animation source, license present |

## Important Observations

### Kenney UI Pack is a theme source, not an actor import

The standalone local pack is now complete: 870 PNG, 434 SVG, 6 OGG, two fonts,
and its source/license records. Blue, Green, Grey, Red, and Yellow each provide
82 Default sprites with exact 2x Double partners; Extra provides 24 pairs. The
All-in-One copy is byte-equal for all 1,315 shared files and adds ten
spritesheet PNG/XML files, so use that copy as canonical and never import both.

Promote only manifest-listed controls into a shared Godot Theme. Prefer the
bundle's atlases and one resolution tier, while retaining vector and high-DPI
sources in the raw vault. Do not run these assets through actor/frame detection
or import every loose variant into every page.

### Kenney All-in-One is an audio-first catalog

The completed 3.6.0 extraction contains 88,346 files and about 0.95 GiB. Its
1,342 OGG files have exceptionally clean local CC0 evidence, while the visual
catalog includes deterministic XML/TSX metadata for many high-value
non-isometric packs. The first pass is now an approved 18-card/50-source Sound
Card library with audition controls and live semantic routing,
followed by UI/icons, document-friendly Generic/Scribble/Letter assets,
pinball/physics/effects, platformer, overhead, space, racing, and RPG slices.
Isometric/axonometric packs are indexed but deferred. See
[`DACK-Kenney-All-in-One-Intake.md`](DACK-Kenney-All-in-One-Intake.md).

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

Recommendation: promote the V0.2 stickman files into `third_party/stickman-pack-v0.2/` after updating provenance. They are small, legible, and directly useful for idle/run/jump/melee/death animation states. `Punch` remains the source-sheet label, but the builder vocabulary should treat it as the general close-action slot: punch, sword, bite, club, wand, tool swing, etc.

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

The project owner reports rights to use The Game Creator's Pack. Until the exact
redistribution grant, creator/source, credit language, and admitted-file map are
recorded, all copies remain REPO/DEV-TEST. Its `Graphic Pack` folder is a small,
useful stress test rather than a huge runtime burden:

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
- interactively calibrated grid slicing for internal development and difficult sheets;
- automatic non-transparent blob detection for loose strips/sheets;
- manual frame rectangles for anything the detector gets wrong;
- clip grouping after detection, because detection finds pictures, not meaning.

`tools/prep_game_creator_graphics.py` is the local prototype for this path. It
writes frame candidates and a draft manifest to ignored quarantine, giving the
current Sprite Studio and importer workbench real calibration data. Selected images currently copied
under `dack/assets/project/game-creators-pack/` are development fixtures, not
publicly admitted assets.

### 6. Unknown-license packs stay design-only

`Props`, `Sprites`, `MountainDuskGodot`, `all_64c`, and `all_spr` need explicit provenance before any curated runtime admission. The seven transparent Knight strips have moved to REPO/DEV-TEST with an exact filename mapping, but still need creator/source/license evidence before public admission.

### 7. Legacy Collection deep pass: sorted animation vault

The Legacy Collection has now been processed into a local-only quarantine catalog:

- `dack/assets/quarantine/legacy-collection-prep/legacy-collection-summary.md`
- `dack/assets/quarantine/legacy-collection-prep/legacy-collection-catalog.json`
- `dack/assets/quarantine/legacy-collection-prep/legacy-collection-bundles.json`

The pack is stronger than a generic art dump. It is organized into coherent game-family folders and contains many complete animation bundles.

Observed pass results:

| Area | Result |
| --- | ---: |
| Files | 2,210 |
| Image files | 1,809 |
| Candidate bundles | 111 |
| Spritesheet-ready bundles | 53 |
| Sequence-ready bundles | 11 |

High-value bundle families:

- **Explosions and Magic:** EnemyDeath, Explosions pack, Ground Explosion, Grotto Escape FX, Warped shooting FX, Water splash. This should feed the reusable effects/projectile library first.
- **Gothicvania characters:** Bridge Heroine, Terrible Knight, Hell Beast, Hell Hound, Ogre, ghost/death/demon/flying-eye/flying-bird families. These are rich side-view character/enemy candidates.
- **TinyRPG:** frogs, mummies, ogres, slimes, wizards, monsters, top-down fantasy, dungeon robot. This supports RPG, maze, Snake/Pac-like, escort, and small arena experiments.
- **Warped:** alien walkers/flyers, mech unit, space marine, spaceship unit, tank unit, top-down boss, top-down shooter enemies/ships, vehicles. This is the best source bucket for overhead/Combat/Robotron/space/tank/Lunar-Lander-style work.
- **Misc sunny characters:** sunny dragon, bunny, froggy, mushroom. These are useful cute-platformer and all-purpose enemy/player test actors.

Importer implications:

- Treat PNG/GIF as the first import tier. PSD, ASE, and ASEPRITE files remain source/reference until we deliberately add those bridges.
- Prefer bundle-level import over file-level import. The editor should show "Bridge Heroine" or "Warped tank unit," then expose detected sheets/sequences inside that bundle.
- The bundle manifest is a better source for shelves than the raw tree because it groups animation intent, common dimensions, preview GIFs, and spritesheets.
- The first promotion pass should be effects/projectiles, then Warped overhead/tank/ship actors, then TinyRPG monsters, then fuller Gothicvania side-view actors.
- Do not ship the raw collection. Runtime copies under `dack/assets/project/`
  are REPO/DEV-TEST fixtures. Promote only curated DACK presets after explicit
  provenance review; changing directories alone is not promotion.

### Importer failure case: mixed-content sheets

The current importer is not flawless. A sheet containing a snake and a green blob was admitted as one character because the draft detector treated nearby/related pixel components as one actor. This is a correctness issue, not merely a presentation issue: an incorrect frame set contaminates animation labels, collision bounds, scale defaults, and enemy/player behavior.

The replacement contract is reviewable and fail-closed:

- detect candidate components, rows, cells, and whitespace gaps separately;
- propose frame rectangles and component groupings with confidence, source hash, and a contact-sheet preview;
- warn when candidates have incompatible dimensions, baselines, palettes, motion silhouettes, or disconnected clusters;
- require an explicit creator acceptance/correction for ambiguous groups;
- compile accepted rectangles, exclusions, origins, and action order into a versioned manifest;
- never rerun blob detection when the actor is spawned or when a level is loaded.

The snake/green-blob sheet is now a required regression fixture. It must produce two reviewable candidates (or an explicit “manual split required” result), never one silent character. The same profile pipeline should support the irregular TGC sheets, Dungeon Runner sequences, effects/projectiles, and individual frame files.

Current import process decision:

- Import one category at a time.
- Start with **Effects / Projectiles**, because those are reusable across every game family.
- First development-runtime test import: `Legacy Enemy Death`, an 8-frame
  48x48 spritesheet from `Explosions and Magic/EnemyDeath`.
- Use imported effects for clear gameplay jobs first. In the RAD, this sheet is bound to enemy defeat bursts while ordinary projectile hits keep the existing fireball impact.
- After the effect shelf feels right, repeat the same process for one actor category: likely Warped overhead/space/tank actors before larger Gothicvania side-view actors.

## Asset Governance Rules

1. **Raw is not runtime.** Nothing in `raw base assets/` ships or enters hub packages by default.
2. **Quarantine is never distributable.** Quarantined files are local-only until promoted.
3. **Repository presence is not release approval.** `project/` may contain
   REPO/DEV-TEST fixtures needed by the RAD.
4. **Promotion requires provenance.** Every promoted asset needs exact source,
   creator, license, retained license record, admitted file list, intended use,
   transformation history, and attribution string where applicable.
5. **Promotion requires an explicit tier.** PUBLIC-BUILD and HUB-EXPORT are
   separate decisions.
6. **Prefer curated subsets.** Admit the smallest useful runtime slice, not whole packs.
7. **Separate source files from runtime files.** PSD/ASE/ASEPRITE can be local source references; runtime should prefer PNG sheets/atlases plus DACK metadata.
8. **Preserve transform history.** If DACK slices/downsamples/recolors an asset, record the source and transformation.
9. **No standalone redistribution traps.** Packs with no-standalone-resale terms should be embedded only as functional game/editor assets, not exposed as raw downloadable asset libraries.
10. **Generated files inherit their source tier.** An atlas, thumbnail, Godot
    import, or cache generated from a REPO/DEV-TEST asset is also
    REPO/DEV-TEST.
11. **Package from an allowlist.** Public and hub packaging must fail on an
    unclassified file rather than silently including it.

### Public release gate

Before the first public build, add a packaging check that:

- compiles an allowlist from reviewed provenance manifests;
- excludes RAW-LOCAL, quarantine, and REPO/DEV-TEST assets plus all derived
  caches;
- verifies source, creator, license, admitted-file map, transformations, and
  required credit for every packaged third-party file;
- produces credits from the same data instead of a separately maintained list;
- audits the final package and fails if any asset is unclassified; and
- retains the packaged-file report with the release.

Hub export runs the same gate at the stricter HUB-EXPORT tier. It exports a
clone, scrubs supported source-document/image metadata from that clone as a
mandatory non-overridable policy, warns
the creator that shared material is cloned and scrubbed, and never edits the
original.

## Sprite Editor / Animator Boundary

DACK should have two cooperating art tools:

Detailed mini-app plan: [DACK Sprite Studio Mini-App](DACK-Sprite-Studio-Mini-App.md).

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
- Define animation clips: idle, run, jump, crawl, shoot, melee/punch, climb, slide, hurt, death, custom.
- Treat `Idle` and `Climb` as first-class baseline labels. They are not optional extras: almost every actor needs an idle/rest pose, and side-view/vertical games need climb labels as soon as ladders, vines, ropes, walls, or text-crawl surfaces exist.
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

### Sprite Studio character workspace

The current sidebar strip editor is useful for RAD work, but it has reached the
limits of a sidebar. The accepted home for serious character work is the full-screen
[Sprite Studio](DACK-Sprite-Studio-Mini-App.md), opened from the selected asset/card
and returned to its caller with selection and scroll state intact. The sprite-frame
editor is one Studio workspace, not the whole character experience.

Recommended page structure:

- **Character summary:** name, role, scale, facing defaults, locomotion type, health/toughness.
- **Sprite/frame source:** source sheet, importer mode, frame strip, frame renumbering, reload defaults, provenance.
- **Animation labels:** Idle, Run, Walk, Turn, Jump Up, Jump Down, Fall, Land, Climb, Crawl, Shoot, Run Shoot, Jump Shoot, Hurt, Death, Special, plus custom labels.
- **Playback settings:** FPS, loop/play-once/hold-last, ping-pong/reverse, strobe/effects, baseline/origin overrides.
- **Gameplay binding:** map animation labels to movement states, AI states, attacks, hit reactions, death, power-ups, and editor previews.
- **Attachment points:** muzzle, hands, head, feet/baseline, hitbox, hurtbox, pickup/interaction point.
- **Rule cards:** movement, AI, projectile, sounds, effects, text interaction, stats.

This keeps the live-linked pad as the quick in-context toy while giving serious
character setup a room of its own. It also lets future importer modes—grid, tight
rectangles, seeded components, metadata, Aseprite JSON—expose their assumptions
without crowding the playfield.

### Character builder: paper-doll plus rule cards

Player and enemy creation should become a drag/drop builder rather than a node graph.

The creator-facing model:

- Pick or import a character body/sprite sheet.
- Click the animation strip to edit labels, frame ranges, reverse/ping-pong/strobe settings, and timing.
- Drag behavior cards onto the character: patrol, chase, guard, hover, crawl, flee, swarm, escort, turret, boss.
- Drag capability cards onto the character: gun, jump, climb, dig, fly, text collision, text destruction, contact damage.
- Drag a projectile card into the projectile slot.
- Drag an explosion/effect card into the impact/death/power-up slot.
- Drag sound cards into firing, impact, hurt, defeat, pickup, and movement/action slots.
- Tune exposed values in the inspector: range, speed, damage, health, gravity sensitivity, opacity, scale, direction, cooldown, invulnerability.

This is intentionally "not quite nodes." It should feel like arranging toys or assigning equipment, not programming a graph. Advanced logic can still be represented internally as components/rulesets, but the visible editor should be a readable set of slots and cards:

```text
Character
  Art / Animation
  Movement Rules
  AI Rules
  Attack / Projectile
  Impact / Explosion
  Sound / Voice
  Text Interaction
  Stats
```

The same builder should support both player characters and enemies. The difference is mostly which rule cards are allowed by default: a player gets input/control cards, while enemies get AI/behavior cards.

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

## Superseded Animator UI Proposal (Historical RAD Note)

The early proposal placed the animator in the right Inspector/bottom tray. That
placement is superseded by the full-screen Sprite Studio specification. The shared
Inspector may show a compact animation summary, current binding, and **Edit in
Sprite Studio** action; it must not attempt to host the complete animator.

The following pane requirements remain valid and now belong inside Sprite Studio:

- **Asset browser:** filterable by Player, Enemy, Object, Projectile, Effect,
  Sound, and Playset, with tier badges for RAW-LOCAL, DEV-TEST, PUBLIC, and HUB.
- **Clip list:** idle/run/jump/etc. with small loop previews.
- **Frame strip:** reorder, duplicate, delete, adjust duration.
- **Preview stage:** selected actor at current playfield scale, with onion-skin and checkerboard options.
- **Binding panel:** motion state, behavior state, projectile/effect hook.
- **Geometry panel:** origin, bounds, collision profile, hitboxes, attachment points.
- **Source/provenance panel:** creator, license, source pack, attribution,
  distribution tier, missing release fields, compile status, cache status, and
  source/profile hashes.

The live pixel pad should appear as an edit mode for an individual frame or small sprite profile.

The UI should prevent status ambiguity:

- use both text and color for tier badges;
- show a persistent `DEVELOPMENT ASSET` ribbon when previewing REPO/DEV-TEST
  material;
- disable `Include in public build` and `Include in hub export` until their
  respective gates pass, with a plain-language list of missing records;
- group importer warnings beside the affected source and frame rather than in a
  distant log;
- show draft versus compiled state and whether the preview is stale; and
- keep rebuild, relink, fork, and provenance actions in Sprite Studio's contextual
  properties pane so they do not add permanent top-level buttons to the main shell.

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
- a per-label strobe toggle plus integer count/intensity, initially useful for death, invincibility, power-up, and damage feedback;
- visual range highlighting for every label;
- nullable/unavailable labels via `- / -` endpoints, because not every character has every action;
- save/export to `.dackanim.json`, including both creator-visible numbering and internal zero-based detected frame indices;
- load/import `.dackanim.json` back into the editor so saved label work can be inspected, corrected, and reapplied instead of being write-only;
- later: delete/reorder labels, duplicate ranges, and bind labels explicitly to engine motion states.

Current RAD test:

- `STICKMAN` uses the admitted OctoPyte stick figure clips and now enters the same frame editor as imported strips: idle, run, jump-up, jump-down, fall, run-shoot, jump-shoot, and death labels can be edited, ping-ponged, strobed, saved, loaded, and reapplied.
- `TGC PLAYER` uses The Game Creator's Pack `Player_DarkOutline.png` strip
  through local blob-detected frame loading. It is a REPO/DEV-TEST actor.
- `SUNNY DRAGON` uses
  `raw base assets/Legacy Collection/Legacy Collection/Assets/Misc/Characters/sunny-dragon/spritesheets/sunny-dragon-fly.png`
  as the first animated enemy import. It exercises a third path: a non-square
  9-frame grid strip rather than curated Stickman sheets or TGC blob detection.
  It remains REPO/DEV-TEST pending subset-level provenance review.
- The selected actor can be renamed in the sidebar. Names should be saved as creator metadata, not inferred permanently from filenames; this is how imported assets become reusable characters.
- The current mapping starts rough, but the prototype exposes a visual 8-column frame strip plus editable label names, numeric endpoints, ping-pong toggles, strobe toggles, and strobe counts. Recognized labels like idle/run/jump-up/jump-down drive the current player animation; extra labels such as run-shoot, jump-shoot, climb-up/down, dig-up/down, shoot-up/down, bounce, and death can be added and highlighted ahead of full engine binding.
- `SAVE ANIM LABELS` writes a source-aware local-only manifest: TGC currently saves to `dack/assets/quarantine/game-creators-pack-graphics-prep/tgc-player.dackanim.json`; Stickman currently saves to `dack/assets/quarantine/stickman-pack-v0.1/stickman-thin.dackanim.json`. `LOAD ANIM LABELS` reads the current source's manifest back into the editor. This is the debugging lens for numbering/range mistakes and the seed of the canonical Character Card format.
- Saved working labels should graduate into curated per-character defaults once
  the mappings are good. The intended flow is: tune frames locally, save
  `.dackanim.json`, test in play, then compile the confirmed manifest into a
  runtime/defaults area keyed by a stable `animationSourceId` such as
  `tgc-orange-worker` or `sunny-dragon-fly`. This improves import correctness
  but does not promote the source asset's distribution tier; provenance review
  remains a separate gate. Future creators should inherit those defaults
  automatically and only adjust them when they want a variant.
- The schema is intentionally variable-length. `Run` can be 12 frames, `Fall` can be one frame, `Death` can be dashed unavailable, and future effects such as `Power Up` can use arbitrarily long ranges. Engine bindings should consume labels by meaning and range, never by assuming a fixed frame count.
- The first saved manifest exposed an important detector bug: horizontal strips must sort detected frames left-to-right before row/top ordering. Sorting by Y first can pull far-right death frames into indices 0/1 and make idle/run labels appear to include the wrong action.
- Cropped frames must retain a stable display box. If every detected crop is stretched to fill the actor rectangle, narrow or short frames appear to change character size during playback. DACK should draw each crop inside a common per-animation frame box, centered and baseline/bottom aligned, while keeping collision size separate.
- The TGC Player strip exposed a second detector boundary: permissive blob detection needed for small enemy/FX sheets can accidentally admit tiny far-right fragments as extra player frames. The player strip therefore uses source-specific constraints: character frames must be tall/substantial enough to exclude the small accessory/projectile fragments that appeared as bogus frames 18-26.
- Thin stick-figure art needs special care at document scale. The v0.1 Stickman sheets are tiny 64 px frames with one-pixel limbs; downscaling can turn those strokes into dotted fragments. The current runtime applies a small one-pixel opaque-pixel expansion to Stickman imports after white-background transparency cleanup. This is an art-preservation pass, not collision geometry, and it should eventually become an importer option such as `preserveHairlineStrokes`.

Swinging vines/ropes should wait for the visible spline/Bezier tool family. A straight-line placeholder would teach the wrong feel. The desired version is an authored curve with draggable handles, visible swing arc, optional attach/detach points, and animation labels for grab, swing, release, and land.

Power-up animations can often be effect composites rather than separate sprite sheets. The cheap useful version is: play the actor's idle or current-state animation, then layer reusable DACK effects over it—rings, glows, color cycling, sparks, outline pulses, rotating text sigils, or Jeff-Minter-style neon bursts. This lets a creator define `Power Up`, `Shielded`, `Hasted`, `Poisoned`, `Charged`, or `Invincible` without requiring new hand-drawn frames for every character.

Projectile/explosion profiles are their own import category. The first RAD
profile uses
`raw base assets/explosion pack 1/explosion pack 1/Bonus/From explosions pack 2/explosion-b/explosion-b.png`,
copied for development runtime as
`dack/assets/project/effects/fireball-impact-explosion.png`. The sheet is
1040x48, interpreted as 13 frames of 80x48: frame 0 projectile, frame 1 impact,
frames 2-12 explosion. Seven additional profiles are copied into
`dack/assets/project/effects/` as `explosion-1-a.png` through
`explosion-1-g.png`. These copies and their generated data are REPO/DEV-TEST,
not public-release assets, until exact pack provenance and original-to-runtime
file mappings are reviewed. The catalog file `projectile-effect-profiles.json`
records frame sizes, frame counts, source paths, and default blast radii.
Designers should eventually assign these profiles per enemy/player weapon, with
credit/provenance stored alongside the profile.

Sound assignment should follow the same card/slot model rather than being hardcoded by game mode. A weapon/projectile profile should be able to carry:

- firing sound;
- flight/loop sound, optional;
- impact sound;
- explosion sound;
- dry-fire/empty sound, optional;
- damage amount, range, speed, cooldown, and text-destruction rules;
- provenance/credit for every admitted sound.

Characters should separately expose actor sounds: hurt, defeat/death,
jump/land, climb, pickup, power-up, alert, attack voice, and ambient/idle
barks. The first RAD wiring hardcodes a tiny locally `CC0`-labeled starter
deck. It remains REPO/DEV-TEST because folder naming is not sufficient release
evidence. The intended editor model is assignable sound slots beside animation,
projectile, and effect slots.

Projectile assignment should scale in complexity:

- **Simple mode:** drag a projectile/weapon card onto a player or enemy. The actor can now fire it using that card's default art, sounds, damage, and range.
- **Intermediate mode:** expose the common fields: firing sound, impact/explosion sound, projectile sprite/effect, speed, range, cooldown, damage/toughness multiplier, blast radius, and whether it damages text.
- **Advanced mode:** open the full weapon profile: spread/burst count, aim style, homing, gravity/inertia, ricochet/pierce/wrap, owner/team rules, friendly fire, conditional zones, semantic word targeting, projectile lifetime, muzzle/attachment point, impact animation, explosion profile, screen shake, and sound randomization.

This keeps the character editor friendly while still letting the same system grow into Contra-style guns, dragon fireballs, Brickbat laser columns, pinball bumpers, tower-defense shots, Lunar Lander thrusters, and Robotron/bullet-hell emitters.

The Game Creator's Pack graphic sheets are staged as REPO/DEV-TEST fixtures
under `dack/assets/project/game-creators-pack/`.
`tgc-graphic-pack-catalog.json` names the first useful profiles: Orange Worker,
Red Runner, Blue Guard, Green Crawler, Shooter Boss, Shooter Fleet, Red Girder,
Gray Blocks, Orange Bricks, and Retro Puzzle Blocks. The RAD has also separated
Green Snake at code/profile-test level; the catalog needs to gain its formal
entry during manifest compilation. The current RAD mixes import experiments:
Orange/Red retain earlier range mappings, while Blue Guard, Green Crawler, and
Green Snake use explicit accepted component-index selections from the
platformer atlas. That fixed the wrapped-feet/half-frame class of errors, but
the reviewed compiled manifest must replace detector indices before these
become stable runtime profiles. The girders/blocks are atlas-region hints for
future level-object shelf import. None of these files belongs in a public build
or hub export until its release record is complete.

## Import Pipeline

### Architecture decision: source profiles compile to runtime manifests

Importer behavior must be source-specific and deterministic. Grid slicing,
near-white cleanup, connected-component detection, hairline preservation, and
frame ordering are valid operations, but no single combination is safe for
every pack. Automatic detection is an import-time assistant, not a runtime
source of truth.

The pipeline is:

```text
source asset
  -> source-specific import profile
  -> draft detection/preview
  -> creator confirmation and clip labeling
  -> compiled runtime manifest + atlas/frames
  -> disposable runtime cache
```

The source-specific profile records:

- stable asset and provenance IDs plus distribution tier;
- source path and content hash;
- importer/profile schema version;
- slicing mode: fixed grid, regular strip, calibrated grid, explicit rectangles,
  metadata, or draft component detection;
- exact cell size or confirmed frame rectangles and deterministic frame order;
- transparent-color/background policy and optional hairline-preservation pass;
- common display box, baseline/origin, pivots, and per-frame visual bounds;
- clip labels, frame order, timing, loop/ping-pong/strobe behavior;
- transformations such as cropping, recoloring, dilation, or downsampling; and
- source-specific exceptions needed to make the import correct.

Once a draft is confirmed, runtime code reads compiled rectangles and clip
data. It does not rerun blob detection, guess transparency, inspect the whole
sheet, or reorder components on every load.

### Compiled runtime manifest

Compile `.dackanim.json` from the reviewed source profile and place it beside
curated runtime output, not beside ignored originals:

```json
{
  "schemaVersion": 1,
  "id": "stickman-thin",
  "provenanceId": "octopyte-stickman-pack",
  "distributionTier": "PUBLIC-BUILD",
  "texture": "thin-run-sheet.png",
  "sourceHash": "sha256:...",
  "profileVersion": 1,
  "slicing": "fixed-grid",
  "frameSize": [64, 64],
  "transparentColor": "#FFFFFF",
  "commonFrameBox": [64, 64],
  "baseline": 63,
  "clips": {
    "run": { "frames": [0,1,2,3,4,5,6,7,8], "fps": 12, "loop": true }
  }
}
```

This removes hardcoded per-pack assumptions from `SpriteAnimationSet` while
preserving them in a visible, testable profile.

### Draft detection for irregular sheets

For irregular strips and mixed sheets, DACK should create a draft import manifest before the creator does any manual cleanup:

```json
{
  "source": "Player_DarkOutline.png",
  "sourceHash": "sha256:...",
  "profile": "tgc-player-dark-outline-v1",
  "slicing": "blob-detect",
  "status": "draft-needs-confirmation",
  "frames": [
    { "rect": [0, 2, 28, 40], "origin": [14, 39], "tags": [] }
  ],
  "clips": {
    "unassigned": { "frames": [0], "fps": 8, "loop": true }
  }
}
```

The animation editor then lets the creator reject noise, split or merge
rectangles, reorder frames, normalize the display box and baseline, name clips,
set timing, and bind states. Saving confirmation converts detector output to
explicit rectangles. Component indices may help during exploration, but
compiled manifests must not depend on detector indices whose meaning could
change when the detector changes.

### Cache and invalidation policy

Compiled atlases, thumbnails, recolored frames, and Godot imports are caches,
not authoring truth. A cache key includes:

```text
source content hash
+ source-profile hash/version
+ importer/compiler version
+ target render profile
```

Any changed input invalidates the cache. Cache output inherits the source
asset's distribution tier and must be excluded by the public packager when its
source is REPO/DEV-TEST. Caches can be deleted and rebuilt without losing clip
labels, origins, collision references, attachment points, or provenance.

Efficiency rules:

- compile and validate in the background when an asset is imported or changed;
- lazy-load shelf thumbnails and full frame textures only when their card,
  character, or clip is visible;
- use packed atlases or texture arrays where that reduces texture churn;
- retain decoded textures while referenced and release unused preview data;
- never scan the full raw vault at application startup;
- avoid per-frame image conversion, blob detection, recoloring, or frame
  extraction during gameplay;
- cap preview animation work when an editor panel is hidden; and
- expose cache size, compile status, warnings, and a safe `Rebuild Preview
  Cache` action in the asset inspector.

### Source-profile test matrix

Each admitted importer profile needs golden tests for:

- frame count, exact rectangle order, and stable IDs;
- no blank, partial, wrapped, or neighboring-frame contamination;
- common display box and baseline, preventing size jumps and feet-over-head
  wraparound;
- transparency/recolor policy;
- clip endpoints and unavailable labels;
- source/profile hash invalidation;
- deterministic recompile; and
- distribution-tier inheritance into every generated output.

### External editor refresh

- Aseprite bridge imports PNG + JSON exported by Aseprite.
- DACK does not embed or redistribute Aseprite.
- Refresh reruns the source-profile compiler and preserves stable clip IDs,
  bindings, origins, hitboxes, attachment points, and behavior assignments
  where possible.

### Source-format research

- `.ase`/`.aseprite` direct reading is a research item only if licensing and implementation are clean.
- PSD import remains out of scope until the broader importer sandbox exists.
- GIF import can be useful for simple effect previews but should normalize to sprite clips.

## Asset Pipeline Backlog and Release Gates

This is the domain backlog, ordered by release risk. It does not replace the cross-project sequence in [`DACK-Optimization-and-Refactoring-Plan.md`](DACK-Optimization-and-Refactoring-Plan.md).

1. Keep the PUBLIC-BUILD/HUB-EXPORT allowlist small and make development builds
   visibly identify themselves as containing REPO/DEV-TEST assets.
2. Add machine-readable provenance entries and a fail-closed public packaging
   audit before any public release.
3. Record exact provenance or replace the current sound, TGC, Legacy, and
   Explosion Pack fixtures before they are allowed into public builds.
4. Separate clearly DACK-created built-ins from third-party development
   fixtures in the Asset Catalog; do not infer tier from either directory.
5. Compile source-specific `.dackanim.json` manifests for Stickman v0.1/v0.2,
   Dungeon Runner, and each working TGC/Legacy test actor.
6. Replace hardcoded `SpriteAnimationSet` sheet assumptions and runtime blob
   detection with compiled manifests and deterministic caches.
7. Add golden importer tests for every source profile, starting with the sheets
   that previously produced extra frames, half-height frames, feet wrapping,
   size changes, or unstable order.
8. Add an Asset Catalog service that displays tier, provenance completeness,
   compile/cache status, and warnings alongside each card.
9. Add a minimal Animator inspector: clip list, frame strip, FPS, loop, origin,
   preview, and `Rebuild Preview Cache`.
10. Keep the live pad as the in-context frame/sprite editor, not a full
    animation suite.
11. Curate only small pinball parts from the large VerzatileDev sheets after
    confirming release and hub-export treatment.
12. Continue using The Game Creator's Pack as an irregular-sheet development
    test: detect a draft once, hand-confirm it, and compile explicit rectangles.

## Product Feeling

DACK's art workflow should feel like:

- quick edits happen directly on the selected actor;
- serious animation can come from external tools;
- imported sheets become understandable clips;
- every asset carries its license/provenance;
- creators can always see whether they are editing a shared sprite, an instance fork, or a runtime-only clone.

## Overhead / Battle Fleet sprite processing

Top-down, fleet, and space sprites need a different interpretation from side-view actors. A horizontal strip of ships is usually not a walk cycle. It is often a set of heading bins: the ship should display the frame that best matches its movement vector.

The Legacy `top-down-shooter-ship` pack is the first clean test case:

- `spritesheets/red/ship-01.png`: 240x48, five 48x48 heading frames.
- `spritesheets/red/ship-02.png`: 320x64, five 64x64 heading frames.
- `spritesheets/red/ship-03.png`: 240x48, five 48x48 heading frames.
- `spritesheets/red/ship-04.png`: 240x48, five 48x48 heading frames.
- Matching yellow variants exist.
- Thrust strips are separate small overlays and should become an optional engine/exhaust layer later.

Processing rule:

- Treat multi-panel ships as **directional frame sets**, not animation loops.
- Store frame count, source path, color/faction, ship class, and intended heading order.
- Movement systems choose the visible frame based on velocity/heading.
- Thruster frames are composited only while thrust/acceleration is active.
- Floating crystals should import as pickups, bumpers, hazards, resources, or mission anchors rather than actors.

The first runtime proof uses `Battle Ship 01` as an Overhead player: WASD/arrow movement steers the ship through five heading frames. Later Battle Fleet packs with many individual images can follow the same taxonomy:

- three-panel ships: left/center/right or bank-left/neutral/bank-right;
- five-panel ships: coarse heading bins;
- eight/sixteen-panel ships: direct compass headings;
- crystals/asteroids: static or slowly rotating pickups/obstacles;
- engine/thrust/fire: optional attachment effects;
- wreckage/debris: destruction effects and pickups.

The sprite pad is the toy. The animator is the librarian and stage manager. Both are needed.
