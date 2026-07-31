# DACK Asset Provenance

> Document status: Active asset-governance record
>
> Last reviewed: 2026-07-30
>
> Release posture: Development-only until the public-release gate passes
>
> Scope: Assets stored in, imported by, or exported from DACK

Every external asset must be listed here before it may ship. A file's directory
does not establish its redistribution rights: assets under `project/` or
`third_party/` can still be development-only, and an asset in the ignored raw
vault can become releasable only after a reviewed promotion.

For the raw-vault audit, importer design, and sprite-editor implications, see
`../../docs/DACK-Asset-Audit-and-Sprite-Animator.md`.

## Distribution tiers

| Tier | Meaning | Allowed destinations |
| --- | --- | --- |
| **RAW-LOCAL** | Original purchase/download, source art, or license-pending reference material. It remains in the ignored raw vault or quarantine. | This developer's machine only; never a repository commit, build, level package, or hub export. |
| **REPO/DEV-TEST** | A deliberately copied or transformed subset used by the RAD. It may be referenced by code and may exist in the repository, but its public redistribution record is incomplete. | Repository and private/local development builds only; excluded from public builds and hub exports. |
| **PUBLIC-BUILD** | Creator, exact source, license text/link, admitted files, transformations, and required attribution have been reviewed. | Public application builds and their generated credits. |
| **HUB-EXPORT** | PUBLIC-BUILD material whose license also permits inclusion in creator-made level/game packages. | Hub package produced from a scrubbed clone; never by modifying the creator's original source. |

Promotion is explicit and one-way per review: `RAW-LOCAL` to `REPO/DEV-TEST`,
then to `PUBLIC-BUILD`, then, where appropriate, to `HUB-EXPORT`. A later
license correction can demote an asset immediately. Generated Godot `.import`
files and importer caches inherit the tier of their source asset.

## Current classification

| Asset family | Current tier | Notes |
| --- | --- | --- |
| DACK procedural stick figure and other clearly project-created fallback art | PUBLIC-BUILD / HUB-EXPORT | Redistributable default; keep project authorship recorded. |
| 8-Bit Dungeon Tile Set Free curated PNG subset | PUBLIC-BUILD / HUB-EXPORT | CC0 record and admitted subset are present. |
| OctoPyte Stickman Pack v0.1/v0.2 admitted sheets | PUBLIC-BUILD / HUB-EXPORT | CC BY 4.0; attribution is retained even though the paid-download terms say it is unnecessary. |
| Creative Commons Sounds copied into `project/sounds/` | REPO/DEV-TEST | Folder names saying `CC0` are not sufficient provenance; exact pack pages, creators, license records, and file-to-source mapping are still required. |
| The Game Creator's Pack files copied into `project/game-creators-pack/` | REPO/DEV-TEST | Owner reports usage rights, but the precise redistribution grant, credit language, and admitted-file list are not yet recorded. |
| Legacy Collection files copied into `project/effects/` or used as actors | REPO/DEV-TEST | Local PDFs exist; each promoted subset still needs license interpretation, source mapping, and credit review. |
| Explosion Pack files copied into `project/effects/` | REPO/DEV-TEST | Local pack/license material exists, but exact creator, public source, license terms, admitted-file mapping, and attribution must be recorded. |
| VerzatileDev Pinball pack | RAW-LOCAL | Useful for design and importer testing; no runtime subset has been promoted. |
| Unknown or incompletely documented packs | RAW-LOCAL | Design/reference use only. |

This classification is the release authority. Phrases such as "cleared,"
"bought," "rights confirmed," or "CC0-labeled" are useful leads but are not a
substitute for the release record.

## Public-release and hub-export gate

A public build must fail closed unless all of the following are true:

1. The packager uses an explicit asset allowlist generated from reviewed
   provenance manifests; it never packages a directory merely because it is
   reachable from Godot.
