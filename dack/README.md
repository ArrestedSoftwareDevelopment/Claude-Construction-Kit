# DACK RAD 01 — Live Sprite Lab

This is the first Desktop Arena Construction Kit proof of concept. It targets
Godot 4.7.1 Mono and .NET 10.

RAD status: the static screenshot proof has done its job. It proved text-as-terrain,
text-as-target, clone-only erasure, cross-playset deformation, platformer
projectiles as text mutation, reusable effects, and the live-linked sprite pad.
The next major work should move from "floating game toolbar over screenshot" to
the real editor shell: Live Desktop capture, asset shelf, draggable toolkit
parts, direct manipulation handles, and property inspectors.

GUI direction is organized in `../docs/DACK-GUI-Architecture.md`: a collapsible
construction cockpit with Play / Build / Understand moods, toolkit shelves,
direct handles, Word Sense status, and an Understanding Overlay for engine
detections, invisible logic, and clone mutations.

Asset and animation direction is organized in
`../docs/DACK-Asset-Audit-and-Sprite-Animator.md`: current approved assets,
raw-vault candidates, provenance rules, and the split between the live-linked
sprite pad and the future sprite animator/catalog module.

Level storage and sharing direction is organized in
`../docs/DACK-Level-Snapshot-Format.md`: the Snapshot workflow, `.dacklevel`
and `.dackpack` layouts, source-clone privacy policy, cached Word Sense/OCR
labels, placed toolkit objects, and mutation variants.

Object attributes are organized in
`../docs/DACK-Object-Attribute-Model.md`: shared presentation, opacity/color,
collision, motion, role, source-binding, player, enemy/NPC, obstacle, and
per-actor text-capability attributes.

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

- Press **Esc** to toggle the first DACK Cockpit overlay. This is the beginning
  of the product UI shell: Play / Build / Understand, platformer shelf,
  inspector notes, Word Sense status, and pinball asset curation notes.
- The always-on strip is intentionally shrinking into global controls only:
  Platformer, Brickbat, Reset, Cockpit, Boss. Toolkit-specific controls now
  move into contextual Cockpit pages; Brickbat owns its paddle orientation,
  letter/word grain, and reset controls there.
- The Cockpit now folds away toolkit pages that do not apply to the selected
  game type. Inspector and Understand stay available; Platformer/Brickbat/
  Pinball/Overhead shelves appear contextually to save screen real estate.
- Use left/right arrows or A/D to move the playable scout.
- Press Space, W, or Up to jump.
- Press J or X to shoot in Platformer mode. Shots travel in the scout's facing direction and erase captured-page text on impact. This started as a platformer projectile, but it is already behaving like a reusable text-mutation verb: shots can remove letters/words and alter the shared cloned terrain for later playsets. Shots also queue lazy OCR for word-shaped regions ahead of their path, making projectile targeting a natural way to prioritize which words the engine should read next.
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
- The right-side prototype now includes a tiny Character Picker: `STICKMAN` restores the admitted stick figure, while `TGC PLAYER` tests a raw-vault strip from The Game Creator's Pack. A small TGC strip editor displays detected frames in renumberable rows of eight, supports editable action labels, preset labels, editable start/end frame numbers, per-label ping-pong toggles, and add-label highlighting, proving the picker/edit/fork/label workflow for actions like idle, run, run-shoot, jump-shoot, climb-up/down, dig-up/down, shoot-up/down, bounce/stomp, turn, jump, land, hurt, and death.
- `SAVE TGC LABELS` writes a local-only `.dackanim.json` manifest under `dack/assets/quarantine/game-creators-pack-graphics-prep/`, including displayed frame numbers, internal detected indices, frame rectangles, labels, and ping-pong flags so numbering mistakes can be inspected.
- Platformer jump is now Space-only. Up remains reserved for climbing/crawling/upward movement so ladders, digging, and vertical games do not fight the jump binding.
- Swinging vines/ropes belong with the upcoming spline/Bezier tool family so they can use graceful visible arcs and draggable curve handles rather than stiff line placeholders.
- A parabola/path editor belongs on the roadmap. Parabolas are the cheap first version: start/end/apex handles for jumps, tossed objects, arcing shots, enemy hops, bounce rebounds, and power-up travel. Bezier/spline paths come after for vines, patrols, racing curves, pinball ramps, and prettier motion.
- Power-up animations can start as idle/current animation plus reusable DACK visual effects: glow, rings, sparks, color cycling, outline pulses, rotating symbols, and other ridiculous neon blessings.
- Use the floating playset toolbar to switch between **Platformer** and **Brickbat**.
- Playsets intentionally share the same cloned page state until you press **Reset** or start a new configured game. Brickbat can erase/deform text, then Platformer can inherit those holes as changed terrain.
- Use **Floor On/Off** to toggle the platformer safety floor. Floor On keeps a bottom catch surface; Floor Off allows death-plunge levels.
- In the Cockpit's Platformer Shelf, add overlay parts to the current playfield:
  ladder, ramp, slide, conveyor, elevator, checkpoint, start point, and hidden
  switch. These are DACK
  construction objects layered over the cloned page; they do not edit the
  source document or screenshot pixels. Click a placed object to select it,
  drag the center grip/body to move it, or drag its **A/B endpoint handles** to
  scale or angle it; the inspector updates live and collision follows the edited
  geometry.
