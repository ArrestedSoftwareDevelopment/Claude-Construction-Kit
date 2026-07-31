# DACK Sprite Studio Mini-App

- **Status:** Active product specification with a working RAD proof
- **Baseline:** Full-screen actor/animation workspace, July 2026
- **Authority:** Sprite Studio ownership, visual editing workflow, actor/card composition, and asset-profile behavior
- **Related:** [DACK GUI Architecture](DACK-GUI-Architecture.md) and [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md)

## Purpose

Sprite Studio is the primary DACK-native actor/animation assembly workspace. It does not replace the live-linked pad’s quick pixel-editing job; it contains or launches that pad as one focused Frame-mode tool while taking responsibility for importing, slicing, animation, boxes, bindings, and reusable profiles.

The goal is not to clone Aseprite. The DACK art tool should be a construction-kit editor for office-game actors:

- pick a character, enemy, projectile, explosion, or object;
- inspect its source frames;
- label actions;
- edit pixels when useful;
- preview the actor live on the playfield;
- bind animation, behavior, projectile, explosion, sound, and text-interaction rules;
- save the result as a reusable DACK asset/profile.

The design center remains "glorified C64," not "junior Aseprite": small canvases, constrained palettes, fast feedback, live playfield binding, and creator-friendly choices.

## Product Shape and Ownership

Sprite Studio is a full-screen, owned DACK workspace opened from the Cockpit. It is not a floating utility, a permanent side panel, or a second independent application state.

Opening Studio hides the ordinary Cockpit surface while preserving the calling tab, card, actor selection, source, working clone, mutations, and unsaved editor state. Studio receives editor input ownership and shows the pointer. Its proper upper-right close gadget and Esc both return to the exact calling context. The Boss Key remains global and independent. Closing the main editor also closes or safely returns from Studio so it cannot become an orphan window.

Entry points:

- Character Workbench -> open selected actor in Sprite Studio.
- Enemies page -> edit selected enemy.
- Projectiles page -> edit selected projectile/effect profile.
- Objects page -> edit selected pickup/solid/prop sprite.
- Sprite Pad button -> opens the quick live-linked pixel pad for the selected frame, with an explicit "Open in Studio" action.
- Asset Picker -> choose "Edit/Fork" on any asset.

Workspace modes:

- **Single monitor:** Sprite Studio owns the full DACK editor surface and returns to the Cockpit when closed.
- **Two monitors:** Sprite Studio owns the editor monitor while the other monitor may show a live, non-editing playfield preview.
- **Quick Sprite Pad:** a small, hideable, in-context pixel tool bound to the selected actor/frame. It is for rapid C64-scale changes, not actor assembly, slicing, animation management, or profile wiring.

The Pad and Studio therefore have different jobs. The Pad preserves the toy-like "change a pixel and see it live" loop. Sprite Studio owns serious frame, animation, attachment, behavior, sound, and asset work.

## Core principle: selected thing first

Sprite Studio should always have a selected target:

- player character;
- enemy actor;
- projectile profile;
- explosion/effect profile;
- pickup/object;
- loose frame/sprite;
- imported sheet draft.

If nothing is selected, it opens to the Asset Picker.

The editor must constantly answer:

- Am I editing a shared asset, a level-local fork, or a temporary runtime clone?
- What game object currently uses this asset?
- Will changes update the live playfield immediately?
- Is this asset approved for runtime/export, quarantined, raw-local, or project-created?

## Main layout

```text
Sprite Studio
  Top Bar
    Back / Close
    Target name
    Asset status
    Selected animation label
    Save / Save As Fork
    Apply To Selected
    Test In Play

  Left: Asset / Target Picker
    Player Characters
    Ground Contact Enemies
    Ground Shooters / Guards
    Flyers / Space Ships
    Tanks / Vehicles
    Bosses / Large Hazards
    Projectiles
    Explosions / Effects
    Objects / Pickups / Props
    Raw Imports
    Project-Created

  Center: Visual Editor
    Neutral high-contrast stage
      Animated selected-label preview
      Editable current-frame preview
    Previous / Next frame arrows
    Source sheet / selected-animation frame strip
    Onion skin / live playfield preview

  Right: Inspector
    Summary
    Animation labels
    Gameplay bindings
    Projectile / explosion slots
    Sound slots
    Hitboxes / origins / attachment points
    Provenance / export status

  Bottom: Timeline / Clip Strip
    Frames in the selected animation only
    Labels
    Timing
    Loop / ping-pong / strobe
    Test playback
```