2. Every included external asset is classified `PUBLIC-BUILD`.
3. Every hub-package asset is separately classified `HUB-EXPORT`.
4. Each entry records creator, exact public or purchase source, license
   name/version, retained license text or durable link, admitted file list,
   transformations, and required attribution.
5. Generated credits include every attribution-bearing dependency and survive
   downstream level/game export.
6. `raw base assets/`, `quarantine/`, and all `REPO/DEV-TEST` assets are absent
   from the package and its generated caches.
7. The packaged-file audit reports no unclassified files and is retained with
   the release.
8. Hub publishing clones the creator's level and source snapshot, scrubs
   document/image metadata from that clone by default, emits a warning that
   shared documents are cloned and scrubbed, and never edits the original.

Until this gate is automated and passing, DACK builds are development builds
regardless of version number or distribution channel.

## PUBLIC-BUILD / HUB-EXPORT assets

### 8-Bit Dungeon Tile Set Free

- Creator: Jamie Cross
- Creator site: <https://www.jamie-cross.net/>
- License: Creative Commons Zero (CC0 1.0)
- License text: `third_party/8-bit-dungeon/LICENSE.txt`
- Local source: `raw base assets/8-Bit-Dungeon-Tiles/`
- Admitted subset:
  - all slugged PNG object/tile files copied from `PNG Files/`, including
    blocks, bricks, cave parts, pickups, door/switch pieces, traversal pieces,
    hazards, small creatures, scenery, and window/platform parts;
  - `third_party/8-bit-dungeon/player-idle.png`;
  - `third_party/8-bit-dungeon/player-run-01.png` through
    `player-run-04.png`;
  - `third_party/8-bit-dungeon/player-climb-01.png` and
    `player-climb-02.png`;
  - `third_party/8-bit-dungeon/player-fall.png`;
  - `third_party/8-bit-dungeon/player-rope-01.png` through
    `player-rope-03.png`.
- Intended use: low-resolution office-window scenery, the climb-native Dungeon
  Runner player card, and a starter Lode Runner/RPG/office-dungeon playset.
- Export decision: CC0 permits both public-build and hub-export use.

### Stickman Pack thin character sheets

- Creator: OctoPyte
- Public source: <https://octopyte.itch.io/stickman-pack>
- Acquired: paid itch.io download on 2026-07-26
- License: Creative Commons Attribution 4.0 International (CC BY 4.0)
- License record: `third_party/stickman-pack-v0.1/LICENSE.md`
- Paid-download terms: the pack page says credit is not required when bought
  and permits commercial and non-commercial project use. DACK supplies
  attribution anyway so downstream packages retain a clear provenance trail.
- Local sources:
  `raw base assets/StickmanPack-V0.1/StickmanPack/Idle/thinIdleSheet.png`
  and `raw base assets/StickmanPack-V0.2/StickmanPack/`.
- Admitted v0.1 files:
  `thin-idle-sheet.png`, `thin-run-sheet.png`, `thin-jump-sheet.png`,
  `thin-jump-up.png`, and `thin-jump-down.png` under
  `third_party/stickman-pack-v0.1/`.
- Admitted v0.2 files:
  `thin-idle.png`, `run.png`, `jump.png`, `jump-up.png`, `jump-down.png`,
  `punch.png`, `death.png`, and `full.png` under
  `third_party/stickman-pack-v0.2/`.
- Intended use: frame extraction, playable character animation, sprite
  binding, live pixel editing, and the default Stickman 2.0 player card.
- Import note: v0.2 mixes horizontal strips and 64x64 grid sheets. Its
  source-specific importer profile uses fixed cells and skips blank cells.
- Required credit: `Stickman Pack by OctoPyte, licensed under CC BY 4.0.`
- Export decision: public-build and hub-export eligible when the credit is
  carried into the generated package credits.

### Project-created fallback

