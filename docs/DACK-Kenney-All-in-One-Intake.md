# Kenney Game Assets All-in-One 3.6.0 Intake

> Status: Audited raw-vault source; approved 18-card / 50-file audio library and Cockpit shelf implemented
>
> Reviewed: 2026-08-03
>
> Source: `raw base assets/Kenney Game Assets All-in-1 3.6.0/`

## Decision

Use this bundle as a catalog of independently curated asset packs, never as one
runtime dependency. Audio is the first priority. Non-isometric 2D art follows
in small playset-oriented slices. Isometric and axonometric packs are deferred
unless a later game type establishes a specific need.

The local `Readme.html` states that all bundle content is Creative Commons Zero
(CC0 1.0), permits personal and commercial use, and makes credit optional. It
also asks purchasers not to redistribute the complete all-in-one bundle and
explicitly permits distributing individual asset packs. DACK will therefore:

- retain the purchased bundle only in the ignored raw vault;
- copy only reviewed files or small pack slices into the runtime tree;
- retain the relevant pack `License.txt` and source mapping with every admitted
  subset;
- generate a voluntary `Kenney / www.kenney.nl` credit;
- never ship, mirror, or hub-export the complete all-in-one directory.

## Implemented Checkpoint

The approved audio intake is now present under
`dack/assets/third_party/kenney-audio/`: 50 byte-identical OGG files, seven
retained pack notices, an exact hash/source manifest, and explicit exclusion of
all preview reels. The 18-card semantic deck retains its two one-source seeds
(document/RPG and racing); each of the other 16 high-repetition cards has three
distinct recordings. The Cockpit has a `Sounds` page with the shared two-level
picker (game family, then Sound Card), source/card details, Audition, Next
Variant, Stop, and global Sound controls.

`SoundCardPlayer` provides reusable selection modes, optional loop intent,
per-card gain and pitch range, cooldowns, a bounded voice pool, and overflow
policy. Cockpit close, entering Play, and the Boss Key stop auditions. The
approved cards now resolve live semantic events for Platformer, Brickbat,
Pinball, combat, restrained UI transitions, and Cockpit opening. Missing card
imports fall back to the former project sounds; cooldown or voice-cap rejection
is intentionally silent instead of leaking through as a doubled legacy sound.

## Inventory Baseline

The completed extraction contains 88,346 files totaling about 0.95 GiB.

| Family | Files | Approximate size | Intake posture |
| --- | ---: | ---: | --- |
| 2D assets | 40,408 | 248.22 MiB | Primary visual source; curate by playset |
| 3D assets | 28,454 | 603.06 MiB | Deferred until a specific 3D-derived workflow exists |
| Archive | 1,308 | 53.38 MiB | Historical/reference only by default |
| Audio | 1,319 total files | 27.30 MiB | Highest priority |
| Early access | 445 | 10.02 MiB | Do not treat as production defaults without a separate review |
| Goodies | 9 | 7.38 MiB | Reference/tool extras |
| Icons | 9,751 | 11.78 MiB | Strong editor, card, action, and status source |
| Other | 68 | 5.74 MiB | Fonts, guides, and samples; review individually |
| UI assets | 6,580 | 7.99 MiB | Strong Cockpit/Studio theme source |

There are 56,190 PNGs, 5,279 SVGs, 1,342 OGG files, and substantial 3D source
material. Paths containing `isometric` account for 6,816 files and about
117.60 MiB; 23 immediate 2D pack folders are named Isometric or Axonometric.
That material remains indexed but outside the active import queue.

The extraction is internally coherent: all 16 Audio packs, all 10 UI packs,
all 8 Icon packs, and 149 of 154 immediate 2D packs have their own
`License.txt`. The five 2D folders without a pack-local license are all
Isometric Miniature families; the bundle-level CC0 statement still exists, but
their deferred status means no exception is needed now.

## Audio Is a First-Class Asset Library

The dedicated `Audio/` folder contains 16 packs and 1,286 OGG files. A further
56 OGG files ship inside `Desert Shooter Pack`, `New Platformer Pack`, and the
standalone `UI Pack`, for 1,342 OGG files overall. Four are long preview reels,
so the gameplay catalog should index 1,338 sources and explicitly exclude every
`Preview*.ogg`. No isometric pack contains audio.

