# Desktop Arena Construction Kit

### Primary Design Document — v0.6 (Windows Only, July 2026)

*Working titles: "Desktop Arena," "Deskscape," "OS Frontier," "Chimera Construction Kit." Referred to below as **DACK** (Desktop Arena Construction Kit) for brevity — rename freely.*

---

## 1. Executive Summary

DACK is an **open-source, Windows-first office game and game-creation suite** in the lineage of **Shoot 'Em Up Construction Kit (SEUCK)** and **Adventure Construction Kit (ACK)**. It provides genre-specific toolkits (Action, RPG/Roguelike, Platformer, Space Shooter, Casual, Racing, Tower Defense, Pinball, Snake/Maze chase) with data-driven rulesets and a level editor, aimed at hobbyists and everyday PC users rather than programmers or traditional controller-first players.

The twist: **the level geometry is a safe game clone of your actual desktop and documents — and it can keep changing as you keep working.** The universal path is visual capture: if Windows can display a file or app, DACK can clone its visible frame and make it playable without understanding the proprietary file format. Later, structured importers can add richer meaning. The most distinctive path is live play while the user works: windows become boundaries, text becomes terrain, and activity can drive game events. Little soldiers, vehicles, creatures, or workers may simply arrive and explore at low intensity; higher-intensity presets turn the same space into a battle, defense, or dungeon.

The finished level can be exported as a **playset**. Early playsets ship the cloned framegrab, collision/semantic map, rules, and assets—the interactivity is the product, and a screenshot makes essentially any file usable. Native source inclusion is a later capability. When DACK does include a source file, it creates and sanitizes a clone; it **never edits, scrubs, repackages, or otherwise mutates the user's original**.

The result is part game engine, part desktop toybox, part document-to-level compiler, part live productivity companion, part construction-kit revival, and part open-source community project. It is designed for short, casual sessions at a PC and always includes a fast **Boss Key** that immediately hides or neutralizes the game layer, silences it, and returns the desktop to an ordinary work state.

Supporting notes:

- Documentation map and authority order: [`docs/README.md`](docs/README.md)
- Active optimization/refactoring plan: [`docs/DACK-Optimization-and-Refactoring-Plan.md`](docs/DACK-Optimization-and-Refactoring-Plan.md)
- GUI architecture: [`docs/DACK-GUI-Architecture.md`](docs/DACK-GUI-Architecture.md)
- Asset audit and sprite animator plan: [`docs/DACK-Asset-Audit-and-Sprite-Animator.md`](docs/DACK-Asset-Audit-and-Sprite-Animator.md)
- Level Snapshot and package format: [`docs/DACK-Level-Snapshot-Format.md`](docs/DACK-Level-Snapshot-Format.md)
- Object/player/enemy attribute model: [`docs/DACK-Object-Attribute-Model.md`](docs/DACK-Object-Attribute-Model.md)
- Brickbat builder and canonical ball/target rules: [`docs/DACK-Brickbat-Builder.md`](docs/DACK-Brickbat-Builder.md)

Current productization plateau: the RAD has proved enough separate ideas that architecture, responsiveness, and creator trust now matter more than adding another isolated button. Platformer, Brickbat, Pinball, Overhead, cards, shelves, actor imports, animation labeling, combat, OCR, effects, sound, and level save/load all exist at proof depth. The active engineering sequence is therefore **stabilize → measure → extract shared services → complete one creator loop → add Live Desktop**, while keeping every proven play loop running. The optimization/refactoring plan is the authoritative “what next” document; genre notes remain design inventories rather than competing schedules.

---

## 2. Core Pillars

1. **Familiar construction-kit UX.** Genre toolkits with parameter sheets, sprite/tile pickers, and a "test level" button — the ACK/SEUCK formula, modernized.
2. **A safe clone of the desktop and documents is the canvas.** DACK can observe pixels, window bounds, accessible text, or—in later phases—a sanitized clone of a supported native file. It never turns gameplay into a write channel back to the original.
3. **Universal screenshot path first; structured understanding later.** *Snapshot Clone Mode* and *Live Desktop Mode* prove the engine. *Native Document Mode* and *Live Document Mode* remain architectural goals, but direct `.docx`/`.psd` parsing does not gate the first useful product.
4. **Genre-agnostic core engine, genre-specific rule modules.** One physics/rendering/input core; each toolkit just supplies rules, sprites, and win/lose logic.
5. **Shareable playsets without source-file risk.** Framegrabs are valid first-class level assets. Optional native source inclusion comes later and always uses a sanitized clone. Hub publishing scrubs metadata 100% by policy, not as an optional checkbox.
6. **Writing and play as one activity, not a distraction from each other.** Ambient activity is the first-run default; challenge is an explicit preset/intensity choice. Even a game loss can only affect the cloned playfield, never the work.
7. **Creation is at least as fun as play.** The editor is a hybrid parameter-sheet-plus-event-grid toy (§10), not a settings form — precise placement, deep tunability, and instant visual feedback are first-class design goals, not an afterthought bolted onto the game engine.
8. **Modern AI thinking in a classic format.** Composable, author-tunable enemy/NPC behaviors (perception, memory, utility-based decisions) replace the old genre's fixed patrol patterns, while still staying fully no-code and preset-driven for newcomers (§10.3).
9. **Start with readable stick figures; earn visual richness.** The first art language is deliberately simple and systemic. It evolves toward the readability of Lemmings/Lode Runner and the scalable battlefield clarity of Kingdom Rush only as the engine earns those capabilities (§11).
10. **Office-native controls.** Keyboard, mouse, and web-page-like UI are the required input surface. Game controllers are not a product priority. A configurable Boss Key is always available.
11. **An RPG/Roguelike construction kit is required.** Rogue/Hack-style rooms, keys, doors, monsters, inventory, procedural layouts, and text/glyph maps are part of the product identity, not a distant genre add-on.
12. **Open source and community-built, with narrow trust boundaries.** Core image/text imports stay deliberately small. Future executable importer plugins are isolated out of process and validated at the boundary.
13. **Writing can become gameplay grammar.** Words are not merely painted scenery. A safe clone can turn written text into terrain, hazards, tools, bonuses, enemies, power-ups, and editor handles while still preserving the option to view the page as ordinary readable text.

---

## 3. Player Experience Walkthrough

1. Player opens DACK and, on first launch, sees a low-intensity ambient scene: a few stick-figure actors or vehicles enter a cloned desktop and begin exploring without demanding attention.
2. Player picks a toolkit (say, **Action Kit** or **RPG/Roguelike Kit**) and a source: **"Capture Desktop," "Capture Window/Region," "Use Image,"** or **"Use Text Grid."** Live modes and structured document import appear only when supported.
3. DACK creates a working clone and runs **auto-terrain analysis**: UI-chrome/edge detection for captures, glyph mapping for text grids, and—in later structured modes—document parsing. Proposed geometry is overlaid as translucent outlines. The source remains untouched.
4. Player enters the **Level Editor**: accepts/rejects/nudges auto-detected platforms, paints extra invisible collision where desired, places enemy spawns, collectibles, hazards, and a goal/exit. Live sources can also configure an **Activity Event Map** (§8.3)—which observed changes trigger which game events.
5. Player opens the **Ruleset panel**: sets gravity, jump height, player sprite, enemy AI type, lives, timer, win condition — parameter sliders and dropdowns, no code.
6. Player hits **Play**. In Snapshot Clone Mode, they play on the captured frame. In live modes, the engine can remain active beside or over the desktop, including a two-monitor arrangement with live work on one display and the cloned playfield on the other.
7. At any moment, the player presses the **Boss Key** to hide/neutralize the overlay, mute audio, release captured input, and restore an ordinary desktop view.
8. Player **exports a playset** containing the level/ruleset data, cloned playfield image, and assets. If a later workflow includes a native source, the packaging wizard sanitizes a clone and warns clearly that a document copy is being shared; the original is never edited.

---

## 4. Historical Positioning

| Era touchstone                                                           | What DACK borrows                                                                                   |
| ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------- |
| Shoot 'Em Up Construction Kit (1987)                                     | Parameter-driven enemy waves, weapon tables, no-code rule editing                                   |
| Adventure Construction Kit (1985)                                        | Tile-based level building from a palette, drag-and-drop entity placement                            |
| Klik & Play / The Games Factory                                          | Event/condition/action logic grid for casual toolkit rules, extended to document edits (§8.3)       |
| Mario Maker                                                              | Modern "playable level as shareable artifact," instant test-play loop                               |
| Screen-capture/compositor tools (OBS, ShareX)                            | Desktop Duplication API-based capture pipeline                                                      |
| Writing-sprint / focus tools (typing-streak games, Pomodoro apps)        | The idea that the act of producing text can drive a game loop, not just be interrupted by one       |
| Atari 2600 design vocabulary                                            | Small sets of strong verbs, readable game states, and many variations produced by recombining a few mechanics (§4.1) |
| Rogue / Hack                                                            | Text/glyph-authored dungeons, procedural rooms, inventory, keys, doors, terminal-style monster glyphs, risk, and emergent runs |
| Cannon Fodder / Syndicate                                               | Small squads operating across readable tactical spaces; autonomous agents with direct intervention   |
| Age of Empires                                                          | Workers, territory, gathering, construction, defense, and escalating activity on a living map        |
| Open-source modding communities (Doom WADs, Tabletop Simulator workshop) | Community hub model: publish, browse, remix, credit the original author                             |
| Lemmings / Lode Runner                                                   | Small, characterful, highly readable sprite/terrain art style even with a lot of placed detail      |
| Kingdom Rush                                                             | Camera/art scalability target: stunning zoomed in, still legible zoomed out to the full battlefield |

DACK's genuinely novel piece is **turning a safe visual/semantic clone of real desktop activity into procedurally-assisted, reactive game space**, then layering a classic construction-kit editor and modern agent behaviors on top.

### 4.1 Atari 2600 Play-Pattern Lens

The Atari 2600 is useful here because its best games express a complete play type with very few verbs. DACK should borrow those compact interaction grammars, not their exact content or controller assumptions.

| Reference play type | Reusable mechanics | Natural office-app playfield mapping |
| --- | --- | --- |
| **Combat / Tank-Pong** | Move, rotate, shoot, ricochet, hide, duel | Window rectangles become rooms and ricochet walls; toolbar groups become cover |
| **Breakout / Super Breakout** | Deflect, aim, clear, catch, reveal | Spreadsheet cells, icon grids, slide thumbnails, or paragraph blocks become bricks |
| **Adventure** | Explore, carry one item, unlock, evade, retrieve | Folder trees, document headings, or text-grid rooms become a key/door dungeon |
| **Asteroids** | Rotate, thrust, drift, shoot, split, evade | Floating thumbnails or detached UI regions become drifting obstacles in open canvas space |
| **Missile Command** | Point, intercept, defend limited assets, survive waves | Protect taskbar icons, document sections, pinned files, or named spreadsheet ranges |
| **Yars' Revenge** | Breach a cellular shield, charge, fire, retreat to safety | Table cells or text blocks form a barrier around a protected app/objective |
| **Surround** | Draw trails, claim space, trap, avoid collision | Whiteboards, slide canvases, and blank document margins become territorial surfaces |
| **Warlords** | Deflect projectiles, defend a corner fort, local multiplayer/AI | Four window quadrants or dashboard panels become forts around a shared center |
| **Pitfall!** | Run, jump, climb, swing, collect, route-plan | Long documents, timelines, and horizontally scrolling boards become traversal courses |
| **Dig Dug** | Dig, tunnel, pump, drop rocks, manipulate pursuit paths | Character grids, spreadsheets, and tables become destructible tunnel fields |
| **Frogger** | Time crossings across moving lanes | Calendar rows, inbox lists, kanban lanes, or scrolling feeds become traffic bands |
| **River Raid** | Navigate a narrowing corridor, shoot, refuel, scroll | Long documents, process maps, and vertically scrolling feeds become constrained routes |
| **Kaboom!** | Track and catch accelerating falling objects | Notifications, falling file icons, review comments, or task cards become catch targets |
| **Centipede** | Shoot a segmented threat whose path changes around obstacles | Cell grids and icon fields become reactive obstacle lattices |
| **Racing / Slot-Car** | Follow a track, steer, drift, checkpoint, lap, time-trial | User-drawn tracks, document margins, flowcharts, process diagrams, spreadsheet paths, or semantic words become courses |
| **Tower Defense** | Place, upgrade, route, slow, defend waves | Paragraph paths, spreadsheet lanes, process diagrams, and document outlines become enemy routes and tower sites |
| **Pinball** | Launch, flip, bump, roll, nudge, drain, multiball, jackpot | Paragraph gutters become lanes; headings/icons/pillboxes become bumpers, rollovers, targets, kickers, and bonus inserts |
| **Snake / Maze Chase** | Collect, grow, route, chase, evade, tunnel, power-up | Text grids, spreadsheet cells, desktop icons, windows, gutters, margins, and UI panels become mazes, pellets, ghosts, portals, and safe/unsafe corridors |

The especially valuable lesson from manuals for **Combat**, **Adventure**, **Asteroids**, **Breakout**, **Missile Command**, and **Yars' Revenge** is variation: maze shape, projectile behavior, visibility, speed, limited resources, and win condition can recombine into many games without creating a new engine system for every preset. DACK's vocabulary (§10.5) should work the same way.

---

## 5. Core Innovation: The Desktop (and Documents) as Playfield

### 5.1 Capture Pipeline (Screen-Based Sourcing)

- **Windows Desktop Duplication API (DXGI)** for fast, low-latency full-desktop or per-monitor capture (this is what OBS/game-capture software uses under the hood).
- Every capture becomes a **DACK-owned working clone**. Cropping, color-keying, metadata removal, collision painting, and terrain dressing operate only on that clone.
- Two capture targets:
  - **Full desktop composite** (everything visible, including overlapping windows) — simplest, matches what the player sees.
  - **Per-window capture** (`PrintWindow` / `Windows.Graphics.Capture` for a specific app) — cleaner if the player wants, say, just their Excel sheet as a level without other clutter behind it.
- Multi-monitor aware: capture one screen, all screens stitched, or let the player pick a region (marquee select, like a screenshot tool).
- **Native-pixel fidelity by default.** Snapshot playfields render the captured source at 1:1 pixels rather than fit-scaling the document and blurring text. Unused monitor space is nonphysical editor/HUD margin unless a creator explicitly adds geometry there. Source, Snapshot, playfield, window, monitor, and per-monitor-DPI coordinates are separate transforms and must never be inferred from one global scale.

### 5.2 Auto-Terrain Extraction (Screen-Based)

Turning a screenshot into "this is a platform, this is a hazard, this is empty space" is layered so simpler heuristics run first and expensive ones only run if needed. The key platformer principle is that **visible text must be playable by itself**; ladders, ramps, elevators, conveyors, triggers, and checkpoints are editor-authored additions rather than prerequisites for basic traversal.

