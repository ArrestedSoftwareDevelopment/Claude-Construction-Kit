# Desktop Arena Construction Kit

### Primary Design Document — v0.4 (Windows Only)

*Working titles: "Desktop Arena," "Deskscape," "OS Frontier," "Chimera Construction Kit." Referred to below as **DACK** (Desktop Arena Construction Kit) for brevity — rename freely.*

---

## 1. Executive Summary

DACK is an **open-source** Windows game-creation suite in the lineage of **Shoot 'Em Up Construction Kit (SEUCK)** and **Adventure Construction Kit (ACK)**: a set of genre-specific toolkits (Platformer, Action, Space Shooter, Casual) with data-driven rulesets and a level editor, aimed at hobbyists rather than programmers.

The twist: **the level geometry is your actual desktop and documents — and it can keep changing as you keep working.** The app can build a level from a screen capture, directly from a real document file (`.docx`, `.psd`, etc.), or — the most distinctive path — **live, while you actively write or edit**, so the game reacts in real time to what you're doing to the document: new paragraphs extend the battlefield, deletions crumble ground out from under enemies, a burst of typing might repel an incursion. Productive writing and active play become the same activity instead of competing for the same window.

The finished level can be exported as a **playset**: a shareable bundle that includes the actual source document(s), not just a picture of them, so the level stays authentic and the underlying file stays fully editable in its native app. A community hub lets players publish, browse, and remix playsets and file-format importers.

The result is part game engine, part document-to-level compiler, part live productivity companion, part construction-kit revival, part open-source community project.

---

## 2. Core Pillars

1. **Familiar construction-kit UX.** Genre toolkits with parameter sheets, sprite/tile pickers, and a "test level" button — the ACK/SEUCK formula, modernized.
2. **The desktop and your documents are the canvas.** Levels can be built from a captured screen, a real native document file, or a live document that's still being written.
3. **Four modes of level sourcing:** *Snapshot Mode* (frozen screen image), *Live Desktop Mode* (overlay on the moving real desktop), *Native Document Mode* (level built by directly parsing a bundled document file), and *Live Document Mode* (the level adapts in real time as the document is actively edited).
4. **Genre-agnostic core engine, genre-specific rule modules.** One physics/rendering/input core; each toolkit just supplies rules, sprites, and win/lose logic.
5. **Authentic, editable, shareable playsets.** A playset bundles the real source file(s) alongside geometry and rules — not a screenshot standing in for the file — so it stays visually authentic and the original document remains fully editable in its native app.
6. **Writing and play as one activity, not a distraction from each other.** Live Document Mode is designed so the game *rewards* real writing progress rather than pulling attention away from it — the metaphor stays optional and tunable, never mandatory or punishing.
7. **Creation is at least as fun as play.** The editor is a hybrid parameter-sheet-plus-event-grid toy (§10), not a settings form — precise placement, deep tunability, and instant visual feedback are first-class design goals, not an afterthought bolted onto the game engine.
8. **Modern AI thinking in a classic format.** Composable, author-tunable enemy/NPC behaviors (perception, memory, utility-based decisions) replace the old genre's fixed patrol patterns, while still staying fully no-code and preset-driven for newcomers (§10.3).
9. **Hi-res, richly detailed art that scales.** Visual direction inspired by Lemmings/Lode Runner's character-per-pixel readability, with a Kingdom Rush-style camera range that's gorgeous zoomed in and still legible zoomed all the way out (§11).
10. **Open source and community-built.** The core engine, toolkits, and file-format importers are open source; a community hub lets people publish playsets and contribute new importers.

---

## 3. Player Experience Walkthrough

1. Player opens DACK, picks a toolkit (say, **Action Kit**).
2. Player picks a source: **"Capture Desktop"** (screen-based), **"Import Document"** (native file-based, static), or **"Live Document"** (native file-based, reactive — see §8).
3. DACK runs **auto-terrain analysis** appropriate to the source: UI-chrome/edge detection for captures, or structural parsing (paragraphs, headings, images, layers, page breaks) for native documents. Proposed geometry is overlaid as translucent outlines.
4. Player enters the **Level Editor**: accepts/rejects/nudges auto-detected platforms, paints extra invisible collision where desired, places enemy spawns, collectibles, hazards, and a goal/exit. For Live Document Mode, the player also configures a **Document Event Map** (§8.3) — which kinds of edits trigger which kinds of game events.
5. Player opens the **Ruleset panel**: sets gravity, jump height, player sprite, enemy AI type, lives, timer, win condition — parameter sliders and dropdowns, no code.
6. Player hits **Play**. Depending on source mode, they platform across a frozen screenshot, a live overlay of their real running desktop, a level rendered from the bundled document, or — in Live Document Mode — a battlefield that grows, shrinks, and shifts as they type in the actual document sitting behind/alongside the game.
7. Player **exports a playset**: a folder/zip containing the DACK player runtime, the level/ruleset data, and — for Native/Live Document Modes — the real source file(s), so recipients see the authentic document and can still open, edit, and replay against it.

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
| Open-source modding communities (Doom WADs, Tabletop Simulator workshop) | Community hub model: publish, browse, remix, credit the original author                             |
| Lemmings / Lode Runner                                                   | Small, characterful, highly readable sprite/terrain art style even with a lot of placed detail      |
| Kingdom Rush                                                             | Camera/art scalability target: stunning zoomed in, still legible zoomed out to the full battlefield |

DACK's genuinely novel piece is **using real desktop captures and real, actively-changing document files as raw material for procedurally-assisted, reactive level geometry**, bundled so the result stays authentic and editable, then layering a classic construction-kit editor and ruleset on top.