### Visual workspace rules

- The large animated preview and editable current frame sit side by side whenever width permits. The animated side continuously plays the selected label; the edit side shows exactly one selected frame and its pixel/origin/box overlays.
- Clicking an animation label selects it and immediately changes the large animated preview. Editing is scoped to that animation's frame sequence so frames from unrelated actions do not create visual confusion.
- Previous/Next arrows step through the selected animation sequence. Left/Right provides the same keyboard action; Home/End jump to the first/last frame.
- The stage is neutral and deliberately high contrast. It may offer light, dark, and transparency-checker backgrounds and should automatically avoid white-on-white or black-on-black previews. A visible baseline and shadow toggle help expose origin and alignment errors.
- The asset picker, label list, Inspector, and timeline have independent scroll containers. The title/selection/close row and current frame controls stay reachable while any of those areas scroll.
- On medium-width screens the two previews may divide the center evenly. On narrow screens they stack, but neither may disappear or push Close/Save controls offscreen.
- Buttons size to their labels instead of stretching across the page. Labels, toggles, frame numbers, disabled states, and focus rings use strong contrast in light, dark, and high-contrast themes.
- `Tab`/`Shift+Tab` follow visual reading order; arrows navigate tabs, labels, frames, and bounded values; `Enter`/`Space` activate the focused command. Typing in names or frame sequences suppresses gameplay shortcuts.
- Studio restores the selected target, label, frame, expanded Inspector section, and scroll positions after preview/test play when those objects still exist.

## Character Editor hub layout

The overall actor editor should draw from every other DACK system. Its first screen should not be a raw timeline. It should be a large selected-character card with shelves beneath it.

Recommended shape:

```text
Character Editor
  Large Idle Preview
    selected sprite displayed big
    default Idle animation running
    baseline/shadow visible
    current role/source/status visible

  Character Pickers
    player characters
    enemy categories
    vehicles/ships/tanks

  Slot Shelves
    Projectile
    Explosion / Impact
    AI / Behavior
    Spawn Rules
    Sounds
    Effects
    Text Rules
    Hitboxes / Origins
    Scale / Physics

  Links
    Sprite / Animation Editor
    Projectile Editor
    Effect Library
    Sound Library
    AI Rule Cards
    Spawn Point Builder
```

The creator-facing model should be: "Here is my creature/person/ship/tank. What do I plug into it?"

For enemies, the AI shelf is prominent. For player characters, the input/control shelf is prominent. For projectiles and objects, incompatible shelves should collapse or read as unavailable rather than remain visually noisy.

Enemy Spawn Points belong in this Builder family too. They are not just map decorations: they bind enemy graphics, enemy pools, behavior cards, spawn cadence, burst count, max-active budget, direction, and optional spawn/death effects. The simple creator flow should be "drag a spawn flag into the level, then drop an enemy card onto it."

The idle preview matters because it anchors the whole page emotionally and visually. The user should not feel like they are editing filenames and ranges; they are working on a little being, ship, tank, or toy.

## Card Composition Model

Cards are DACK's friendly alternative to a node graph.

A card can be:

- atomic: one reusable ingredient such as a sprite sheet, animation label set, AI rule, projectile, sound, explosion, text rule, or physics rule;
- composite: a finished object made by combining other cards, such as a player, enemy, weapon, spawner, pickup, pinball bumper, tower, semantic word-object, level, world/chapter, or complete playset.

The crucial design idea is that a complex object becomes a card too. For example:

```text
Blue Guard Enemy Card
  Sprite Card: Blue Guard
  Animation Card: Blue Guard default labels
  AI Card: Patrol + Track Player
  Attack Card: Fireball
  Sound Card: Guard fire / explosion
  Text Rule Card: Stands on text, shots destroy text
  Physics Card: Grounded
```

That completed `Blue Guard Enemy Card` can then be dragged into a level, dropped onto an Enemy Spawn Point, used as a tower-defense wave unit, or forked into a variant. The creator is not wiring raw systems every time; they are building little recipes and then reusing them as larger pieces.

The same recursion applies above individual objects. A tuned level becomes a `Level Card` containing its Snapshot, actors, placed tools, rules, OCR/Word Sense cache, mutations, score rules, and local assets. Several Level Cards can become a `World` or `Chapter Card`. A full `.dackpack` is effectively a `Playset/Campaign Card` made from those larger cards plus shared assets and publishing policy.

Recommended card rule:

> If a creator can configure it, save it, drag it, or reuse it, it should probably be represented as a card.

