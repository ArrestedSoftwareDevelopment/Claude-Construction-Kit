# Desktop Arena Construction Kit

### Primary Design Document — v0.5 (Windows Only)

*Working titles: "Desktop Arena," "Deskscape," "OS Frontier," "Chimera Construction Kit." Referred to below as **DACK** (Desktop Arena Construction Kit) for brevity — rename freely.*

---

## 1. Executive Summary

DACK is an **open-source, Windows-first office game and game-creation suite** in the lineage of **Shoot 'Em Up Construction Kit (SEUCK)** and **Adventure Construction Kit (ACK)**. It provides genre-specific toolkits (Action, RPG/Roguelike, Platformer, Space Shooter, Casual, Racing, Tower Defense) with data-driven rulesets and a level editor, aimed at hobbyists and everyday PC users rather than programmers or traditional controller-first players.

The twist: **the level geometry is a safe game clone of your actual desktop and documents — and it can keep changing as you keep working.** The universal path is visual capture: if Windows can display a file or app, DACK can clone its visible frame and make it playable without understanding the proprietary file format. Later, structured importers can add richer meaning. The most distinctive path is live play while the user works: windows become boundaries, text becomes terrain, and activity can drive game events. Little soldiers, vehicles, creatures, or workers may simply arrive and explore at low intensity; higher-intensity presets turn the same space into a battle, defense, or dungeon.

The finished level can be exported as a **playset**. Early playsets ship the cloned framegrab, collision/semantic map, rules, and assets—the interactivity is the product, and a screenshot makes essentially any file usable. Native source inclusion is a later capability. When DACK does include a source file, it creates and sanitizes a clone; it **never edits, scrubs, repackages, or otherwise mutates the user's original**.

The result is part game engine, part desktop toybox, part document-to-level compiler, part live productivity companion, part construction-kit revival, and part open-source community project. It is designed for short, casual sessions at a PC and always includes a fast **Boss Key** that immediately hides or neutralizes the game layer, silences it, and returns the desktop to an ordinary work state.

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

### 5.2 Auto-Terrain Extraction (Screen-Based)

Turning a screenshot into "this is a platform, this is a hazard, this is empty space" is layered so simpler heuristics run first and expensive ones only run if needed. The key platformer principle is that **visible text must be playable by itself**; ladders, ramps, elevators, conveyors, triggers, and checkpoints are editor-authored additions rather than prerequisites for basic traversal.

1. **Text-band heuristics (first platformer proof):** in a cloned framegrab, detect dark horizontal text bands and expose them as basic standable surfaces. This is not OCR; it is a cheap "the words are the floor" pass that proves documents can become platform levels without hand-placing every object.
2. **UI-chrome heuristics (cheap, high-value):** query the **Windows UI Automation (UIA) tree** for on-screen windows at capture time — real bounding boxes for title bars, scrollbars, the taskbar, window edges, buttons, and icons, with zero image processing.
3. **Edge/contour detection (fallback):** for regions UIA can't describe (e.g., inside a canvas), run a lightweight edge-detection + rectangle-fit pass (OpenCV) to propose likely platform lines from high-contrast edges.
4. **Player correction layer:** all auto-detected geometry is editable/deletable; nothing is baked until accepted in the Level Editor.
5. **Manual-only fallback:** hand-paint collision directly onto the image, like a traditional tile editor.

### 5.2.1 Semantic Word-Objects

The text pipeline has two layers that should remain separate:

1. **Fast geometry layer:** image analysis finds text pixels, letters, words, lines, background regions, gutters, and margins without caring what the words mean. This layer powers immediate collision, Brickbat targets, platformer surfaces, erasure/mutation, and "white space is empty space" behavior.
2. **Optional meaning layer:** OCR, UIA accessible text, or native importer text later labels the geometry with actual words, confidence scores, and semantic tags. This pass can happen after play begins; slow arrival is a feature, not merely latency.