- Selected toolkit objects now expose first-pass attributes in the inspector:
  speed/force, thickness/collision pad, reverse direction, and a
  **Ramp Up / Slide Down** normalization button. Elevators also expose a
  range-of-motion slider and draw their travel rail/limits in the editor.
  Objects also expose a color picker, custom-color checkbox, and opacity
  slider. Conveyors can be reversed by flipping speed direction. Slides have
  their own downhill push instead of piggybacking on generic ramp detection.
- Platformer world rules now expose separate toggles for captured text behavior:
  **Text Terrain**, **Text Crawl**, and **Shot Text Damage**. These are currently
  player/global rules; the intended product model is per-actor capabilities so
  enemies, projectiles, and the player can each decide whether text is solid,
  climbable, destructible, ignored, or semantically meaningful.
- Start points and hidden switches are the first editor-only logic objects:
  visible while the Cockpit is open, hidden during play. The latest start point
  controls the scout spawn.
- In the Cockpit's Brickbat Page, switch paddle orientation and target grain.
  This is the first contextual toolkit page, replacing the old crowded toolbar
  buttons.
- In Brickbat mode, the score/word HUD starts with automatic whitespace
  placement, but becomes draggable while the Cockpit is open. Drag the panel to
  pin it somewhere better for the level; use **Auto-Place Score** to return it
  to automatic placement.
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
- **Text collision rules**: text targets can bounce the ball, let the ball pierce through while still scoring/erasing, or switch modes inside conditional zones. In Brickbat this becomes both a creator setting and a power-up vocabulary item: ghost ball, piercing shot, hard-copy bounce, semantic pass-through, or only-bounce-on-keywords.
- **Target recipes**: letters, words, lines, headings, icons, pillboxes, selected colors, OCR-discovered words, punctuation clusters, margin notes, or manually painted regions.
- **Paddle tools**: bottom/side/top orientation, width/height, curved deflection, sticky paddle, moving/AI paddle, split paddle, or document-edge paddles.
- **Bonus deck**: choose which literary/arcade bonuses can appear, their frequency, their visual style, and whether they are geometry-triggered, OCR-triggered, score-triggered, or manually placed.
- **Laser/beam editor**: strength range, width, delay, direction, scoring, color cycling, target snapping, and whether the beam cuts text, only scores it, or temporarily reveals semantic labels. Higher laser strength should mean more reliable deletion: longer reach, wider hit band, and better snapping toward nearby text.
- **Bonus pacing**: multiball is capped at three balls and should have a visible cooldown, roughly 30 seconds in the starter rule set, so lasers and other bonuses keep rotating into play.
- **Persistence policy**: keep the damaged clone for cross-playset terrain, reset on new game, or save the deformed clone as a new level variant.
- **Visual personality**: quiet office mode, red-pen markup mode, neon/Jeff-Minter mode, monochrome terminal mode, or custom palettes.
- **Sound hooks**: simple event slots first — paddle hit, text hit, word destroyed, laser arm/fire, bonus spawn, ball lost, round won — before building a deeper audio system.

## Pinball construction kit note

Pinball belongs beside Brickbat, Racing, Platformer, and Overhead as a future construction-kit mode. The creator places flippers, a plunger lane, bumpers, rollovers, drop targets, ramps, gates, drains, nudges, bonus inserts, jackpot/multiball rules, and score logic directly onto the cloned page. Document gutters can become lanes, headings/icons/pillboxes can become bumpers or lit inserts, bullet lists can become drop targets, and semantic words can become missions or jackpots. It should reuse the same effects deck heavily: flashing inserts, analog score text, word explosions, jackpot banners, and big ridiculous neon.

