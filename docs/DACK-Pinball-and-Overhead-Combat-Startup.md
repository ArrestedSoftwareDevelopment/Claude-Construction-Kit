# DACK Pinball and Overhead Toolkit Startup Plan

## Why these two next

Pinball and the Overhead toolkit are good next reaches because they stress different parts of the same emerging engine.

Pinball proves curved/rotating handles, continuous ball physics, bouncy surfaces, scoring inserts, and spectacular reusable effects. It wants canvas-like sources: image editors, drawing apps, PowerPoint/Draw, desktop layouts, and opaque BBS/textmode boards.

The Overhead toolkit is a camera/control family, not one game. Its first preset can be Combat, but the same foundation also supports driving, planes, spaceships, RPG/adventure actors, animals, insects, office creatures, and swarm systems. It proves directional actors, rotation, steering, shooting or interacting, cover, line of sight, route following, flocking/swarming, and desktop/window geometry as rooms/roads/terrain.

The order should be:

1. Keep Platformer and Brickbat as the shared foundation.
2. Start Pinball with a tiny playable table.
3. Start Overhead with a tiny Combat duel/arena.
4. Extract shared services when both need the same thing twice.

## Shared services both need

Do not build Pinball and Combat as separate games. They should consume the same DACK services:

- source clone / Snapshot image;
- environmental awareness map;
- placed objects with direct handles;
- visible/invisible editor-only logic;
- collision regions from text, icons, windows, pillboxes, and painted zones;
- mutation events against the working clone;
- reusable effects;
- Word Sense / OCR labels;
- HUD placement/fade;
- saveable level Snapshot/package format.

## Pinball module: minimum playable proof

The first Pinball pass should prove one small table, not a full construction kit.

### PinballOverlay

Create a `PinballOverlay` sibling to `BrickbatOverlay`.

Responsibilities:

- own balls;
- own flipper input;
- query table parts from the playfield;
- run ball physics while in `PlaysetMode.Pinball`;
- emit score/effect events;
- publish mutation events later.

### First controls

- Left flipper: `A` or Left arrow.
- Right flipper: `D` or Right arrow.
- Plunger / launch: Space.
- Nudge: light keyboard/mouse nudge later.

### First parts

Start with procedural/debug parts before imported art:

- ball spawn;
- plunger lane;
- left flipper;
- right flipper;
- bumper;
- drain;
- rollover / lit insert;
- wall/rail;
- jackpot target.

These can initially draw as clean debug shapes. The VerzatileDev pinball asset pack should enter through a batch prep/scaler before it becomes runtime art:

- originals stay untouched in `raw base assets/`;
- scaled local candidates go to ignored `dack/assets/quarantine/pinball-verzatiledev-prep`;
- sheets/backgrounds get `preview-1024` and `thumb-256` tiers;
- individual parts get `candidate-512` and `thumb-128` tiers;
- the prep script writes a manifest so we can see source size, output size, tier, and scale;
- only a hand-picked subset graduates into `dack/assets/third_party/` after provenance and gameplay roles are recorded.

Do not write a full pinball importer until the first playable table tells us what metadata the parts actually need. The likely importer boundary later is not “scale image into Godot,” but “admit this source into the editor shelf with pivots, collision shapes, flipper arcs, bumper radii, insert states, sound/effect hooks, license/source badges, and object defaults.” Until then, batch scaling is the safer and faster route.

### First physics

Keep it arcade-simple:

- circle ball collision against lines/rects/circles;
- gravity/table tilt vector;
- elasticity;
- friction/max speed clamp;
- flipper impulse from sweep angle;
- bumper impulse from center;
- drain detection;
- stuck-ball rescue.

The table should feel readable before it feels realistic.

### First editor handles

Pinball needs richer handles than Platformer:

- flipper pivot handle;
- flipper length handle;
- flipper min/max sweep arc handles;
- bumper center/radius;
- plunger lane rectangle and launch direction;
- drain width;
- gate direction arrow;
- ramp/rail spline handles.