When enabled, OCR can become a play mechanic: the level starts with raw document physics, then words gradually become highlighted as the engine "reads" the page. Discovered terms can entice the player with bonuses, hazards, or transformations:

- `TARPIT` → sticky hazard / slowing platform.
- `LADDER` → climbable tool.
- `BRIDGE` → connector over whitespace.
- `DOOR` / `KEY` → lock-and-unlock pair.
- `FIRE`, `ICE`, `BOUNCE`, `CONVEYOR`, `ELEVATOR` → physical modifiers.
- `GHOST`, `GRUE`, `MONSTER`, or names/proper nouns → enemy spawns.
- `FOOTNOTE`, `BOOKMARK`, `DRAFT`, `QUOTE`, `RED PEN` → literary power-ups.

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
- Drag endpoints: the creator stretches the ladder between any two points, including arbitrary angle/length.
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

---

## 6. Frozen Import Surface & Future Importer Architecture

Importer breadth is explicitly frozen until the engine, interaction model, collision semantics, Boss Key, and playset loop are good. The screenshot path is not a fallback of last resort; it is the universal compatibility feature.

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
│   ├── level01.dacklvl    (geometry + rules referencing playfields/desktop-clone.png)
│   └── level02.dacklvl    (geometry + rules referencing playfields/dungeon-map.txt)
├── sources/               (future, optional: scrubbed clones only; never originals)
└── assets/                 (toolkit sprites, sfx, fonts used by the ruleset)
```

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

Because a playset should be playable without the recipient owning the full editor, DACK ships a **free, open-source, minimal "Player" build**: no editor UI, just capture/import + render + physics + input, driven entirely by the bundled `.dacklvl` files. The full editor remains the primary distribution for creators; the Player is the "just play it" distribution for everyone else, and can optionally be embedded inside the playset itself (per the `player/` folder above) so a playset is double-clickable and self-contained even for someone who has never installed DACK.

### 7.5 Rebuilding a Level After Its Source Changes

When a creator recaptures a changed source or updates an included text/source clone, DACK supports an explicit **"Rebuild Level"** action: derive a new region tree, diff it against the level's previous tree, and:

- keep hand-placed entities/rules that map cleanly to unchanged regions,
- flag entities anchored to regions that moved or disappeared for the player to re-place,
- never silently discard the player's editing work.

This on-demand action never modifies or overwrites the originating file. It is the manual, discrete cousin of live modes, which update the playfield clone continuously.

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

### 9.1 Platformer Kit

- Physics params: gravity, jump height/count (incl. double-jump), run speed, friction, wall-jump toggle.
- Terrain from window edges/taskbar, image contours, glyph maps, creator painting, or later validated structured regions.
- Paragraph/text tools include **slant paragraph**, **raise/lower line**, and **stagger rows** operations so creators can turn ordinary text blocks into Donkey Kong-style diagonal ramps while preserving native document readability in the clone. The tool stores a transform/physics overlay, not a mutation of the original source.
- Single-spaced text can act as a crawl/climb surface in Climber mode: the character should be able to crawl up or down dense rows of text with a distinct crawl animation, separate from ladder climbing.
- Entities: player, patrol enemies, collectibles (icons or images), hazards, moving platforms, goal flag.
- Win conditions: reach exit, collect N items, survive timer, or (Live Document Mode) reach a word-count goal.

### 9.2 Action Kit (Ground & Flight)

- **Ground sub-mode:** top-down or side-view run-and-gun; desktop icons or document images as cover/obstacles, windows/paragraphs as walls/rooms.
- **Flight sub-mode:** free-scrolling shooter weaving between floating "window"/image obstacles and taskbar/margin hazard zones.
- Params: weapon type/rate of fire, enemy HP/AI (patrol, chase, turret), health/armor, scroll speed (flight).
- Entities: turrets, chasers, pickups, destructible props (simulation-only—never touch the originating file or app).
- **Featured preset: "Word War."** Live Document Mode ruleset where an enemy line besieges the page; every sentence written pushes the line back, every idle stretch lets it creep forward. Designed as the flagship example of §8's "productive, not punishing" intent.
- Tactical inspiration includes **Cannon Fodder** and **Syndicate** for small autonomous squads plus direct orders, and **Age of Empires** for workers, gathering, construction, territory, and escalation. These are behavior references, not scope commitments to full RTS simulation.

### 9.3 Space Shooter Kit

- Classic vertical/horizontal shmup rules: wave patterns, bullet patterns, boss parameter sheets — the most direct homage to SEUCK.
- Desktop/document imagery is typically used as a **backdrop/parallax layer** rather than hard collision, though hard-edge mode is available. A later validated layered-image importer could map source layers to literal parallax depth.
- Wave editor: place enemy formations on a timeline, exactly like SEUCK's wave/attack pattern designer; in Live Document Mode, waves can instead be triggered by document milestones rather than a fixed timeline.

### 9.4 Casual Kit (Breakout, Zuma, and similar)

- **Breakout-style:** the "brick wall" auto-generated from a grid of desktop icons, tiled window thumbnails, or spreadsheet cells.
- **Zuma-style:** marble-match track hand-drawn as a spline path across the captured/imported source; marble colors sampled from the source's actual palette.
- Params: ball speed, combo rules, track shape, color count.
- **Featured preset: "Grow a Garden."** Live Document Mode ruleset where new writing sprouts decorative platforms/flora with no combat framing — the ambient, non-adversarial counterpart to "Word War."

### 9.5 Racing Kit

- Minimal authoring requirement: define a track and a starting point. Optional finish line, checkpoints, lap count, timer, ghost car, hazards, boosts, and AI racers build from there.
- Track sources: creator-drawn splines, hand-painted corridors, document margins, process diagrams, flowcharts, spreadsheet paths, presentation arrows, or text/word-object routes.
- Track semantics: `START`, `FINISH`, `CHECKPOINT`, `BOOST`, `OIL`, `PIT`, `TARPIT`, `SLOW`, `JUMP`, and `SHORTCUT` can become suggested racing objects when OCR/semantic labels are enabled.
- Params: steering model, acceleration, braking, drift, traction, off-track slowdown, collision bounce, lap rules, checkpoint order, timer, and ghost replay.
- Office mappings: race through the gutters of a document, around the edge of a spreadsheet table, along a project workflow diagram, through slide connector arrows, or around a captured window layout.
- Featured preset candidate: **"Margin Rally."** A tiny car races around a document's margins and paragraph corridors while semantic words become hazards, boosts, or checkpoints.

### 9.6 Tower Defense Kit

- Routes come from paragraph flow, document outlines, spreadsheet rows/columns, process diagrams, creator-drawn splines, or UIA text/region order.
- Towers are placed on margins, headings, icons, table cells, comment balloons, or creator-painted anchor zones; upgrades use the same parameter-sheet + event-grid system as every toolkit.
- Enemy waves can be timed, document-triggered, or glyph-born from visible letters/words. A wave might crawl out of repeated `e`s, headings, TODO markers, tracked changes, or section labels.
- Params: route branching, wave composition, tower range/rate/effect, projectile behavior, slowdown fields, resource income, objective health, and escalation curve.
- Office mappings: defend the document title, a selected paragraph, a worksheet total, a project milestone, or a "home base" window while text/tiles advance along readable routes.
- Featured preset candidate: **"Margin Defense."** Enemies march along text lines and outline paths while the player places simple towers in margins, headings, and whitespace.

### 9.7 RPG/Roguelike Kit (Required)

- Rogue/Hack-style grid or free-layout dungeons with rooms, corridors, doors, keys, locks, traps, items, inventory, monsters, stairs, fog of war, and turn-based or real-time movement.
- **Glyph Map mode:** configurable character/word legend with synchronized text and world views (§6.3). A Word document can supply a copied monospaced map without a Word add-in or native `.docx` parser.
- **Glyph-born actors:** letters can become actor silhouettes before they become polished sprites. A `G` can be a terminal-style monster, `S` can slither, `O` can roll, `T` can march like a hammer, and `W/#` can remain walls or evolve into wall-creatures. This preserves the Rogue/Hack feel while making the document appear to generate its own ecology.
- Generators: room-and-corridor, cellular cave, maze, office-floorplan, and "trace captured boundaries."
- Rules: permadeath toggle, hunger/clock toggle, procedural loot tables, encounter tables, status effects, line of sight, and save policy.
- Office mappings: headings as floors, tables as rooms, cells as tiles, comments/markers as secrets, windows as buildings, and desktop icons as loot or portals.
- Featured preset: **"Document Dungeon."** Toggle readable glyphs into dressed walls/floors, explore, then toggle back to inspect or edit the map.

