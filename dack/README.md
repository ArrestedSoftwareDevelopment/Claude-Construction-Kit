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
- Adjust the character scale slider to match the apparent text size of the playfield; the demo defaults extra tiny.
- If the root screenshot test image is present, it is cloned into the playfield as a captured-page background and dark text bands become basic platforms.
- Endpoint-built ramps, conveyors, elevators, and ladders are live world objects for editor-authored additions; screenshot mode currently focuses on text-only terrain.
- Use **Show Sprite Pad** / **Hide Sprite Pad** to restore or collapse the editor sidebar.
- The playable scout uses Stickman Pack idle, run, and jump animation frames.
- Click one of the three actors to select it.
- Paint on the 32 x 32 pad; every actor sharing that sprite changes instantly.
- Right-click or choose **Erase** to make pixels transparent.
- Choose **Fork Selected** to give the selected actor an independent sprite.
- Choose **Reset Figure** to restore the simple procedural stick figure.
- Press **Ctrl+Alt+B** for the Boss Key.

## Asset boundary

Only assets listed in `assets/ASSET_PROVENANCE.md` may be shipped. The large
`raw base assets` vault is local and ignored by Git. License-pending experiments
belong under `assets/quarantine`, which is also ignored.