| Pack/family | OGG files | Best initial DACK uses |
| --- | ---: | --- |
| Casino Audio | 55 | Pinball scoring, ball launch, physical tokens, board-game playsets |
| Digital Audio | 62 | Brickbat bonuses, lasers, phase effects, pickups, arcade UI |
| Foley Sounds | 85 | Melee, dragged objects, impacts, water, whooshes |
| Impact Sounds | 130 | Material-aware collisions: glass, metal, wood, plate, bell, soft, mining, footsteps |
| Interface Sounds | 100 | Cockpit open/close, select, toggle, error, confirmation, scroll, minimize/maximize |
| Music Jingles | 85 | Goal, death, level-complete, bonus, high-score, word discovery |
| Music Loops | 29 | Optional playset ambience and prototype scoring beds |
| Retro Sounds 1 | 34 | Creature, jump, laser, pickup, radar, rumble, lose |
| Retro Sounds 2 | 65 | Coin, engine, explosion, fall, game-over, hit, hurt, jump, laser, secret, upgrade |
| RPG Audio | 51 | Books/pages, doors, coins, footsteps, cloth, knife, metal; especially document-native RPG play |
| Sci-Fi Sounds | 73 | Space/air combat, thrusters, engines, force fields, lasers, explosions, computer ambience |
| Synth Voice 1/2 | 325 | Later announcer, robot, accessibility, and character-voice experiments |
| UI Audio | 51 | Compact click, switch, rollover, mouse press/release families |
| Voiceover packs | 141 | Later fighter/announcer cards; lower priority than nonverbal sound |
| Desert Shooter embedded set | 40 | Complete shoot/explosion/hurt/jump/lose/move/coin prototype deck |
| New Platformer embedded set | 10 | Compact bump, coin, hurt, jump, magic, throw, and selection defaults |
| UI Pack embedded set | 6 | Small click/switch/tap starter family |

The RPG pack is unusually aligned with DACK's identity. `bookOpen`,
`bookClose`, `bookFlip1-3`, and `bookPlace1-3` can sonically reinforce page
levels, document refresh, word discovery, snapshot capture, and RPG inventory
actions without pretending the document is merely a conventional backdrop.
Likewise, Interface Sounds includes literal `open`, `close`, `scroll`,
`minimize`, `maximize`, `select`, `switch`, and `toggle` families suitable for
the office-native Cockpit.

## Recommended First Audio Audition Deck

Do not admit all 1,342 files. The first audition deck is 18 exact files chosen
to cover the RAD's current hooks and one seed for each near-term game family:

| Candidate | Proposed semantic binding |
| --- | --- |
| `2D assets/Desert Shooter Pack/Sounds/shoot-a.ogg` | Player shot |
| `2D assets/New Platformer Pack/Sounds/sfx_bump.ogg` | Enemy/contact hit |
| `2D assets/New Platformer Pack/Sounds/sfx_disappear.ogg` | Enemy defeat |
| `2D assets/New Platformer Pack/Sounds/sfx_hurt.ogg` | Player hurt |
| `2D assets/New Platformer Pack/Sounds/sfx_magic.ogg` | Power-up |
| `2D assets/New Platformer Pack/Sounds/sfx_jump.ogg` | Platformer jump |
| `Audio/Impact Sounds/Audio/impactMetal_light_000.ogg` | Brickbat paddle / light pinball rail |
| `Audio/Impact Sounds/Audio/impactBell_heavy_000.ogg` | Pinball bumper |
| `Audio/Retro Sounds 2/Audio/hit1.ogg` | Brickbat text hit |
| `Audio/Retro Sounds 2/Audio/explosion1.ogg` | Brickbat word break |
| `Audio/Retro Sounds 2/Audio/lose1.ogg` | Ball lost |
| `Audio/Sci-Fi Sounds/Audio/laserLarge_000.ogg` | Brickbat laser |
| `Audio/Sci-Fi Sounds/Audio/laserSmall_000.ogg` | Space/actor projectile |
| `Audio/Sci-Fi Sounds/Audio/explosionCrunch_000.ogg` | Projectile explosion |
| `UI assets/UI Pack/Sounds/click-a.ogg` | UI accept |
| `UI assets/UI Pack/Sounds/switch-a.ogg` | UI toggle |
| `Audio/RPG Audio/Audio/bookOpen.ogg` | Document/RPG interaction |
| `Audio/Retro Sounds 2/Audio/engine1.ogg` | First racing prototype |

