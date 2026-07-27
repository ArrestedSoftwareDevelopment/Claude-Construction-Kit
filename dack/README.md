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
- Press J or X to shoot in Platformer mode. Shots travel in the scout's facing direction and erase captured-page text on impact. This started as a platformer projectile, but it is already behaving like a reusable text-mutation verb: shots can remove letters/words and alter the shared cloned terrain for later playsets.
- Choose **Pitfall** for horizontal platforming or **Climber** for vertical ladder play.
- In Climber mode, dense captured text can act as a crawl surface prototype: hold Up/Down while touching single-spaced text to crawl through the text row. Dedicated crawl art is still needed.
- Adjust the character scale slider to match the apparent text size of the playfield; the demo defaults to a small but visible roughly 32 px-tall office-platformer character.
- During gameplay, the Windows pointer is hidden. It reappears for the sprite pad and Boss Key screen.
- Press **F1** to collapse or restore the floating toolbar. The pointer is visible while the toolbar is expanded and hidden when it is collapsed for play.
- If the root screenshot test image is present, it is cloned into the playfield as a captured-page background and dark text bands become basic platforms.
- Captured documents are displayed at native 1:1 pixel resolution. Extra fullscreen space is non-play toolkit/status margin, not scaled document terrain.
- Endpoint-built ramps, conveyors, elevators, and ladders are live world objects for editor-authored additions; screenshot mode currently focuses on text-only terrain.
- The prototype opens as playfield-only real estate; use the floating toolbar to restore the sprite pad or switch playsets.
- The playable scout uses Stickman Pack idle, run, and jump animation frames.
- Use the floating playset toolbar to switch between **Platformer** and **Brickbat**.
- Playsets intentionally share the same cloned page state until you press **Reset** or start a new configured game. Brickbat can erase/deform text, then Platformer can inherit those holes as changed terrain.
- Use **Floor On/Off** to toggle the platformer safety floor. Floor On keeps a bottom catch surface; Floor Off allows death-plunge levels.
- In Brickbat mode, detected letters or words become invisible collision objects; when struck, the cloned page visually erases that text object. The mouse controls the paddle.
- Brickbat starts with three balls, tracks score, hits, remaining targets, and balls, and now treats bonus effects as big analog arcade typography: colorful, fading, rotating, strobing text. Laser bonuses pick a random 10-100% strength, fire after a short arming delay, and delete/score the intersected column of text.
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

Approach:

1. Geometry bonuses work without OCR: hit count, word length, paragraph region, punctuation density, or target grain can trigger Multiball, Laser, Red Pen, Bookmark, etc.
2. OCR bonuses layer on later: words such as `draft`, `quote`, `footnote`, `bookmark`, `edit`, `revise`, or `chapter` can become named targets once the slow-reveal OCR pass labels them.
3. Literary bonuses should visibly alter the cloned document: underlines, marginal notes, strike-throughs, citation marks, bookmarks, red-pen beams, or shrapnel letters.
4. Every bonus should have a quiet document-world explanation, not just arcade fireworks.

Word-shrapnel algorithm sketch:

1. When a word target is destroyed, retrieve its child letter rectangles from the text-object map.
2. For each letter, copy its current pixels from the cloned page into a small particle image.
3. Erase the original word using the connected-ink eraser.
4. Spawn each letter particle from the word center with velocity based on:
   - distance from impact point
   - letter order in the word
   - a small random angular spread
5. Simulate letter particles briefly as shrapnel:
   - collide with nearby text targets
   - fade or settle into the margin
   - optionally count as secondary hits for Red Pen / Plot Twist variants
6. Cleanup tiny leftover ink specks after the explosion pass.

## Brickbat builder direction

Brickbat should graduate from "a Breakout-like proof of concept" into a document-native game builder. The default mode can stay simple, but the toolkit should expose enough knobs that a creator can turn any page into a different kind of target-clearing game.

