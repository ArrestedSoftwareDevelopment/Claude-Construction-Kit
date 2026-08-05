# DACK RAD — Godot Prototype

DACK turns a safe clone of an ordinary Windows screen or document into a construction-kit playfield. This Godot/.NET project is the active proof and stabilization workspace.

**Prototype baseline:** July 2026. Platformer, Brickbat, Pinball, Overhead, the Cockpit, actor/object shelves, animation editing, OCR-assisted word effects, combat, sound, direct handles, and RAD level save/load are present at proof depth. The next engineering milestone is consolidation and optimization, not a larger pile of one-off controls.

## Documentation

- Product design: [`../Desktop-Arena-Design-Doc.md`](../Desktop-Arena-Design-Doc.md)
- Documentation map: [`../docs/README.md`](../docs/README.md)
- Active engineering sequence: [`../docs/DACK-Optimization-and-Refactoring-Plan.md`](../docs/DACK-Optimization-and-Refactoring-Plan.md)
- GUI architecture: [`../docs/DACK-GUI-Architecture.md`](../docs/DACK-GUI-Architecture.md)
- Sprite Studio: [`../docs/DACK-Sprite-Studio-Mini-App.md`](../docs/DACK-Sprite-Studio-Mini-App.md)
- Brickbat rules/builder: [`../docs/DACK-Brickbat-Builder.md`](../docs/DACK-Brickbat-Builder.md)
- Snapshot/level format: [`../docs/DACK-Level-Snapshot-Format.md`](../docs/DACK-Level-Snapshot-Format.md)
- Object/card attributes: [`../docs/DACK-Object-Attribute-Model.md`](../docs/DACK-Object-Attribute-Model.md)
- Asset provenance: [`assets/ASSET_PROVENANCE.md`](assets/ASSET_PROVENANCE.md)

The README is the run/test handoff for the current build. Future design and sequencing belong in the linked documents.

## Development Environment

- Windows-first.
- Godot 4.7.1 stable Mono at `../Godot_v4.7.1-stable_mono_win64/`.
- .NET SDK 10.0.302, targeting `net10.0`.
- Godot C# SDK `Godot.NET.Sdk/4.7.1`.
- Project: `project.godot`.
- C# project: `DACK.csproj`.
- Local Godot package source: `../Godot_v4.7.1-stable_mono_win64/GodotSharp/Tools/nupkgs`.

Visual Studio is the preferred IDE, but the project builds without depending on an IDE.

## Build and Run

From the `dack` directory:

```powershell
dotnet build .\DACK.csproj
```

To run:

1. Open `dack/project.godot` in the Mono build of Godot.
2. Press **F5** or use the project Play button.

The RAD looks for the current captured-page test source in:

- `../Screenshot 2026-07-26 174658.png`
- `assets/captured/current-page.png`

If neither exists, procedural fallback terrain is used.

## Global Controls

| Action | Current RAD control |
| --- | --- |
| Open/close ordinary Cockpit or return from Sprite Studio | **Esc** |
| Toggle Build/Play and back without changing the active playset | **F6** |
| Freeze/resume the running simulation | **F7** |
| Toggle the thin RAD quick strip | **F1** |
| Boss Key | **Ctrl+Alt+B** |

`Ctrl+Alt+B` is the current test binding, not yet the settled production default. Esc is ordinary navigation; the Boss Key is the safety/privacy escape and should hide/neutralize all DACK surfaces.

At cold launch, DACK shows only a translucent logo and the `Ctrl+Alt+B` show/hide hint. Press **Esc** or **F1** to reveal the ordinary workspace.

## Play Controls

### Platformer

| Action | Control |
| --- | --- |
| Move | **A/D** or **Left/Right** |
| Jump | **Space** |
| Climb/crawl | **W/S** or **Up/Down** when touching an eligible surface |
| Shoot | **J** or **X**, when Gun is enabled |

Up is intentionally reserved for climbing/crawling rather than jumping.

### Brickbat

- Move the active paddle with the mouse.
- The standard run has three total served balls.
- Multiball may create up to three active balls and does not consume reserves.
- Letter and Word modes clear detected document targets from the working clone.
- The score/word HUD is draggable while Build/Cockpit editing is active.

### Pinball