### 9.8 Shared Engine Services (DACK Core)

- Rendering: 2D sprite/tile renderer, camera, particle FX, transparent overlay rendering for Live Mode.
- Physics: simple AABB/2D rigidbody suitable for platformer/action/casual; separate scrolling-shooter movement model for Space Shooter.
- Input: keyboard, mouse, wheel, and configurable global Boss Key. No controller dependency or controller-first UI.
- Import surface: built-in capture/image/text paths; future out-of-process importer host (§6).
- Activity tracking: capture/window events + UIA text pattern feeding the Activity Event Map (§8.2–8.3).
- Rule Engine: data-driven, JSON/YAML-backed parameter sheets and a lightweight visual event system, extended to consume both gameplay events and document-change events through the same grid.
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
| Combat | aim, shoot, burst, charge, melee, block, dodge, take cover | range, rate, spread, damage, ammo, cooldown, line of sight |
| Projectile | travel, arc, home, bounce/ricochet, pierce, split, explode | velocity, lifetime, turn rate, bounce count, blast radius |
| Terrain | dig, cut, build, repair, crumble, reveal, paint, transform | material, hardness, health, support, replacement tile/state |
| Object handling | collect, carry, drop, push, pull, throw, consume, equip | capacity, weight, slot, ownership, stack, use action |
| Access/progression | unlock, open, activate, teleport, checkpoint, exit | key/tag, state, destination, requirement, persistence |
| Defense/tactics | defend, escort, intercept, capture, claim, surround | objective health, zone, threat, formation, reinforcement rule |
| Casual/puzzle | deflect, catch, match, clear, chain, sort, balance | color/type, combo window, trajectory, quota, timer |
| Growth/economy | gather, harvest, spend, build, spawn, upgrade, trade | resource type, rate, cost, cap, production queue |
| Stealth/information | hide, detect, hear, remember, reveal, distract | visibility, sight cone, hearing radius, memory time, suspicion |
| Time/spawn | wait, schedule, wave, repeat, randomize, escalate | delay, interval, count, seed, curve, intensity |
| Office/activity | type, add, delete, select, focus, idle, open, move, resize | threshold, debounce, region anchor, privacy scope, intensity |
| Presentation | animate, emote, speak, highlight, shake, play sound | duration, layer, volume, accessibility alternative |