1. **Contrast/background geometry pass:** build one reusable analysis product from local contrast and regional background estimates, not a hardcoded “black on white” assumption. It finds anti-aliased/different-color text, glyph components, words, lines, gutters, whitespace/background zones, icons, pillboxes, and other high-confidence UI regions. The first dark-band detector remains a historical proof, not the target algorithm.
2. **Stable environmental objects:** every detected region receives an ID, bounds/mask, authority/confidence, source/background metadata, active/deleted state, and spatial-index entry. Collision, erasure, OCR binding, HUD avoidance, and every toolkit query the same records.
3. **UI-chrome evidence (cheap, high-value):** query the **Windows UI Automation (UIA) tree** for real bounding boxes and accessible roles/text where available, then reconcile those signals with image regions rather than building a separate physics world.
4. **Edge/contour evidence (fallback):** for regions UIA cannot describe, use lightweight contours/rectangle fits or creator-approved component grouping to propose additional objects.
5. **Creator correction layer:** all detected geometry is visible in Understand mode and can be accepted, rejected, split, merged, rebound, or replaced. Detection proposes; the editor disposes.
6. **Manual-only fallback:** hand-paint collision/regions directly onto the DACK clone, like a traditional construction kit.

The analysis runs at capture/Snapshot time or as bounded incremental work after a live-source change. Gameplay never re-scans the whole image just to ask whether a nearby word or platform still exists. See ADR-0009.

### 5.2.1 Semantic Word-Objects

The text pipeline has two layers that should remain separate:

1. **Fast geometry layer:** image analysis finds text pixels, letters, words, lines, background regions, gutters, and margins without caring what the words mean. This layer powers immediate collision, Brickbat targets, platformer surfaces, erasure/mutation, and "white space is empty space" behavior.
2. **Optional meaning layer:** OCR, UIA accessible text, or native importer text later labels the geometry with actual words, confidence scores, and semantic tags. This pass can happen after play begins; slow arrival is a feature, not merely latency.

Platformer projectiles exposed an important general rule: any gameplay verb that can hit text should be able to publish a **text mutation event** against the working clone. A bullet, ball, laser, drill, spell, pinball bumper, or explosion may remove letters/words, weaken terrain, reveal background, create gaps, or trigger semantic effects. This is not limited to Brickbat; it is part of the shared terrain/deformation vocabulary.

Visual effects must respect the same distinction. A rectangle-only word hit can produce generic text shrapnel, score text, and impact bursts immediately. Exploding the *actual struck word* into its own component letters requires the optional meaning layer: OCR, UIA accessible text, or native text labels attached to the geometry. Once labels exist, Brickbat, Pinball, Platformer shots, RPG spells, and Snake/Maze pickups can all reuse true word-shard effects.

OCR should be **lazy, local, and gameplay-prioritized**. DACK does not need to OCR a whole page before play. Instead, each toolkit should predict which regions are likely to matter next and queue those first: Brickbat reads word targets near the paddle and ball path; Platformer reads words along the player's facing/projectile ray; Pinball reads lit lanes, rollover banks, and ball-near targets; Snake/Maze reads nearby pellets, doors, and power words. Tesseract is the preferred first spike because it is free/open and can run offline, but it should remain behind a swappable service boundary so UIA/native labels or another OCR provider can replace or supplement it later.

End-user packaging should present this as **Word Sense** or **Page Reading**, not as a scary OCR subsystem. The product rule is:

- **Geometry is always enough to play.** OCR is never required for collision, erasure, Brickbat, Platformer, or playset loading.
- **Fallback text/effects always exist.** Unknown words can display generic `TEXT`, score captions, glyph shards, punctuation sparks, or toolkit-themed fallback labels.
- **Local meaning is an upgrade.** If a local OCR provider is available, DACK upgrades nearby word-shaped regions into real labels, enabling found-poem score tickers, true word-shard explosions, semantic word bonuses, and word-object suggestions.
- **No cloud OCR by default.** Reading the user's page is privacy-sensitive. Cloud OCR requires an explicit opt-in provider and clear warning; the default route is local/offline.
- **Status must be legible.** The UI should show states such as `Word Sense: Off`, `Local reader unavailable`, `Reading nearby words`, or `23 words known`.

OCR provider strategy:

1. **Prototype provider: command-line Tesseract.** Fastest integration, easy to inspect, no compile-time dependency, and enough to prove lazy gameplay-prioritized reading. This is the current RAD path.
2. **Preferred product provider: embedded Tesseract/libtesseract behind the same service boundary.** Tesseract has an API and Apache 2.0 license, so DACK can integrate it internally via native bindings or a .NET wrapper if packaging, language-data size, thread isolation, and redistribution are validated. This should feel built-in to the user even if the implementation still uses native DLLs and tessdata files under the hood.
3. **Fallback provider: external executable discovery.** If the embedded provider is absent or fails, DACK can still discover an installed `tesseract.exe` in common locations or PATH. This keeps open-source builds flexible.
4. **Heavy/advanced provider: PaddleOCR-style ML OCR.** Keep as a research option for difficult layouts, handwriting-like sources, or multilingual packs, but it is probably too large/heavy for the Stage 1/2 office-game loop.
5. **Avoid commercial closed OCR as a core dependency.** Commercial .NET OCR libraries may be convenient, but licensing/redistribution costs conflict with DACK's open-source community goal. They can be documented as optional private integrations, not the default engine.

When enabled, OCR can become a play mechanic: the level starts with raw document physics, then words gradually become highlighted as the engine "reads" the page. Discovered terms can entice the player with bonuses, hazards, or transformations:

- `TARPIT` → sticky hazard / slowing platform.
- `LADDER` → climbable tool.
- `BRIDGE` → connector over whitespace.
- `DOOR` / `KEY` → lock-and-unlock pair.
- `FIRE`, `ICE`, `BOUNCE`, `CONVEYOR`, `ELEVATOR` → physical modifiers.
- `GHOST`, `GRUE`, `MONSTER`, or names/proper nouns → enemy spawns.
- `FOOTNOTE`, `BOOKMARK`, `DRAFT`, `QUOTE`, `RED PEN` → literary power-ups.

This also creates **word-goal variants** of existing games. A ruleset can ask the player to seek, avoid, collect, erase, protect, quarantine, or tunnel toward words by category: find all verbs, avoid negative words, eat every `TODO`, rescue proper nouns, hunt `KEY` before `DOOR`, or steer clear of `TARPIT`. The geometry layer keeps the level playable immediately; Word Sense gradually upgrades raw word rectangles into named objectives as the local reader catches up.

This creates a signature design promise: **the creator's writing is the map, the rules, and the monsters.** A sentence such as "The hero crossed the BRIDGE, avoided the TARPIT, climbed the LADDER, and found the KEY" can become a playable micro-level without the user writing code.

Semantic objects must preserve a dual identity:

- **Text face:** the original word remains readable and playable as text.
- **Graphic face:** the word toggles into an equivalent sprite/tile/effect.
- **Hybrid face:** the word remains readable while gaining visual behavior, such as tar bubbles behind `TARPIT` or rung handles over `LADDER`.

The Boss Key and safe-preview modes can always force text/plain-document presentation. The transformation is applied to the DACK clone only; originals remain untouched.

### 5.2.2 Word-Summoned, Editor-Authored Tools

Semantic words should not trap the creator inside the typography. A word can summon a tool, while the editor lets the creator place that tool.

Example: `LADDER`

- Default behavior: the word itself is climbable.
- Drag endpoints: the creator moves the top/bottom endpoints and adjusts a bounded width, but the ladder remains vertical. Angled/curved traversal belongs to ropes, vines, rails, ramps, and spline tools.
- Presentation toggle: text, graphic ladder, or hybrid word-plus-rungs.
- Binding mode:
  - **Bound to word:** follows the source word exactly.
  - **Offset from word:** behavior is moved but remains linked to the source word.
  - **Detached but linked:** behavior becomes a normal placed tool with provenance back to the source word.

The same pattern applies to `BRIDGE`, `CONVEYOR`, `ELEVATOR`, `DOOR`, `CHECKPOINT`, `TARPIT`, and other toolkit primitives. This is the bridge between automatic document magic and construction-kit authorship.

### 5.3 Four Modes of Level Sourcing, Delivered in Order

- **Snapshot Clone Mode (engine baseline):** the desktop, window, region, or image is cloned once and frozen as a static background/level asset. It is deterministic, shareable, safe, and works with any visible file.
- **Live Desktop Mode (second):** DACK renders a cloned playfield beside the desktop or a transparent overlay above it. Geometry re-samples on window-move events so actors understand moving/resizing window boundaries. The game may *simulate* a window being breached, disabled, or closed inside the clone; it never sends that action to the real window.
- **Native Document Mode (later):** a supported, sandboxed importer derives geometry from a cloned file's structure. This is enhancement, not the universal compatibility layer.
- **Live Document Mode (later, no add-in required):** DACK observes accessible text, selection, save, and layout signals and updates the cloned level in near-real time. The game never writes into the document. An app-specific add-in is considered only after a demonstrated use case cannot be served by capture/UIA.

RAD milestone note: the static screenshot prototype has achieved its purpose. It proved text-as-platform, text-as-target, clone-only erasure, cross-playset deformation, reusable projectiles/effects, multiple play families, and a first editor shell. It remains the deterministic smoke-test path. The active plateau is to stabilize and extract the shared session/UI/environment/asset/persistence spine; **Live Desktop Mode and coordinated two-monitor editing are the next product validation path built on that spine**, not a second parallel engine.

**Snapshot** is the authoring version of Snapshot Clone Mode. It is the moment the creator says: this document/window/desktop region now works as a level; freeze the playable clone, save the detected geometry, cache any background OCR/Word Sense results, and let me build on that stable artifact. The creator can keep editing the real source and re-snapshot later, but the level itself is grounded in a DACK-owned clone that can be reset, damaged, promoted to a variant, or exported without touching the original.

The level/package format for this lives in `docs/DACK-Level-Snapshot-Format.md`. In short, a `.dacklevel` stores the frozen image, geometry, OCR labels, placed tools, rules, assets, source-clone policy, and mutation variants. A `.dackpack` bundles one or more levels for sharing. Hub publishing uses scrubbed clones only; metadata scrubbing is mandatory and cannot be disabled.

### 5.4 Source-Family / App-Archetype Affinity Matrix

DACK should not pretend "supported apps" means a brittle allowlist of individual programs. If Windows can render a thing, DACK can usually capture a safe clone of it and make that clone playable. Support should therefore be described as **source families** or **app archetypes**: office documents, plain text editors, image editors, drawing canvases, the Windows desktop itself, and so on.

Specific apps such as GIMP, Krita, OpenOffice, TextPad, Photoshop, PowerPoint, Excel, browsers, and File Explorer are useful examples because they tell us what visual grammar to expect. They are not the deeper contract. The deeper contract is: what does the visible source look like, what shapes does it expose, and what kind of game does that shape naturally want to become?

The source family's native visual grammar should suggest the first few game types, shelf defaults, auto-detection rules, and onboarding presets. The creator can override anything, but recommendations should feel obvious.

| Source family / examples | Native visual grammar | Strong toolkit affinities | Why it fits |
| --- | --- | --- | --- |
| **Windows desktop / OS shell**: desktop, taskbar, Start/menu surfaces, dialogs, File Explorer, icons, thumbnails, open windows | spatial layout, icons, windows, panels, menus, drag targets, taskbar, notifications | Action, RPG/adventure, Tower Defense, Collection/casual, Pinball toybox, Snake/Maze chase, Boss Key scenarios | Windows itself is the universal playfield. Icons are pickups/loot/enemies/targets; folders become rooms/portals; windows become buildings, cover, boundaries, bosses, maze walls, or objectives. |
| **Rich text / office documents**: Microsoft Word, OpenOffice/LibreOffice Writer, WordPad, Markdown previewers | paragraphs, headings, margins, gutters, words, footnotes, selection/caret | Platformer, Brickbat, RPG/Roguelike, Tower Defense, Action/Word War, Snake/Maze chase, BBS/textmode | Text becomes terrain, targets, semantic objects, dungeon glyphs, routes, bonuses, maze corridors, pellets, and writing-reactive events. |
| **Plain text / code editors**: TextPad, Notepad, VS Code, terminal editors | monospace or near-monospace text, line structure, gutters, indentation, selections | RPG/Roguelike, Snake/Maze chase, Platformer, Brickbat, BBS/textmode, hacking/casual, Tower Defense | Lines, columns, words, indentation, and glyphs can become clean collision, maps, ladders, routes, pellets, enemies, and semantic triggers. |
| **Spreadsheets / grid apps**: Excel, OpenOffice/LibreOffice Calc, CSV/table viewers | cells, tables, rows/columns, totals, charts, formulas, filters | Brickbat, Tower Defense, Puzzle, Snake/Maze chase, Racing/slot routes, Centipede/grid shooters, RPG tile maps | Strong grid structure supports bricks, lanes, tower routes, maze cells, tile dungeons, cell-based enemies, and puzzle rules. |
| **Slide / presentation canvases**: PowerPoint, OpenOffice/LibreOffice Impress, diagram slides | canvases, shapes, connectors, diagrams, thumbnails, title/body regions | Racing, Pinball, Brickbat, Action arena, Tower Defense, presentation minigames | Shapes and connectors become tracks, ramps, bumpers, rails, targets, routes, arenas, and staged encounters. |
| **Raster/image editors**: Photoshop, Krita, GIMP, Paint.NET, MS Paint | layers, masks, selections, brushes, color regions, composited art | Pinball, Racing, Platformer, Space Shooter backdrop, art-board action arena | Visual art and layer-like regions suit table boards, ramps, masks, collision painting, parallax, and decorative skins more than text-native semantics. |
| **Illustrator / Inkscape / vector/CAD-like apps** | paths, splines, bezier curves, fills/strokes, symbols, artboards | Racing, Pinball, Platformer ramps, Tower Defense routes, Puzzle | Vector paths naturally become tracks, rails, flipper arcs, ramp splines, routes, and precise draggable construction handles. |
| **Browser / web apps / dashboards** | cards, buttons, nav bars, scroll regions, forms, charts | Casual, Brickbat, Tower Defense, Action arena, Puzzle | UI cards and widgets become targets, lanes, objectives, towers, and readable interaction zones. |
| **Email / chat / task apps** | message lists, threads, avatars, timestamps, unread badges, notifications | Catch/Kaboom, Tower Defense, Action/defense, Casual sorting, Brickbat | Items arrive, queue, escalate, or need triage; unread markers become enemies/objectives/bonuses. |
| **Calendar / timeline / project boards** | rows, columns, timeslots, lanes, cards, milestones | Racing, Tower Defense, Route planning, Casual scheduling, Action defense | Time/lane structure suggests tracks, waves, checkpoints, patrols, deadlines, and route defense. |
| **PDF/document viewers** | pages, scrollable text/images, annotations, forms | Platformer, Brickbat, RPG/BBS, Puzzle, Tower Defense | Similar to Word for visible text, but usually less live-reactive; great for snapshot playsets and annotation-themed games. |
| **Paint / whiteboard / drawing canvas** | blank space, strokes, shapes, sticky notes, freehand paths | Pinball, Racing, Platformer, Action sandbox, Puzzle | User-created lines and shapes are ideal for manual rails, ramps, tracks, collision, and toybox construction. |
| **Terminal / console / logs** | monospace text, prompts, columns, ASCII UI, streams | RPG/Roguelike, BBS mode, Tower Defense/log defense, Brickbat, Hacking/casual | Already textmode; maps naturally to glyph dungeons, terminal monsters, command panels, and scrolling hazards. |

