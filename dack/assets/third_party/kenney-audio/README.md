# Kenney Audio Sound Card Library

This directory is DACK's deliberately small, runtime-readable Sound Card subset
of **Kenney Game Assets All-in-1 3.6.0**. It contains 50 OGG files selected in
`docs/DACK-Kenney-All-in-One-Intake.md` to exercise current gameplay and editor
sound roles without importing the complete 1,342-file catalog. The creator
approved the original 18-card deck, after which 16 high-repetition cards were
expanded to three deliberately related source variants apiece. The document/RPG
interaction and racing prototype remain single-source cards.

## Scope

- Creator and distributor: **Kenney** (`www.kenney.nl`)
- License: **Creative Commons Zero 1.0 Universal (CC0 1.0)**
- Source root: `raw base assets/Kenney Game Assets All-in-1 3.6.0/`
- Runtime root: `res://assets/third_party/kenney-audio/`
- Admission count: exactly **50 OGG files** from seven named source packs
- Transformation: byte-for-byte copy with a semantic filename only; there is
  no transcoding, resampling, trimming, normalization, channel conversion, or
  metadata rewrite
- Status: rights-cleared PUBLIC-BUILD / HUB-EXPORT library. This does not
  approve the other 1,292 gameplay files. Live default event bindings are an
  editable engine concern and do not expand this positive asset allowlist.

`MANIFEST.csv` is the authoritative one-row-per-file mapping. Its source and
runtime SHA-256 fields are deliberately identical. `LICENSES.md` indexes the
original license evidence, while `licenses/` retains a whitespace-normalized
copy of every contributing pack's complete notice.

## Runtime Layout

| Directory | Runtime role | Admitted files |
| --- | --- | ---: |
| `desert-shooter/` | Player weapon | 3 |
| `new-platformer/` | Platformer actions and combat | 15 |
| `impact-sounds/` | Paddle, rail, and bumper impacts | 6 |
| `retro-sounds-2/` | Brickbat and racing | 10 |
| `sci-fi-sounds/` | Lasers and projectile explosions | 9 |
| `ui-pack/` | Interface accept and toggle | 6 |
| `rpg-audio/` | Document/RPG interaction | 1 |

The normalized runtime names describe the current default DACK binding. They do not
erase source identity: the original bundle path, source filename, pack,
semantic candidate, byte count, license evidence, and both hashes remain in
the manifest. A runtime directory expresses the Sound Card's role, so a close
variant may originate in another one of the seven admitted source packs.

## Approved Variant Families

The following high-repetition cards have three distinct source recordings.
The unsuffixed runtime file is variant 1; `-v2` and `-v3` are the added
alternates. `MANIFEST.csv` supplies the complete source paths and exact hashes.

| Sound Card | Source recordings, in variant order |
| --- | --- |
| Player shot | `shoot-a.ogg`, `shoot-b.ogg`, `shoot-c.ogg` |
| Platformer jump | `sfx_jump.ogg`, `sfx_jump-high.ogg`, `jump-a.ogg` |
| Player hurt | `sfx_hurt.ogg`, `hurt-a.ogg`, `hurt-b.ogg` |
| Enemy/contact hit | `sfx_bump.ogg`, `impactGeneric_light_000.ogg`, `impactGeneric_light_001.ogg` |
| Enemy defeat | `sfx_disappear.ogg`, `explosion-a.ogg`, `explosion-b.ogg` |
| Power-up | `sfx_magic.ogg`, `sfx_coin.ogg`, `sfx_gem.ogg` |
| Brickbat text hit | `hit1.ogg`, `hit2.ogg`, `hit3.ogg` |
| Brickbat word break | `explosion1.ogg`, `explosion2.ogg`, `explosion3.ogg` |
| Ball lost | `lose1.ogg`, `lose2.ogg`, `lose3.ogg` |
| Brickbat laser | `laserLarge_000.ogg`, `laserLarge_001.ogg`, `laserLarge_002.ogg` |
| Paddle/light rail | `impactMetal_light_000.ogg`, `impactMetal_light_001.ogg`, `impactMetal_light_002.ogg` |
| Pinball bumper | `impactBell_heavy_000.ogg`, `impactBell_heavy_001.ogg`, `impactBell_heavy_002.ogg` |
| Space/actor projectile | `laserSmall_000.ogg`, `laserSmall_001.ogg`, `laserSmall_002.ogg` |
| Projectile explosion | `explosionCrunch_000.ogg`, `explosionCrunch_001.ogg`, `explosionCrunch_002.ogg` |
| UI accept | `click-a.ogg`, `click-b.ogg`, `tap-a.ogg` |
| UI toggle | `switch-a.ogg`, `switch-b.ogg`, `tap-b.ogg` |

## Preview Exclusion

No preview reel is admitted. The following four `Preview*.ogg` files found in
the complete bundle are explicitly outside this library:

- `Audio/Casino Audio/Preview.ogg`
- `Audio/Voiceover Pack/Preview (Female).ogg`
- `Audio/Voiceover Pack/Preview (Male).ogg`
- `Audio/Voiceover Pack Fighter/Preview.ogg`

The exclusion is name-independent at runtime: the manifest is a positive list
of the only 50 admitted source files. Adding another audio file requires a new
manifest row and provenance review.

## Integration Boundary

Nothing in this directory changes the source audio. Runtime volume, pitch,
cooldown, polyphony, looping, and spatial behavior belong to DACK Sound Cards.
Keeping the OGG files unchanged makes listening comparisons and replacements
reproducible. Gameplay emits semantic event names and resolves them to these
cards separately, so a future actor, projectile, or level card can replace a
sound without changing collision or movement code.
