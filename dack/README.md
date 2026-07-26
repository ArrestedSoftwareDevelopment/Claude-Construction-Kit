# DACK RAD 01 — Live Sprite Lab

This is the first Desktop Arena Construction Kit proof of concept. It targets
Godot 4.7.1 Mono and .NET 10.

## Development Environment

- Godot: 4.7.1 stable Mono, expected at `../Godot_v4.7.1-stable_mono_win64/`.
- .NET SDK: 10.0.302 or newer compatible .NET 10 SDK.
- C# target: `net10.0`.
- Godot SDK: `Godot.NET.Sdk/4.7.1`.
- Project: `dack/project.godot`.
- Local package source: `../Godot_v4.7.1-stable_mono_win64/GodotSharp/Tools/nupkgs`.
- Runtime style: direct PNG loading for curated assets and captured-page backgrounds, so smoke tests do not depend on Godot import-cache state.

## Run it

1. Start the Godot 4.7.1 Mono editor in the repository's
   `Godot_v4.7.1-stable_mono_win64` folder.
2. Import or open `dack/project.godot`.
3. Press **F5** or the project play button.

The project can also be built from the `dack` directory with:

```powershell
dotnet build DACK.csproj
```

## Prototype controls

- Use left/right arrows or A/D to move the playable scout.
- Press Space, W, or Up to jump.
- Choose **Pitfall** for horizontal platforming or **Climber** for vertical ladder play.
- Adjust the character scale slider to match the apparent text size of the playfield; the demo defaults to a small but visible roughly 32 px-tall office-platformer character.
- During gameplay, the Windows pointer is hidden. It reappears for the sprite pad and Boss Key screen.
- Press **F1** to collapse or restore the floating toolbar. The pointer is visible while the toolbar is expanded and hidden when it is collapsed for play.
- If the root screenshot test image is present, it is cloned into the playfield as a captured-page background and dark text bands become basic platforms.
- Captured documents are displayed at native 1:1 pixel resolution. Extra fullscreen space is non-play toolkit/status margin, not scaled document terrain.
- Endpoint-built ramps, conveyors, elevators, and ladders are live world objects for editor-authored additions; screenshot mode currently focuses on text-only terrain.
- The prototype opens as playfield-only real estate; use the floating toolbar to restore the sprite pad or switch playsets.
- The playable scout uses Stickman Pack idle, run, and jump animation frames.
- Use the floating playset toolbar to switch between **Platformer** and **Brickbat**.
- In Brickbat mode, detected letters or words become invisible collision objects; when struck, the cloned page visually erases that text object. The mouse controls the paddle.
- Click one of the three actors to select it.
- Paint on the 32 x 32 pad; every actor sharing that sprite changes instantly.
- Right-click or choose **Erase** to make pixels transparent.
- Choose **Fork Selected** to give the selected actor an independent sprite.
- Choose **Reset Figure** to restore the simple procedural stick figure.
- Press **Ctrl+Alt+B** for the Boss Key.

## Brickbat literary power-up sketch

These are first-pass mechanics for text-native Brickbat variants. They should read like document magic rather than arcade pickups pasted on top.

- **Footnote**: widens the paddle and adds a small trailing citation mark. Useful, readable, and forgiving.
- **Plot Twist**: sharply changes the ball's angle after the next paddle hit. Good for breaking stale rallies.
- **Second Draft**: cancels one missed ball and rewrites it back into play from the paddle.
- **Red Pen**: temporarily lets the ball erase every text object it grazes without bouncing off the first one.
- **Bookmark**: drops a temporary checkpoint; if the ball is missed, it relaunches from that marked page position.
- **Alliteration**: chains hits across nearby objects that start with the same detected letter once OCR/text identity exists.
- **Marginalia**: creates a temporary side paddle in the margin for weird two-axis saves.

## Semantic word-object concept

The recurring DACK mechanic is that text can remain readable while also becoming gameplay.

- Fast image analysis finds letters, words, lines, background regions, gutters, and margins without waiting for OCR.
- Optional OCR can run later as a slow-reveal layer, highlighting meaningful words after play begins.
- Words can become semantic objects:
  - `TARPIT` becomes a sticky hazard.
  - `LADDER` becomes a climbable tool.
  - `BRIDGE` spans whitespace.
  - `KEY` and `DOOR` become lock/unlock objects.
  - `FOOTNOTE`, `BOOKMARK`, `DRAFT`, and `RED PEN` become literary power-ups.
- Every semantic object should support text, graphic, or hybrid presentation.
- A word can summon an editor tool without trapping the creator in the word's typography. For example, `LADDER` may start on the word, then expose draggable endpoints so it can be stretched, angled, offset, or detached while remaining linked to the source word.

## Racing kit note

Racing is a natural future toolkit because the minimum authoring model is small: draw or derive a track, place a starting point, and optionally add finish/checkpoints/laps. Tracks can come from creator splines, document margins, process diagrams, spreadsheet paths, presentation arrows, or semantic words such as `START`, `FINISH`, `CHECKPOINT`, `BOOST`, `OIL`, `TARPIT`, and `SHORTCUT`.

## Sprite scale note

The current platformer target should favor a 32 px-tall gameplay sprite tier. A 64 px source is still useful for richer future art and zoomed/detail views, but thin one-pixel stick limbs do not scale down cleanly to text-sized play without aliasing into broken dots. For the RAD prototype, 32 px is the better “office document creature” size; complexity can move into letters, enemies, power-ups, and higher-detail sprites once the camera/scale tiers are stable.

## Asset boundary

Only assets listed in `assets/ASSET_PROVENANCE.md` may be shipped. The large
`raw base assets` vault is local and ignored by Git. License-pending experiments
belong under `assets/quarantine`, which is also ignored.