Pinball should also use the textmode/BBS layer as actual table art, not just UI. Starter board skin ideas: office memo table, dungeon terminal table, neon BBS jackpot board, sci-fi system-console table, and literary word-processor table. These can be generated from open FIGlet-style banners, box drawing, CP437-like glyphs, DACK procedural borders, and carefully licensed curated art.

For the VerzatileDev starter pack, the preferred path is a batch prep/scaler, not a full importer yet. Keep the purchased originals untouched in `raw base assets/`, generate local-only scaled candidates under `dack/assets/quarantine/`, then curate a small admitted subset into `dack/assets/third_party/` only after we pick the parts, confirm provenance, and decide how each piece behaves. The helper script is:

```powershell
python tools/prep_pinball_assets.py
```

It creates two tiers for large sheets/backgrounds (`preview-1024`, `thumb-256`) and two tiers for individual pieces (`candidate-512`, `thumb-128`), plus a manifest. This gives the shelf/editor useful thumbnails and working candidates without committing huge raw art or pretending we understand pivots, collision shapes, flipper arcs, bumper radii, insert states, or table metadata yet.

Builder rules to design:

- **Table geometry**: document/page bounds, gutters, detected text blocks, manually painted rails, one-way gates, ramps, holes, kickers, lanes, outlanes, drain zones, and safe launch lanes.
- **Ball rules**: ball count, launch force, gravity/table tilt, elasticity, friction, spin/english, max speed, stuck-ball rescue, multiball cap, and whether pinball deforms the shared clone.
- **Flipper rules**: left/right/custom flippers, strength, return speed, angle limits, cooldown, keyboard/mouse binding, and visible sweep preview handles.
- **Target rules**: bumpers, rollovers, drop targets, word targets, letter banks, headings, icons, pillboxes, tables/cells, and manually painted targets.
- **Text collision rules**: textual table art can either behave as solid pins/targets, pass-through scoring ink, or conditional stateful geometry. A pinball table might make headings solid rails, body copy pass-through scoring texture, and lit jackpot words temporarily solid.
- **Scoring rules**: target value, combos, lit/unlit state, lane completion, word completion, multipliers, jackpots, hurry-up timers, and bonus count-up.
- **Mission rules**: semantic words can become modes such as `JACKPOT`, `LOCK`, `MULTIBALL`, `BONUS`, `DRAIN`, `SAVE`, `RAMP`, or `TILT`.
- **Effects/sound hooks**: bumper hit, rollover lit, ramp made, drain, ball save, tilt warning, jackpot, multiball, word completed, and table clear.
- **Construction UI**: drag handles for flipper arcs, bumper radii, ramp splines, gate direction, plunger lane strength, drain width, and insert/target lighting.

## Overhead toolkit note

Overhead is a camera/control family, not a single game type. Combat is the first preset, but the same foundation should support driving, planes/spaceships, RPG/adventure actors, animals, insects, office creatures, workers, and swarms.

Movement presets:

- **Combat/tank**: rotate, drive, shoot, ricochet, hide behind cover.
- **Driving**: steer, accelerate, brake/reverse, drift, follow roads/tracks.
- **Plane/space**: rotate, thrust, coast/inertia, wrap/bounce at boundaries, shoot.
- **RPG/adventure**: 8-way or click-to-move, interact, pick up, open, talk/fight.
- **Animals/insects**: crawl, wander, forage, flee, follow scent/trails, swarm, climb text/UI shapes.

First proof: one overhead actor on the cloned playfield with tank-style rotate/drive/fire controls, ricochet projectiles, cover/solid regions, and one simple enemy behavior.

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
- **ExplodeWord**: breaks a known word into oversized glowing letter shards, each flying along a randomized arcing/spline-like path with independent scale, spin, palette, and fade. Rectangle-only hits can use generic text shrapnel; true struck-word shards require OCR/UIA/native labels.

Style target: deliberately excessive analog/neon energy — strobing color, fading glow, rotating text, shock rings, particle sparks, and radial bursts. Each burst chooses one strong solid event color so it reads clearly, while consecutive bursts vary wildly across the arcade palette. Effects render at reduced opacity and gameplay-critical objects can redraw above them with high-contrast/inverted treatments so spectacle never hides the ball, player, or cursor-equivalent object. This is the seed of a skinnable effects deck, not a one-off Brickbat flourish.