Design principle: **recommend by source grammar, not by file extension alone, and never lock the creator out of a weird idea.** Any toolkit should be applicable to any captured screen. The affinity matrix only chooses good defaults: a Word document full of diagrams might recommend Racing or Pinball; a Photoshop canvas full of text might recommend Brickbat; an Excel sheet used as a dungeon map should recommend RPG. The source detector should therefore combine app identity, geometry, text density, grid/path/layer signals, and creator intent, then let the creator force any playset onto any source.

Pinball's natural home is visual/layered/canvas-heavy material: Photoshop, Illustrator, PowerPoint, Paint/whiteboards, and desktop/icon layouts. It can work on Word, especially for BBS backglass or text-themed boards, but Word's strongest native games remain text traversal, text destruction, semantic word-objects, and writing-reactive play.

---

## 6. Frozen Import Surface & Future Importer Architecture

Importer breadth is explicitly frozen until the engine, interaction model, collision semantics, Boss Key, and playset loop are good. The screenshot path is not a fallback of last resort; it is the universal compatibility feature.

“Import” refers to three different boundaries and they must not be conflated:

| Boundary | Current policy | Trust model |
| --- | --- | --- |
| **Playfield/source ingress** | Frozen to capture, common raster images, and plain text/glyph maps during the engine phase | Reviewed core decoders; future native/community document importers run out of process |
| **Capture/Snapshot analysis** | Actively improving: pixels/UIA evidence become text, background, icon/pillbox, and environmental regions | Trusted DACK core working only on a clone; cached/versioned output; no executable source content |
| **Creator asset compiler** | Actively improving for sprites, sheets, explosions, sounds, and object art | Local, provenance-aware authoring tool that produces explicit reviewed manifests; raw candidates are not automatically export-safe |

Runtime actor/object loading is a fourth, deliberately boring step: it consumes compiled manifests and admitted assets. It does not rerun heuristic blob detection or inspect the raw vault. Therefore the sprite-import overhaul does not weaken the frozen source-import policy.

### 6.1 Engine-Phase Import Set

- **Captured pixels:** desktop, monitor, window, or selected region.
- **Open raster images:** `.png`, `.jpg`/`.jpeg`, and `.bmp`, normalized into a DACK-owned PNG clone. Animated formats are deferred until animation semantics are intentional.
- **Plain text/glyph maps:** `.txt` and simple Markdown text, interpreted through a configurable glyph legend. For example, `W` or `#` can mean wall, `D` door, `.` floor, `E` enemy, and `@` player spawn.
- **Clipboard/text selection:** a user can copy a monospaced block from Word or another editor and create a dungeon without DACK parsing the native file.
- **Everything else:** open it in its normal app and use **Capture Window/Region**. If Windows can render it, DACK can make its frame playable.

Direct `.docx`, `.psd`, `.pptx`, `.xlsx`, and `.pdf` structure parsing is **not part of the engine phase**. Legitimate native-file inclusion/export is reconsidered after the interactive engine is compelling.

### 6.2 Future Structured Importer Contract and Trust Boundary

When structured importers return, each consumes a read-only clone and emits a versioned, declarative region tree (`textBlock`, `heading`, `image`, `table`, `pageBreak`, `layer`, `shape`, `cell`, etc.) with bounds, z-order, and semantic tags.

- Built-in image/text decoders may run in process because they are part of the reviewed core and accept the frozen formats above.
- **Every third-party/community importer runs in a fully sandboxed subprocess.** It receives only the cloned input, a temporary working directory, a schema version, and explicit resource limits.
- The importer has no network access, no access to the original path, no arbitrary filesystem writes, and no ability to return executable objects—only data matching the validated schema.
- Timeouts, memory limits, archive-expansion limits, malformed-output rejection, provenance/version recording, and deterministic fallback to a framegrab are required.
- A short security spike must validate the sandbox and hostile-file test corpus before the first executable importer is enabled. A `.psd` importer does not set this pattern by accident; it waits for the pattern.

### 6.3 Glyph Map / Text Dungeon Mode

Text is the one structured source worth supporting early because it is open, simple, and directly serves the required RPG/Roguelike Kit. The editor shows two synchronized views:

- **Text view:** the original characters remain readable and editable in DACK's clone/editor.
- **World view:** mapped characters render as walls, floors, doors, actors, items, and triggers.

The creator can toggle between views or blend them, so a `W` can visibly remain a letter, become a wall sprite, or show both. Mapping is per playset and accepts words as well as characters (for example, `[SHOP]`, `[BOSS]`, or a heading line).

---

## 7. Playsets: Bundling Safe Clones & Interactive Levels

A playset is first and foremost the portable interaction: cloned playfield, geometry/semantics, actors, rules, and assets. It does not need the creator's original file to be legitimate. Native source clones are an optional later enhancement, never a requirement for play.

### 7.1 What a Playset Is

A playset is a distributable folder/zip with a predictable layout:

```
MyPlayset.dackpack/
├── manifest.json          (toolkit, version, author, license, level list)
├── player/                (lightweight DACK Player runtime, optional — see §7.4)
├── playfields/
│   ├── desktop-clone.png  (sanitized framegrab used by the level)
│   └── dungeon-map.txt    (optional open text/glyph source)
├── levels/
│   ├── level01.dacklevel  (geometry + rules referencing playfields/desktop-clone.png)
│   └── level02.dacklevel  (geometry + rules referencing playfields/dungeon-map.txt)
├── sources/               (future, optional: scrubbed clones only; never originals)
└── assets/                 (toolkit sprites, sfx, fonts used by the ruleset)
```

The current package shape should evolve around **Snapshots** rather than loose framegrabs. Each level owns one or more `snapshots/` folders containing the frozen image, detection results, OCR/Word Sense cache, and optional mutation variants. The framegrab remains the universal playable surface; the Snapshot adds enough structure for reliable editing, rebuilding, sharing, and cross-playset deformation.

The engine phase intentionally ships the framegrab or open text map. When native-source inclusion is later supported, DACK first creates a private working clone, scrubs it according to the export policy, previews exactly what will be shared, and writes only the clone into `sources/`.

### 7.2 Multiple Levels from Multiple Sources

A playset's `manifest.json` lists an ordered set of levels, each of which can reference a different capture, image, text map, or later sanitized source clone. This gives a simple campaign structure without requiring recipients to own or open the originating apps.

### 7.3 Multiple Levels from One Source

Long sources can still yield several levels during the engine phase:

- capture several pages, slides, sheets, windows, or regions as ordered frames;
- divide one tall/wide framegrab into manually or automatically detected regions;
- split a text map at explicit markers such as `--- LEVEL ---`;
- later, let sandboxed structured importers suggest headings, pages, layers, artboards, sheets, or named ranges.

The Level Editor exposes this as a **Split View**: a thumbnail/region strip assigns captured or detected sections to level slots and lets the creator reorder them.

### 7.4 The DACK Player (Lightweight Runtime)

Because a playset should be playable without the recipient owning the full editor, DACK ships a **free, open-source, minimal "Player" build**: no editor UI, just capture/import + render + physics + input, driven entirely by the bundled `.dacklevel` files. The full editor remains the primary distribution for creators; the Player is the "just play it" distribution for everyone else, and can optionally be embedded inside the playset itself (per the `player/` folder above) so a playset is double-clickable and self-contained even for someone who has never installed DACK.

### 7.5 Rebuilding a Level After Its Source Changes

When a creator recaptures a changed source or updates an included text/source clone, DACK supports an explicit **"Re-snapshot / Rebuild Level"** action: derive a new region tree, diff it against the level's previous tree, and:

- keep hand-placed entities/rules that map cleanly to unchanged regions,
- flag entities anchored to regions that moved or disappeared for the player to re-place,
- never silently discard the player's editing work.

This on-demand action never modifies or overwrites the originating file. It is the manual, discrete cousin of live modes, which update the playfield clone continuously.

### 7.6 Cross-Playset Deformation as a Feature

The cloned playfield is not merely a resettable background; it can become a shared mutable stage across toolkit modes. A Brickbat session can erase letters, burn columns, or otherwise deform the cloned document, and then Platformer, Racing, Tower Defense, RPG, or Action modes can inherit that changed terrain. This makes "play one game to alter the level for another game" a first-class design possibility.

Default behavior for the editor/RAD prototype: switching playsets preserves the current cloned-page state. Pressing **Reset** or explicitly starting a new configured game restores the clone from its immutable source capture. Later save/export flows should let creators package either the pristine clone, the deformed clone, or both as named level variants.

Design implications:

- The environmental awareness map must distinguish **source geometry** from **current mutable geometry**.
- Text erasure, laser cuts, pixel damage, explosions, terrain painting, and semantic transformations should publish change events into the shared level state.
- Playsets need a clear persistence policy: "shared damage," "reset on mode switch," "per-level snapshot," or "save deformation as remix."
- The UI should treat deformed clones like normal creative artifacts: previewable, undoable where practical, resettable, and exportable with the same metadata/privacy rules as any other clone.

---

## 8. Live Document Mode: Adaptive & Reactive Gameplay

This is the "world waking up around your work" feature. Once launched, the engine itself is active: actors can enter, inspect, patrol, gather, fight, or build around a cloned representation of the desktop. At higher intensity, the same feed can become the "war going on around your text while you write." DACK observes activity and translates it into game events; it does not control the work app.

### 8.1 Why This Is Different From Live Desktop Mode

Live Desktop Mode (§5.3) tracks *windows moving on screen*. Live Document Mode tracks *the document's actual content changing underneath* — new sentences, deleted paragraphs, formatting changes, cursor position — which is a much richer and more precise signal than pixels, but requires a different technical path since a `.docx` sitting open in Word isn't just a static file on disk until it's saved.

### 8.2 Getting Live Edit Signals Out of the Host App

Three tiers, from universal to optional:

1. **Capture + window events (baseline):** pixels and UIA window/element bounds reveal layout, focus, movement, resize, appearance, and disappearance. This powers actors' awareness of windows and other fixed items without app-specific integration.
2. **Accessibility text (preferred semantic tier):** UI Automation `TextPattern`/`TextRange` can read visible accessible text and selection changes in many apps. It is coarser than a native add-in but requires no Office installation step and keeps the product app-agnostic.
3. **File/save observation (explicit, supported clones only):** when the user chooses an open text file or another approved open format, DACK can diff its working clone or explicitly selected save output. It never hunts through arbitrary autosave/temp directories.

An Office add-in is back-burnered until a concrete flagship use case proves that these tiers cannot deliver the experience. If it ever returns, it is a separate opt-in integration with its own threat model and compatibility matrix.

### 8.3 Activity Event Map: Translating Work into Gameplay

A small, editable table maps desktop/document activity to game events. Internally this is the **Activity Event Map**; the Document Event Map is its text-focused preset.

| Document event                                        | Example game reaction                                                                                             |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| New paragraph/sentence added                          | New platform/wall extends the battlefield; territory "reclaimed" from the encroaching enemy                       |
| Text deleted                                          | Ground crumbles or a platform retracts where that text used to be                                                 |
| Sustained typing (words-per-minute above a threshold) | Player deals more damage / spawns a shield / pushes enemies back — "productive momentum"                          |
| Idle / no edits for N seconds                         | Enemy forces advance, pressure increases — a gentle nudge back to writing, tunable in intensity                   |
| Word-count milestone reached                          | Checkpoint, boss wave defeated, or a wave of reinforcements arrives for the player                                |
| Heading added                                         | New "zone" or stage boundary spawns, similar to the static split-by-heading behavior in §7.3                      |
| Formatting change (bold, highlight)                   | Cosmetic effect or a marked "objective" tile, useful for turning review/editing passes into a mini-objective list |
| Window opened/moved/resized                            | A building arrives, terrain shifts, a route opens/closes, or scouts investigate the new boundary                  |
| Window loses focus or visually "closes" in the clone  | Trigger a defense objective or simulated loss state; never close the real window                                  |

This table is per-ruleset, not hardcoded — a "Word War" preset (Action Kit) leans hard into the adversarial framing (enemies literally advance on inactivity, retreat as you write), while a gentler "Grow a Garden" preset (Casual Kit) could use the same event feed to grow decorative platforms without any combat framing at all. The metaphor is a ruleset choice, not an engine assumption.

### 8.4 Design Intent: Productive, Not Punishing

The explicit goal is that this makes writing *more* engaging, not that it turns focus into a stressful timer:

- **Ambient is the product and first-run default.** Actors explore, emote, gather, or stage tiny skirmishes; inactivity is neutral and there is no fail state.
- **Engaged is the flagship `Word War` default.** There are visible objectives and simulated stakes, but long grace periods, recoverable setbacks, and no penalty to the document or user's productivity record.
- **Siege is explicit opt-in.** Cadence can drive real game difficulty and fail states, but failure means the cloned playfield loses and can be restarted/continued. It never means lost work.
- A visible, always-available **pause/detach** control instantly freezes the game without affecting the document — writing should never feel trapped by the game layer.
- The **Boss Key** is stronger than pause: it hides/neutralizes all DACK windows/overlays, mutes audio, releases input, and presents no sensitive level preview in task-switch thumbnails where Windows permits.
- This pairs naturally with existing writing-sprint tools (word-count goals, timers) as prior art for "productivity as a game loop," but DACK's version is spatial and toolkit-driven rather than a single score number.

### 8.5 Always-Active Desktop & Multi-Monitor Staging

The live engine is a resident world, not merely a modal mini-game. A useful first-launch sequence is intentionally subtle: a small squad, worker, creature, or vehicle enters the playfield and starts doing things. The user can ignore it, inspect it, or raise intensity.

On two-monitor systems, DACK should support:

- **Work + clone:** native apps remain live on one monitor while the cloned playfield runs on the other.
- **Overview + detail:** one monitor shows the battlefield overview while the other shows a zoomed action/editor view.
- **Per-monitor safety:** only explicitly selected monitors/windows are sampled; the Boss Key clears both displays.

Window boundaries, the taskbar, icons, captured panels, and creator-painted regions all enter one **environmental awareness map**. Actors query it for solidity, cover, climbability, traversal cost, line of sight, ownership, and hazard status. Dynamic boundaries emit events so AI can re-plan rather than walk blindly through changed windows.

