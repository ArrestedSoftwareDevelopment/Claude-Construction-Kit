# DACK Level Snapshot and Package Format

## Purpose

`Snapshot` is the author-facing word for freezing a playable clone at the moment it feels right.

The creator can tune the source document, window, desktop region, image, or text map in its native app; DACK then captures a stable clone, reads what it can in the background, and packages the resulting image, geometry, OCR labels, placed tools, rules, assets, and mutation state into a level file.

This turns the current proof-of-concept loop into an intentional workflow:

1. Tune the source until the page/screen has the right visual shape.
2. Capture a Snapshot.
3. Let DACK detect text, regions, background color, icons, pillboxes, and possible bonus anchors.
4. Optionally run Word Sense/OCR in the background.
5. Place ladders, ramps, elevators, triggers, enemies, routes, pinball parts, score panels, and other toolkit objects.
6. Test and deform the clone through play.
7. Save the pristine Snapshot, the deformed variant, or both.
8. Export a shareable pack.

The key promise: the original source remains untouched. DACK always works on a clone.

## Snapshot vs. Source

A Snapshot is not the original file. It is DACK's frozen, playable representation of the source at a specific time, resolution, capture rectangle, and detection state.

For sharing, DACK should support three source policies:

- **Frame-only Snapshot:** stores the frozen playable image plus DACK geometry/rules. This is the safest default and works even when the source was a proprietary document, browser page, app window, or desktop.
- **Scrubbed source clone:** optionally includes a copy of an approved open or supported source file after metadata scrubbing and preview. This is useful for editable remix packs, but it is never required for play.
- **External source reference:** stores provenance only, such as "captured from Word window" or a creator note. This may help the author rebuild locally, but shared play still uses the Snapshot image.

Hub publishing should scrub metadata 100% by default. If a source clone is included, it must be a DACK-created clone, never the original, and the export UI must make clear exactly what is being shared.

## Editable Level vs. Published Pack

Use two related containers:

- `.dacklevel` for an editable creator project or single level.
- `.dackpack` for a distributable playset/campaign bundle.

Both can be folders during development and zip-like packages later.

Suggested editable layout:

```text
MyLevel.dacklevel/
  manifest.json
  snapshots/
    snapshot-0001/
      snapshot.json
      image.png
      geometry.json
      words.json
      mutations.json
  placed/
    objects.json
    actors.json
    triggers.json
    routes.json
  rules/
    toolkit.json
    events.json
    ai.json
  assets/
    manifest.json
    sprites/
    effects/
    audio/
  sources/
    source-manifest.json
    clones/
  provenance/
    licenses.json
    attribution.txt
```

Suggested published layout:

```text
MyPlayset.dackpack/
  manifest.json
  player/
  levels/
    level01.dacklevel/
    level02.dacklevel/
  shared-assets/
  provenance/
    licenses.json
    attribution.txt
```

The package should be playable from the frozen Snapshot data alone. Included source clones are an optional remix/editing feature.

## Manifest Fields

`manifest.json` should answer the broad questions quickly:

```json
{
  "format": "dacklevel",
  "formatVersion": 1,
  "id": "level-office-climber-001",
  "title": "Memo Climber",
  "dackVersion": "0.1-rad",
  "createdAt": "2026-07-28T00:00:00-04:00",
  "modifiedAt": "2026-07-28T00:00:00-04:00",
  "author": {
    "displayName": "Creator"
  },
  "toolkitDefault": "platformer",
  "activeSnapshot": "snapshot-0001",
  "sourcePolicy": "frame-only",
  "publishPolicy": {
    "metadataScrubbed": true,
    "includesSourceClone": false
  }
}
```

For `.dackpack`, the manifest also lists levels, shared assets, campaign order, and runtime requirements.

## Snapshot Record

Each Snapshot stores how the clone was made and what coordinate system all gameplay data uses.