---

## 5. Core Innovation: The Desktop (and Documents) as Playfield

### 5.1 Capture Pipeline (Screen-Based Sourcing)

- **Windows Desktop Duplication API (DXGI)** for fast, low-latency full-desktop or per-monitor capture (this is what OBS/game-capture software uses under the hood).
- Two capture targets:
  - **Full desktop composite** (everything visible, including overlapping windows) — simplest, matches what the player sees.
  - **Per-window capture** (`PrintWindow` / `Windows.Graphics.Capture` for a specific app) — cleaner if the player wants, say, just their Excel sheet as a level without other clutter behind it.
- Multi-monitor aware: capture one screen, all screens stitched, or let the player pick a region (marquee select, like a screenshot tool).

### 5.2 Auto-Terrain Extraction (Screen-Based)

Turning a screenshot into "this is a platform, this is a hazard, this is empty space" is layered so simpler heuristics run first and expensive ones only run if needed:

1. **UI-chrome heuristics (cheap, high-value):** query the **Windows UI Automation (UIA) tree** for on-screen windows at capture time — real bounding boxes for title bars, scrollbars, the taskbar, window edges, buttons, and icons, with zero image processing.
2. **Edge/contour detection (fallback):** for regions UIA can't describe (e.g., inside a canvas), run a lightweight edge-detection + rectangle-fit pass (OpenCV) to propose likely platform lines from high-contrast edges.
3. **Player correction layer:** all auto-detected geometry is editable/deletable; nothing is baked until accepted in the Level Editor.
4. **Manual-only fallback:** hand-paint collision directly onto the image, like a traditional tile editor.

### 5.3 Four Modes of Level Sourcing

- **Snapshot Mode:** the desktop image is captured once and frozen as a static background/level asset. Deterministic, shareable, no dependency on a live desktop — but only an *image* of the source, not the source itself.
- **Live Desktop Mode:** DACK renders a transparent, always-on-top overlay window directly above the real desktop (layered window, `WS_EX_LAYERED` + `WS_EX_TRANSPARENT` toggled per input mode). Geometry re-samples on window-move events (`SetWinEventHook`) so platforms track moving/resizing windows. This is strictly a **read-only mock-up** of the desktop — the game reads window positions and pixels to build its geometry, but nothing in gameplay ever writes back to or manipulates the real windows underneath (see §19.1).
- **Native Document Mode:** instead of capturing pixels, DACK opens the actual document file through a **format importer** (see §6) and derives level geometry directly from the document's real structure. The same file can be reopened and edited in its native app, and the level "rebuilt" from the updated version on demand (§7.5).
- **Live Document Mode (adaptive/reactive):** an evolution of Native Document Mode where DACK doesn't just parse the document once — it watches it change *while the player is actively writing* and updates the level in near-real-time, so gameplay and document-editing happen concurrently in the same session. Like Live Desktop Mode, this is strictly read-only against the source: DACK observes edit events to drive the mock-up level, but the game itself never writes into the document — all real edits come only from the player typing in the native app. Full detail in §8.

---

## 6. Format Importer Architecture (Native Document Mode)

Native Document Mode is powered by a plugin-style importer system so the community can extend it to new file types without touching the core engine.

- **Importer contract:** each importer takes a document file and returns a structured tree of typed regions (`textBlock`, `heading`, `image`, `table`, `pageBreak`, `layer`, `shape`, `cell`, etc.) with bounding boxes, z-order, and any relevant metadata (font size, layer name, page number) — this tree feeds the same auto-terrain mapping step that screen capture uses (§5.2), just with much cleaner input.
- **v1 importers (ship with core):**
  - **`.docx`** — parsed via the OOXML structure (python-docx-style or a custom OOXML reader): paragraphs and headings become platforms sized to text bounds, images become obstacles/collectibles, page breaks become level/stage boundaries, tables become platform grids.
  - **`.psd`** — parsed via the Photoshop file format's layer table: each layer's real pixel bounds becomes a platform or backdrop element, with layer order preserved as depth/parallax; layer names can drive semantics (a layer named "spikes" auto-tags as a hazard).
- **Community-contributed importers (planned):** `.pptx` (slides as discrete levels), `.pdf` (pages as levels, similar to docx), `.xlsx` (cells/rows as a grid level — natural fit for the Casual Kit), `.svg`, Markdown, and others, each as a self-contained plugin against the same importer contract.
- **Fallback behavior:** any importer failure (corrupt file, unsupported version, encrypted document) degrades gracefully to "treat as opaque image" — DACK renders the document's native app preview/thumbnail as a plain background and the player falls back to manual tile painting.
- **No proprietary app dependency:** importers parse the *file format* directly; they don't require Word or Photoshop to be installed. (A "render via installed app" fast path can be offered as an optional accelerator where available, but the core importer must work standalone so DACK doesn't require owning the original software.)

---

## 7. Playsets: Bundling Real Files & Multi-Level Documents

This is the mechanism that makes a level *authentic and editable* rather than a screenshot standing in for the real thing, and how a single document (or a folder of them) becomes a multi-level game.

### 7.1 What a Playset Is

A playset is a distributable folder/zip with a predictable layout:

```
MyPlayset.dackpack/
├── manifest.json          (toolkit, version, author, license, level list)
├── player/                (lightweight DACK Player runtime, optional — see §7.4)
├── documents/
│   ├── chapter1.docx      (the real, bundled source file — openable & editable)
│   └── coverart.psd
├── levels/
│   ├── level01.dacklvl     (geometry + rules referencing documents/chapter1.docx, page range 1-3)
│   ├── level02.dacklvl     (geometry + rules referencing documents/chapter1.docx, page range 4-6)
│   └── level03.dacklvl     (geometry + rules referencing documents/coverart.psd)
└── assets/                 (toolkit sprites, sfx, fonts used by the ruleset)
```