### 8.6 Multiplayer/Shared-Document Angle (Future Exploration)

Word and similar apps increasingly support real-time co-authoring; a natural later extension is two players editing the *same* shared document from different machines, each seeing the other's edits reflected as opposing or cooperative forces in their own game view—genuinely gamifying collaborative writing or editing sessions. This sits beyond the published four-phase roadmap.

---

## 9. Toolkit Modules

All toolkits share the **DACK Core** (rendering, physics, keyboard/mouse input, save/load, capture pipeline, environmental awareness map, activity tracking, editor shell) and differ in rule schema, entity types, and default sprite/parameter sets.

The product UI groups construction by **view/control family**, then offers named presets. The headings below are capability inventories, not permanent top-level tabs:

| Product family | Preset/capability sections below |
| --- | --- |
| Side View | Platformer/Climber, side-view shooting/digging, some space/artillery presets |
| Overhead | Combat, driving, planes/space, RPG walkers, animals/insects/swarms |
| Ball / Table Physics | Pinball, marble/pachinko/table toys |
| Paddle / Clearing | Brickbat, word/letter clearing, paddle variants |
| Grid / Text | RPG/Roguelike, Snake/Maze, glyph/BBS play |
| Route / Flow | Racing, Crossing/Escort, Tower Defense/Offense |
| Ambient / Desktop Toybox | low-intensity workers, creatures, gardens, reactive presets |

[`docs/DACK-Top-Level-Menu-Plan.md`](docs/DACK-Top-Level-Menu-Plan.md) is authoritative for menu taxonomy.

### 9.1 Side View: Platformer / Climber

- Physics params: gravity, jump height/count (incl. double-jump), run speed, friction, wall-jump toggle.
- Terrain from window edges/taskbar, image contours, glyph maps, creator painting, or later validated structured regions.
- Paragraph/text tools include **slant paragraph**, **raise/lower line**, and **stagger rows** operations so creators can turn ordinary text blocks into Donkey Kong-style diagonal ramps while preserving native document readability in the clone. The tool stores a transform/physics overlay, not a mutation of the original source.
- Single-spaced text can act as a crawl/climb surface in Climber mode: the character should be able to crawl up or down dense rows of text with a distinct crawl animation, separate from ladder climbing.
- Text-as-terrain and text-as-climbable must be rules, not assumptions. A creator can decide whether text supports the player, whether text can be crawled/climbed, and whether projectiles/actors can destroy it. The same capability flags should later apply per actor/enemy/projectile: a spider enemy might climb text, a ghost might ignore it, a drill might destroy it, and a courier might treat it as solid ground.
- Action verbs include shoot and dig. Shooting removes/mutates text regions at range; digging removes/mutates terrain locally over time. Both operate only on the cloned playfield and publish deformation events that other playsets can inherit.
- Entities: player, patrol enemies, collectibles (icons or images), hazards, moving platforms, goal flag.
- Win conditions: reach exit, collect N items, survive timer, or (Live Document Mode) reach a word-count goal.

### 9.2 Overhead Family

- **Overhead family:** top-down movement over a cloned desktop/document. Combat tanks, driving, planes/spaceships, RPG/adventure actors, animals, insects, office creatures, workers, and swarms are all presets over the same world model.
- **Combat/tank preset:** rotate, drive, shoot, ricochet, hide, duel. Desktop icons or document images become cover/obstacles, windows/paragraphs become walls/rooms.
- **Driving preset:** steer, accelerate, brake/reverse, drift, follow creator-drawn roads or document/diagram paths.
- **Plane/space preset:** rotate, thrust, coast/inertia, wrap or bounce at bounds, shoot, avoid floating UI/document hazards.
- **RPG/adventure preset:** 8-way or click-to-move, interact, pick up, open, talk/fight, pathfind around document objects.
- **Animal/insect preset:** crawl, wander, forage, flee, follow trails, swarm, climb text/UI shapes, and react to semantic words or live document events.
- Params: weapon type/rate of fire, enemy HP/AI (patrol, chase, turret), health/armor, scroll speed (flight).
- Entities: turrets, chasers, pickups, destructible props (simulation-only—never touch the originating file or app).
- **Featured preset: "Word War."** Live Document Mode ruleset where an enemy line besieges the page; every sentence written pushes the line back, every idle stretch lets it creep forward. Designed as the flagship example of §8's "productive, not punishing" intent.
- Tactical inspiration includes **Cannon Fodder** and **Syndicate** for small autonomous squads plus direct orders, and **Age of Empires** for workers, gathering, construction, territory, and escalation. These are behavior references, not scope commitments to full RTS simulation.

### 9.3 Space / Air Shooter Presets

- Classic vertical/horizontal shmup rules: wave patterns, bullet patterns, boss parameter sheets — the most direct homage to SEUCK.
- Desktop/document imagery is typically used as a **backdrop/parallax layer** rather than hard collision, though hard-edge mode is available. A later validated layered-image importer could map source layers to literal parallax depth.
- Wave editor: place enemy formations on a timeline, exactly like SEUCK's wave/attack pattern designer; in Live Document Mode, waves can instead be triggered by document milestones rather than a fixed timeline.

### 9.4 Paddle / Clearing: Brickbat and Marble-Track Presets

- **Brickbat:** the target wall is auto-generated from letters, words, headings, icon grids, tiled thumbnails, cells, or accepted regions, with document-native erasure and literary bonuses.
- **Marble-track preset:** a color/match/route track is hand-drawn as a spline across the cloned source; marble colors may be sampled from the source palette.
- Params: ball speed, combo rules, track shape, color count.
- **Featured preset: "Grow a Garden."** Live Document Mode ruleset where new writing sprouts decorative platforms/flora with no combat framing — the ambient, non-adversarial counterpart to "Word War."

### 9.5 Route / Flow: Racing

- Minimal authoring requirement: define a track and a starting point. Optional finish line, checkpoints, lap count, timer, ghost car, hazards, boosts, and AI racers build from there.
- Track sources: creator-drawn splines, hand-painted corridors, document margins, process diagrams, flowcharts, spreadsheet paths, presentation arrows, or text/word-object routes.
- Track semantics: `START`, `FINISH`, `CHECKPOINT`, `BOOST`, `OIL`, `PIT`, `TARPIT`, `SLOW`, `JUMP`, and `SHORTCUT` can become suggested racing objects when OCR/semantic labels are enabled.
- Params: steering model, acceleration, braking, drift, traction, off-track slowdown, collision bounce, lap rules, checkpoint order, timer, and ghost replay.
- Office mappings: race through the gutters of a document, around the edge of a spreadsheet table, along a project workflow diagram, through slide connector arrows, or around a captured window layout.
- Featured preset candidate: **"Margin Rally."** A tiny car races around a document's margins and paragraph corridors while semantic words become hazards, boosts, or checkpoints.

### 9.6 Route / Flow: Defense, Offense, and Escort

- Routes come from paragraph flow, document outlines, spreadsheet rows/columns, process diagrams, creator-drawn splines, or UIA text/region order.
- Towers are placed on margins, headings, icons, table cells, comment balloons, or creator-painted anchor zones; upgrades use the same parameter-sheet + event-grid system as every toolkit.
- Enemy waves can be timed, document-triggered, or glyph-born from visible letters/words. A wave might crawl out of repeated `e`s, headings, TODO markers, tracked changes, or section labels.
- Params: route branching, wave composition, tower range/rate/effect, projectile behavior, slowdown fields, resource income, objective health, and escalation curve.
- Office mappings: defend the document title, a selected paragraph, a worksheet total, a project milestone, or a "home base" window while text/tiles advance along readable routes.
- Featured preset candidate: **"Margin Defense."** Enemies march along text lines and outline paths while the player places simple towers in margins, headings, and whitespace.

### 9.7 Grid / Text: RPG/Roguelike (Required)

- Rogue/Hack-style grid or free-layout dungeons with rooms, corridors, doors, keys, locks, traps, items, inventory, monsters, stairs, fog of war, and turn-based or real-time movement.
- **Glyph Map mode:** configurable character/word legend with synchronized text and world views (§6.3). A Word document can supply a copied monospaced map without a Word add-in or native `.docx` parser.
- **Glyph-born actors:** letters can become actor silhouettes before they become polished sprites. A `G` can be a terminal-style monster, `S` can slither, `O` can roll, `T` can march like a hammer, and `W/#` can remain walls or evolve into wall-creatures. This preserves the Rogue/Hack feel while making the document appear to generate its own ecology.
- Generators: room-and-corridor, cellular cave, maze, office-floorplan, and "trace captured boundaries."
- Rules: permadeath toggle, hunger/clock toggle, procedural loot tables, encounter tables, status effects, line of sight, and save policy.
- Office mappings: headings as floors, tables as rooms, cells as tiles, comments/markers as secrets, windows as buildings, and desktop icons as loot or portals.
- Featured preset: **"Document Dungeon."** Toggle readable glyphs into dressed walls/floors, explore, then toggle back to inspect or edit the map.

### 9.8 Ball / Table Physics: Pinball

- Core authoring model: place a plunger/launch lane, flippers, bumpers, slingshots, rollover lanes, drop targets, gates, ramps, kickers, drains, outlanes, bonus inserts, and score rules on top of the cloned document.
- Natural document mappings: paragraph gutters become lanes, heading blocks become bumpers, bullet lists become drop targets, icons/pillboxes become lit inserts, margins become outlanes, tables become rollover grids, and semantic words become missions or jackpot targets.
- Table geometry rules: document/page bounds, gutters, detected text blocks, creator-painted rails, one-way gates, ramp splines, holes, kickers, lanes, outlanes, drain zones, safe launch lanes, and optional invisible guide walls.
- Ball rules: ball count, launch force, gravity/table tilt, elasticity, friction, spin/english, max speed, stuck-ball rescue, multiball cap, and whether ball impacts deform the shared clone.
- Flipper rules: left/right/custom flippers, strength, return speed, angle limits, cooldown, keyboard/mouse binding, and visible sweep-preview handles.
- Target rules: bumpers, rollovers, drop targets, word targets, letter banks, headings, icons, pillboxes, tables/cells, and manually painted targets. Targets can be lit/unlit, timed, chained, completed as banks, or promoted from semantic words.
- Scoring rules: base target value, combos, lane completion, word completion, multipliers, jackpots, hurry-up timers, end-of-ball bonus count-up, and table-clear bonuses.
- Mission rules: semantic words can become modes such as `JACKPOT`, `LOCK`, `MULTIBALL`, `BONUS`, `DRAIN`, `SAVE`, `RAMP`, or `TILT`. The creator can keep the word visible, replace it with an insert/target graphic, or show a hybrid.
- Nudge/tilt rules: nudge strength, cooldown, accumulated tilt meter, warning effects, lockout duration, and accessibility-friendly alternatives for users who dislike reflex-heavy nudging.
- Construction UI: drag handles for flipper arcs, bumper radii, ramp splines, gate direction, plunger lane force, drain width, insert lighting, and target bank grouping.
- Visual identity: pinball is a perfect home for the reusable effects deck — flashing inserts, jackpot banners, word explosions, laser-like lane lights, analog score reels, and ridiculous neon typography.
- Featured preset candidate: **"Document Pinball."** Launch a ball through text gutters, bounce off headings/icons, light target words, and drain into the page margin while the cloned document keeps its scars for other toolkits.

### 9.9 Grid / Text: Snake / Maze Chase

- Core authoring model: define a maze, place a player start, place collectibles, define enemy homes/spawns, and tune chase/evade behavior. The family covers snake growth, maze chase, pellet collection, tunnels, doors, power states, and pursuit/evasion without depending on any single named arcade game.
- Natural document mappings: spreadsheet cells become maze tiles; monospaced text becomes glyph corridors; desktop icons become pellets/pickups; windows and panels become walls; gutters and margins become safe lanes, tunnels, or wrap edges; semantic words become power-ups, hazards, doors, or enemy triggers.
- Maze sources: text lines, spreadsheet grids, desktop/window boundaries, manually painted walls, BBS/ASCII opaque layers, creator-drawn paths, and OCR/UIA-discovered labels.
- Snake rules: movement speed, turn buffering, growth per collectible, body collision, wrap/tunnel behavior, tail-as-terrain toggle, length caps, shedding, and clone deformation when the snake eats text.
- Paragraph tunneling rules: a snake can carve through paragraph text as mutable terrain, leaving erased tunnels, highlighted trails, bite marks, or temporary whitespace corridors in the clone. Single letters can be eaten as pellets before OCR resolves the word; once Word Sense labels nearby regions, the same tunnels can become purposeful routes toward or away from named words.
- Word-goal rules: seek target words, avoid forbidden words, collect words by category, chase a moving/highlighted word, grow only from approved words, shrink or poison on taboo words, and score phrase chains from the route taken through the document. Example: a snake tunnels through paragraphs seeking `KEY`, `FOOD`, or verbs, while avoiding `TARPIT`, `POISON`, or deadlines.
- Maze-chase rules: pellet count, enemy count, enemy home, scatter/chase/flee states, power duration, route choice, tunnel speed, frightened-state scoring, and whether enemies obey visible text/window boundaries or editor-painted invisible rails.
- Actor behavior presets: generic equivalents of the classic four-chaser idea — direct chaser, ambusher, pincer/flanker, and erratic/patrol-biased chaser — expressed through DACK's AI behavior blocks rather than hardcoded character lore.
- Construction UI: paint walls and pellets, drag tunnel endpoints, mark enemy homes, define forbidden/safe regions, set wrap edges, preview reachable routes, show enemy heatmaps, and toggle text/graphic/hybrid presentation.
- Cross-playset mutation: Brickbat, Pinball, Platformer, or Action can punch holes or scars into the clone, and Snake/Maze can inherit those holes as new corridors, blocked paths, or dangerous gaps until reset.
- Featured preset candidate: **"Desktop Gobbler."** Navigate through icons, window edges, and text pellets while office-themed chasers patrol the visible desktop clone.

### 9.10 Shared Engine Services (DACK Core)

- Rendering: 2D sprite/tile renderer, camera, particle FX, transparent overlay rendering for Live Mode.
- Physics: simple AABB/2D rigidbody suitable for platformer/action/casual; separate scrolling-shooter movement model for Space Shooter.
- Input: keyboard, mouse, wheel, and configurable global Boss Key. No controller dependency or controller-first UI.
- Import surface: built-in capture/image/text paths; future out-of-process importer host (§6).
- Activity tracking: capture/window events + UIA text pattern feeding the Activity Event Map (§8.2–8.3).
- Rule Engine: data-driven, JSON-backed parameter sheets and a lightweight visual event system, extended to consume both gameplay events and document-change events through the same grid.
- Save/Load: level files, ruleset presets, capture/document assets, playset packaging.
- Editor shell: shared UI chrome (toolbox, property inspector, Split View, Activity Event Map, timeline where relevant) themed per toolkit.

---