Toolkits expose curated subsets. The underlying runtime uses the same verbs everywhere, so "bounce" can describe a Breakout ball, a Combat ricochet, or a thrown RPG object; "dig" can modify a tile dungeon, a spreadsheet grid, or creator-painted terrain.

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

### 11.3 Live-Linked Sprite Pad & Advanced Aseprite Bridge

The in-app tool is not "junior Aseprite." It is a **glorified C64 sprite pad**: deliberately tiny, constrained, immediate, and inseparable from the construction-kit playfield.

- **Primary path — live-linked sidebar pad.** Selecting an entity opens its sprite beside the playfield. Every pixel edit updates that entity in the editor and running preview immediately—no export, refresh, or re-import step. This applies §10.4's instant-feedback principle to art.
- **Constraint is a feature.** Start with fixed profiles: C64-like 24×21, DACK 32×32, and a 64×64 compatibility profile for imported/RAD sheets. Each uses a small creator-selected palette, one transparent entry, and nearest-neighbor display zoom. These are aesthetic/product constraints, not an attempt to emulate Commodore hardware exactly.
- **Small first toolset:** pencil, eraser, fill, line, picker, mirror, palette slots, transparent-color preview, undo/redo, clear, and duplicate. Animation timelines, layers, masks, scripting, and broad image manipulation stay out of the initial pad.
- **Safe binding semantics:** the header always states whether the creator is editing the shared entity-type sprite or a per-instance fork. Choosing "Edit this one" clones the sprite before the first pixel change so a local tweak cannot silently alter every actor of that type.
- **Play and edit concurrently.** Pixel changes propagate to idle, selected, and live test actors on the next render update. Collision remains a separate author-controlled shape so transparent-pixel edits do not unpredictably change physics.
- **Advanced path — Aseprite.** Aseprite remains the right tool for serious frame-by-frame animation, layers, tags, timing, polished asset production, and sprite-sheet packing. DACK's optional bridge imports/refreshes exported PNG + JSON; manual PNG/sprite-sheet import always works.
- **Independent implementation boundary.** Aseprite source may be studied for general behavior and interoperability, but DACK does not copy or redistribute Aseprite code, binaries, UI assets, or protected implementation. Aseprite's current source/release license restricts redistribution; provenance must be recorded for any separately licensed reusable module.