- **Game rules**: ball count, launch randomness, ball speed tiers, miss behavior, win condition, score multipliers, time pressure, target grain, and whether destroyed text persists when switching playsets.
- **Target recipes**: letters, words, lines, headings, icons, pillboxes, selected colors, OCR-discovered words, punctuation clusters, margin notes, or manually painted regions.
- **Paddle tools**: bottom/side/top orientation, width/height, curved deflection, sticky paddle, moving/AI paddle, split paddle, or document-edge paddles.
- **Bonus deck**: choose which literary/arcade bonuses can appear, their frequency, their visual style, and whether they are geometry-triggered, OCR-triggered, score-triggered, or manually placed.
- **Laser/beam editor**: strength range, width, delay, direction, scoring, color cycling, and whether the beam cuts text, only scores it, or temporarily reveals semantic labels.
- **Persistence policy**: keep the damaged clone for cross-playset terrain, reset on new game, or save the deformed clone as a new level variant.
- **Visual personality**: quiet office mode, red-pen markup mode, neon/Jeff-Minter mode, monochrome terminal mode, or custom palettes.
- **Sound hooks**: simple event slots first — paddle hit, text hit, word destroyed, laser arm/fire, bonus spawn, ball lost, round won — before building a deeper audio system.

## Pinball construction kit note

Pinball belongs beside Brickbat, Racing, and Platformer as a future construction-kit mode. The creator places flippers, a plunger lane, bumpers, rollovers, drop targets, ramps, gates, drains, nudges, bonus inserts, jackpot/multiball rules, and score logic directly onto the cloned page. Document gutters can become lanes, headings/icons/pillboxes can become bumpers or lit inserts, bullet lists can become drop targets, and semantic words can become missions or jackpots. It should reuse the same effects deck heavily: flashing inserts, analog score text, word explosions, jackpot banners, and big ridiculous neon.

Builder rules to design:

- **Table geometry**: document/page bounds, gutters, detected text blocks, manually painted rails, one-way gates, ramps, holes, kickers, lanes, outlanes, drain zones, and safe launch lanes.
- **Ball rules**: ball count, launch force, gravity/table tilt, elasticity, friction, spin/english, max speed, stuck-ball rescue, multiball cap, and whether pinball deforms the shared clone.
- **Flipper rules**: left/right/custom flippers, strength, return speed, angle limits, cooldown, keyboard/mouse binding, and visible sweep preview handles.
- **Target rules**: bumpers, rollovers, drop targets, word targets, letter banks, headings, icons, pillboxes, tables/cells, and manually painted targets.
- **Scoring rules**: target value, combos, lit/unlit state, lane completion, word completion, multipliers, jackpots, hurry-up timers, and bonus count-up.
- **Mission rules**: semantic words can become modes such as `JACKPOT`, `LOCK`, `MULTIBALL`, `BONUS`, `DRAIN`, `SAVE`, `RAMP`, or `TILT`.
- **Effects/sound hooks**: bumper hit, rollover lit, ramp made, drain, ball save, tilt warning, jackpot, multiball, word completed, and table clear.
- **Construction UI**: drag handles for flipper arcs, bumper radii, ramp splines, gate direction, plunger lane strength, drain width, and insert/target lighting.

## Reusable visual effects library

`PsychedelicEffects` is the first modular effects layer. Brickbat uses it now, but the intent is engine-wide reuse across platformer combat, tower defense, RPG spells, racing crashes, document events, and boss/round transitions.

Current effect vocabulary:

- **TextHit**: score text plus rings, sparks, and impact burst.
- **ImpactBurst**: reusable explosion core for text, bullets, enemies, and bonus pickups.
- **PaddleSpark**: quick contact flash for paddle/actor collisions.
- **Multiball**: big neon caption, double expanding rings, and radial starburst.
- **LaserArmed**: warning caption and charging burst.
- **LaserColumn**: full beam render plus score/miss caption.
- **RoundBanner**: oversized end-state typography with shockwaves.
- **ExplodeWord**: breaks a word into oversized glowing letter shards, each flying along a randomized arcing/spline-like path with independent scale, spin, palette, and fade.