## 10. The Rule Engine: Parameters, Event Grid & AI Behaviors

Guiding principle for this whole layer: **making a DACK game should be at least as fun as playing one.** That means the editor isn't a settings form bolted onto a game engine — it's a satisfying toy in its own right, with enough depth that experimentation is genuinely engaging, while never requiring code.

### 10.1 Two Layers, One System

- **Parameter Sheets (SEUCK-style):** every game element ships with sliders/dropdowns/numeric fields tuned live with instant visual feedback — drag a gravity slider and watch the test-dummy's jump arc redraw in the canvas immediately. This is the fast, low-friction layer that covers most day-to-day tuning.
- **Event/Condition/Action Grid (Klik & Play-style):** for behavior parameters alone can't express—sequencing, branching, cross-entity interaction, and reacting to observed activity (§8.3)—a visual node grid: pick a trigger, narrow it with conditions, attach one or more actions. Every node is drag-and-drop with its own mini parameter sheet; no text scripting anywhere.
- **They're the same underlying data model at different zoom levels of complexity**, not two bolted-together systems. A slider is really a pre-wired event/action pair with the grid hidden; any slider can be "popped open" into the grid to add a condition or branch. A beginner never needs to see the grid; a power user can turn any control into one.

### 10.2 Precise Placement & Fine-Grained Control

- Free, pixel-precise placement—arrow-key nudging, optional snap-to-grid, and alignment guides—rather than only tile-locked placement, matching the evolving stick-figure/32×32 art direction (§11).
- Multi-select with batch parameter editing (select a dozen patrol enemies, drag one speed slider, all update together) so precision doesn't become tedious at scale.
- **Type defaults + per-instance overrides**, the classic construction-kit pattern, exposed through direct manipulation: every placed entity inherits its type's defaults and can diverge on any individual parameter without affecting siblings.

### 10.3 AI Behaviors: Modernizing the Format

Bringing genuinely modern AI thinking into an old-school construction-kit format is one of DACK's clearest differentiators from its 1980s ancestors, where "enemy AI" usually meant a fixed movement pattern.

- **Composable behavior building blocks**, assembled visually rather than a small fixed menu of archetypes: perception (line-of-sight cones, hearing radius — all slider-tunable), memory (last-known player position), decision (a lightweight utility AI — weighted scoring across a handful of author-tunable considerations: distance, health, cover, even document-event pressure from §8.3), and action (move, attack, flee, call for backup).
- **Presets remain the entry point**, exactly like SEUCK/ACK always offered — "Patrol," "Chase," "Turret" — but each preset is really a pre-wired graph of the same underlying blocks, so an advanced author can crack one open in the grid and add a new consideration ("retreat below 20% health," "call nearby enemies when the player is spotted") without ever touching code.
- **Squad/group behaviors** as a stretch goal: simple coordination (surround, cover-fire, retreat-together) built from the same blocks plus a shared blackboard, so a level designer can create genuinely tactical encounters instead of isolated patrol loops — a real step up from the genre's classic single-enemy scripting.
- **Performance guardrail:** AI evaluation runs on a staggered tick rather than every entity every frame, and behavior complexity scales down automatically at high enemy counts, so design ambition doesn't silently tank frame rate.

### 10.4 Making Creation Itself Fun

Concrete commitments that follow from "creation should be as fun as playing":

- **Instant, juicy feedback everywhere** — placing an enemy shows its patrol/perception range live on the canvas; tuning a jump-height slider re-simulates the arc in real time; wiring a grid node visibly animates the connection so cause-and-effect is never abstract.
- **Playable at every step, not just at export** — One-Click Test Play (§15) should be reachable mid-edit, with the editor able to stay open alongside the play view so tuning and testing interleave rather than alternate.
- **A toybox feel to the palette itself** — sprite/behavior pickers with hover previews and satisfying drag-and-drop interactions, not a dry parts list.

### 10.5 Canonical Mechanics Vocabulary

DACK needs a small, stable vocabulary of verbs that toolkits, AI behaviors, event nodes, tutorials, presets, and importer semantics all share. A "mechanic" is a reusable verb plus tunable parameters, not a genre-specific code path.

| Family | Canonical verbs/mechanics | Shared parameters and signals |
| --- | --- | --- |
| Locomotion | move, stop, patrol, chase, flee, wander, follow, orbit | speed, acceleration, friction, path cost, facing, formation |
| Platforming | jump, fall, land, climb, hang, mantle, wall-slide, swing | gravity, impulse, air control, climb surface, ledge probe |
| Vehicle/space | rotate, thrust, brake, drift, strafe, wrap, hyperspace | angular speed, thrust, drag, inertia, boundary behavior |
| Racing/routes | steer, accelerate, brake, drift, checkpoint, lap, boost, go off-track | traction, turn rate, checkpoint order, lap count, best time, off-track penalty, ghost replay |
| Pinball | launch, flip, bump, nudge, tilt, rollover, drain, lock, jackpot | table tilt, flipper arc/force, bumper force, lane state, tilt meter, ball save, bonus count |
| Snake/maze | collect, grow, turn, wrap, tunnel, chase, scatter, flee, patrol, eat wall/text | grid size, turn buffer, pellet value, growth amount, route cost, power duration, enemy mode, tunnel endpoints |
| Combat | aim, shoot, burst, charge, melee, block, dodge, take cover | range, rate, spread, damage, ammo, cooldown, line of sight |
| Projectile | travel, arc, home, bounce/ricochet, pierce, split, explode | velocity, lifetime, turn rate, bounce count, blast radius |
| Terrain / mutation | dig, cut, build, repair, crumble, reveal, paint, transform, erase text, deform clone | material, hardness, health, support, replacement tile/state, source/current geometry delta |
| Object handling | collect, carry, drop, push, pull, throw, consume, equip | capacity, weight, slot, ownership, stack, use action |
| Access/progression | unlock, open, activate, teleport, checkpoint, exit | key/tag, state, destination, requirement, persistence |
| Defense/tactics | defend, escort, intercept, capture, claim, surround | objective health, zone, threat, formation, reinforcement rule |
| Casual/puzzle | deflect, catch, match, clear, chain, sort, balance | color/type, combo window, trajectory, quota, timer |
| Growth/economy | gather, harvest, spend, build, spawn, upgrade, trade | resource type, rate, cost, cap, production queue |
| Stealth/information | hide, detect, hear, remember, reveal, distract | visibility, sight cone, hearing radius, memory time, suspicion |
| Time/spawn | wait, schedule, wave, repeat, randomize, escalate | delay, interval, count, seed, curve, intensity |
| Office/activity | type, add, delete, select, focus, idle, open, move, resize | threshold, debounce, region anchor, privacy scope, intensity |
| Presentation | animate, emote, speak, highlight, shake, play sound | duration, layer, volume, accessibility alternative |

Toolkits expose curated subsets. The underlying runtime uses the same verbs everywhere, so "bounce" can describe a Brickbat ball, a pinball bumper rebound, a Combat ricochet, or a thrown RPG object; "dig" can modify a tile dungeon, a spreadsheet grid, creator-painted terrain, or text-bearing regions in a cloned document. Mutation verbs publish source/current geometry deltas so later playsets can inherit, reset, undo, or save the deformed clone deliberately.

---

## 11. Visual & Art Direction

### 11.1 Capability-Led Art Progression

- **Stage 0 — debug sticks:** lines, circles, arrows, bounding boxes, and flat shapes make AI intent, collision, and mechanics unmistakable.
- **Stage 1 — authored stick figures:** recognizable poses, simple equipment/vehicles, readable team colors, hit reactions, and a small animation set. This is a valid public style, not an apology.
- **Stage 2 — constrained live sprite language:** user-authored characters and props use C64-scale canvas/palette profiles (24×21 and 32×32 for new work, plus a constrained 64×64 compatibility profile for the RAD stick-figure sheets) to preserve clarity while adding personality.
- **Stage 3 — richer scalable skins:** move toward Lemmings/Lode Runner character-per-pixel readability and a Kingdom Rush-like ability to read both close skirmishes and the whole battlefield.

Art advances only when a capability needs it. New mechanics first appear in the simplest visual form that makes them testable.

### 11.2 What This Means Technically

- **A 32×32 logical authoring unit with resolution-independent placement.** The engine may render larger source art and multiple LODs later, but early creator content has a simple, teachable baseline.
- **A real camera/zoom system, not a fixed viewport:** smooth zoom between a tight "gameplay" framing and a wide "overview" framing, with level-of-detail-aware rendering (simplify particle density, secondary animation, or parallax layer count at extreme zoom-out) so both ends of that range stay performant.
- **A consistent art bible per toolkit**, so community-contributed sprites/tiles/behaviors don't visually clash — a lightweight style guide (palette ranges, outline weight, silhouette rules) shipped alongside the toolkit contribution template (§14.4).
- **A "dress-up" pass for captured/semantic geometry.** Much terrain comes from window edges, image contours, glyph cells, or later validated document regions rather than hand-placed tiles, so the renderer needs a tileable reskinning layer that turns arbitrary regions into readable terrain automatically.
- **Color-key transparency is mandatory at import.** The creator can choose a transparent color—white by default—with tolerance, edge cleanup, live checkerboard preview, and an undoable result. DACK converts the selected key to alpha only in its working clone; it never alters the source image.

### 11.3 Opaque Textmode / BBS Display Layer

DACK should support an intentionally opaque **Textmode/BBS layer**: a rendered grid of ASCII/ANSI/CP437-style characters that can sit above the cloned desktop as a complete presentation skin. This is not merely debug text. It is a deliberate aesthetic layer for Rogue/RPG, Pinball backglass, Brickbat title cards, menu overlays, score panels, mission banners, enemy silhouettes, and "terminal mode" variants of every toolkit.

Design target:

- **Opaque layer, not transparent overlay by default.** When enabled, the layer can fully cover regions of the clone, producing a BBS/ANSI-board feel while the underlying clone remains safe and recoverable.
- **Grid-aware renderer.** Fixed cell size, monospace layout, palette control, optional CP437/box-drawing characters, scanline/CRT treatment, and a readable fallback for modern Unicode/ASCII-only contexts.
- **Generated first, curated later.** The safest default path is generated textmode art: FIGlet/FIGfont banners, boxed panels, ANSI-style borders, procedural glyph sprites, and word/letter transformations created from DACK data. Curated external ASCII/ANSI art packs are admitted only with explicit provenance and compatible license terms.
- **Skinnable display modes.** Quiet terminal, BBS neon, monochrome green/amber, IBM DOS CP437, ANSI color board, and "opaque dungeon map" presets.
- **Two-way presentation.** A semantic object can display as plain text, textmode art, equivalent sprite, or hybrid. Example: `JACKPOT` can stay as readable text, become an ANSI backglass insert, or render as a flashing BBS banner while retaining the underlying word anchor.

Candidate open sources to evaluate:

- **FIGlet / FIGfonts:** useful for generated banners, toolkit titles, score callouts, and BBS-like labels. The classic FIGlet distribution and bundled fonts have permissive/open licensing in common package sources, but individual third-party FIGfont collections must still be checked font-by-font before bundling.
- **Author-created/generated DACK glyphs:** preferred default. Procedural borders, glyph sprites, menus, and ANSI panels avoid asset-license risk.
- **Roy/SAC galleries and archives:** a superb reference point for the BBS/ANSI/ASCII mood DACK wants. Roy/SAC's Best Of page describes a personal selection of favorite ANSI/ASCII pieces and offers a gallery zip; the broader site says Roy's own prior ANSI/ASCII work was made reusable, while also warning that not everything hosted there was created by him or has the same rights. Treat Roy/SAC as preservation/inspiration and a candidate source only after per-piece provenance/license review, not as a blind bulk asset import. Source: https://www.roysac.com/roy_bestof.html
- **Candy Box 2 ASCII art:** explicitly CC BY-SA 3.0; suitable as a study/candidate source, but ShareAlike implications mean it should not become default shipped content until the project decides how CC BY-SA interacts with playset/theme distribution.
- **Textmode tooling inspiration:** modern textmode editors/generators are useful workflow references, but hosted generators are not asset provenance by themselves. DACK should record the license of any imported font/art file, not merely the tool used to make it.

Toolkit uses:

- **RPG/Roguelike:** primary opaque presentation mode; maps, monsters, inventory, fog of war, spell effects, and glyph-born actors.
- **Snake/Maze chase:** pellet fields, maze walls, wrap tunnels, power-state banners, enemy glyphs, route heatmaps, and retro maze skins.
- **Pinball:** BBS/ANSI backglass, score reels, mission callouts, lit inserts, jackpot banners, table-attract mode, and full opaque textmode table art. A few themed starter boards should ship as generated/open textmode skins: office memo pinball, dungeon terminal pinball, neon BBS jackpot board, sci-fi system-console table, and literary/word-processor table.
- **Brickbat:** title cards, score callouts, word-shrapnel letters, bonus text, and alternate "terminal wall" target skins.
- **Platformer:** optional BBS skin for ladders, ramps, enemies, checkpoints, speech signs, and level titles.
- **Tower Defense/Action/Racing:** route labels, wave banners, squad command panels, lap boards, warning signs, and semantic objective markers.

### 11.4 Live Sprite Pad, Sprite Studio & Advanced Aseprite Bridge

The in-app art system is not "junior Aseprite." It has two complementary DACK-native surfaces plus an external advanced path:

- **Live-linked pad — the quick pixel path.** Selecting an entity can open a small constrained pad beside the playfield or inside Sprite Studio's Frame task. Every pixel edit updates that entity in the editor and running preview immediately—no export, refresh, or re-import step. This applies §10.4's instant-feedback principle to art.
- **Sprite Studio — the primary DACK actor/animation assembly path.** The larger owned workspace handles picking/importing frames, source-specific slicing, action labels and sequences, preview/edit, origins/boxes, behavior cards, projectiles, explosions, sounds, effects, and reusable character profiles. It contains the pad; it does not turn the pad into a general-purpose image editor.
- **Constraint is a feature.** Start with fixed profiles: C64-like 24×21, DACK 32×32, and a 64×64 compatibility profile for imported/RAD sheets. Each uses a small creator-selected palette, one transparent entry, and nearest-neighbor display zoom. These are aesthetic/product constraints, not an attempt to emulate Commodore hardware exactly.
- **Small first toolset:** pencil, eraser, fill, line, picker, mirror, palette slots, transparent-color preview, undo/redo, clear, and duplicate. Animation timelines, layers, masks, scripting, and broad image manipulation stay out of the initial pad.
- **Safe binding semantics:** the header always states whether the creator is editing the shared entity-type sprite or a per-instance fork. Choosing "Edit this one" clones the sprite before the first pixel change so a local tweak cannot silently alter every actor of that type.
- **Play and edit concurrently.** Pixel changes propagate to idle, selected, and live test actors on the next render update. Collision remains a separate author-controlled shape so transparent-pixel edits do not unpredictably change physics.
- **Advanced path — Aseprite.** Aseprite remains the right tool for serious frame-by-frame animation, layers, tags, timing, polished asset production, and sprite-sheet packing. DACK's optional bridge imports/refreshes exported PNG + JSON; manual PNG/sprite-sheet import always works.
- **Independent implementation boundary.** Aseprite source may be studied for general behavior and interoperability, but DACK does not copy or redistribute Aseprite code, binaries, UI assets, or protected implementation. Aseprite's current source/release license restricts redistribution; provenance must be recorded for any separately licensed reusable module.