The sidebar pad and Aseprite therefore serve different jobs: **fast in-context construction-kit play** versus **advanced external art production**. See ADR-0007.

---

## 12. Level Data Model (sketch)

```json
{
  "dackVersion": "0.5",
  "toolkit": "action",
  "source": {
    "mode": "liveDesktopClone",
    "playfieldAsset": "playfields/desktop-clone.png",
    "origin": "windowCapture",
    "trackingTier": "windowUIA",
    "originalMutable": false,
    "metadataPolicy": "scrubbedClone"
  },
  "geometry": [
    { "type": "platform", "source": "capture:windowEdge", "rect": [120, 400, 640, 20] },
    { "type": "cover", "source": "capture:uiRegion", "rect": [900, 200, 220, 160] },
    { "type": "hazard", "source": "manual", "pos": [80, 900] }
  ],
  "entities": [
    { "type": "playerSpawn", "pos": [50, 380] },
    {
      "type": "enemy",
      "kind": "siegeLine",
      "pos": [2400, 380],
      "behaviorPreset": "patrol",
      "behaviorOverrides": {
        "perception": { "sightRange": 400, "hearingRadius": 120 },
        "considerations": ["distanceToPlayer", "activityPressure"],
        "retreatHealthPct": 20
      }
    },
    { "type": "goal", "pos": [2400, 100] }
  ],
  "ruleset": {
    "gravity": 1800,
    "jumpVelocity": 620,
    "runSpeed": 300,
    "lives": 3,
    "winCondition": "wordCountGoal:1500",
    "activityEventMap": {
      "windowMoved": "rebuildBoundary",
      "sustainedTyping": "playerShield",
      "idleTimeoutSeconds": 120,
      "onIdleTimeout": "ambientEmote"
    },
    "intensity": "ambient"
  },
  "input": {
    "profile": "officeKeyboardMouse",
    "bossKey": "Ctrl+Alt+B"
  },
  "artSkin": {
    "toolkitStyle": "action-default",
    "terrainDressing": "autoRectangleSkin",
    "cameraZoomRange": [0.5, 2.5]
  }
}
```

This keeps the cloned *source*, the *geometry derived from it*, the *rules*, the *activity event map*, and the *input/safety policy* as separable layers. A creator can reuse one playfield across several rulesets/intensities without granting the engine authority over the originating app or file.

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
| Rule engine                    | Shared parameter-sheet + event/condition/action grid runtime (§10.1); node graphs serialize into the same `.dacklvl` JSON as plain parameters                                                  |
| AI behaviors                   | Lightweight custom utility-AI/behavior-graph runtime (perception → decision → action), staggered-tick evaluation for performance at high entity counts (§10.3)                                 |
| Camera & rendering LOD         | Continuous zoom camera with level-of-detail-aware rendering (particle/parallax/secondary-animation scaling at zoom extremes) to hit the Kingdom Rush-style zoomed-in/zoomed-out target (§11.2) |
| Level/playset format           | JSON (`.dacklvl`) + zip/folder bundle (`.dackpack`) containing safe playfield clones; optional scrubbed source clones only in later phases                                                     |
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