These rights-cleared seed choices were creator-approved on 2026-08-03. The
manifest remains their positive shipping allowlist. Approval did not itself
bind a card; the later semantic-routing pass established the editable runtime
defaults independently.

## Approved High-Repetition Variant Expansion

Sixteen approved cards now have three source recordings each. Variants stay
within the original seven licensed packs, remain byte-identical, and use an
unsuffixed runtime file for variant 1 followed by `-v2` and `-v3` files. The
complete source-to-runtime paths and SHA-256 values are in
`dack/assets/third_party/kenney-audio/MANIFEST.csv`.

| Sound Card | Three-recording family |
| --- | --- |
| Player shot | Desert Shooter `shoot-a`, `shoot-b`, `shoot-c` |
| Platformer jump | New Platformer `sfx_jump`, `sfx_jump-high`; Desert Shooter `jump-a` |
| Player hurt | New Platformer `sfx_hurt`; Desert Shooter `hurt-a`, `hurt-b` |
| Enemy/contact hit | New Platformer `sfx_bump`; Impact Sounds `impactGeneric_light_000`, `_001` |
| Enemy defeat | New Platformer `sfx_disappear`; Desert Shooter `explosion-a`, `explosion-b` |
| Power-up | New Platformer `sfx_magic`, `sfx_coin`, `sfx_gem` |
| Brickbat text hit | Retro Sounds 2 `hit1`, `hit2`, `hit3` |
| Brickbat word break | Retro Sounds 2 `explosion1`, `explosion2`, `explosion3` |
| Ball lost | Retro Sounds 2 `lose1`, `lose2`, `lose3` |
| Brickbat laser | Sci-Fi Sounds `laserLarge_000`, `_001`, `_002` |
| Paddle/light rail | Impact Sounds `impactMetal_light_000`, `_001`, `_002` |
| Pinball bumper | Impact Sounds `impactBell_heavy_000`, `_001`, `_002` |
| Space/actor projectile | Sci-Fi Sounds `laserSmall_000`, `_001`, `_002` |
| Projectile explosion | Sci-Fi Sounds `explosionCrunch_000`, `_001`, `_002` |
| UI accept | UI Pack `click-a`, `click-b`, `tap-a` |
| UI toggle | UI Pack `switch-a`, `switch-b`, `tap-b` |

Further auditions should add new roles rather than bulk-importing variants:
book flip/place, glass/plate impacts, footsteps, UI open/close/error, and
matching jingle indices across Hit, Retro, Pizzicato, Saxophone, and Steeldrum.

## Sound Card Contract

The current RAD emits semantic event keys, resolves them through an editable
binding table, and plays the resulting Sound Card through a bounded pool. A
Sound Card owns variants and playback policy; actors, projectiles, effects,
objects, and playsets can later override those default semantic slots without
rewriting gameplay code.

Minimum Sound Card fields:

- stable card ID, display name, tags, source pack, provenance ID, and credit;
- one or more variant resource paths with optional weights;
- selection mode: fixed, random-no-repeat, shuffle, or sequential;
- one-shot or loop mode, with loop start/end where needed;
- volume, pitch range, stereo/spatial mode, and optional distance falloff;
- cooldown, maximum simultaneous voices, and interrupt/steal policy;
- semantic material tags such as `glass`, `metal`, `wood`, `text`, `paper`,
  `interface`, `magic`, or `engine`;
- editor audition button and a visible warning for excessively loud or long
  sources.

Collision-heavy games need voice limits and cooldowns from the first version.
A pinball or three-ball Brickbat burst can otherwise trigger dozens of nearly
identical impacts in one frame. Footsteps should normally use shuffle or
random-no-repeat; loops such as engines should crossfade or update pitch rather
than restart every tick.

