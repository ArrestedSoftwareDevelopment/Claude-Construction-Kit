# DACK Sprite Studio Mini-App

## Purpose

Sprite Studio is the fully integrated DACK sprite editor/animator mini-app. It replaces the idea of a simple sidebar sprite pad as the primary art tool, while preserving the pad as one fast editing mode inside the larger workflow.

The goal is not to clone Aseprite. The DACK art tool should be a construction-kit editor for office-game actors:

- pick a character, enemy, projectile, explosion, or object;
- inspect its source frames;
- label actions;
- edit pixels when useful;
- preview the actor live on the playfield;
- bind animation, behavior, projectile, explosion, sound, and text-interaction rules;
- save the result as a reusable DACK asset/profile.

The design center remains "glorified C64," not "junior Aseprite": small canvases, constrained palettes, fast feedback, live playfield binding, and creator-friendly choices.

## Product shape

Sprite Studio should feel like a mini-app opened from the Cockpit, not a permanent side panel.

Entry points:

- Character Workbench -> open selected actor in Sprite Studio.
- Enemies page -> edit selected enemy.
- Projectiles page -> edit selected projectile/effect profile.
- Objects page -> edit selected pickup/solid/prop sprite.
- Sprite Pad button -> opens Sprite Studio directly in live-pixel mode.
- Asset Picker -> choose "Edit/Fork" on any asset.

Recommended window modes:

- Docked right panel for quick edits.
- Large Cockpit page for normal editing.
- Later two-monitor layout: playfield on one monitor, Sprite Studio/Cockpit on the other.

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
    Source sheet / frame strip
    Frame canvas / pixel pad
    Onion skin / previous-next preview
    Live playfield preview thumbnail

  Right: Inspector
    Summary
    Animation labels
    Gameplay bindings
    Projectile / explosion slots
    Sound slots
    Hitboxes / origins / attachment points
    Provenance / export status

  Bottom: Timeline / Clip Strip
    Frames
    Labels
    Timing
    Loop / ping-pong / strobe
    Test playback
```

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
```

The creator-facing model should be: "Here is my creature/person/ship/tank. What do I plug into it?"

For enemies, the AI shelf is prominent. For player characters, the input/control shelf is prominent. For projectiles and objects, incompatible shelves should collapse or read as unavailable rather than remain visually noisy.

The idle preview matters because it anchors the whole page emotionally and visually. The user should not feel like they are editing filenames and ranges; they are working on a little being, ship, tank, or toy.

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
- AI: contact, patrol, chase, defend, flee, horde/flock, hover, turret, boss;
- physics: ground, flying, thrust/inertia, gravity-sensitive, climb-capable, text-aware;
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

## First implementation path

### Pass 1: Studio shell

- Add a larger Sprite Studio panel/page opened from Character Workbench.
- Move the current frame strip and label rows into this page.
- Keep the old sidebar button as a shortcut into Sprite Studio.
- Add target summary: actor name, role, source, export status, scale.
- Add visible tabs: Pick, Frame, Slice, Animate, Bind, Attack, Effects, Sounds, Boxes.

### Pass 2: Character profiles

- Create a first `DackCharacterProfile` class/JSON.
- Save/load selected actor name, source id, labels, scale, projectile flag, toughness.
- Let selected enemies and player characters load/save profile defaults.
- Keep existing `.dackanim.json` compatible.

### Pass 3: Real sequence editor

- Replace start/end-only labels with editable frame sequences.
- Support dashed unavailable labels.
- Support ping-pong expansion preview.
- Preview the chosen label on the live actor.
- Store creator-facing frame numbers and internal indices.

### Pass 4: Binding cards

- Add role cards: Player, Ground Contact, Ground Shooter, Flyer, Space Ship, Tank, Boss, Object Toy.
- Add behavior cards: Patrol, Chase, Defend, Contact Damage, Shooter, Horde/Flock.
- Add text cards: Text Solid, Text Crawl, Text Destroyer, Text Harvester, Text Friendly.

### Pass 5: Projectile/effect/sound slots

- Let actors select a weapon profile.
- Let weapon profiles select projectile frame, explosion strip, fire sound, impact sound.
- Connect the current fireball/explosion profile to this UI instead of hardcoding.

### Pass 6: Hitbox/origin tools

- Visual baseline/origin editor.
- Muzzle point editor.
- Hitbox/hurtbox editor.
- Shadow anchor editor.
- Debug overlay on playfield.

## Immediate UI recommendation

The next prototype should not attempt every feature at once. The best next visible upgrade is:

1. Open Sprite Studio from Character Workbench.
2. Show selected actor summary.
3. Show categorized asset picker.
4. Show the current animation strip/labels inside the Studio.
5. Add placeholder tabs for Bind, Attack, Effects, Sounds, Boxes.
6. Make "Save Profile" write a combined actor profile that includes current animation labels plus the actor's combat defaults.

That gets the architecture correct while preserving the RAD momentum.