- **Large genre-specific toolkit overlay**: the small floating toolbar is only the quick mode switcher. Each toolkit also has an expandable overlay/panel with its own tools, presets, meters, and authoring handles. Platformer shows text ramps, crawl surfaces, ladders, checkpoints, moving platforms, slides, elevators, and enemy spawns; Brickbat shows letter/word grain, paddle orientation, scoring, power-ups, multiball/laser tuning, and target filters; Racing shows track drawing, start/finish/checkpoints, lap rules, boosts, and hazards.

- **Toolbox sidebar** per toolkit (platform brush, ladder brush, enemy stamp, item stamp) — same interaction model as ACK/SEUCK's tile/sprite pickers.
- **Source switcher:** "Capture Desktop / Capture Window or Region / Use Image / Use Text Grid," with Live Desktop and structured modes appearing only when available.
- **Split View** (§7.3): thumbnail/region strip of captured frames, text sections, or later structured sections; drag each to a level slot.
- **Activity Event Map editor** (§8.3): a simple grid—desktop/document event on one side, game reaction on the other—with an Ambient/Engaged/Siege intensity control and live preview of the active observation tier.
- **Event/Condition/Action grid canvas** (§10.1): the same node-grid surface used for the Activity Event Map generalizes to any entity's behavior—poppable open from any parameter slider for players who want to go deeper.
- **Auto-detect overlay toggle**: show/hide proposed auto-terrain outlines.
- **Semantic word-object inspector:** detected words can be promoted into gameplay objects, assigned behaviors (`TARPIT`, `LADDER`, `KEY`, `BRIDGE`, etc.), and toggled between text, graphic, and hybrid presentation. OCR-discovered suggestions should arrive non-blockingly and be clearly marked as suggestions.
- **Word-summoned tool handles:** semantic objects such as `LADDER`, `BRIDGE`, `CONVEYOR`, or `ELEVATOR` expose draggable endpoints/handles so the creator can stretch, rotate, detach, or rebind the generated tool instead of being limited to the word's original typography.
- **Precision placement tools** (§10.2): pixel-nudge, optional snap-to-grid, alignment guides, and multi-select batch editing.
- **Property inspector**: click any placed object → parameter panel, consistent "select and tweak" workflow, with live-updating previews (jump arcs, patrol/perception ranges) drawn directly on the canvas.
- **One-click Test Play**: launches the level immediately without a separate export step.
- **Ruleset presets**: ready-made rulesets per toolkit ("Word War," "Grow a Garden," and the earlier static presets) so a new player gets a working game before touching a slider.
- **Sprite workflow:** selecting an entity opens the constrained live-linked sidebar pad; pixel edits appear on the playfield immediately. Aseprite export-refresh is the advanced animation path (§11.3).
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
| Multi-monitor/DPI scaling                  | Per-monitor DPI awareness needed so captured/imported coordinates map correctly to overlay coordinates at all scale factors.                                                                                                                                                                                               |
| Boss Key/global hotkey reliability         | The key must work across focus states and multiple monitors without capturing ordinary typing; test conflicts, secure-desktop limitations, task-switch previews, audio, and input release.                                                                                                                                 |
| Anti-malware false positives               | Screen capture, always-on-top overlays, UIA, and global hotkeys can resemble unwanted software; minimize privileges/hooks, sign builds, explain behavior, and expect AV reputation work.                                                                                                                                   |
| Accidental sensitive-content sharing       | Framegrabs, text maps, and later native clones may expose sensitive visible content even after metadata scrubbing; mandatory preview, scope control, consent tags, and takedown tools remain necessary.                                                                                                                     |
| Rule engine progressive-disclosure failure | If "pop a slider open into the grid" isn't genuinely seamless, the hybrid system (§10.1) risks becoming two disconnected tools instead of one — needs early usability testing with non-programmers specifically.                                                                                                           |
| AI behavior performance at scale           | Utility-AI evaluation across many entities (especially with squad coordination, §10.3) can get expensive fast; the staggered-tick + auto-scale-down guardrail needs to be built and profiled early, not bolted on after content creators start building large encounters.                                                  |
| Transparency/keying artifacts              | White or near-white backgrounds can erase intended detail or create halos. Use tolerance/edge previews, non-destructive clone conversion, undo, and explicit alpha inspection.                                                                                                                                              |
| Art production scope/cost                  | Rich scalable skins remain expensive, but the stick-figure → 32×32 → polished-skin ladder prevents art scope from gating engine validation.                                                                                                                                                                                 |