| Action | Control |
| --- | --- |
| Charge/release plunger | Hold/release **Space** |
| Left flipper | **A** or **Left** |
| Right flipper | **D** or **Right** |

Pinball plows through document letters by default. Bounce remains an authored object/zone option.

### Overhead

- Move with **WASD** or the arrow keys.
- The current pass is a movement/actor-library seed, not yet a complete combat builder.

## Build/Edit Interaction

- Open the Cockpit with Esc.
- Select the relevant contextual page.
- Drag cards or use shelf actions to place players, enemies, objects, and toolkit parts.
- Drag actors directly on the playfield.
- For endpoint objects, drag the body to move it and the A/B handles to resize it.
- Ladders remain vertical; ramps, slides, and conveyors may angle.
- Start points, hidden switches, spawn markers, and other invisible logic are visible while building and hidden during play.
- Right-click an actor or supported world object in Build to open the movable floating Inspector. Its current actor controls edit the placed instance directly; full inherited-versus-override Card authority is still planned.
- The docked/floating Inspector edits the selected object’s applicable attributes.
- Enter Play to hide anchors/editor-only objects and honor the configured Start Point.
- Return to Build without restoring the source; clone deformation should remain until an explicit restore/new-game action.

## Sprite and Animation Editing

The current prototype contains two related surfaces:

- **Live sprite pad:** quick, constrained pixel editing bound to the selected actor.
- **Sprite Studio:** larger actor/animation workspace for choosing a source, previewing a selected action, editing frame ranges/sequences, ping-pong/reverse behavior, strobe count, names, defaults, and future behavior/projectile/effect/sound/box cards.

Current animation behavior:

- labels may use any number of frames;
- `-` in either endpoint makes an action unavailable;
- ping-pong can turn a short sequence into a forward/reverse motion;
- Strobe and count support death/damage/power-up previews;
- Save/Load animation labels uses source-aware `.dackanim.json` manifests;
- creator-tested mappings are intended to become curated source defaults.

The full Sprite Studio is transitional UI. The product target is a responsive, high-contrast owned page with a neutral preview stage, side-by-side animated/edit frames, label-click preview, frame arrows, and scrollable strips/labels.

## Implemented RAD Status

| Area | Current state |
| --- | --- |
| Captured source | Native 1:1 page pixels; spare display area is nonphysical |
| Text geometry | Letter, word, line/platform, background/whitespace, and bonus-anchor proofs |
| Mutation | Clone-only erasure, regional background replacement, letter shrapnel, cross-playset persistence |
| Platformer | Gravity, text terrain, climb/crawl, ladders, ramps/slides, conveyors, elevators, start/goal, combat, enemies, radar, projectiles, score/lives |
| Brickbat | Letter/word targets, reserve balls, multiball cooldown, laser, found-word ticker, HUD placement/drag, effects/sound, destructible actors |
| Pinball | Plunger, ball, flippers, bumpers/parts seed, drain, destructive text plow |
| Overhead | Movement family and categorized actor-library seed |
| Editor | Cockpit tabs, contextual pages, shelves, cards, movable right-click Inspector, Understand seed, handles, F6 edit/play split that preserves the active playset |
| Actors | Stickmen, Dungeon Runner, Knight, Sunny Dragon, TGC characters including Green Snake, shooter/fleet seeds |
| Animation | Source-aware import proofs, editable labels/sequences, preview, strobe, ping-pong, save/load |
| OCR/Word Sense | Optional lazy local command-line Tesseract proof with geometry-only fallbacks |
| Persistence | RAD JSON level and animation-manifest save/load |
| Audio/effects | Live semantic sound routing, 18-card/50-source Kenney CC0 shelf with pooled random-no-repeat playback and legacy fallbacks, projectile/explosion profiles, comic text and psychedelic effects |
| Multi-monitor | Monitor enumeration and move-window primitive; coordinated windows are not implemented, and the required target is one simulation with multiple bound views |

“Implemented” here means present in the RAD, not fully generalized, optimized, or cleared for public distribution.

## Current Smoke Test