The UI should still hide depth until needed. Beginners see friendly cards like `Flying Shooter`, `Fireball`, or `Three-Ball Brickbat`. Advanced creators can open the card to inspect the ingredients inside.

## Current Implementation Baseline

The working prototype already establishes:

- a large/full-screen Sprite Studio surface launched from the actor/character workflow;
- categorized actor and asset picking, a large animation preview, editable labels and frame sequences, hand-numbered frames, dashed unsupported actions, reverse/ping-pong, strobe, save, and load;
- selected-label preview and creator-tested default mappings for several imported actors;
- source-specific import experiments for fixed grids, fixed rectangles, component extraction, blob detection, and individual frame files;
- Player is now a top-level Cockpit tab, separate from the general Builder.
- Player Character selection now uses draggable Builder Cards instead of plain buttons.
- `Stickman 2.0` is the default stickfigure card. It uses OctoPyte v0.2 sheets for Idle, Run, Jump, Melee/Punch, and Death.
- `Classic Stickman` preserves the OctoPyte v0.1 thin-stick baseline.
- `Dungeon Runner` is the first climb-native player card. It uses the CC0 8-Bit Dungeon player frames for idle, four-frame run, fall, rope/tool poses, and a real two-frame climb cycle. This is the right test body for Lode Runner-style ladders, ropes, gutters, text-crawl surfaces, and compact RPG/platformer hybrids.
- Dungeon Runner imports recolor near-white source pixels to black at runtime so the white CC0 pixel art remains readable on white document playfields. The original asset files are not modified.
- The full 8-Bit Dungeon PNG object set is admitted as the first total playset seed. The current shelf maps key pieces to existing behaviors; exact art binding belongs in the shared asset/rendering work rather than another one-off importer path.
- Blue Guard, Green Crawler, and Green Snake are the current component-selection import tests from the irregular TGC platformer sheet. Their working selections must be compiled into explicit rectangles/order with a source hash before runtime profiles stop depending on detector indices.
- Pressing `Use` applies the player card.
- Dragging a player card onto the playfield applies it and, when no Start Point is present, places the player at the drop point.
- If a Start Point exists, the card still changes the player but the Start Point remains the authority for play-mode spawn.
- This is the interaction pattern to reuse for Enemy Cards, Projectile Cards, Spawn Point Cards, Object Cards, and eventually Level Cards.
- `Punch` is now a first-class source animation state/label for preview and future melee/contact rules. In the builder UI, this should surface as `Melee / Close Action` rather than only `Punch`.
- RPG mode should bind the same close-action slot to swords, daggers, staves, claws, bites, unarmed punches, or tool swings instead of inventing a separate verb for each skin.
- Enemies now need invisible awareness/radar as a first-class AI attribute. Patrol, facing, tracking, and firing should only become player-directed once the player enters that enemy's radar bubble. Smarter enemies simply get larger radar values; dumb/contact enemies get shorter ones.

This baseline is valuable but still prototype-grade: parts of the Studio are constructed by the root controller, curated imports rely on hardcoded factories, some long lists can overflow, and the character/profile model is not yet the single runtime authority. Those are consolidation targets, not reasons to discard the working editor.

## Modes

Sprite Studio should expose modes as large friendly tabs. These are task modes, not different apps.

### 1. Pick

Choose the asset or actor to edit.

Categories:

- Player Characters
- Ground Contact Enemies
- Ground Shooters / Guards
- Flyers / Space Ships
- Tanks / Vehicles
- Bosses / Large Hazards
- Projectiles
- Explosions / Effects
- Objects / Pickups / Props
- Imported Drafts
- Project-Created

Each card should show:

- thumbnail;
- name;
- role tags;
- source pack/provenance badge;
- runtime/export status;
- labels available;
- "Use as Player," "Add Enemy," "Use Projectile," "Edit/Fork."

### 2. Frame

The frame editor is the current live-linked sprite pad expanded into a serious but constrained tool.

Features:

- fixed small canvas presets: 16x16, 24x21, 32x32, 48x48, 64x64;
- custom size allowed only with a warning once the source is imported;
- palette strip with small palettes by default;
- transparent color picker, usually white by default;
- color replace and transparent-color flood cleanup;
- hairline preservation toggle for one-pixel stick-figure art;
- pencil, eraser, fill, line, rectangle, mirror, nudge;
- frame crop/trim;
- frame padding/common display box;
- baseline/bottom alignment;
- onion skin previous/next;
- live update on selected actor.

Non-goals for this stage:

- layers;
- full brush engine;
- PSD-style editing;
- advanced color management.