---

## 18. Suggested MVP Scope (Phase 1)

The MVP validates one proposition: **a safe clone of an ordinary Windows workspace becomes fun because actors understand and interact with its boundaries.**

### Phase 1 — Engine Sandbox & Snapshot Clone

1. Godot/.NET solution skeleton with clean `Core`, `Windows`, `Editor`, and `Player` boundaries (§13.1).
2. **Snapshot Clone Mode:** capture monitor/window/region; import PNG/JPEG/BMP; create an immutable-source working clone.
3. Text-object detection for captured documents (letters/words/lines), background-region/gutter detection, UIA + edge/rectangle detection, manual collision painting, and the shared environmental awareness map.
4. A small **top-down Action/ambient vertical slice**: stick-figure actors enter, wander, patrol, chase, take cover, shoot, ricochet, collect, and defend a simulated document/window objective.
5. Keyboard/mouse controls, web-page-like menus, configurable and testable Boss Key, multi-monitor-safe teardown.
6. Rule engine v0 with parameter sheets and a minimal event/condition/action grid built from the canonical vocabulary.
7. AI v0 with Wander, Patrol, Chase, Flee, Defend, and Investigate presets; visible perception/path overlays.
8. Plain-text/glyph map input and a **Document Dungeon** micro-level proving `W/#` walls, doors, actors, and text/world toggle.
9. Transparent-color conversion on clones (white default), 24×21/32×32 creation profiles plus 64×64 RAD compatibility, a collapsible live-linked sidebar pad, and the initial animated stick-figure asset set.
10. Playset export containing framegrab/text map + `.dacklvl` + assets + manifest; mandatory preview and sanitized packaging.
11. Minimal open-source Player and one end-to-end loop: capture/import → detect → edit → play → Boss Key → package → reload.

**Exit criterion:** a casual PC user can turn any visible office file into a playable level in minutes, understand what is interactive, and exit instantly without DACK touching the original.

### Phase 2 — Living Desktop & Required RPG Kit

- Live Desktop Mode with dynamic window-boundary re-planning and work+clone two-monitor staging.
- Ambient first-launch experience and intensity controls.
- Full first pass of the **RPG/Roguelike Kit**: inventory, keys/doors, enemies, traps, procedural rooms/caves, fog of war, turn-based option.
- Action Kit squad orders and light worker/gather/build mechanics inspired by Cannon Fodder, Syndicate, and Age of Empires.
- Aseprite PNG/JSON export-refresh adapter for advanced animation and polished asset work; expand the live pad only where playtesting demonstrates high-value construction-kit operations.
- Platformer movement primitives if the environmental map proves stable.
- Optional local OCR label pass for captured text objects, used first as a slow-reveal suggestion layer rather than a blocking import step.
- First semantic word-object tools: `LADDER`, `TARPIT`, `BRIDGE`, `KEY`, and `DOOR`, with text/graphic/hybrid toggles and draggable handles where appropriate.