The procedural stick figure generated by DACK is project-created test art. It
is the safest redistributable default and is deliberately simple enough to edit
on the live 32 x 32 sprite pad. New project-created art should receive a stable
asset ID and authorship entry before release so it cannot be confused with raw
third-party material.

## REPO/DEV-TEST assets

The following files are intentionally useful to the RAD but are excluded from
public packaging by the release allowlist.

### Creative Commons Sounds starter deck

- Local sources:
  `raw base assets/Creative Commons Sounds/50-CC0-retro-synth-SFX/` and
  `raw base assets/Creative Commons Sounds/100-CC0-SFX/`.
- Runtime test copies: all `.ogg` files under `project/sounds/`.
- Current evidence: local folder naming asserts CC0.
- Missing release evidence: exact pack URL(s), creator(s), retained license
  text, and a mapping from every renamed `.ogg` to its original file.
- Intended evaluation use: platformer combat and Brickbat feedback.
- Promotion action: compile the file mapping and provenance records, verify
  each source, then review the exact admitted subset.

### The Game Creator's Pack

- Local source:
  `raw base assets/The Game Creator's Pack/The Game Creator's Pack/`.
- Runtime test copies: selected files and catalog under
  `project/game-creators-pack/`.
- Current evidence: the project owner reports the pack is available for DACK
  use.
- Missing release evidence: exact license/grant, creator/source, required
  attribution, and an admitted-file mapping.
- Intended evaluation use: source-specific sprite import, frame grouping,
  animation labeling, player/enemy cards, and shelf-object experiments.

### Legacy Collection

- Local source: `raw base assets/Legacy Collection/Legacy Collection/`.
- Local license records: `public-license.pdf` and
  `Legacy Collection Assets Guide.pdf` within that source folder.
- Catalog outputs:
  `quarantine/legacy-collection-prep/legacy-collection-summary.md` and
  `quarantine/legacy-collection-prep/legacy-collection-bundles.json`.
- Runtime test copy: `project/effects/legacy-enemy-death.png`; other actor/effect
  candidates may also be referenced by local development profiles.
- Missing release evidence: reviewed interpretation of the local license,
  bundle/file-level source mapping, creator/credit data, and admitted subsets.
- Intended evaluation use: effects/projectile shelves, enemy animation,
  overhead actors, and text-shrapnel behavior.

### Explosion Pack project effects

- Local source: `raw base assets/explosion pack 1/`.
- Runtime test copies:
  `project/effects/fireball-impact-explosion.png` and
  `project/effects/explosion-1-a.png` through `explosion-1-g.png`.
- Runtime test metadata: `project/effects/projectile-effect-profiles.json`.
- Missing release evidence: exact pack and creator page, license text/terms,
  original-to-renamed-file mapping, transformations, and attribution.
- Intended evaluation use: projectile, impact, explosion, and letter-shrapnel
  profile design.

## RAW-LOCAL candidate assets

### PinBall_By_VerzatileDev

- Creator: VerzatileDev
- License page: <https://verzatiledev.itch.io/license>
- Local source:
  `raw base assets/PinBall_By_VerzatileDev/PinBall_By_VerzatileDev/`.
- Local license record: `Asset(s)_License_VerzatileDev.txt` in that folder.
- License summary checked 2026-07-27: CC BY 4.0 with additional terms
  prohibiting standalone resale/redistribution and NFT/blockchain use.
- Attribution to preserve if a curated subset is promoted:
  `Assets by VerzatileDev, licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/).`
- Intended evaluation use: pinball-part taxonomy, scaling tests, and future
  curated skin experiments.
- Promotion caution: admit only small functional slices and confirm that the
  intended public/hub packaging complies with the additional terms.

### Other raw packs

`Knight`, `Props`, `Sprites`, `MountainDuskGodot`, `all_64c`, `all_spr`, and
any pack absent from the classification table remain `RAW-LOCAL`. They may
inform design and importer behavior, but they are not release inputs.