```json
{
  "id": "snapshot-0001",
  "createdAt": "2026-07-28T00:00:00-04:00",
  "sourceFamily": "rich-text-document",
  "sourceAppHint": "Word/OpenOffice/TextPad/browser/etc",
  "captureMode": "window",
  "pixelSize": { "width": 1920, "height": 1080 },
  "dpiScale": 1.0,
  "coordinateSpace": "snapshot-pixels",
  "imagePath": "image.png",
  "snapshotImageHash": "sha256:...",
  "backgroundSamples": [
    { "rect": [1200, 820, 320, 160], "color": "#ffffff", "confidence": 0.94 }
  ],
  "wordSense": {
    "enabled": true,
    "status": "partial",
    "provider": "tesseract/libtesseract",
    "providerVersion": "unknown",
    "language": "eng",
    "completedAt": null
  }
}
```

Snapshots should be immutable once frozen. If the creator recaptures the source, DACK writes a new Snapshot and offers to rebind existing placed objects.

## Geometry Record

`geometry.json` stores the engine's understanding of the frozen image. It should preserve both original source geometry and current mutable geometry.

Important categories:

- `textLetters`: per-letter collision/erase regions.
- `textWords`: word-level regions, with optional links to child letters.
- `textLines`: line/paragraph/subheading bands for platforming and traversal.
- `backgroundRegions`: sampled empty regions suitable for HUDs, score panels, shelves, or safe spawn zones.
- `iconRegions`: desktop icons, app icons, bullets, pillboxes, UI chips, thumbnails, and other non-text objects.
- `fixedBoundaries`: windows, taskbars, panels, gutters, rulers, sidebars, margins, and other environmental boundaries.
- `bonusAnchors`: suggested places for power-ups, score inserts, pinball lights, or semantic triggers.
- `collisionRegions`: the current playable collision map, derived from source geometry plus mutations and placed objects.

The current proof of concept already points to this split: Brickbat erases text from the mutable clone; Platformer must treat those erased letters as holes, while Reset can restore the immutable Snapshot.

## Word Sense / OCR Record

`words.json` stores OCR results as optional upgrades, not as required geometry.

```json
{
  "words": [
    {
      "id": "word-0042",
      "rect": [604, 318, 72, 21],
      "text": "LADDER",
      "confidence": 0.91,
      "provider": "tesseract/libtesseract",
      "sourceRegion": "textWord-0042",
      "semanticTags": ["tool-candidate", "climbable"],
      "acceptedByCreator": false
    }
  ]
}
```

This lets a level ship with already-read words. End users do not need OCR installed for the authored level to play with word bonuses, word-goals, literary score tickers, or semantic hazards. OCR remains useful when the player captures new local material or the creator re-snapshots a changed source.

Design rule: OCR can add meaning, but the level must still play without it.

## Placed Objects

`placed/objects.json` stores creator-authored toolkit objects layered over the Snapshot.

```json
{
  "objects": [
    {
      "id": "ladder-001",
      "kind": "ladder",
      "start": [420, 710],
      "end": [540, 430],
      "thickness": 18,
      "presentation": "hybrid",
      "collisionProfile": "climbable",
      "sourceBinding": {
        "mode": "detached",
        "wordId": "word-0042"
      }
    },
    {
      "id": "elevator-001",
      "kind": "elevator",
      "start": [900, 820],
      "end": [900, 520],
      "speed": 64,
      "phase": 0.0,
      "collisionProfile": "moving-platform"
    }
  ]
}
```

The same schema should cover platformer ladders, ramps, slides, conveyors, elevators, checkpoints, invisible triggers, enemy spawns, spline routes, pinball rails, flippers, bumpers, racing start lines, tower-defense paths, and RPG room markers.

Endpoint editing is central. A placed object can be moved as a body or reshaped by dragging endpoints, and its saved record should reflect that directly.

## Mutations and Variants

`mutations.json` records intentional changes to the clone:

- erased letters/words;
- laser cuts;
- projectile damage;
- pixel-level destruction;
- paragraph slants/warps;
- placed text-to-graphic transformations;
- pinball dents, lit inserts, or dropped targets;
- gameplay-created holes, bridges, and hazards.

For performance, DACK can store a flattened variant image beside the mutation log:

```text
snapshots/snapshot-0001/
  image.png
  variants/
    pristine.png
    brickbat-damaged-run-003.png
```

Creators should be able to choose:

- **Reset to Snapshot:** discard current play damage.
- **Save as Variant:** preserve the damaged clone as a new level state.
- **Promote Variant:** make the damaged clone the new baseline for further editing.

This formalizes the emergent feature where one game type deforms the page and another game type inherits it.

## Snapshot Lifecycle in the UI

The Cockpit should get a contextual Snapshot page or shelf.

Recommended first controls:

- **Capture Snapshot:** freeze the current window/region/monitor/image/text map.
- **Re-snapshot:** capture the source again and attempt to preserve placed objects.
- **Freeze Baseline:** lock the current clone as the pristine reset point.
- **Run Word Sense:** start or resume background OCR.
- **Save Variant:** package current deformations as a named variant.
- **Show Understanding:** overlay detected letters, words, background regions, icons, boundaries, and OCR labels.
- **Export Pack:** create a `.dackpack`.
- **Include Source Clone:** optional, scrubbed, previewed, and off unless the creator explicitly wants remix/editable sharing.

The snapshot state should be visible but not fussy: `Draft`, `Frozen`, `Word Sense reading`, `42 words known`, `Damaged variant unsaved`, `Hub-safe`.

## Re-snapshot and Rebinding

Creators will tune source documents after testing. DACK should make this normal.

When a new Snapshot is captured from a changed source:

1. Keep the old Snapshot.
2. Detect the new geometry.
3. Match old and new regions by position, text shape, OCR label, line/paragraph similarity, and nearby anchors.
4. Preserve placed objects that map cleanly.
5. Flag uncertain bindings in the Inspector.
6. Never silently throw away a creator's ladder, route, trigger, enemy, or pinball mechanism.

This is the level-editor cousin of Live Document Mode: stable, deliberate, and creator-controlled.

## Near-Term RAD Implementation

The first useful version does not need the entire package system.

Minimum viable Snapshot save:

- save the current captured image as `image.png`;
- save detected text objects and background regions;
- save OCR labels currently known by `LazyOcrService`;
- save placed world objects;
- save active playset settings;
- save current clone mutation state for Brickbat/platformer damage;
- load the same data back into the prototype scene.

Once this round-trips, DACK has a real level format instead of a demo screenshot.

Current RAD `dacklevel` pass:

- The prototype writes `dack/levels/rad-test.dacklevel.json`.
- This is a deliberately small local test slot, not the final package directory structure.
- It saves placed world objects, Start/Checkpoint/Goal markers, object attributes, visible actors/enemies, actor names, coarse animation source IDs, text scale, actor scale, playset mode, platformer mode, and gameplay toggles.
- Loading a level returns to Editor Mode by default. Entering Play Mode is an explicit test action that honors the Start Point, hides editor-only markers, clears transient shots, resets the player, and allows enemy collision/projectile rules to run.
- It does not yet package the frozen Snapshot image, OCR cache, detected text geometry, or mutated playfield pixels. Those remain the next layer after live/snapshot source handling stabilizes.
- Actor animation source IDs are currently pragmatic (`stickman-v0.1`, `tgc-player`, `sunny-dragon-fly`) and should become stable library asset IDs before hub publishing.
- Projectile/explosion assignments should also become stable IDs. The RAD catalog lives at `dack/assets/project/effects/projectile-effect-profiles.json`; future `.dacklevel` actor/weapon records should reference profile IDs such as `explosion-b-fireball-impact` rather than hardcoded textures.