### 3. Slice

The importer/slicer turns source sheets into candidate frames.

Importer modes:

- grid slice;
- strip by fixed frame size;
- blob-detected tight rectangles;
- manual rectangles;
- Aseprite PNG + JSON import;
- GIF-to-clip normalization;
- later source-format research only for `.ase`, `.aseprite`, PSD.

Required controls:

- frame width/height;
- columns/rows;
- margin/padding;
- background/transparent color;
- minimum blob size;
- row/column sorting;
- frame renumbering;
- delete false positives;
- merge/split frame candidates;
- common display box preview.

Important lesson already learned: detection finds pictures, not meaning. The creator still labels actions.

### 4. Animate

Animation is label-first. A creator should not need to think in engine states first.

Core label set:

- Idle
- Walk
- Run
- Turn
- Jump
- Jump Up
- Jump Down
- Fall
- Land
- Climb
- Climb Up
- Climb Down
- Crawl
- Slide
- Dig
- Dig Up
- Dig Down
- Shoot
- Run Shoot
- Jump Shoot
- Shoot Up
- Shoot Down
- Hurt
- Death
- Bounce
- Power Up
- Special
- Custom

Per-label fields:

- enabled/unavailable toggle;
- frame sequence, not only start/end;
- FPS;
- per-frame duration override;
- loop, play once, hold last;
- ping-pong/reverse;
- strobe toggle and count/intensity;
- effect overlay slots;
- engine binding;
- preview button.

The current start/end editor is useful, but the mini-app should graduate to editable sequences:

```text
Run: 3,4,5,6,7,8,9,10
Jump: 11,12,13,12,11
Death: 20,21,22,23 [strobe 8]
Climb: -
```

A dash means the character does not support that action. DACK should gracefully fall back to Idle, Fall, or the closest usable label.

### 5. Bind

Binding connects art to gameplay.

Binding groups:

- locomotion: platformer, overhead, flyer, tank, ball/table toy;
- control: player input, AI-controlled, passive object;
- AI: contact, patrol, chase, defend, flee, horde/flock, hover, turret, boss, radar/awareness;
- spawning: enemy pool, spawn point, interval, burst count, max active, spawn direction, spawn effect;
- physics: ground, flying, thrust/inertia, gravity-sensitive, climb-capable, text-aware;
- environmental awareness: stand on text/platforms/ramps/elevators, ride conveyors, slide on slides, reverse at gaps, optionally use ladders/routes;
- collision: hitbox, hurtbox, contact damage, solid, sensor-only;
- text rules: can stand on text, can crawl text, can destroy text, can harvest text, protected-text friendly;
- scale: base size, game-family size overrides, text-relative ratio.

This is the place where "ground enemy," "shooter," "space ship," "tank," and "player character" become visible role cards instead of hidden code assumptions.

### 6. Attack

Attack profiles should be assignable cards.

Fields:

- projectile profile;
- muzzle/attachment point;
- fire sound;
- cooldown;
- range;
- speed;
- damage;
- shot count/spread;
- aim style: straight, face player, aimed, arc, homing, turret, random;
- text interaction: pierce, bounce, erase, harvest, explode letters, ignore;
- explosion profile;
- impact sound;
- blast radius;
- friendly-fire/team rules.

Simple view:

```text
Weapon: Fireball
Damage: 1
Range: Mid
Text: Blast letters
Sound: pop-laser-01
```

Advanced view can expose the full profile later.

### 6a. Spawn

Spawn profiles should be assignable cards that connect an editor-only flag to one or more enemy/actor profiles.

Fields:

- spawn point graphic;
- enemy pool / allowed enemy cards;
- spawn interval, whole number 1-10;
- burst count, whole number 1-10;
- max active per enemy/sprite, hard capped at 10;
- spawn direction: left, right, up, down, toward player, away from player, route, random;
- spawn speed multiplier;
- spawn effect;
- linked route, patrol zone, defend target, or home anchor;
- active condition: always, near player, after switch, after checkpoint, wave timer, boss phase.

Simple view:

```text
Spawn: Blue Guards
Every: 5 sec
Burst: 1
Max: 3
Direction: Toward Player
```

The design stance is intentionally bounded. Spawners make action, escort, tower-defense, Robotron, and office-gremlin modes possible, but the UI should keep them friendly and safe by default.

### 7. Effects

Effects should be reusable across actors, projectiles, Brickbat, pinball, words, and UI feedback.

Slots:

- spawn effect;
- fire effect;
- impact effect;
- hurt effect;
- death effect;
- pickup effect;
- power-up effect;
- shield/invulnerability effect;
- text harvest effect.

Effect types:

- sprite-sheet explosion;
- comic word burst;
- letter shrapnel;
- spiral/starburst text;
- whole-word spinaway;
- glow/pulse/ring;
- color cycle;
- screen shake;
- shadow/skew projection.

### 8. Sounds

Sounds should use the same slot model.

Actor sound slots:

- jump;
- land;
- climb;
- dig;
- hurt;
- death/defeat;
- pickup;
- power-up;
- alert;
- idle bark;
- footstep/loop.

Weapon sound slots:

- fire;
- impact;
- explosion;
- empty/cooldown;
- charge;
- release.

Object sound slots:

- collect;
- activate;
- open/close;
- bump;
- break;
- deny/locked.

### 9. Hitboxes, origins, and attachment points

This needs its own visual sub-panel because it will prevent endless scale/offset bugs.

Per asset:

- display box;
- trim box;
- baseline;
- origin/pivot;
- feet point;
- head point;
- center/mass point;
- hitbox;
- hurtbox;
- pickup radius;
- muzzle point;
- hand points;
- shadow anchor.

Per animation label:

- optional label-specific hitbox/hurtbox;
- optional muzzle override;
- optional baseline override.

Debug display:

- show/hide all boxes;
- show current frame's origin;
- show previous frame ghost;
- show collision body separately from art.

## Data model

Sprite Studio should gradually replace hardcoded `SpriteAnimationSet.TryLoadX()` assumptions with manifest loading.

Proposed files:

```text
*.dacksprite.json
  asset identity, source/provenance, frames, palettes, display box

*.dackanim.json
  animation labels, frame sequences, timing, loop/strobe/ping-pong

*.dackactor.json
  character/enemy role, movement/AI/combat/text rules, hitboxes, sounds

*.dackweapon.json
  projectile, attack, explosion, sound, text interaction profile

*.dackeffect.json
  reusable effect profile
```

For early implementation, these can be one combined character profile:

```text
DackCharacterProfile
  id
  name
  roleTags[]
  spriteAsset
  animationClips[]
  scale
  displayBox
  hitboxes
  attachmentPoints
  movementRules
  aiRules
  attackProfile
  soundSlots
  effectSlots
  provenance
```

## Runtime/editor asset states

Every asset card needs a visible state:

- Approved runtime asset
- Project-created
- Level-local fork
- Quarantined local asset
- Raw-local evaluation asset
- Missing source
- Needs provenance
- Needs labeling
- Export-safe
- Not export-safe

This protects the hub/export path and makes metadata/licensing visible without making the creator feel like they are doing paperwork.

## Save and fork model

Creators must always know what they are changing.

Recommended actions:

- **Save**: update the current editable project asset.
- **Save As Fork**: duplicate the current profile and rebind the selected actor to the fork.
- **Apply To Selected**: use the edited asset on the selected actor.
- **Promote Defaults**: developer-only/project-curator action that turns a tested local mapping into the default for that source.
- **Revert To Source Default**: reload the curated/default label set.
- **Export-Safe Copy**: clone only approved/project-created files into a package-ready area.

Never edit raw originals. DACK edits copies, manifests, and level-local forks.

## Implementation and Optimization Relationship

This specification no longer maintains a competing pass list or "next prototype" checklist. The authoritative extraction, performance, test, and migration sequence is the [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md), especially its UI-shell, asset-catalog, sprite-import compiler, and actor-profile work.

Sprite Studio work must preserve the current editing loop while converging on these outcomes:

- the full-screen owned-workspace and return behavior described above;
- a side-by-side selected-label animation preview and current-frame editor that remain reachable at supported monitor sizes;
- independent scrolling, strong contrast, predictable keyboard navigation, and no offscreen controls;
- versioned character/animation/import manifests compatible with existing `.dackanim.json` work;
- source-specific import profiles compiled once, including accepted frames, order, origins, display boxes, transparency/recolor policy, provenance, and diagnostics;
- saved creator mappings that become reusable defaults without adding another hardcoded root-controller branch;
- assignable role, behavior, text, projectile, explosion, sound, effect, hitbox, and attachment cards;
- the quick Sprite Pad retained as the small live pixel tool rather than expanded into a competing editor.

Any implementation slice is complete only when Studio can close cleanly, restore its calling selection and frame, avoid mutating raw originals, and leave Play/Build/Understand, the source clone, and the mutation history unchanged.