Godot can ingest the OGG files directly, but the sources are intentionally
heterogeneous: 529 are mono and 813 are stereo, with sample rates spanning
8 kHz, 11.025 kHz, 22.05 kHz, 44.1 kHz, 48 kHz, and 96 kHz. All 85 Foley
clips are stereo 96 kHz. Preserve the raw originals; downsample only admitted
derivatives if profiling warrants it, and preserve Retro Sounds 1's low-rate
character. Normalize perceived loudness non-destructively through Sound Card
gain and audio buses first. Namespace every ID by pack because stems such as
`laser1`, `jump1`, and `click1` are not interchangeable. The only exact audio
duplicates found are two pairs of carpet footsteps in Impact Sounds.

## Non-Isometric Visual Queue

The highest-value early visual packs are those that expand existing DACK
mechanics or provide deterministic atlases/metadata:

1. `Rolling Ball Assets` and `Physics Assets` for pinball-like obstacles,
   rolling-ball gates, breakable materials, debris, glass, wood, metal, and
   explosive elements.
2. `Particle Pack` and `Smoke Particles` for reusable effects cards.
3. `New Platformer Pack`, `Platformer Pack Remastered`, the Pixel Platformer
   families, and Extra Animations & Enemies for side-view shelves.
4. `Topdown Shooter`, `Topdown Tanks Remastered`, and `Tower Defense` for the
   shared overhead/escort/tower vocabulary.
5. `Simple Space` and `Space Shooter Remastered` for the air/space branch.
6. `Roguelike Base`, Characters, Dungeon, Interior, Micro Roguelike, Tiny
   Dungeon, and Monochrome RPG for terminal/document RPG construction.
7. `Racing Pack`, `Pixel Vehicle Pack`, and road textures for track builders.
8. Icon and UI families for the two-level picker, cards, Inspector, status,
   input hints, and game-specific toolkit tabs.

There is no dedicated Kenney pinball pack. The strongest composite is Rolling
Ball plus selected Puzzle Pack 2 balls/paddles, Physics Assets material pieces,
Explosion Pack, and a small transparent Particle Pack subset.

Particularly strong document-native additions are `Generic Items` (computers,
paper, books, disks, cups, mail, and paint tools), `Scribble Platformer`,
`Scribble Dungeons`, `Letter Tiles Redux`, `Brick Pack`, and `Pattern Pack
Lines`. Keyboard & Mouse Input Prompts match DACK's office-PC target and should
take precedence over controller prompt art.

Prefer pack-provided spritesheets, XML atlases, and SVG sources over importing
hundreds of loose duplicates. Keep Default/Retina or raster/vector variants as
source alternatives, not simultaneous runtime copies.

The atlas reader must handle one recurring Kenney quirk: 162 of 357
non-isometric XML atlases declare generic or missing image names such as
`sheet.png` or `sprites.png`. Resolve the declared `imagePath` first, then try a
same-directory PNG matching the XML basename. The XML otherwise follows a
consistent `TextureAtlas` / named `SubTexture` rectangle model. TSX metadata is
also valuable: 1-Bit Pack declares 1,024 16×16 tiles, Tiny Dungeon declares 132
16×16 tiles, and Scribble Dungeons declares 154 64×64 tiles.

The now-complete standalone `kenney_ui-pack` has 1,315 files and is byte-equal
to the matching bundle files. The bundle copy adds ten spritesheet PNG/XML
files. Use the bundle version as the canonical intake source and do not import
both copies.

## Admission Sequence

1. Generate a read-only catalog containing pack, file, source hash, dimensions
   or audio duration, license path, tags, and candidate DACK roles.
2. Audition the short audio deck and approve small variant groups.
3. Copy approved files and the relevant license into a dedicated
   `third_party/kenney-*` runtime subtree.
4. Create provenance entries and a strict packaging allowlist before wiring
   the new assets into default cards.
5. Add Sound Cards and a pooled audio router before admitting collision-heavy
   families.
6. Import one non-isometric visual pack at a time, beginning with a mechanic
   already present in the RAD.
7. Leave isometric/axonometric and broad 3D ingestion deferred.

Current position: the 50-file manifest, license retention, runtime admission,
Sound Card pool, audition shelf, creator approval, focused variant expansion,
and live semantic routing complete steps 2-5 for the initial deck. Full-catalog
indexing and automated packaging enforcement remain planned; future content
passes should add only roles justified by a current mechanic.

This order turns the bundle into a useful library without allowing its size to
dictate the architecture or swamp Godot's importer.