Style target: deliberately excessive analog/neon energy — strobing color, fading glow, rotating text, shock rings, particle sparks, and radial bursts. Each burst chooses one strong solid event color so it reads clearly, while consecutive bursts vary wildly across the arcade palette. Effects render at reduced opacity and gameplay-critical objects can redraw above them with high-contrast/inverted treatments so spectacle never hides the ball, player, or cursor-equivalent object. This is the seed of a skinnable effects deck, not a one-off Brickbat flourish.

## Semantic word-object concept

The recurring DACK mechanic is that text can remain readable while also becoming gameplay.

- Fast image analysis finds letters, words, lines, background regions, gutters, and margins without waiting for OCR.
- The detector should find light/colored sub-headers as text when they contrast with the page, not only dark body text.
- Non-text UI/document objects such as icons, pillboxes, badges, buttons, and colored labels are bonus anchors: ideal places to attach power-ups, targets, or semantic behaviors.
- Optional OCR can run later as a slow-reveal layer, highlighting meaningful words after play begins.
- Words can become semantic objects:
  - `TARPIT` becomes a sticky hazard.
  - `LADDER` becomes a climbable tool.
  - `BRIDGE` spans whitespace.
  - `KEY` and `DOOR` become lock/unlock objects.
  - `FOOTNOTE`, `BOOKMARK`, `DRAFT`, and `RED PEN` become literary power-ups.
- Every semantic object should support text, graphic, or hybrid presentation.
- A word can summon an editor tool without trapping the creator in the word's typography. For example, `LADDER` may start on the word, then expose draggable endpoints so it can be stretched, angled, offset, or detached while remaining linked to the source word.

## Digging / terrain mutation note

Digging should become a shared action beside shooting, cutting, erasing, painting, and exploding. In a document clone, digging means removing or transforming collision-bearing pixels/letters/regions in the working clone only. It can serve several kits:

- Platformer: dig through text floors, gutters, walls, or manually painted dirt.
- RPG/Roguelike: tunnel through glyph caves, secret walls, or destructible map regions.
- Action/Tower Defense: create trenches, choke points, or breach routes.
- Brickbat/Pinball: turn repeated hits into crumbled lanes, pits, or scoreable holes.

Design questions: tool range, dig speed, cooldown, material hardness, whether text erases visually or toggles into a dug graphic, whether debris becomes particles/letters, and whether the mutation persists across playsets.

## Toolkit overlay note

The floating toolbar is only the quick switcher. Each genre needs a larger expandable toolkit overlay: Platformer gets text ramps, paragraph slanting, crawl surfaces, ladders, checkpoints, elevators, slides, and enemy tools; Brickbat gets scoring, target grain, power-ups, multiball/laser tuning, and target filters; Racing gets track drawing, start/finish/checkpoints, laps, boosts, and hazards.

## Racing kit note

Racing is a natural future toolkit because the minimum authoring model is small: draw or derive a track, place a starting point, and optionally add finish/checkpoints/laps. Tracks can come from creator splines, document margins, process diagrams, spreadsheet paths, presentation arrows, or semantic words such as `START`, `FINISH`, `CHECKPOINT`, `BOOST`, `OIL`, `TARPIT`, and `SHORTCUT`.

## Sprite scale note

The current platformer target should favor a 32 px-tall gameplay sprite tier. A 64 px source is still useful for richer future art and zoomed/detail views, but thin one-pixel stick limbs do not scale down cleanly to text-sized play without aliasing into broken dots. For the RAD prototype, 32 px is the better “office document creature” size; complexity can move into letters, enemies, power-ups, and higher-detail sprites once the camera/scale tiers are stable.

## Asset boundary

Only assets listed in `assets/ASSET_PROVENANCE.md` may be shipped. The large
`raw base assets` vault is local and ignored by Git. License-pending experiments
belong under `assets/quarantine`, which is also ignored.