Because the actual `.docx`/`.psd` files travel inside the playset (not a rendered picture of them), a recipient can: (a) play the levels exactly as designed, and (b) open `documents/chapter1.docx` in Word and genuinely edit it — including re-importing it to rebuild the level from the edited version, or dropping straight into Live Document Mode against it.

### 7.2 Multiple Levels from Multiple Documents

A playset's `manifest.json` lists an ordered set of levels, and each level references any bundled document. This gives a simple **campaign structure**: level 1 sourced from a Word doc, level 2 from a spreadsheet, level 3 from a PSD cover — one playset, one continuous game, several source files.

### 7.3 Multiple Levels Within a Single Document

Documents are commonly long enough to yield several levels on their own, so importers expose a natural **split unit** per format:

- **`.docx`** — split by page break, by heading level (e.g., every `Heading 1` starts a new level/stage), or by a manual "level marker" the author inserts as a comment or bookmark.
- **`.psd`** — split by top-level layer group ("Level 1", "Level 2" groups become discrete stages), or by artboard if the file uses them.
- **`.pptx`** (future) — one slide per level, essentially for free.
- **`.xlsx`** (future) — one sheet, or one named range, per level.

This "document as a level pack" behavior is exposed directly in the Level Editor as a **Split View**: the player sees a thumbnail strip of the document's detected sections and assigns each to a level slot (auto-suggested, manually reorderable) rather than hand-authoring the split.

### 7.4 The DACK Player (Lightweight Runtime)

Because a playset should be playable without the recipient owning the full editor, DACK ships a **free, open-source, minimal "Player" build**: no editor UI, just capture/import + render + physics + input, driven entirely by the bundled `.dacklvl` files. The full editor remains the primary distribution for creators; the Player is the "just play it" distribution for everyone else, and can optionally be embedded inside the playset itself (per the `player/` folder above) so a playset is double-clickable and self-contained even for someone who has never installed DACK.

### 7.5 Rebuilding a Level After the Source Document Changes

Since the bundled document is the *real, editable* file, its content can drift from the level that was built on top of it. DACK supports an explicit **"Rebuild Level"** action: re-run the relevant importer against the current copy of the document, diff the new structural tree against the one the level was built from, and:

- keep hand-placed entities/rules that map cleanly to unchanged regions,
- flag entities anchored to regions that moved or disappeared for the player to re-place,
- never silently discard the player's editing work.

This on-demand "Rebuild Level" action is the manual, discrete cousin of Live Document Mode (§8), which does the same kind of diffing continuously, in real time, while the player is still typing.

---

## 8. Live Document Mode: Adaptive & Reactive Gameplay

This is the "war going on around your text while you write" feature: instead of importing a document once (Native Document Mode) or rebuilding on demand (§7.5), DACK **watches the document change while it's open and being edited**, and translates those edits into game events as they happen — turning writing itself into the primary input the game reacts to.

### 8.1 Why This Is Different From Live Desktop Mode

Live Desktop Mode (§5.3) tracks *windows moving on screen*. Live Document Mode tracks *the document's actual content changing underneath* — new sentences, deleted paragraphs, formatting changes, cursor position — which is a much richer and more precise signal than pixels, but requires a different technical path since a `.docx` sitting open in Word isn't just a static file on disk until it's saved.

### 8.2 Getting Live Edit Signals Out of the Host App

Three tiers, from most precise to most universal:

1. **Office Add-in / COM Automation (preferred for Word):** a DACK companion add-in (VSTO or a Word JS Add-in) hooks the Word Object Model directly — `Document_ContentControlOnEnter`, `Window.Selection_Change`, `Document.Paragraphs` deltas, Track Changes events. This gives precise, low-latency, structured edit events (what changed, where, what kind of change) without any file polling.
2. **Accessibility (UIA Text Pattern) fallback:** for apps without an add-in (or for a first pass that doesn't require installing one), read the visible text content live via UI Automation's `TextPattern`/`TextRange` interfaces — the same technique screen readers use. Coarser than the add-in path (harder to distinguish "typed" vs "pasted" vs "autocorrected") but works against more apps out of the box.
3. **Autosave/temp-file polling (universal, lowest fidelity):** watch the app's autosave or temp file location and diff periodically (e.g., every few seconds) for apps with neither an add-in nor a rich accessibility tree. Highest latency, but never zero — every editor autosaves or can be told to.

DACK should pick the best available tier automatically per app and be upfront in the UI about which tier is active, since it changes how responsive and precise the gameplay feels.

### 8.3 Document Event Map: Translating Edits into Gameplay

A small, editable table (exposed in the Level Editor, per §3 step 4) that maps document-change types to game events, following the same event/condition/action spirit as the existing Rule Engine (§8.5 in the Toolkit section):

| Document event                                        | Example game reaction                                                                                             |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| New paragraph/sentence added                          | New platform/wall extends the battlefield; territory "reclaimed" from the encroaching enemy                       |
| Text deleted                                          | Ground crumbles or a platform retracts where that text used to be                                                 |
| Sustained typing (words-per-minute above a threshold) | Player deals more damage / spawns a shield / pushes enemies back — "productive momentum"                          |
| Idle / no edits for N seconds                         | Enemy forces advance, pressure increases — a gentle nudge back to writing, tunable in intensity                   |
| Word-count milestone reached                          | Checkpoint, boss wave defeated, or a wave of reinforcements arrives for the player                                |
| Heading added                                         | New "zone" or stage boundary spawns, similar to the static split-by-heading behavior in §7.3                      |
| Formatting change (bold, highlight)                   | Cosmetic effect or a marked "objective" tile, useful for turning review/editing passes into a mini-objective list |

This table is per-ruleset, not hardcoded — a "Word War" preset (Action Kit) leans hard into the adversarial framing (enemies literally advance on inactivity, retreat as you write), while a gentler "Grow a Garden" preset (Casual Kit) could use the same event feed to grow decorative platforms without any combat framing at all. The metaphor is a ruleset choice, not an engine assumption.

### 8.4 Design Intent: Productive, Not Punishing

The explicit goal is that this makes writing *more* engaging, not that it turns focus into a stressful timer:

- Default rulesets should be **forgiving of pauses** (thinking is part of writing) — "idle" thresholds default long, and consequences default mild (cosmetic pressure, not instant fail states).
- Every adaptive ruleset ships with an **intensity slider** from "ambient" (mostly cosmetic reactions, no fail condition) to "high-stakes" (real difficulty tied to typing cadence), so the player chooses how gamified their writing session gets.
- A visible, always-available **pause/detach** control instantly freezes the game without affecting the document — writing should never feel trapped by the game layer.
- This pairs naturally with existing writing-sprint tools (word-count goals, timers) as prior art for "productivity as a game loop," but DACK's version is spatial and toolkit-driven rather than a single score number.

### 8.5 Multiplayer/Shared-Document Angle (Future Exploration)

Word and similar apps increasingly support real-time co-authoring; a natural (later-phase) extension is two players editing the *same* shared document from different machines, each seeing the other's edits reflected as opposing or cooperative forces in their own game view — genuinely gamifying collaborative writing or editing sessions. Flagged here as a Phase 4+ idea, not part of the near-term scope.

---

## 9. Toolkit Modules

All toolkits share the **DACK Core** (rendering, physics, input, save/load, capture pipeline, importer pipeline, live-edit tracking, editor shell) and differ only in their rule schema, entity types, and default sprite/parameter sets — exactly like how SEUCK and ACK were both built on a shared "construction kit" shell with genre-specific data.

### 9.1 Platformer Kit

- Physics params: gravity, jump height/count (incl. double-jump), run speed, friction, wall-jump toggle.
- Terrain from window edges/taskbar (capture), paragraph/image/layer bounds (native document), or live-growing terrain (Live Document Mode).
- Entities: player, patrol enemies, collectibles (icons or images), hazards, moving platforms, goal flag.
- Win conditions: reach exit, collect N items, survive timer, or (Live Document Mode) reach a word-count goal.

### 9.2 Action Kit (Ground & Flight)

- **Ground sub-mode:** top-down or side-view run-and-gun; desktop icons or document images as cover/obstacles, windows/paragraphs as walls/rooms.
- **Flight sub-mode:** free-scrolling shooter weaving between floating "window"/image obstacles and taskbar/margin hazard zones.
- Params: weapon type/rate of fire, enemy HP/AI (patrol, chase, turret), health/armor, scroll speed (flight).
- Entities: turrets, chasers, pickups, destructible props (cosmetic only — never touches the real bundled file).
- **Featured preset: "Word War."** Live Document Mode ruleset where an enemy line besieges the page; every sentence written pushes the line back, every idle stretch lets it creep forward. Designed as the flagship example of §8's "productive, not punishing" intent.

### 9.3 Space Shooter Kit

- Classic vertical/horizontal shmup rules: wave patterns, bullet patterns, boss parameter sheets — the most direct homage to SEUCK.
- Desktop/document imagery typically used as a **backdrop/parallax layer** (e.g., a PSD's layers as literal parallax depth) rather than hard collision, though hard-edge mode is available.
- Wave editor: place enemy formations on a timeline, exactly like SEUCK's wave/attack pattern designer; in Live Document Mode, waves can instead be triggered by document milestones rather than a fixed timeline.

### 9.4 Casual Kit (Breakout, Zuma, and similar)

- **Breakout-style:** the "brick wall" auto-generated from a grid of desktop icons, tiled window thumbnails, or spreadsheet cells.
- **Zuma-style:** marble-match track hand-drawn as a spline path across the captured/imported source; marble colors sampled from the source's actual palette.
- Params: ball speed, combo rules, track shape, color count.
- **Featured preset: "Grow a Garden."** Live Document Mode ruleset where new writing sprouts decorative platforms/flora with no combat framing — the ambient, non-adversarial counterpart to "Word War."

### 9.5 Shared Engine Services (DACK Core)

- Rendering: 2D sprite/tile renderer, camera, particle FX, transparent overlay rendering for Live Mode.
- Physics: simple AABB/2D rigidbody suitable for platformer/action/casual; separate scrolling-shooter movement model for Space Shooter.
- Input: keyboard/mouse/gamepad (XInput), global-hook mode for Live Desktop Mode.
- Importer pipeline: plugin loader for document-format importers (§6).
- Live-edit tracking: Office add-in / UIA text-pattern / autosave-polling tiers feeding the Document Event Map (§8.2–8.3).
- Rule Engine: data-driven, JSON/YAML-backed parameter sheets and a lightweight visual event system, extended to consume both gameplay events and document-change events through the same grid.
- Save/Load: level files, ruleset presets, capture/document assets, playset packaging.
- Editor shell: shared UI chrome (toolbox, property inspector, Split View, Document Event Map, timeline where relevant) themed per toolkit.

---

## 10. The Rule Engine: Parameters, Event Grid & AI Behaviors

Guiding principle for this whole layer: **making a DACK game should be at least as fun as playing one.** That means the editor isn't a settings form bolted onto a game engine — it's a satisfying toy in its own right, with enough depth that experimentation is genuinely engaging, while never requiring code.

### 10.1 Two Layers, One System

- **Parameter Sheets (SEUCK-style):** every game element ships with sliders/dropdowns/numeric fields tuned live with instant visual feedback — drag a gravity slider and watch the test-dummy's jump arc redraw in the canvas immediately. This is the fast, low-friction layer that covers most day-to-day tuning.
- **Event/Condition/Action Grid (Klik & Play-style):** for behavior parameters alone can't express — sequencing, branching, cross-entity interaction, reacting to document edits (§8.3) — a visual node grid: pick a trigger, narrow it with conditions, attach one or more actions. Every node is drag-and-drop with its own mini parameter sheet; no text scripting anywhere.
- **They're the same underlying data model at different zoom levels of complexity**, not two bolted-together systems. A slider is really a pre-wired event/action pair with the grid hidden; any slider can be "popped open" into the grid to add a condition or branch. A beginner never needs to see the grid; a power user can turn any control into one.

### 10.2 Precise Placement & Fine-Grained Control

- Free, pixel-precise placement — arrow-key nudging, an optional snap-to-grid, alignment guides — rather than only tile-locked placement, matching the "a lot of fine detail" ambition and the hi-res art direction (§11).
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

---

## 11. Visual & Art Direction

### 11.1 Reference Points

- **Lemmings / Lode Runner:** small, extremely readable, characterful sprites that carry a lot of personality per pixel even at modest character size — the visual language for "a lot of end-user-placed detail that still reads clearly at a glance."
- **Kingdom Rush:** the scalability target. Hand-crafted, richly detailed environments that are gorgeous zoomed in on a single skirmish, yet stay fully legible and good-looking zoomed all the way out to see the whole battlefield. This is a camera-and-art bar to hit, not just a mood-board reference.

### 11.2 What This Means Technically

- **Hi-res source art rendered across multiple effective zoom levels**, rather than a single fixed-resolution tile grid — sprites and terrain need to hold up both close-in (animation detail visible) and pulled back to a full-level or full-screen overview (silhouette and color read clearly at a glance).
- **A real camera/zoom system, not a fixed viewport:** smooth zoom between a tight "gameplay" framing and a wide "overview" framing, with level-of-detail-aware rendering (simplify particle density, secondary animation, or parallax layer count at extreme zoom-out) so both ends of that range stay performant.
- **A consistent art bible per toolkit**, so community-contributed sprites/tiles/behaviors don't visually clash — a lightweight style guide (palette ranges, outline weight, silhouette rules) shipped alongside the toolkit contribution template (§14.4).
- **A "dress-up" pass for document/desktop-sourced geometry.** A lot of terrain in Native/Live Document Mode comes from real paragraph or window bounds rather than hand-placed tiles, so the renderer needs a tileable reskinning layer that turns arbitrary auto-detected rectangles into Lemmings/Lode-Runner-quality terrain automatically — procedurally-sourced levels shouldn't look like debug placeholders next to hand-authored ones.

### 11.3 Asset Production Implications

- This raises the art bar — and cost — well above a typical retro-styled indie construction kit, which is worth treating as an explicit scope/budget decision rather than a detail to discover late (see updated risk table, §17).
- A strong argument for leaning on the community model here too: an art-asset/skin index alongside the importer plugin index (§14.3), so the core team ships one polished default style per toolkit and the community contributes alternate skins over time, rather than the core team needing to hand-produce every possible aesthetic.

---

## 12. Level Data Model (sketch)

```json
{
  "dackVersion": "0.3",
  "toolkit": "action",
  "source": {
    "mode": "liveDocument",
    "importer": "docx",
    "documentAsset": "documents/chapter1.docx",
    "trackingTier": "officeAddin",
    "splitUnit": "headingLevel1",
    "sectionRange": [1, 3]
  },
  "geometry": [
    { "type": "platform", "source": "doc:paragraph", "rect": [120, 400, 640, 20] },
    { "type": "platform", "source": "doc:image", "assetRef": "img-04", "rect": [900, 200, 220, 160] },
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
        "considerations": ["distanceToPlayer", "documentEventPressure"],
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
    "documentEventMap": {
      "paragraphAdded": "extendPlatform",
      "textDeleted": "crumbleGround",
      "sustainedTyping": "playerShield",
      "idleTimeoutSeconds": 45,
      "onIdleTimeout": "enemyAdvance"
    },
    "intensity": "ambient"
  },
  "artSkin": {
    "toolkitStyle": "action-default",
    "terrainDressing": "autoRectangleSkin",
    "cameraZoomRange": [0.5, 2.5]
  }
}
```

This keeps the *source* (capture image, or a reference into a bundled real document plus a split unit and — for Live Document Mode — a tracking tier), the *geometry derived from it*, the *rules*, and now the *document event map* as separable, swappable layers — a player could reuse one live-tracked source across several rulesets/intensities, or apply one event map to many documents.

---

## 13. Technical Architecture (Windows Only, v1)

| Layer                          | Technology                                                                                                                                                                                     |
| ------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Screen capture                 | DXGI Desktop Duplication API; `Windows.Graphics.Capture` for per-window capture; UI Automation (UIA) for window/element bounding boxes                                                         |
| Document importers             | Format-specific plugin modules: OOXML parsing for `.docx`, PSD layer-table parsing for `.psd`; plugin contract for community-added formats (§6)                                                |
| Live edit tracking             | Office Add-in (VSTO/COM, Word Object Model events) as primary tier; UIA `TextPattern`/`TextRange` as fallback; autosave/temp-file polling as universal fallback (§8.2)                         |
| Overlay rendering (Live Modes) | Layered/transparent Win32 window (`WS_EX_LAYERED`) + Direct3D 11 (or bgfx) composited above the desktop or beside the document window                                                          |
| Editor / non-overlay UI        | Standard Win32/WinUI 3, or Dear ImGui for editor panels mounted on a D3D swap chain                                                                                                            |
| Physics                        | Custom lightweight 2D AABB physics (Box2D as a fallback/option)                                                                                                                                |
| Input                          | Raw Input API + XInput (controllers) + low-level global hooks (`SetWindowsHookEx`) for Live Desktop Mode                                                                                       |
| Auto-terrain mapping           | Shared mapping layer consuming a UIA/OpenCV region tree (capture), an importer's structural tree (native document), or a streamed diff of that tree (live document)                            |
| Rule engine                    | Shared parameter-sheet + event/condition/action grid runtime (§10.1); node graphs serialize into the same `.dacklvl` JSON as plain parameters                                                  |
| AI behaviors                   | Lightweight custom utility-AI/behavior-graph runtime (perception → decision → action), staggered-tick evaluation for performance at high entity counts (§10.3)                                 |
| Camera & rendering LOD         | Continuous zoom camera with level-of-detail-aware rendering (particle/parallax/secondary-animation scaling at zoom extremes) to hit the Kingdom Rush-style zoomed-in/zoomed-out target (§11.2) |
| Level/playset format           | JSON (`.dacklvl`) + zip/folder bundle (`.dackpack`) containing real source documents, human-readable and diffable                                                                              |
| Distribution runtime           | Full Editor build (creators) + minimal open-source **DACK Player** build (playing only), either standalone or embedded per playset                                                             |

**Why Windows-only for v1:** Desktop Duplication API, UI Automation, layered windows, global input hooks, and Office COM Automation are all Win32-specific and are the backbone of the screen-based and live-tracking modes. Static Native Document Mode is inherently more portable (OOXML/PSD parsing has no OS dependency) and is a natural first candidate for a future cross-platform Player build once the Windows-only capture/live-tracking features are out of scope for that platform.

---

## 14. Open Source & Community Model

### 14.1 Licensing

- **Core engine, toolkits, and the DACK Player: permissive open-source license (MIT or Apache-2.0)** — lowers the bar for contributors and downstream embedding, matching the spirit of the ACK/SEUCK community-tool lineage.
- **Community-contributed importers, toolkits, and event-map presets** live in their own repos/plugin packages, each free to choose a compatible license, registered in a community plugin index.
- **Playsets are separate from the app's license.** A playset bundles a user's own document(s); the creator chooses how they license/share *that content* (e.g., a simple "share-alike," "personal use only," or public-domain tag in `manifest.json`), independent of the engine's license. DACK should never assume a shared playset's contents are free to redistribute further without checking that tag.

### 14.2 Repository Structure

- `dack-core` — engine, physics, rendering, importer contract, live-tracking contract, Player runtime.
- `dack-toolkits/{platformer,action,spaceshooter,casual}` — genre modules built on core.
- `dack-importers/{docx,psd,...}` — format importer plugins, official and community.
- `dack-editor` — the full authoring app (toolkits + capture UI + Split View + Document Event Map + packaging).
- `dack-hub` — the community website/service (optional, could start as a static index over GitHub Releases before a dedicated service is justified).

### 14.3 Community Hub

- **Browse/publish playsets**, filterable by toolkit, tags, and whether they're screenshot-based, document-based, or live-adaptive.
- **Plugin index** for community importers, with a lightweight review/sandboxing process before listing (an importer is untrusted code parsing untrusted files — see §17).
- **Remix credit chain**: since playsets can be rebuilt from an edited document (§7.5), the hub can track "forked from" lineage the way level-editor communities (Doom WADs, LittleBigPlanet) traditionally do.
- **Moderation for embedded content**: because playsets can contain real personal documents, the hub needs a takedown/report path from day one, not as an afterthought — see the expanded privacy section below.

### 14.4 Contribution Model

- Clear `CONTRIBUTING.md` per repo, an importer-plugin template/starter kit so adding a new file format doesn't require understanding the whole engine, and a small "reference importer" (e.g., plain-text `.md`) shipped as a worked example.
- Governance: start as a benevolent-maintainer model (typical for a young open-source project); revisit toward a steering committee if/when the importer ecosystem grows.

---

## 15. Editor UX Notes

- **Toolbox sidebar** per toolkit (platform brush, ladder brush, enemy stamp, item stamp) — same interaction model as ACK/SEUCK's tile/sprite pickers.
- **Source switcher**: a persistent control for "Capture Desktop / Live Desktop / Import Document / Live Document," since all four feed the same downstream editor.
- **Split View** (§7.3): thumbnail strip of a document's auto-detected sections, drag to assign each to a level slot.
- **Document Event Map editor** (§8.3): a simple grid — document event on one side, game reaction on the other — with an intensity slider and live preview of the current tracking tier in use.
- **Event/Condition/Action grid canvas** (§10.1): the same node-grid surface used for the Document Event Map generalizes to any entity's behavior — poppable open from any parameter slider for players who want to go deeper.
- **Auto-detect overlay toggle**: show/hide proposed auto-terrain outlines.
- **Precision placement tools** (§10.2): pixel-nudge, optional snap-to-grid, alignment guides, and multi-select batch editing.
- **Property inspector**: click any placed object → parameter panel, consistent "select and tweak" workflow, with live-updating previews (jump arcs, patrol/perception ranges) drawn directly on the canvas.
- **One-click Test Play**: launches the level immediately without a separate export step.
- **Ruleset presets**: ready-made rulesets per toolkit ("Word War," "Grow a Garden," and the earlier static presets) so a new player gets a working game before touching a slider.
- **Playset packaging wizard**: walks the player through picking which documents to include, which levels reference them, license/share tag, and a mandatory preview-before-export step (§16).

---

## 16. Privacy & Safety Considerations

Bundling *real source files* — and, in Live Document Mode, continuously watching them change — raises the stakes further, so this section now covers capture, native-document, and live-tracking risks:

- **Local-only by default.** Captures, imports, live-tracking sessions, and levels stay on-device unless the player explicitly exports/publishes a playset.
- **Explicit share preview.** Since a playset can literally contain someone's real Word document or PSD, the export wizard must show exactly which files are about to be bundled, with a clear list and preview, before any export completes — no silent inclusion.
- **Document metadata scrubbing.** Real `.docx`/`.psd` files often carry hidden metadata: author name, tracked changes, comments, revision history, embedded file paths. Offer a one-click **"Strip metadata before sharing"** pass in the packaging wizard, off by default only for local saves, on by default (or at least strongly prompted) for anything headed to the community hub.
- **Live Mode scope control.** Restrict capture/overlay to a single monitor, app, or region to avoid exposing sensitive windows.
- **Live Document Mode is opt-in per document, with a visible "being tracked" indicator.** Continuously reading document content in real time is a materially bigger privacy footprint than a one-time import — DACK should never enable live tracking silently, should show a persistent on-screen indicator while it's active, and should make it trivial to pause/detach tracking (§8.4) at any moment.
- **License/consent tag on every published playset.** Since a shared playset can contain someone else's document if the creator isn't careful (e.g., importing a coworker's file), the publish flow should require the creator to affirm they have rights to share the bundled content — not a legal guarantee, but a meaningful friction point and a basis for hub takedowns.
- **No network transmission** of captures, imports, live-tracking data, or playsets without explicit user-initiated export/publish.

---

## 17. Key Technical Risks

| Risk                                       | Notes                                                                                                                                                                                                                                                                                                                      |
| ------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Live Desktop Mode input routing            | Overlay input handling without stealing focus unpredictably from the app underneath; needs a clear engage/disengage hotkey.                                                                                                                                                                                                |
| Geometry drift in Live Mode                | Moved/resized/closed source windows need explicit update/freeze/vanish rules for their derived platforms.                                                                                                                                                                                                                  |
| Auto-terrain accuracy (capture)            | UIA coverage varies by app; manual-editing fallback must be first-class, not an edge case.                                                                                                                                                                                                                                 |
| Importer format-version drift              | Office/Adobe file formats evolve; importers need versioned parsers and a graceful "unsupported version" fallback rather than a hard crash.                                                                                                                                                                                 |
| Untrusted importer plugins                 | A malicious or buggy community importer parsing an untrusted file is a real attack surface; plugins should run sandboxed (e.g., isolated process, restricted I/O) and go through a lightweight hub review before listing.                                                                                                  |
| Office Add-in reliability/versioning       | COM Automation and the Word Object Model shift across Office versions/builds; the add-in needs its own compatibility matrix and a clean fallback to the UIA tier when it can't attach.                                                                                                                                     |
| Live-tracking latency vs. distraction      | Too slow feels disconnected from typing; too aggressive feels like it's fighting the writer. Needs tunable thresholds and playtesting across writing styles (bursty vs. steady).                                                                                                                                           |
| Gameplay actively distracting from writing | The central design risk of §8: default rulesets need genuinely forgiving pacing, and the pause/detach control (§8.4) must be fast and always reachable, or this feature undermines the "productive" framing it's built on.                                                                                                 |
| Performance                                | Real-time capture/compositing/import/live-tracking + physics + rendering needs a tight frame budget; profile capture, UIA polling, importer parsing, and live-diffing separately.                                                                                                                                          |
| Multi-monitor/DPI scaling                  | Per-monitor DPI awareness needed so captured/imported coordinates map correctly to overlay coordinates at all scale factors.                                                                                                                                                                                               |
| Anti-malware false positives               | Global input hooks + screen capture + always-on-top overlays + an Office add-in resemble techniques used by malicious software; expect to need code-signing and AV allow-listing work.                                                                                                                                     |
| Accidental sensitive-content sharing       | The single biggest risk from bundling and live-tracking real files: mitigated by the export preview, metadata scrubbing, consent tag, and visible tracking indicator in §16 — but this needs ongoing product attention, not a one-time fix.                                                                                |
| Rule engine progressive-disclosure failure | If "pop a slider open into the grid" isn't genuinely seamless, the hybrid system (§10.1) risks becoming two disconnected tools instead of one — needs early usability testing with non-programmers specifically.                                                                                                           |
| AI behavior performance at scale           | Utility-AI evaluation across many entities (especially with squad coordination, §10.3) can get expensive fast; the staggered-tick + auto-scale-down guardrail needs to be built and profiled early, not bolted on after content creators start building large encounters.                                                  |
| Art production scope/cost                  | The Lemmings/Lode-Runner-detail + Kingdom-Rush-scalability bar (§11) is a real production commitment, well beyond a typical retro-styled indie toolkit; needs an explicit budget/scope decision, and the community skin-index idea (§11.3) should be validated early as a genuine cost-sharing mechanism, not just a hope. |

---

## 18. Suggested MVP Scope (Phase 1)

To validate the core hook without building everything at once:

1. **Platformer Kit only**, **Snapshot Mode + `.docx` Native Document Mode** (defer Live Desktop Mode, Live Document Mode, and `.psd`).
2. Capture pipeline: full-desktop/single-window capture + UIA-based auto-terrain.
3. `.docx` importer: paragraph/heading/image structural parsing, page-break-based level splitting.
4. **Rule engine v0:** parameter sheets for all Phase 1 entities, plus a minimal event/condition/action grid covering a handful of trigger/condition/action types — enough to validate the "pop a slider open into the grid" progressive-disclosure flow (§10.1) before investing in the full node library.
5. **AI v0:** two or three hand-built behavior presets (Patrol, Chase) built on the perception/decision/action block structure (§10.3), not yet author-editable in the grid — proves the architecture without needing the full authoring UI on day one.
6. Level editor: platform accept/reject/nudge, enemy/item/goal placement, precision placement tools (§10.2), Split View for multi-page documents.
7. **Placeholder-quality art** for Phase 1, deliberately below the full Lemmings/Kingdom-Rush target (§11) — validates gameplay and editor feel first; final art direction and the camera/zoom LOD system are explicitly Phase 2+ investments once the core loop is proven.
8. Playset export: bundle source `.docx` + `.dacklvl` files + minimal manifest, with the mandatory preview/metadata-scrub step.
9. Minimal open-source **DACK Player** that can load and play a `.dackpack` with no editor installed.
10. One playable end-to-end loop: import → auto-detect → edit → split into levels → playtest → package → reload in Player.

**Phase 2:** `.psd` importer, Action Kit, Space Shooter Kit (reuse most of Phase 1's editor shell); first pass at target art direction and the zoom/camera LOD system (§11); expand the AI behavior library and open it up in the grid editor for author-level tuning.
**Phase 3:** Live Desktop Mode; "Rebuild Level" diffing (§7.5); first Live Document Mode pass using the UIA text-pattern tracking tier only (skip the Office add-in initially, to validate the concept before the deeper integration work).
**Phase 4:** Office Add-in tracking tier for lower-latency Live Document Mode, "Word War" and "Grow a Garden" presets, Casual Kit, squad/group AI behaviors, community hub (publish/browse/remix), community importer and art-skin plugin indexes and review process.

---

## 19. Decisions Log & Open Questions

### 19.1 Resolved

- **Live Mode is always non-destructive — no "chaos mode."** Live Desktop Mode and Live Document Mode never manipulate the real desktop, real windows, or the real document content. What the player sees and interacts with is a live *mock-up*: a continuously-refreshed read of the source (pixels, UIA tree, or document structure) rendered into the game layer, not a control channel back into the source. Destroying an "enemy" derived from a window never closes/minimizes it; nothing the game does can alter the bundled document outside of the explicit, player-initiated edits made in the native app itself. This is a locked decision, not just a default — no future opt-in destructive mode is planned.
- **The rule engine is a hybrid: parameter sheets and an event/condition/action grid, sharing one data model.** Sliders stay the fast default path; any slider can be "popped open" into the visual grid for players who want branching or conditional behavior — this is one progressively-disclosed system, not two bolted together. This decision extends to a real ambition to bring modern, composable AI thinking (perception/decision/action, utility scoring) into the classic construction-kit format, while staying fully no-code (§10). Creation itself is a first-class design target — "at least as fun as playing" — which also locks in the visual direction: hi-res, characterful art in the spirit of Lemmings/Lode Runner, with Kingdom Rush-level camera scalability from close-up detail to full-battlefield overview (§11). This is a meaningfully higher production bar than a typical retro construction kit, deliberately deferred past Phase 1 (placeholder art first, per §18) so the core loop gets validated before the full art investment.

### 19.2 Open Questions to Resolve Next

- Should the community hub launch as a first-party hosted service, or start as a lightweight index over community-run repos/releases, given the added moderation burden of hosting real user documents?
- What's the right default for metadata scrubbing — always-on for hub publishing, or a strongly-worded prompt the creator can override?
- Controller support priority for Action/Space Shooter kits vs. keyboard/mouse-first for Platformer/Casual?
- Where should importer plugin trust boundaries sit exactly (in-process with strict validation vs. fully sandboxed subprocess) — worth a short spike before Phase 2's `.psd` importer sets the pattern.
- For Live Document Mode: should the default intensity ship as "ambient," or should the flagship "Word War" preset default to something with real stakes to make the concept land in a first impression — and if so, how is that reconciled with the "never punishing by default" design intent in §8.4?
- Is the Office Add-in worth building in Phase 4, or does the UIA text-pattern tier (universal, no install step) turn out to be good enough that the add-in becomes a "nice to have" rather than a requirement?
- How deep should the "pop a slider into the grid" progressive-disclosure interaction go before it stops feeling seamless — worth an early usability spike with non-programmer testers, per the risk in §17.
- Given the raised art bar (§11), is a small in-house art team + community skin index the right production model from day one, or should Phase 1–2 lean entirely on placeholder/programmer art until the core loop is proven and funding/contributor interest is clearer?

---

## 20. One-Line Pitch

**"Turn your actual documents and desktop — Word files, Photoshop canvases, the taskbar and all — into real, shareable, editable game levels that can even react live as you write, using open-source construction-kit tools to build platformers, shooters, and casual games out of whatever you're working on."**