The three surfaces serve different jobs: **fast in-context pixel play**, **DACK-native actor/animation composition**, and **advanced external art production**. See amended ADR-0007 and [`docs/DACK-Sprite-Studio-Mini-App.md`](docs/DACK-Sprite-Studio-Mini-App.md).

---

## 12. Level Data Model

[`docs/DACK-Level-Snapshot-Format.md`](docs/DACK-Level-Snapshot-Format.md) is the normative serialization contract. The canonical editable level extension is `.dacklevel`; `.dackpack` is the distributable bundle. The current `rad-test.dacklevel.json` name is a transitional developer artifact, not a third public format.

The enduring conceptual layers are:

```text
DACK level
├─ format/compatibility
│  ├─ formatVersion
│  ├─ minimum/maximum compatible engine version
│  └─ migration/provenance records
├─ source policy
│  ├─ provider and capture bounds/DPI transforms
│  ├─ immutable-original declaration
│  └─ Snapshot/source-clone/export policy
├─ Snapshot
│  ├─ immutable original clone pixels
│  ├─ mutable working pixels and named variants
│  ├─ cached analysis regions with stable IDs
│  └─ optional OCR/Word Sense labels
├─ creator world
│  ├─ placed visible objects
│  ├─ invisible logic, markers, paths, and HUD zones
│  ├─ actor/card/profile references
│  └─ creator corrections/source bindings
├─ runtime state
│  ├─ reversible mutation log
│  ├─ current active/damaged region state
│  └─ optional checkpoint/run state
├─ playset/rules
│  ├─ active family/preset and parameter values
│  ├─ events/conditions/actions and AI cards
│  └─ activity/intensity policy
└─ presentation/safety
   ├─ art/effect/audio/theme profiles
   ├─ input profile and current RAD Boss binding
   └─ privacy, provenance, and package eligibility
```

Every reusable or mutable record has a stable ID. `formatVersion` is separate from the DACK engine version. List order is presentation, never identity. Source geometry, creator-authored geometry, and runtime mutations remain separable so one Snapshot can support several rulesets/intensities and can deliberately preserve, reset, undo, or package deformation without granting authority over the originating app or file.

---

## 13. Technical Architecture (Windows Only, v1)

### 13.1 Chosen Development Environment

**Recommendation: Godot 4.x (current stable .NET build) + C#/.NET, with Visual Studio as the primary code IDE and the Godot editor for scenes/resources.**

Why this stack:

- Godot is open source, strong for 2D, fast to iterate in, and does not impose the production overhead of Unreal or the licensing/platform coupling of Unity.
- C# is a good bridge between game code and Windows APIs. A separate `.NET` Windows bridge can own capture, UI Automation, global hotkeys, DPI/window events, and packaging behind interfaces that the Godot project can mock in tests.
- The repository builds from the `dotnet` and Godot command lines; contributors may use Rider, VS Code, or another editor. Visual Studio is a preference, not a project dependency.
- Native C++/GDExtension code is added only for a measured performance or API-access need. The first implementation should not split logic across three languages.
- Pin a supported Godot minor version per release and upgrade deliberately. Godot's C# editor support is intentionally paired with an external IDE, and current Godot 4 C# builds support desktop targets; DACK remains Windows-first because of its OS integration.

Current RAD environment:

- Godot 4.7.1 Mono, stored locally as `Godot_v4.7.1-stable_mono_win64/`.
- .NET SDK 10.0.302, targeting `net10.0`.
- Godot C# SDK package `Godot.NET.Sdk/4.7.1`.
- Prototype project root: `dack/project.godot`.
- Local Godot package source: `Godot_v4.7.1-stable_mono_win64/GodotSharp/Tools/nupkgs`, with `nuget.org` as fallback.
- The RAD uses direct PNG loading for selected assets and captured-page backgrounds so the prototype is not dependent on Godot's import cache for runtime smoke tests.

Proposed solution boundaries:

- `Dack.Core` — rules, mechanics vocabulary, data model, AI, deterministic simulation; as OS-agnostic as practical.
- `Dack.Windows` — DXGI/Windows Graphics Capture, UIA, window events, DPI mapping, Boss Key, clone creation/scrubbing.
- `Dack.Editor` — Godot-based creator UI and previews.
- `Dack.Player` — Godot-based lightweight runtime.
- `Dack.ImporterHost` — later restricted subprocess protocol; not loaded into the editor/player process.

### 13.2 Current RAD Architecture Audit

The July 2026 prototype is deliberately more integrated than the target solution. That helped ideas compound quickly, but it should not become the permanent module boundary:

- `Main.cs` is approximately 6,100 lines and currently coordinates application state, UI construction, input, actors, toolkit simulation, combat, animation-editor state, and level persistence.
- `PlayfieldSurface.cs` is approximately 2,000 lines and currently combines document rendering/mutation, text-region queries, collision helpers, world-object editing, effects display, and environment interpretation.
- `SpriteAnimationSet.cs` is approximately 1,700 lines and currently combines curated actor factories, runtime loading, transparency cleanup, several frame detectors, component extraction, and sequence assembly.
- Cockpit pages and controls are mostly constructed imperatively in C#, so layout policy, button style, visibility rules, and toolkit content are difficult to change independently.
- The same captured source is interpreted through several related image scans, while runtime consumers repeatedly map/filter text regions instead of querying one stable indexed environmental model.
- The RAD save schema is nested in the root controller, source-specific sprite mappings still require code branches, and there is not yet an automated test/benchmark project.

This is not evidence that the RAD failed; it is evidence that it succeeded broadly. The productization task is to preserve its behavior while extracting seams. The active decomposition sequence and acceptance gates live in [`docs/DACK-Optimization-and-Refactoring-Plan.md`](docs/DACK-Optimization-and-Refactoring-Plan.md).

Target responsibility boundaries inside the editor/player:

- **Session and command layer:** one authoritative source/Snapshot/playset/edit-selection state; UI renders it and issues undoable commands.
- **Input and UI shell:** Esc/Boss/focus routing, window ownership, responsive tabs, shelves, Inspector, Sprite Studio, and HUD placement.
- **Source and analysis:** Snapshot/live providers, DPI/coordinate transforms, one cached image-analysis product, and optional OCR/semantic labels.
- **Environment and mutation:** stable source-derived/creator-authored/runtime-mutated regions, spatial queries, dirty regions, undo/redo, and cross-playset variants.
- **Simulation:** actors, projectiles, damage, perception, triggers, goals, fixed-step rules where predictability matters, and named gameplay events.
- **Toolkit registry:** each family declares shelves, verbs, parameters, preflight mutations, HUD widgets, and win/lose rules without owning global capture/UI/audio/effects.
- **Asset/profile pipeline:** provenance-aware asset catalog, compiled source-specific import manifests, animation/action profiles, and reusable character/weapon/effect/sound cards.
- **Persistence:** versioned Snapshot/level/package DTOs, migrations, atomic saves, and asset IDs independent of scene/controller internals.

### 13.3 Performance and Efficiency Contract

DACK should target 60 FPS at 1920×1080 on an agreed ordinary office-PC baseline, with 30 FPS as the sustained minimum guardrail for a normal level. The provisional 16.7 ms frame budget is less important than the rules it enforces:

- no whole-image analysis, OCR process wait, sprite blob detection, or file I/O in the gameplay frame loop;
- event-driven UI refresh instead of rebuilding/fitting static panels every frame;
- cached, stable text/region objects with spatial queries instead of scanning the whole document for every actor, ball, or projectile;
- bounded dirty-region mutation instead of treating every erased letter as a reason to reinterpret/upload the entire clone;
- admitted sprite sheets compiled once into explicit manifests rather than rediscovered when an actor spawns;
- cancelable, priority-bounded background work tied to the active source/session;
- graceful load shedding that reduces particles, shadows, glow, distant animation, OCR urgency, and AI tick frequency before degrading input or collision;
- a developer performance overlay and deterministic benchmark levels before speculative micro-optimization.

The Boss Key is a separate safety budget: hiding/neutralizing DACK, muting audio, and releasing input should complete within roughly 100 ms and may never wait for capture, OCR, save, or import work.

| Layer                          | Technology                                                                                                                                                                                     |
| ------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Screen capture                 | DXGI Desktop Duplication API; `Windows.Graphics.Capture` for per-window capture; UI Automation (UIA) for window/element bounding boxes                                                         |
| Frozen import surface          | Core PNG/JPEG/BMP decoding + plain text/Markdown glyph maps; all other visible files enter through capture (§6.1)                                                                              |
| Future importer host           | Fully sandboxed subprocess, read-only cloned input, temp-only output, schema validation, time/memory/archive limits, no network (§6.2)                                                         |
| Live activity tracking         | Window/UIA events and UIA `TextPattern`/`TextRange`; explicit supported-file observation only; no Office add-in dependency (§8.2)                                                              |
| Overlay rendering (Live Modes) | Godot transparent/borderless window where sufficient; Windows bridge owns extended window styles, placement, capture behavior, and Boss Key teardown; use a native compositor only if a focused spike proves it necessary |
| Engine/editor UI               | Godot 4.x .NET + C#; Godot scenes/resources for editor and player, with Windows services isolated behind interfaces                                                                           |
| Physics                        | Godot 2D bodies/areas for the first prototype; introduce a small deterministic kinematic/grid layer only where construction-kit predictability requires it                                    |
| Input                          | Godot keyboard/mouse input + narrowly scoped Windows global hotkey for Boss Key; avoid broad low-level hooks unless a tested live-mode feature requires them                                   |
| Environmental mapping          | Shared semantic region graph consuming UIA/OpenCV capture regions, glyph maps, creator-painted regions, or later validated importer trees                                                     |
| Rule engine                    | Shared parameter-sheet + event/condition/action grid runtime (§10.1); node graphs serialize into the same `.dacklevel` JSON as plain parameters                                                |
| AI behaviors                   | Lightweight custom utility-AI/behavior-graph runtime (perception → decision → action), staggered-tick evaluation for performance at high entity counts (§10.3)                                 |
| Camera & rendering LOD         | Continuous zoom camera with level-of-detail-aware rendering (particle/parallax/secondary-animation scaling at zoom extremes) to hit the Kingdom Rush-style zoomed-in/zoomed-out target (§11.2) |
| Level/playset format           | JSON (`.dacklevel`) + zip/folder bundle (`.dackpack`) containing safe playfield clones; optional scrubbed source clones only in later phases                                                    |
| Distribution runtime           | Full Editor build (creators) + minimal open-source **DACK Player** build (playing only), either standalone or embedded per playset                                                             |

**Why Windows-only for v1:** Desktop Duplication, UI Automation, layered windows, global hotkeys, per-monitor DPI behavior, and window-event integration are the backbone of the distinctive modes. `Dack.Core` and snapshot-only Player code should remain portable where practical, but cross-platform delivery must not dilute the Windows office-desktop experience before it works.

---

## 14. Open Source & Community Model

### 14.1 Licensing

- **Core engine, toolkits, and the DACK Player: permissive open-source license (MIT or Apache-2.0)** — lowers the bar for contributors and downstream embedding, matching the spirit of the ACK/SEUCK community-tool lineage.
- **Community-contributed toolkits, presets, skins, and later sandboxed importers** live in their own repos/plugin packages, each with a declared compatible license.
- **Playsets are separate from the app's license.** A creator chooses how to license/share the cloned visual/text content and assets in a playset. DACK never assumes that a visible document or app screen is lawful to redistribute merely because it could be captured.

### 14.2 Repository Structure

- `dack-core` — mechanics vocabulary, rules, AI, environmental map, data model, and shared simulation.
- `dack-toolkits/{action,rpg,platformer,spaceshooter,casual}` — genre modules built on core.
- `dack-importer-host` — deferred sandboxed process/protocol and hostile-input test corpus.
- `dack-importers-experimental/{...}` — later format importers, never loaded into the editor/player process.
- `dack-editor` — the full authoring app (toolkits + capture UI + Split View + Activity Event Map + packaging).
- `dack-hub` — the community website/service (optional, could start as a static index over GitHub Releases before a dedicated service is justified).

### 14.3 Community Hub

- **Browse/publish playsets**, filterable by toolkit, tags, and whether they're screenshot-based, document-based, or live-adaptive.
- **Plugin index** for toolkits/skins first; importer listing arrives only after the sandbox contract and hostile-file testing are established.
- **Remix credit chain:** since playsets can be rebuilt from a recaptured or edited clone (§7.5), the hub can track "forked from" lineage the way level-editor communities traditionally do.
- **Moderation for captured/embedded content:** framegrabs can expose real personal or copyrighted material just as native documents can. The hub needs a takedown/report path from day one.
- **Mandatory publish sanitization:** the hub accepts only the sanitized upload artifact produced from a clone. Metadata scrubbing cannot be disabled for hub publishing.

### 14.4 Contribution Model

- Clear `CONTRIBUTING.md` per repo, mechanics-node/toolkit templates, an art skin template, and—only later—an importer SDK that targets the subprocess schema rather than engine internals.
- Governance: start as a benevolent-maintainer model (typical for a young open-source project); revisit toward a steering committee if/when the importer ecosystem grows.

---

## 15. Editor UX Notes

- **Editor shell over game HUD.** The current floating toolbar is a RAD switcher, not the product UI. The real editor needs a persistent construction shell with three jobs: choose the source/live capture, place assets/tools on the playfield, and tune selected objects/rules. Gameplay can still collapse to full-screen playfield, but authoring requires shelves, inspectors, drag handles, and preview overlays.

- **Esc toggles the main game/editor menu.** `Esc` is the reliable everyday shorthand for opening/closing the main DACK menu overlay. It is distinct from the Boss Key: Esc pauses or reveals normal game/editor controls; the Boss Key hides/neutralizes DACK for privacy and work safety. Esc must work consistently in play mode, edit mode, menu mode, sprite-pad mode, and toolkit test-play. The menu can fade when unattended after a configurable delay, but it should remain instantly recoverable with Esc, mouse movement, or direct interaction.

- **Layered overlay model.** UI should be separated into named layers: Playfield, Gameplay Actors, Effects, Gameplay-Critical Markers, HUD, Toolkit Overlay, Main Menu, Editor Shell, and Boss/Safety Overlay. Gameplay-critical objects such as balls, players, cursors, projectiles, and selection handles always redraw above spectacle effects. HUD and menus are allowed to fade; safety/Boss UI is not.