1. Build the C# project successfully.
2. Launch and confirm the source page is sharp at native resolution.
3. Open/close the Cockpit and Sprite Studio with Esc; verify the pointer returns for editing. Right-click an actor in Build, move the floating Inspector, edit an instance value, and close it without changing playset.
4. In Platformer, enter Play, move/jump, fall through a real gap, then test ladder/text climbing only with a character profile whose climb capability and animation are enabled; ride a conveyor/elevator, shoot text/enemies, die with a named cause, and reach a Goal.
5. Save the RAD level, move/delete something, load it, and verify actors/objects/markers/settings return.
6. In Brickbat, verify letter and word clearing, three-ball reserve behavior, multiball cap/cooldown, laser deletion, word ticker, HUD dragging, and persistent deformation.
7. Switch to Platformer and confirm Brickbat-created gaps remain until an explicit restore.
8. In Pinball, charge/release the plunger, operate both flippers, and confirm the ball plows through letters.
9. Select several actors in Sprite Studio, including Knight; confirm Knight exposes 96 nonblank frames and named Idle/Run/Jump-Fall/Roll/Attack/Shield/Death ranges, then preview/edit labels, save/load an animation manifest, and confirm no blank/extra/wrapped frames appear.
10. Open Cockpit → Sounds; filter by family, sample cards, use Next Variant to hear the three-source families, and confirm the visible event list matches the selected card. Confirm the Builder's Sounds slot opens the same page without changing the selected actor or playset.
11. In play, verify jump/fire/hurt/defeat, Brickbat text/word/laser/drain, and Pinball launch/flipper/bumper/rollover/plow/drain produce varied but coherent cues. Temporarily remove one imported card only in a disposable test copy and confirm its legacy fallback still plays.
12. Close the Cockpit during an audition and trigger the Boss Key during another; verify audition/game audio stops and input is released.
13. From Brickbat, Pinball, Platformer, and Overhead, press F6 into Build and F6 back into Play; verify the selected playset, clone mutations, placed objects, and run state do not change merely because the mode changed.

## Known RAD Limitations

- The early Play/Edit path that forced Platformer is fixed: the current F6 transition preserves the active playset. Session, shell, input, selection, and window-layout responsibilities are still concentrated in the root controller, so this remains a required regression invariant rather than a completed architectural extraction.
- Dense pages can become slow because active text regions are repeatedly remapped/pixel-checked and some mutations update the full texture multiple times.
- UI, simulation, persistence, and Sprite Studio logic are still concentrated in the root controller.
- Cockpit/Studio pages need systematic responsive layout, scrolling, contrast, keyboard focus, and state restoration.
- Several current UI strings contain visible encoding artifacts in place of bullets, multiplication signs, degrees, ellipses, and close gadgets.
- Save Level currently targets a fixed RAD path and does not yet provide Save As, autosave/recovery, or the canonical `.dacklevel` package with separate baseline, Intake Recipe, Analysis Revision, Level Definition, Variant, and optional cache records.
- OCR currently discovers an external Tesseract executable; embedded LibTesseract is the preferred optional product provider.
- Live Desktop capture and coordinated editor/playfield windows are not implemented yet. The planned ingress is manual-refresh: capture once, build/play against the stable clone, and use an explicit `Refresh Source` transaction with diff/rebind/rollback; continuous source polling is not the default. Any second window must be another view of the same session and simulation, never a cloned level or second simulation.
- Source-specific sprite detectors are still authoring experiments. Runtime content must move to reviewed compiled manifests before the asset library is stable.
- The new 8-bit player character has not yet been proven on ladders or text-crawl surfaces; its climb capability, climb animation binding, and text-surface policy remain an active implementation/test item.
- Dragon shadow orientation/offset and occasional blank Sprite Studio previews remain active visual correctness fixes; expanded palettes and the shared two-level character picker are planned UI work.

The active correction plan and measurable exit criteria are in [`../docs/DACK-Optimization-and-Refactoring-Plan.md`](../docs/DACK-Optimization-and-Refactoring-Plan.md).

## Asset Boundary

- `raw base assets/` is a local ignored vault, not a shipping folder.
- `assets/quarantine/` is ignored and local-only.
- `assets/third_party/` is for assets with recorded redistribution evidence.
- `assets/project/` currently includes developer-test material; repository presence does not automatically make it public-build or hub-export safe.
- Public builds and playset export must filter by the distribution state recorded in [`assets/ASSET_PROVENANCE.md`](assets/ASSET_PROVENANCE.md).

Never infer a license from a folder name alone. Exact source, creator, license evidence, admitted files, attribution, and export state must be recorded before public packaging.