This is why Pinball should follow the current A/B endpoint work: it is the next handle family.

## Pinball and text/document features

Pinball should reuse DACK's document-native tricks:

- headings, icons, pillboxes, and bullets become bumpers/inserts/targets;
- gutters become lanes;
- words become missions: `JACKPOT`, `LOCK`, `MULTIBALL`, `BONUS`, `SAVE`, `TILT`, `RAMP`, `DRAIN`;
- word hits can use the same PsychedelicEffects word-shard/explosion vocabulary;
- text collision can be per-table, per-zone, or per-object: solid/bouncy text, pass-through scoring ink, temporary ghost-ball/pierce states, or conditional regions where only specific words become solid;
- OCR should prioritize lit lanes, rollover banks, and ball-near targets.

## Overhead toolkit: movement families

Overhead should expose movement models as presets:

- **Tank / Combat:** rotate left/right, drive forward/back, fire, ricochet, hide behind cover.
- **Driving:** steer, accelerate, brake/reverse, drift/traction, collide with rails/roads.
- **Plane / spaceship:** rotate, thrust, coast/inertia, wrap or bounce at bounds, shoot, avoid hazards.
- **RPG / adventure:** click-to-move or 8-way walk, interact, talk, pick up, open, fight, pathfind around document objects.
- **Animal / insect:** crawl, wander, forage, flee, swarm, follow scent/trails, climb over/around text and UI shapes.
- **Office creatures / workers:** inspect, patrol, carry, build, repair, attack, emote, respond to live desktop/document events.

These should share one top-down actor model with different movement components and AI capability flags.

## Overhead Combat preset: minimum playable proof

Overhead should begin with a Combat preset: an Atari Combat-like / office-commando arena rather than a full shooter.

### OverheadActorController

Create a controller that can drive the existing scout or a new top-down debug actor.

Responsibilities:

- position;
- rotation;
- velocity;
- aim direction;
- weapon cooldown;
- health/lives later;
- collision against the environmental awareness map.

### First controls

Three useful modes:

- **Tank mode:** left/right rotate, up/down drive, fire.
- **Office shooter mode:** WASD move, mouse aim, click/J fire.
- **Creature/RPG mode:** click-to-move or 8-way walk, interact/fire as contextual action.

Start with Tank mode because it proves Combat's identity and keeps keyboard-only office play intact. Mouse aim can follow.

### First parts

- player spawn;
- enemy spawn;
- cover region;
- ricochet wall;
- destructible text/object rule;
- pickup;
- patrol route;
- objective/safe zone;
- door/gate;
- invisible trigger.

Most can reuse existing placed-object infrastructure, with new kinds as needed.

### First projectile rules

Combat projectiles should reuse Platformer shot ideas but add:

- direction/rotation;
- lifetime;
- ricochet count;
- collision material;
- pierce/bounce/explode;
- whether text is destroyed, ignored, or treated as cover;
- per-actor/per-projectile capability flags.

### First enemy AI

Use small behavior presets:

- direct chaser;
- ambusher;
- flanker/pincer;
- patrol/erratic;
- turret/guard.
- wanderer/forager;
- flee-from-player;
- swarm/follow-leader.

These are the same seeds that will later serve Snake/Maze, RPG, Tower Defense, and Action kits.

## What to implement first

Recommended next coding sequence:

1. Add `PinballOverlay` with one ball, launch button, table bounds, drain, and debug score.
2. Add procedural flipper objects and flipper input.
3. Add bumper circles and score/effect events.
4. Add pinball placed-object kinds plus simple shelf buttons.
5. Add `OverheadActorController` in a new Overhead/Combat mode using the current scout as the placeholder actor.
6. Add rotate/drive/fire controls.
7. Add ricochet projectiles against text/window/placed boundaries.
8. Add one enemy with direct-chase behavior.

If time is tight, Pinball step 1 is the best next visible milestone: a ball rolling and draining on the cloned document/table will immediately show whether the module has life.