## Textmode / BBS opaque art layer

The next visual vocabulary layer should be an opaque ASCII/ANSI/BBS-style display layer. It can cover regions of the cloned page as an intentional skin, not merely sit transparently over it. Use cases:

- RPG/Roguelike: terminal dungeon maps, monster glyphs, inventory panels, fog of war.
- Snake/Maze chase: pellet fields, maze walls, tunnel labels, power-state banners, enemy glyphs, route overlays.
- Pinball: full table-board art, backglass, score reels, lit inserts, jackpot banners, attract mode.
- Brickbat: bonus typography, title cards, target-wall skins, score panels.
- Platformer: BBS ladders, ramps, checkpoint signs, enemy glyphs, level title cards.
- Racing/Tower Defense/Action: route boards, warning signs, wave banners, squad command panels.

Source policy: generated/open textmode first. Good candidate directions include FIGlet/FIGfont-style generated banners, DACK-created CP437-like borders and glyphs, and curated ASCII/ANSI art only when license/provenance is explicit. Avoid random web ASCII art unless it has a clear redistributable license.

## Source family → toolkit affinity

The editor should recommend toolkits based on the captured source's visual grammar, not a fixed list of blessed apps. "Supported" means DACK understands useful source families and app archetypes. If Windows can render it, the capture path can usually clone it; the source-family presets simply help DACK choose a sensible first toolkit, shelf, detector, and onboarding path.

Examples matter because they reveal visual grammar: GIMP and Krita behave like raster/image editors; OpenOffice Writer behaves like a rich text document; OpenOffice Calc behaves like a spreadsheet; TextPad behaves like a plain text editor; Windows itself behaves like a desktop/icon/window playfield.

| Source family / examples | Best-fit starting toolkits |
| --- | --- |
| Windows desktop / OS shell: desktop, taskbar, dialogs, File Explorer, icons, windows | Action, RPG/adventure, Tower Defense, Collection/casual, Pinball toybox, Snake/Maze chase, Boss Key scenarios |
| Rich text / office documents: Word, OpenOffice/LibreOffice Writer, WordPad, Markdown previews | Platformer, Brickbat, RPG/Roguelike, Tower Defense, Action/Word War, Snake/Maze chase, BBS/textmode |
| Plain text / code editors: TextPad, Notepad, VS Code, terminal editors | RPG/Roguelike, Snake/Maze chase, Platformer, Brickbat, BBS/textmode, hacking/casual, Tower Defense |
| Spreadsheets / grid apps: Excel, OpenOffice/LibreOffice Calc, CSV/table viewers | Brickbat, Tower Defense, Puzzle, Snake/Maze chase, Racing routes, grid shooters, RPG tile maps |
| Slides / presentation canvases: PowerPoint, OpenOffice/LibreOffice Impress, diagram slides | Racing, Pinball, Brickbat, Action arena, Tower Defense |
| Raster/image editors: Photoshop, Krita, GIMP, Paint.NET, MS Paint | Pinball, Racing, Platformer, Space Shooter backdrop, art-board action arena |
| Vector/diagram/CAD-like apps: Illustrator, Inkscape, LibreOffice Draw, CAD canvases | Racing, Pinball, Platformer ramps, Tower Defense routes |
| Browser dashboards/web apps | Casual, Brickbat, Tower Defense, Action arena, Puzzle |
| Email/chat/task apps | Catch/Kaboom, Tower Defense, Action defense, Casual sorting, Brickbat |
| Calendar/timelines/project boards | Racing, Tower Defense, Route planning, Casual scheduling |
| PDF/document viewers | Platformer, Brickbat, RPG/BBS, Puzzle, Tower Defense |
| Paint/whiteboards | Pinball, Racing, Platformer, Action sandbox, Puzzle |
| Terminal/console/logs | RPG/Roguelike, BBS mode, Tower Defense/log defense, Brickbat |

Pinball feels most native to visual/layered/canvas sources — Photoshop, Illustrator, PowerPoint, Paint/whiteboards, and desktop/icon layouts. Word can host a pinball table, especially a BBS/text-themed one, but Word's strongest native games are text traversal, text destruction, semantic word-objects, and writing-reactive play.

## Snake / Maze chase kit note