### Phase 3 — Activity-Reactive Presets & Toolkit Breadth

- UIA text/selection activity feed and the Activity Event Map—no Office add-in.
- `Word War` (Engaged default), `Grow a Garden` (Ambient), and document-defense presets.
- Platformer, Casual, Racing, and Space Shooter toolkit shells composed from the same vocabulary.
- Rebuild/diff flow for recaptured frames and text maps.
- Semantic word-object expansion across toolkits: literary Brickbat bonuses, platformer hazards/tools, RPG glyph/word actors, and tower-defense routes/towers derived from meaningful document text.
- Camera/zoom LOD and first richer skin, while preserving stick-figure/debug visibility modes.

### Phase 4 — Structured Sources & Community, Only After Engine Proof

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
- **RPG/Roguelike creation is a required toolkit.** Glyph maps and Rogue/Hack-style systems are part of the planned product, with a small text dungeon proven in Phase 1 and the full kit prioritized in Phase 2.
- **Semantic word-objects are a signature feature.** Fast image geometry makes text playable immediately; optional OCR/UIA/native text labels add meaning later. Words can stay text, become equivalent graphics, or run in hybrid presentation, and word-summoned tools can be edited with normal construction-kit handles.
- **Art begins with evolving stick figures and a live-linked constrained pad.** C64-like 24×21, DACK 32×32, and RAD-compatible 64×64 profiles, small palettes, color-key transparency, and immediate entity binding are the primary in-app art path. Aseprite is the optional advanced animation path.
- **Godot 4.x .NET + C# is the chosen implementation stack.** Visual Studio is the preferred IDE, but command-line builds and repository structure remain editor-neutral.

### 19.2 Open Questions to Resolve Next

- Should the community hub launch as a first-party hosted service, or start as a lightweight index over community-run repos/releases, given the moderation burden of hosting captured user content and later source clones?
- Which default Boss Key chord has the fewest conflicts across Windows/Office/browser workflows, and should a tray-menu panic action accompany it?
- Should non-hub share exports permit an advanced metadata-scrub opt-out, or should every DACK-labeled sharing workflow enforce the hub policy?
- What exact behavior should dynamic platforms use when a source window minimizes or disappears: vanish, freeze, or remain solid until actors are clear?
- Which glyph legend should ship as the beginner default, and how should proportional text be normalized into a grid without surprising the creator?
- Should a playset be allowed to mix the 24×21, 32×32, and 64×64 profiles freely, or should each toolkit declare one native profile and scale imported exceptions?
- How deep should the "pop a slider into the grid" progressive-disclosure interaction go before it stops feeling seamless — worth an early usability spike with non-programmer testers, per the risk in §17.
- Which toolkit follows the Phase 1 Action/RPG proofs first: Platformer for terrain validation, or Casual for the broadest office audience?

### 19.3 Research References

- Godot: [C# basics and platform support](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html) and [release policy](https://docs.godotengine.org/en/stable/about/release_policy.html).
- Aseprite: [command-line interface](https://www.aseprite.org/docs/cli/), [sprite-sheet workflow](https://www.aseprite.org/docs/sprite-sheet/), and [official licensing FAQ](https://www.aseprite.org/faq/).
- Atari mechanics/manual references: [Combat](https://www.atariage.com/manual_html_page.php?SoftwareID=935), [Adventure](https://atariage.com/manual_html_page.php?SoftwareLabelID=1), [Asteroids](https://www.atariage.com/manual_html_page.php?SoftwareLabelID=8), [Breakout](https://www.atariage.com/manual_html_page.php?SoftwareID=889), [Missile Command](https://atariage.com/manual_html_page.php?SoftwareID=1154), and [Yars' Revenge](https://atariage.com/manual_html_page.php?SoftwareID=1452).

---

## 20. One-Line Pitch

**"Turn a safe clone of anything on your Windows desktop into a living game—stick-figure squads, dungeons, shooters, and casual worlds that understand your windows and can react while you work, without ever touching the original."**