- **Gameplay-aware HUD.** Score/stat panels must not become accidental obstacles or hide important objects. HUD widgets should choose low-activity whitespace when possible, fade or slide when a ball/player/projectile approaches, and expose an "always visible / fade on approach / hidden during play" setting per playset. Brickbat score panels specifically should fade when a ball enters an approach radius and restore after the object leaves.

- **Pre-game object cleanup.** Toolkit starts may need a local cleanup pass before play begins. Brickbat should blank or neutralize letters very close to the paddle/drain/launch zone so the first ball does not collide with unreadable clutter near the control surface. This cleanup operates on the working clone only, is undoable/resettable, and should be represented as a named "preflight mutation" in the level state rather than an invisible side effect.

- **Asset shelf / parts palette.** Every toolkit gets a shelf of draggable assets and tools. The shelf behaves like a construction-kit toybox: thumbnail, name, source/license badge, category, and drag preview. Dragging an item onto the playfield creates a placed object with immediate handles and a property sheet. Shelf contents include built-in primitives, curated third-party assets, user sprites, semantic word tools, invisible triggers, and generated objects from auto-detection.

- **Shelf categories.** Suggested top-level categories: Actors, Terrain, Motion Tools, Hazards, Pickups, Triggers, Effects, Text/Semantic Tools, Invisible Logic, and Toolkit Parts. Pinball adds Flippers, Balls, Plungers, Bumpers, Ramps, Gates, Rollovers, Inserts, Drains, Nudges, and Score/Mission Logic. Brickbat adds Paddles, Ball Rules, Target Recipes, Powerups, Lasers, Scoring, and Persistence. Platformer adds Platforms, Ladders, Slides, Conveyors, Elevators, Checkpoints, Diggable Terrain, Enemies, and Projectiles. Snake/Maze adds Maze Walls, Pellets, Tunnels, Enemy Homes, Chase/Flee Presets, Power States, Wrap Edges, Route Heatmaps, and Growth Rules.

- **Drag-create-edit loop.** Shelf interaction must be one fluid gesture: drag from shelf → preview snaps/aligns on playfield → release to place → handles appear → inspector opens. Handles are direct manipulation first (flipper sweep arcs, ramp splines, ladder endpoints, bumper radius, laser beam width, spawn range, patrol path) with numeric fields as backup.

- **Live Desktop pivot.** The editor should treat static captures as one source provider and Live Desktop as the main engine validation path. Live Desktop needs a source/capture panel for monitor/window/region selection, resample/freeze controls, geometry-update policy, overlay vs. two-monitor clone staging, and a visible "live source active" indicator. Snapshot Clone Mode remains valuable for reproducible tests and shareable playsets, but it cannot be the only authoring surface.

### 15.1 Post-RAD UI Productization

Detailed GUI architecture lives in [`docs/DACK-GUI-Architecture.md`](docs/DACK-GUI-Architecture.md). The short version: DACK should become a collapsible construction cockpit around a sacred playfield, with Play / Build / Understand moods, toolkit shelves instead of toolkit-specific apps, direct manipulation handles, and an explicit Understanding Overlay that shows what the engine thinks it detected.

**July 2026 RAD baseline:** the first cockpit, contextual toolkit pages, Player/Enemy/Projectile/Object/Builder shelves, Inspector, Understanding page, draggable cards, actor selection, and editor/play split now exist at proof depth. Platformer parts expose direct A/B handles and a body move grip; actors and HUDs can be moved; common attributes include speed, direction, thickness, opacity/tint, elevator range, gravity, radar, health/damage, and visibility/editor-only behavior. Brickbat and Pinball have their own contextual pages, and Sprite Studio has become a larger character/animation workbench rather than another permanent sidebar.

The important remaining UI work is consolidation, not another control column:

- make every full-screen page responsive to the active monitor and DPI, with fixed headers, visible close gadgets, and independent scroll areas for long shelves/labels/inspectors;
- make one explicit mode state drive cursor, input, simulation, anchors, invisible logic, HUD editability, and panel visibility;
- preserve selected tab, selected object, scroll position, and the deformed clone across test-play;
- make Esc follow one predictable ownership stack—dismiss transient edit, close Sprite Studio, close Cockpit, reopen Cockpit—while the Boss Key remains a separate safety action;
- keep the Inspector beside the selected tab rather than allowing it to become another offscreen window;
- remove placeholder/reminder prose from production panels and put concise help in tooltips, an optional Learn panel, or Understand mode;
- use shared high-contrast components so label capitalization, button width, toggle state, focus, disabled state, and close behavior are consistent;
- move tabs, shelf groups, cards, and property rows to descriptors/registries rather than hand-building another branch for each asset or toolkit.

Invisible logic remains a first-class object family: start points, hidden switches, triggers, checkpoints, enemy spawners, route nodes, OCR/semantic anchors, score zones, and camera/HUD avoidance regions are visible while building and hidden during play. They serialize as ordinary placed objects with presentation/authority attributes, not as special-case UI flags.

The thin global strip should contain only global session actions—mode, source/Snapshot status, reset/variant, Cockpit, and Boss/Safety. Toolkit-specific controls belong to the active contextual page. Full-screen pages such as the main editor and Sprite Studio share one session and selection model; closing the owner returns cleanly rather than leaving orphaned panels.

The former “build the first shell, then start Pinball” order has been overtaken by the prototype. The authoritative sequence is now:

1. add tests, diagnostic counters, and deterministic benchmark levels;
2. extract session/input/UI-shell/selection/HUD state and stop frame-by-frame form refresh;
3. move Snapshot analysis, text objects, spatial queries, and clone mutations into shared services;
4. compile sprite imports into manifests and move character defaults out of root-controller switches;
5. extract shared simulation and toolkit descriptors for the four existing play families;
6. migrate RAD save/load to the versioned Snapshot contract;
7. build Live Desktop and the two-monitor editor/playfield model on that spine.

Detailed responsibilities, performance budgets, and exit criteria are maintained in [`docs/DACK-Optimization-and-Refactoring-Plan.md`](docs/DACK-Optimization-and-Refactoring-Plan.md).
Session-preserving navigation and explicit layer ownership are locked in ADR-0010.

- **Large genre-specific toolkit overlay**: the small floating toolbar is only the quick mode switcher. Each toolkit also has an expandable overlay/panel with its own tools, presets, meters, and authoring handles. Platformer shows text ramps, crawl surfaces, ladders, checkpoints, moving platforms, slides, elevators, and enemy spawns; Brickbat shows letter/word grain, paddle orientation, scoring, power-ups, multiball/laser tuning, and target filters; Racing shows track drawing, start/finish/checkpoints, lap rules, boosts, and hazards; Pinball shows flippers, plunger lanes, bumpers, rollovers, gates, drains, nudges, inserts, multipliers, and jackpot/multiball rules; Snake/Maze shows maze painting, pellet recipes, tunnels, enemy homes, chase/flee AI presets, wrap edges, power states, and route heatmaps.

- **Brickbat builder overlay**: Brickbat should expose a full construction-kit layer, not just a fixed arcade mode. Creator controls include ball count and launch randomness; bottom/side/top paddle orientation; curved/sticky/AI paddle options; target recipes for letters, words, lines, headings, OCR terms, icons, pillboxes, colors, and manually painted regions; a bonus deck for literary/arcade effects; laser/beam settings for strength, width, delay, direction, scoring, and whether beams cut, score, reveal, or transform text; and a persistence policy for whether deformation carries into other playsets, resets on new game, or saves as a named variant. The visual style presets should range from quiet office markup to full neon/Jeff-Minter analog typography.

- **Reusable effects deck**: visual feedback should be a modular engine service, not per-toolkit one-off drawing. The first deck includes score captions, impact bursts, shock rings, sparks, paddle flashes, multiball blooms, laser charge/fire effects, round banners, and word/letter shard explosions with spline-like motion paths. Toolkits call named effects and can swap palettes/intensity presets: quiet office annotation, red-pen markup, monochrome terminal, arcade neon, or full Jeff-Minter overkill. Effects should be usable for Brickbat hits, platformer shots, RPG spells, tower-defense impacts, racing crashes, document-change events, and live-mode alarms.

- **Toolbox sidebar** per toolkit (platform brush, ladder brush, enemy stamp, item stamp) — same interaction model as ACK/SEUCK's tile/sprite pickers.
- **Source switcher:** "Capture Desktop / Capture Window or Region / Use Image / Use Text Grid," with Live Desktop and structured modes appearing only when available.
- **Split View** (§7.3): thumbnail/region strip of captured frames, text sections, or later structured sections; drag each to a level slot.
- **Activity Event Map editor** (§8.3): a simple grid—desktop/document event on one side, game reaction on the other—with an Ambient/Engaged/Siege intensity control and live preview of the active observation tier.
- **Event/Condition/Action grid canvas** (§10.1): the same node-grid surface used for the Activity Event Map generalizes to any entity's behavior—poppable open from any parameter slider for players who want to go deeper.
- **Auto-detect overlay toggle**: show/hide proposed auto-terrain outlines.
- **Semantic word-object inspector:** detected words can be promoted into gameplay objects, assigned behaviors (`TARPIT`, `LADDER`, `KEY`, `BRIDGE`, etc.), and toggled between text, graphic, and hybrid presentation. OCR-discovered suggestions should arrive non-blockingly and be clearly marked as suggestions.
- **Word-summoned tool handles:** semantic objects such as `LADDER`, `BRIDGE`, `CONVEYOR`, or `ELEVATOR` expose appropriate draggable handles so the creator can resize, move, detach, or rebind the generated tool instead of being limited to the word's original typography. Each tool preserves its own constraints: ladders remain vertical with bounded width; bridges/conveyors may rotate; elevators edit their platform and travel rail; ropes/vines may use curves.
- **Precision placement tools** (§10.2): pixel-nudge, optional snap-to-grid, alignment guides, and multi-select batch editing.
- **Property inspector**: click any placed object → parameter panel, consistent "select and tweak" workflow, with live-updating previews (jump arcs, patrol/perception ranges) drawn directly on the canvas.
- **One-click Test Play**: launches the level immediately without a separate export step.
- **Ruleset presets**: ready-made rulesets per toolkit ("Word War," "Grow a Garden," and the earlier static presets) so a new player gets a working game before touching a slider.
- **Sprite workflow:** selecting an entity can open the constrained live-linked pad for quick pixels or Sprite Studio for frame/animation/actor-card work; edits appear in the bound preview immediately. Aseprite export-refresh is the advanced production path (§11.4).
- **Boss Key settings:** visible during onboarding and in the title bar/tray menu, configurable, conflict-checked, and testable. The escape route must never be hidden inside a game-only screen.
- **Playset packaging wizard:** shows every cloned frame/source/asset, license/share tag, sanitization result, and a mandatory preview before export (§16).

---

## 16. Privacy & Safety Considerations

Captures can expose private information even without a native file, and live observation raises the stakes further. The safety model begins with an immutable-original rule:

- **Local-only by default.** Captures, imports, live-tracking sessions, and levels stay on-device unless the player explicitly exports/publishes a playset.
- **Never mutate originals.** Capture, color-key conversion, metadata scrubbing, terrain dressing, importer reads, and packaging operate on DACK-owned clones. The original path is read-only from the product's perspective.
- **Explicit share preview and warning.** The export/publish screen shows every cloned image, text map, source clone, and asset being bundled. If any document clone is included, a persistent warning explains that a document copy—not merely gameplay data—is being shared.
- **Hub publishing scrubs 100% by policy.** The publish artifact is always rebuilt from clones and scrubbed of supported metadata; there is no override. If a type cannot be scrubbed with confidence, the hub accepts a rendered framegrab instead of the native clone.
- **Other share/export flows scrub by default.** Local project saves preserve the working clone as needed. A non-hub export may expose an advanced opt-out only if the UI can explain exactly what metadata remains; the original still cannot be modified.
- **Scrubbing is not a content guarantee.** Visible names, confidential text, comments rendered into pixels, or sensitive imagery can remain after metadata removal. The mandatory preview is the final human check.
- **Live Mode scope control.** Restrict capture/overlay to a single monitor, app, or region to avoid exposing sensitive windows.
- **Live Document Mode is opt-in per document, with a visible "being tracked" indicator.** Continuously reading document content in real time is a materially bigger privacy footprint than a one-time import — DACK should never enable live tracking silently, should show a persistent on-screen indicator while it's active, and should make it trivial to pause/detach tracking (§8.4) at any moment.
- **License/consent tag on every published playset.** A framegrab can expose someone else's content just as a native clone can. The publish flow requires the creator to affirm they have rights to share the bundled content—not a legal guarantee, but meaningful friction and a basis for hub takedowns.
- **No network transmission** of captures, imports, live-tracking data, or playsets without explicit user-initiated export/publish.

---

## 17. Key Technical Risks