Snake/Maze chase belongs beside RPG/Roguelike and Brickbat as a text/grid-native arcade toolkit. The generic family covers snake growth games, maze chase games, pellet collection, route planning, pursuit/evasion, tunnels, doors, power states, and grid hazards without depending on any single named classic.

Builder rules to design:

- **Maze sources**: text lines, spreadsheet cells, desktop icons, window boundaries, margins/gutters, manually painted walls, semantic words, and BBS/ASCII opaque art.
- **Collectibles**: letters, punctuation, OCR-discovered words, icons, cells, colored chips, manually placed pellets, and bonus anchors.
- **Word-goal play**: seek target words, avoid forbidden words, collect words by category, grow from approved words, shrink or poison on taboo words, and form odd phrase chains from the route taken through a document.
- **Paragraph tunneling**: the snake can carve through paragraph text as mutable terrain, eating letters before OCR is ready and then using Word Sense labels to steer toward or away from known words.
- **Actor rules**: snake length/growth, head/body collision, wrap tunnels, player speed, turn buffering, chase enemies, flee enemies, patrol enemies, and Inky/Pinky/Blinky/Clyde-like behavior presets expressed generically.
- **Power states**: temporary invulnerability, reverse chase, slow/freeze enemies, double score, text magnet, tunnel reveal, wall-eating, and clone-only terrain deformation.
- **Construction UI**: paint maze walls, mark pellets, drag tunnel endpoints, place enemy homes/spawns, define patrol/chase zones, set wrap edges, and preview route heatmaps.
- **Cross-playset mutation**: Brickbat or platformer projectiles can punch holes into a page, then Snake/Maze can inherit those holes as altered corridors, blocked lanes, or dangerous gaps until the clone is reset.

## Semantic word-object concept

The recurring DACK mechanic is that text can remain readable while also becoming gameplay.

- Fast image analysis finds letters, words, lines, background regions, gutters, and margins without waiting for OCR.
- Lazy OCR is a background service over those regions, not a blocking import step. Brickbat queues word targets near balls/paddles; Platformer queues words along projectile rays; later Pinball and Snake/Maze can queue lit targets, lanes, pellets, doors, and power words.
- End-user packaging should call this **Word Sense** or **Page Reading**. Gameplay never requires it: generic `TEXT` labels, score text, glyph shards, and toolkit-specific fallback effects stay available when OCR is missing or late.
- Provider plan: the RAD build uses command-line Tesseract because it was the fastest proof. Product builds should prefer an embedded Tesseract/libtesseract provider behind the same service boundary, with external `tesseract.exe` discovery as a fallback. Heavier PaddleOCR-style providers stay research-only for now; commercial OCR libraries are not a good core dependency for an open-source/community tool.
- The detector should find light/colored sub-headers as text when they contrast with the page, not only dark body text.
- Non-text UI/document objects such as icons, pillboxes, badges, buttons, and colored labels are bonus anchors: ideal places to attach power-ups, targets, or semantic behaviors.
- Optional OCR can run later as a slow-reveal layer, highlighting meaningful words after play begins.
- Words can become semantic objects:
  - `TARPIT` becomes a sticky hazard.
  - `LADDER` becomes a climbable tool.
  - `BRIDGE` spans whitespace.
  - `KEY` and `DOOR` become lock/unlock objects.
  - `FOOTNOTE`, `BOOKMARK`, `DRAFT`, and `RED PEN` become literary power-ups.
- OCR also enables word-goal versions of existing games: seek, avoid, collect, erase, protect, quarantine, or tunnel toward words by category. A Snake/Maze ruleset might tunnel through paragraphs looking for `KEY` or food words while avoiding `TARPIT`, `POISON`, or deadlines.
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

The floating toolbar is only the RAD quick switcher. The product UI needs a real construction shell:

- **Esc main menu**: Esc should toggle the normal DACK game/editor menu on and off. It is not the Boss Key. Boss Key hides/neutralizes DACK; Esc reveals ordinary controls. The menu can fade after N unattended seconds and return instantly on Esc, mouse movement, or interaction.
- **Source/live-capture panel**: choose desktop, monitor, window, region, image, or text grid; freeze/resample the live source; show a visible Live Desktop indicator.
- **Asset shelf / parts palette**: draggable assets with thumbnails, categories, license/source badges, and live placement previews.
- **Toolkit shelf categories**: Platformer gets text ramps, paragraph slanting, crawl surfaces, ladders, checkpoints, elevators, slides, diggable terrain, projectiles, and enemy tools; Brickbat gets scoring, target grain, power-ups, multiball/laser tuning, paddles, target recipes, and persistence; Racing gets track drawing, start/finish/checkpoints, laps, boosts, hazards, and route tools; Pinball gets flippers, balls, plunger lanes, bumpers, rollovers, ramps, gates, drains, nudges, inserts, jackpots, and multiball logic; Snake/Maze gets maze walls, pellets, tunnels, enemy homes, chase/flee presets, wrap edges, power states, growth rules, and route heatmaps.
- **Direct handles + inspector**: drag an asset onto the playfield, release to place, then immediately edit flipper arcs, ramp splines, ladder endpoints, bumper radii, patrol paths, trigger bounds, and numeric properties.
- **Gameplay-aware HUD**: score/stat panels should find whitespace, fade or slide when a ball/player/projectile approaches, and never obscure gameplay-critical objects.
- **Preflight cleanup**: Brickbat should blank letters too close to the paddle/drain/launch zone before the game starts, recorded as a reversible clone-only preflight mutation.
- **One-click play collapse**: authoring UI can collapse away for fullscreen testing, then return without resetting the cloned/deformed playfield.

## Refactor plan: RAD toolbar to product shell

Suggested build order:

1. Add an input router: Esc toggles main menu, Boss Key remains privacy escape, F1 becomes debug/RAD-only.
2. Add a UI shell controller: main menu, editor shell, toolkit overlay, HUD, fade timers, play/edit/menu state.
3. Add a HUD manager: whitespace placement, approach-radius fade, gameplay-critical redraw above effects/HUD.
4. Add Brickbat preflight cleanup: blank text near paddle/drain/launch zone before ball launch.
5. Add minimum viable asset shelf: draggable items, placement preview, source/license badge, direct handles.
6. Add source provider interface: SnapshotImageSource first, then LiveDesktopSource.
7. Move screenshot import/detection behind SnapshotImageSource so Live Desktop can share the same environmental map.
8. Start Pinball only after the shelf/handles/source-provider shell exists.

## Racing kit note

Racing is a natural future toolkit because the minimum authoring model is small: draw or derive a track, place a starting point, and optionally add finish/checkpoints/laps. Tracks can come from creator splines, document margins, process diagrams, spreadsheet paths, presentation arrows, or semantic words such as `START`, `FINISH`, `CHECKPOINT`, `BOOST`, `OIL`, `TARPIT`, and `SHORTCUT`.

Snake/Maze can share some of the same route-preview tools, but its play language is collect/grow/chase/evade rather than lap/checkpoint/time-trial.

## Crossing / Escort note

Frogger-like games fit under **Route / Flow** as Crossing / Escort presets. The construction grammar is lanes, traffic bands, moving hazards, carried platforms, safe islands, start/end markers, and timing windows. On desktop/document sources this could map cleanly to calendar rows, inbox lists, kanban lanes, spreadsheet bands, scrolling feeds, or paragraph gutters.

It is almost a self-escort mission: the player is the vulnerable object being escorted across hostile flows. Later variants can escort NPCs, files/icons, words, little workers, animals, or semantic objects across the cloned screen.

Escort and Tower Defense share a deeper **Route Conflict** model. Route actors try to traverse a path while placed tools help, hinder, protect, delay, destroy, heal, shield, or redirect them. The same module can express:

- **Tower Defense**: stop hostile waves from reaching a protected objective.
- **Tower Offense**: help friendly waves/convoys breach or reach an enemy objective.
- **Escort / Convoy**: protect one or more vulnerable travelers along a route.
- **Crossing**: time movement through hostile lanes to reach safe endpoints.
- **Process Defense**: protect a document/diagram/project flow from disruption.

The key shared setting is route polarity: defend, offend, escort, race, cross, or sort.

## Sprite scale note

The current platformer target should favor a 32 px-tall gameplay sprite tier. A 64 px source is still useful for richer future art and zoomed/detail views, but thin one-pixel stick limbs do not scale down cleanly to text-sized play without aliasing into broken dots. For the RAD prototype, 32 px is the better “office document creature” size; complexity can move into letters, enemies, power-ups, and higher-detail sprites once the camera/scale tiers are stable.

## Asset boundary

Only assets listed in `assets/ASSET_PROVENANCE.md` may be shipped. The large
`raw base assets` vault is local and ignored by Git. License-pending experiments
belong under `assets/quarantine`, which is also ignored.