| Risk                                       | Notes                                                                                                                                                                                                                                                                                                                      |
| ------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Live Desktop Mode input routing            | Overlay input handling without stealing focus unpredictably from the app underneath; needs a clear engage/disengage hotkey.                                                                                                                                                                                                |
| Geometry drift in Live Mode                | Moved/resized/closed source windows need explicit update/freeze/vanish rules for their derived platforms.                                                                                                                                                                                                                  |
| Auto-terrain accuracy (capture)            | UIA coverage varies by app; manual-editing fallback must be first-class, not an edge case.                                                                                                                                                                                                                                 |
| Dynamic boundary churn                     | Actors must re-plan predictably when windows move, resize, overlap, minimize, or disappear; debounce layout events and provide freeze/vanish/solid-until-clear policies.                                                                                                                                                  |
| Importer format/version drift              | Deferred structured importers still face evolving formats; schema/version errors must fall back to a framegrab instead of blocking play.                                                                                                                                                                                   |
| Untrusted importer plugins                 | Future executable importers are untrusted code parsing untrusted files. Keep them out of process with strict I/O/resource/schema limits; do not enable the ecosystem before the security spike passes.                                                                                                                     |
| App-integration scope creep                | Native Office/Adobe hooks could consume a roadmap without improving the core play loop. Require evidence from capture/UIA prototypes before approving any add-in.                                                                                                                                                          |
| Live-tracking latency vs. distraction      | Too slow feels disconnected from typing; too aggressive feels like it's fighting the writer. Needs tunable thresholds and playtesting across writing styles (bursty vs. steady).                                                                                                                                           |
| Gameplay actively distracting from writing | The central design risk of §8: default rulesets need genuinely forgiving pacing, and the pause/detach control (§8.4) must be fast and always reachable, or this feature undermines the "productive" framing it's built on.                                                                                                 |
| Performance                                | Real-time capture/compositing/import/live-tracking + physics + rendering needs a tight frame budget; profile capture, UIA polling, importer parsing, and live-diffing separately.                                                                                                                                          |
| Root-controller coupling                   | The RAD currently lets UI navigation, playset selection, simulation, import/editor state, and persistence influence one another through shared fields. Extract an explicit session state and commands before Live Desktop or more toolkits multiply the coupling. Navigation must never silently change the active playset, source, Snapshot, or mutation state. |
| Dense-document query cost                  | Letter/word/line lists can contain thousands of objects. Rebuilding mapped arrays or testing current pixels for every object on every actor/ball/projectile/OCR query will collapse frame rate. Cache stable regions, track active IDs, and use spatial indexing. |
| Clone-mutation bandwidth                   | A one-letter hit can trigger background sampling, connected-component cleanup, collision revalidation, and a full texture update. Coalesce bounded dirty regions and separate immediate gameplay state from deferred visual cleanup. |
| Sprite-import instability                  | Heuristic blob/component detection is useful for intake but cannot be a runtime contract: a threshold or sort change can silently renumber curated frames. Compile reviewed source-specific manifests with source hashes, explicit rectangles/order, diagnostics, and migrations. |
| Save-schema coupling                       | RAD level/animation classes currently reflect controller implementation details. Versioned DTOs, stable asset/object IDs, atomic saves, migrations, and golden round-trip tests are required before creator levels become valuable. |
| UI responsiveness and reachability         | Full-screen pages, long animation lists, shelves, and the Inspector can exceed the usable monitor area or become low-contrast. Fixed headers, scroll ownership, clamped panels, shared style tokens, keyboard focus, and multi-DPI tests are release requirements. |
| Multi-monitor/DPI scaling                  | Per-monitor DPI awareness needed so captured/imported coordinates map correctly to overlay coordinates at all scale factors.                                                                                                                                                                                               |
| Boss Key/global hotkey reliability         | The key must work across focus states and multiple monitors without capturing ordinary typing; test conflicts, secure-desktop limitations, task-switch previews, audio, and input release.                                                                                                                                 |
| Anti-malware false positives               | Screen capture, always-on-top overlays, UIA, and global hotkeys can resemble unwanted software; minimize privileges/hooks, sign builds, explain behavior, and expect AV reputation work.                                                                                                                                   |
| Accidental sensitive-content sharing       | Framegrabs, text maps, and later native clones may expose sensitive visible content even after metadata scrubbing; mandatory preview, scope control, consent tags, and takedown tools remain necessary.                                                                                                                     |
| Rule engine progressive-disclosure failure | If "pop a slider open into the grid" isn't genuinely seamless, the hybrid system (§10.1) risks becoming two disconnected tools instead of one — needs early usability testing with non-programmers specifically.                                                                                                           |
| AI behavior performance at scale           | Utility-AI evaluation across many entities (especially with squad coordination, §10.3) can get expensive fast; the staggered-tick + auto-scale-down guardrail needs to be built and profiled early, not bolted on after content creators start building large encounters.                                                  |
| Transparency/keying artifacts              | White or near-white backgrounds can erase intended detail or create halos. Use tolerance/edge previews, non-destructive clone conversion, undo, and explicit alpha inspection.                                                                                                                                              |
| Art production scope/cost                  | Rich scalable skins remain expensive, but the stick-figure → 32×32 → polished-skin ladder prevents art scope from gating engine validation.                                                                                                                                                                                 |

---

## 18. Delivery Roadmap After the RAD Proof

The product still validates one proposition: **a safe clone of an ordinary Windows workspace becomes fun because actors understand and interact with its boundaries.** The difference is that the prototype has already proved many individual parts of that proposition. The roadmap now prioritizes a trustworthy creator loop and reusable product spine.

### Proven Plateau — Snapshot RAD (Achieved at Proof Depth)

The July 2026 Godot RAD proves:

- native-resolution captured-page play;
- text as Platformer terrain and Brickbat/Pinball/projectile targets;
- clone-only text erasure and cross-playset deformation;
- reusable effects, sounds, OCR-assisted word labels, and literary feedback;
- draggable/scalable actors, enemies, toolkit objects, markers, HUDs, and direct endpoint handles;
- Platformer, Brickbat, Pinball, and Overhead play-family seeds;
- actor combat, perception range, projectiles, damage, death causes, goals, and basic enemy behavior;
- live sprite editing, animation sequence labeling, multiple source-specific sprite import methods, and reusable-card direction;
- first Cockpit tabs/shelves/Inspector/Understand/Player/Builder/Sprite Studio surfaces;
- RAD JSON level and animation-manifest save/load.

These are **proven mechanics**, not a finished architecture or publishable product. Screenshot mode remains the deterministic smoke-test and share fallback.

### Phase 1 — Stabilize the Construction-Kit Spine (Current)

1. Add deterministic smoke levels, unit/golden tests, and a developer performance overlay.
2. Fix UI state invariants: navigation and Play/Build/Studio transitions may never silently change the selected playset, source, Snapshot, selection, or mutation state.
3. Make every editor surface responsive, scrollable, high-contrast, and recoverable through a consistent Esc/close ownership stack.
4. Extract session, input, UI shell, selection, HUD, and command/undo boundaries from the root controller.
5. Build one cached document-analysis product, stable region IDs, spatial environment queries, active/deleted state, and dirty-region mutations.
6. Compile source-specific sprite imports into reviewed manifests and load curated actor defaults from data.
7. Extract shared actor/projectile/damage/perception/goal systems and register the existing playsets through toolkit descriptors.
8. Migrate level/animation persistence to versioned DTOs with atomic saves, stable IDs, migrations, and Snapshot caches.
9. Complete one creator-authored office-document level end to end: choose source → detect/understand → place player/start/goal/enemies/tools → tune cards/animations/rules → play → save → reload.
10. Keep Platformer and Brickbat as the primary acceptance loops; Pinball and Overhead remain architectural stress tests rather than parallel full-content productions.

**Exit criterion:** a casual PC user can turn a visible office page into a stable playable level, understand and correct what DACK detected, save/reload it, and exit instantly without the original being touched; the dense benchmark remains responsive and new content no longer requires another root-controller branch.

### Phase 2 — Live Desktop, Two Monitors, and the First Complete Kit

- `SnapshotImageSource`, `LiveDesktopSource`, window, monitor, and region providers behind one source contract.
- Explicit source/Snapshot/playfield/window/monitor/DPI transforms; incremental boundary changes and freeze/vanish/solid-until-clear policies.
- Separate coordinated playfield and editor windows sharing one session, with multi-monitor focus and Boss Key teardown tested.
- Ambient first-launch experience and intensity controls.
- A polished Platformer/Document Runner construction loop using the now-stable terrain, actors, cards, goals, enemies, ladders, conveyors/elevators, semantic tools, and creator defaults.
- First compact **RPG/Roguelike Document Dungeon** using the 8-bit dungeon playset, glyph/text maps, keys, doors, hazards, monsters, inventory seed, and text/graphic toggle.
- Embedded/local OCR provider work behind the existing optional Word Sense boundary; geometry-only play remains complete when it is off.
- First semantic word-object tools: vertical `LADDER`, `TARPIT`, `BRIDGE`, `KEY`, and `DOOR`, with text/graphic/hybrid presentation and appropriate direct handles.
- Snapshot/package export with mandatory preview, clone-only policy, provenance, and always-on hub scrubbing.

### Phase 3 — Activity-Reactive Presets and Toolkit Breadth

- UIA text/selection activity feed and the Activity Event Map—no Office add-in.
- `Word War` (Engaged default), `Grow a Garden` (Ambient), and document-defense presets.
- Deeper Pinball/Brickbat/Overhead, followed by Racing/Route, Snake/Maze, Tower Defense/Offense/Escort, Space/Air, and other presets composed from shared verbs.
- Action squad orders and light worker/gather/build mechanics inspired by Cannon Fodder, Syndicate, and Age of Empires.
- Rebuild/diff flow for recaptured frames and text maps.
- Semantic word-object expansion across toolkits: literary bonuses, platformer hazards/tools, RPG glyph/word actors, route objectives, and word harvesting/protection.
- Camera/zoom LOD and richer skins while preserving stick-figure/debug visibility modes.
- Aseprite PNG/JSON export-refresh for advanced animation; the in-context pad remains the fast pixel tool and Sprite Studio remains the DACK actor/animation assembly surface.

### Phase 4 — Structured Sources and Community, Only After Engine Proof

- Security spike and hostile-input corpus for `Dack.ImporterHost`.
- The first native structured importer chosen by demonstrated creator demand; `.docx` and `.psd` are candidates, not promises.
- Optional native source-clone inclusion with format-specific scrubbing and framegrab fallback.
- Community hub for sanitized playsets, remix lineage, moderation, and toolkit/skin index.
- Sandboxed importer index only after the host is proven.
- An Office add-in remains back-burnered and requires a specific, validated use case plus explicit roadmap approval.

---

## 19. Decisions Log & Open Questions

### 19.1 Resolved

- **Live Mode is always non-destructive—no "chaos mode."** Live Desktop and Live Document modes never manipulate the real desktop, windows, or document content. The game reads pixels/UIA/approved cloned data into a playfield; destroying or closing something in the simulation affects only that playfield. This is locked, with no future destructive mode planned.
- **The rule engine is a hybrid: parameter sheets and an event/condition/action grid sharing one data model.** Sliders remain the fast path; any slider can be opened into the grid for branching or conditions. Modern composable AI (perception, memory, utility decisions, actions) stays no-code. Creation is a first-class play experience.
- **Original files are immutable.** DACK always works on a clone. Capture, transparency conversion, scrubbing, importer processing, editing, and packaging never modify the original.
- **Hub metadata scrubbing is always-on and not overridable.** Hub uploads are rebuilt from sanitized clones. The creator still receives a prominent warning and full preview because metadata scrubbing cannot remove sensitive visible content.
- **The product is keyboard/mouse-first and office-casual.** Controllers are not a target or roadmap priority. Web-page-like UI and a reliable Boss Key are required.
- **Screenshot/clone compatibility precedes native formats.** Image capture plus an interaction-rich engine is the foundation; direct `.docx`/`.psd` import and native source inclusion wait until after the engine is proven.
- **Third-party importers are fully out of process.** The future importer host uses read-only clones, a restricted temporary directory, no network, strict resource limits, and schema-validated data output.
- **Ambient is the first-run default; `Word War` defaults to Engaged.** Engaged has real simulated objectives and recoverable setbacks. Siege is opt-in. No intensity can harm the work.
- **The Office add-in is back-burnered.** Capture, UIA window/text signals, and explicit open-format observation get the first opportunity to prove the concept.
- **RPG/Roguelike creation is a required toolkit.** Glyph maps and Rogue/Hack-style systems are part of the product identity; the first compact Document Dungeon follows the stabilized creator spine and the complete kit grows from that proof.
- **Semantic word-objects are a signature feature.** Fast image geometry makes text playable immediately; optional OCR/UIA/native text labels add meaning later. Words can stay text, become equivalent graphics, or run in hybrid presentation, and word-summoned tools can be edited with normal construction-kit handles.
- **Art begins with evolving stick figures and constrained tools.** The live-linked pad is the fast in-context pixel path; Sprite Studio is the DACK-native frame/animation/actor-card workspace; Aseprite is the optional advanced external production path. C64-like 24×21, DACK 32×32, and RAD-compatible 64×64 profiles preserve the intended small-sprite language.
- **Snapshot rendering is native-resolution by default.** Readable captured sources stay at 1:1 pixels. Spare monitor space is nonphysical UI/HUD margin unless authored, and all source/Snapshot/playfield/window/monitor/DPI transforms are explicit.
- **The canonical editable level extension is `.dacklevel`; distributable bundles use `.dackpack`.** Transitional RAD filenames may retain `.json` while migrations are built, but they do not define another public format.
- **Ladders are vertical climb volumes.** Creators may move top/bottom endpoints and adjust bounded width. Ramps/conveyors may angle; ropes/vines/rails may use curves.
- **UI navigation preserves the game.** Opening/closing the Cockpit or Sprite Studio and entering Play/Build/Understand never silently changes the active playset, source, Snapshot, selection, placed objects, or clone mutations. Esc closes the deepest ordinary surface; the Boss Key atomically overrides every DACK surface and later restores prior state.
- **Source import and creator-asset import are separate trust boundaries.** Playfield/source formats stay frozen and future third-party native importers are sandboxed. The local provenance-aware sprite/effect compiler may evolve, but runtime loads only admitted compiled manifests.
- **The RAD is now in an incremental productization refactor.** The project will add measurement/tests, cache/index document geometry, batch mutations, compile sprite manifests, and extract session/UI/simulation/persistence boundaries while preserving the proven play loops. A wholesale rewrite or premature native extension is not planned.
- **Godot 4.x .NET + C# is the chosen implementation stack.** Visual Studio is the preferred IDE, but command-line builds and repository structure remain editor-neutral.

### 19.2 Open Questions to Resolve Next

- Should the community hub launch as a first-party hosted service, or start as a lightweight index over community-run repos/releases, given the moderation burden of hosting captured user content and later source clones?
- Which default Boss Key chord has the fewest conflicts across Windows/Office/browser workflows, and should a tray-menu panic action accompany it?
- Should non-hub share exports permit an advanced metadata-scrub opt-out, or should every DACK-labeled sharing workflow enforce the hub policy?
- What exact behavior should dynamic platforms use when a source window minimizes or disappears: vanish, freeze, or remain solid until actors are clear?
- Which glyph legend should ship as the beginner default, and how should proportional text be normalized into a grid without surprising the creator?
- Should a playset be allowed to mix the 24×21, 32×32, and 64×64 profiles freely, or should each toolkit declare one native profile and scale imported exceptions?
- How deep should the "pop a slider into the grid" progressive-disclosure interaction go before it stops feeling seamless — worth an early usability spike with non-programmer testers, per the risk in §17.
- What hardware becomes the published office-PC performance baseline, and which dense-document/actor/effects limits define the Balanced quality preset?

### 19.3 Research References

- Godot: [C# basics and platform support](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html) and [release policy](https://docs.godotengine.org/en/stable/about/release_policy.html).
- Aseprite: [command-line interface](https://www.aseprite.org/docs/cli/), [sprite-sheet workflow](https://www.aseprite.org/docs/sprite-sheet/), and [official licensing FAQ](https://www.aseprite.org/faq/).
- Atari mechanics/manual references: [Combat](https://www.atariage.com/manual_html_page.php?SoftwareID=935), [Adventure](https://atariage.com/manual_html_page.php?SoftwareLabelID=1), [Asteroids](https://www.atariage.com/manual_html_page.php?SoftwareLabelID=8), [Breakout](https://www.atariage.com/manual_html_page.php?SoftwareID=889), [Missile Command](https://atariage.com/manual_html_page.php?SoftwareID=1154), and [Yars' Revenge](https://atariage.com/manual_html_page.php?SoftwareID=1452).

---

## 20. One-Line Pitch

**"Turn a safe clone of anything on your Windows desktop into a living game—stick-figure squads, dungeons, shooters, and casual worlds that understand your windows and can react while you work, without ever touching the original."**
