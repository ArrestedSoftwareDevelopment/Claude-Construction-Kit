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

## Pinball physics blueprint

Pinball should not begin with full rigid-body simulation. It should begin as a deterministic arcade solver that we understand completely, then graduate individual parts to Godot physics only if the hand-rolled model becomes a bottleneck.

### Core simulation model

Use a simple fixed-step loop inside `PinballOverlay`:

1. Accumulate frame time.
2. Step the ball in small fixed ticks, probably `1/120s`.
3. Apply table forces.
4. Move the ball.
5. Resolve collisions.
6. Emit score/effect/sound/mutation events.
7. Clamp speed and rescue stuck balls.

The ball state should be small and inspectable:

- position
- previous position
- velocity
- radius
- spin, later
- live / captured / draining / launching state
- last touched object
- grace timers: drain save, gate debounce, rollover debounce, bumper cooldown

Default first-pass values:

- ball radius: `TextUnitPixels * 1.0` to `1.25`
- gravity / table tilt: downward vector, tunable per table
- max speed: high but clamped for legibility
- restitution:
  - rails/text: medium-high
  - rubber/bumper: high
  - flippers: high + active impulse
  - gates/rollovers: low/no bounce unless specified
- damping: tiny per tick so the ball eventually settles

### Collision order

Resolve collisions in this order so gameplay feels intentional:

1. Drains / outlanes / capture zones.
2. Plunger lane walls and launch lock.
3. Active flippers.
4. Bumpers / circular pop targets.
5. Gates / one-way guides.
6. Ramps / rails / line walls.
7. Rollovers / inserts / sensors.
8. Text, icons, actors, and source-derived targets.
9. Table bounds.

The order matters. A ball in the drain should not also score a rollover on the same tick unless the table explicitly allows it. A flipper strike should take precedence over a nearby decorative wall.

### Ball collision primitives

Minimum useful primitives:

- circle vs. line segment;
- circle vs. thick line/capsule;
- circle vs. circle;
- circle vs. rectangle;
- point-in-sensor region for rollovers/drains;
- optional polygon later for imported art silhouettes.

For lines and flippers, use capsule-style collision: line segment plus radius. This is more forgiving and closer to pinball rails than strict infinitely thin lines.

Collision response:

- push the ball out along the collision normal;
- reflect velocity across the normal;
- multiply by material elasticity;
- apply tangential friction;
- add active impulse if the object is a flipper, bumper, kicker, or plunger.

### Flippers

Flippers are the heart of the module and need special treatment.

Data:

- pivot point
- rest angle
- active angle
- length
- thickness
- side: left / right / freeform
- activation key
- sweep speed
- return speed
- strength
- elasticity
- cooldown / hit debounce

First geometry:

- Existing A/B handles map naturally to pivot and tip-at-rest.
- Add arc handles later for rest/active angle.
- In play, compute the current tip from pivot + length + animated angle.
- Collision uses a capsule from pivot to current tip.

Flipper hit response:

- reflect using the flipper surface normal;
- add impulse based on sweep direction and how close the ball is to the tip;
- tip hits should be stronger than root hits;
- clamp final speed to keep the ball readable.

Do not chase simulation purity here. Good pinball feel is mostly impulse tuning, not realism.

### Bumpers

Bumpers are circular impulse objects.

Data:

- center
- radius
- score value
- strength
- cooldown
- lit/unlit state
- sound/effect profile

Behavior:

- if ball overlaps bumper circle, push out from center;
- set velocity away from center at at least `minKickSpeed`;
- add score;
- trigger comic/electric burst;
- optionally light or advance a mission word.

Source-derived bumpers:

- icons, app buttons, pillboxes, bullets, badges, and selected OCR words can become bumpers.
- The editor should allow “promote detected object to bumper.”

### Plunger lane

The plunger is a capture-and-release mechanic.

Data:

- lane rectangle or capsule
- ball spawn/capture point
- launch direction
- charge amount
- max charge
- release strength
- auto-launch toggle

Behavior:

- at serve, ball rests in the lane;
- holding Space charges;
- releasing Space launches along the lane direction;
- if no plunger object exists, use a default lower-right launch lane.

Editor handles:

- A/B defines lane direction;
- width/thickness defines lane width;
- speed/force slider defines max launch power.

### Drains and ball save

Drain is a sensor, not a wall.

Data:

- sensor line/region
- ball lost policy
- ball-save seconds
- score penalty, optional
- reset/serve target

Behavior:

- when ball enters drain and ball save is inactive, remove ball and serve next.
- if ball save is active, relaunch from plunger lane or nearest safe launch point.
- first playable demo can use one bottom drain object plus table-bottom fallback.

### Rollovers and inserts

Rollovers are low-profile sensors. They should often be invisible-ish in play but clear in edit mode.

Data:

- region / line / capsule
- lit state
- score value
- mission group
- one-shot or repeatable
- reset condition
- effect style

Behavior:

- crossing the sensor scores and toggles state;
- lit groups can spell words like `DACK`, `WORD`, `LOCK`, `BONUS`, `JACKPOT`;
- completed groups trigger bonus, multiball, ball save, gate opening, or jackpot.

### Gates

Gates are one-way or conditional barriers.

Data:

- A/B line segment
- allowed direction
- bounce/stop/pass-through policy
- open/closed state
- trigger source

Behavior:

- if ball approaches from allowed side, pass through;
- if from blocked side, reflect or stop depending on material;
- can open when a rollover bank or word mission completes.

### Ramps, rails, and wireforms

First pass: ramps are line/capsule guides with special z-state later.

Data:

- polyline/spline path
- entrance direction
- exit point
- speed boost / drag
- elevated flag, later
- lock/capture flag, later

Behavior:

- simple ramp: acts like a rail/guide line and nudges ball along path.
- later ramp: changes ball layer to “raised,” allowing it to pass over lower targets and land at the exit.

This will eventually share the visible spline editor planned for patrol paths, vines, racing tracks, and motion arcs.

### Text, actors, and document-derived targets

Pinball should reuse Brickbat’s target logic without inheriting Brickbat’s paddle rules.

Modes:

- text as rubber wall;
- text as scoring ink / pass-through sensor;
- text as destructible target;
- selected words as missions;
- icons/pillboxes as bumpers or inserts;
- enemies/actors as animated toy targets.

Actor targets:

- animated enemies can be placed on a pinball table as toys, guards, bosses, or jackpot targets.
- hits can damage them using the same shot toughness model, or use a simpler `pinballHitsToDestroy` value later.
- destruction should call the same explosion + random letter shrapnel routine used by combat explosions.

Word missions:

- `LOCK`: starts ball lock / multiball prep.
- `SAVE`: lights ball save.
- `JACKPOT`: lights jackpot.
- `BONUS`: advances bonus multiplier.
- `TILT`: nudge warning or danger word.
- `DRAIN`: dangerous word / drain lane theme.

OCR can stay lazy and opportunistic: prioritize targets near the ball, lit inserts, and rollover banks.

### Table parts as DACK objects

Existing placed objects are enough for the first pass, but the inspector should grow pinball-specific meanings:

- **Flipper**
  - Speed = sweep speed.
  - Range = sweep angle.
  - Thickness = rubber width.
  - Direction/reverse = handedness or activation side.
- **Bumper**
  - Radius = distance A→B.
  - Speed = impulse strength.
  - Range = cooldown or score tier later.
- **Plunger**
  - Speed = max launch strength.
  - Range = lane/capture length.
  - A/B = launch direction.
- **Drain**
  - A/B = drain mouth.
  - Visible in play can be off for invisible fail zones.
- **Rollover**
  - A/B = sensor width.
  - Speed/Range can become score or reset behavior later.
- **Gate**
  - A/B = gate segment and direction arrow.
  - Reverse flips allowed direction.

Eventually these should become named presets rather than raw object kinds: `Left Flipper`, `Right Flipper`, `Pop Bumper`, `Slingshot`, `Outlane`, `Inlane`, `Ball Lock`, `Kicker`, `Spinner`, `Drop Target`, `Standup Target`, `Saucer`, `Magnet`, `Gate`.

### Pinball component roadmap

#### Phase PB-1: rolling ball

- Add `PinballOverlay`.
- One ball, gravity/tilt, table bounds, drain.
- Space serves/relaunches.
- Draw debug ball and velocity trail.
- HUD: score, balls, status.

Success test: ball rolls, bounces off table bounds, drains, and relaunches predictably.

#### Phase PB-2: flipper feel

- Query placed flippers.
- Animate flipper angle from rest to active.
- Add capsule collision.
- Tune impulse until it feels arcade-good.
- Add left/right inputs.

Success test: player can keep the ball alive with two debug flippers.

#### Phase PB-3: scoring table parts

- Add bumper collision/scoring/effects.
- Add rollovers as sensors.
- Add drain/ball-save.
- Add basic sounds using existing CC pool or new pinball picks.

Success test: table is a tiny playable pinball toy with score events.

#### Phase PB-4: DACK document magic

- Text collision toggle: bounce / sensor / destructible / ignore.
- Icons/pillboxes can be promoted to bumpers/inserts.
- Animated enemies act as destructible table targets.
- Word missions begin with fallback text, then lazy OCR.

Success test: a screenshot/document table produces meaningful pinball targets without custom art.

#### Phase PB-5: construction kit depth

- Flipper arc handles.
- Bumper radius handles.
- Plunger lane handles.
- Gate direction handles.
- Rollover groups and mission words.
- Save/load all pinball object attributes.

Success test: creator can build a small table from shelf parts and save/load it.

#### Phase PB-6: assets and polish

- Curate VerzatileDev pieces into a runtime-safe sheet.
- Add pinball-specific sound deck.
- Add backglass/score-reel effects.
- Add table themes: office document, BBS/ANSI, Photoshop canvas, desktop icons.
- Add multiball with a hard cap and clear visual tracking.

Success test: the table feels like a deliberate DACK pinball kit, not just debug circles.

### ANSI / BBS office underlays

Pinball can use dim, glowing ANSI-style underlays as table art, especially for funny office-themed boards. The preferred Document Pinball stack is:

1. captured/source document clone;
2. dimmed ANSI/ASCII table art composited underneath or through low-opacity glow;
3. readable native-looking document text;
4. promoted physics parts: flippers, bumpers, rails, drains, inserts, gates;
5. actors, balls, effects, HUD, and editor handles.

The important rule: ANSI table art should support the document text, not bury it. The source document remains the legibility anchor. The table art behaves like a haunted backglass or phosphor glow bleeding up from beneath the page.

The target feel is old BBS / cracktro / ANSI art: chunky CP437 blocks, neon gradients, fake chrome, scroll-text attitude, and absurdly serious office words.

Candidate table themes:

- **COLLATE!** — paper trays, copier glass, staplers as slingshots, toner clouds, `SORT / STACK / STAPLE` rollover bank.
- **TPS MULTIBALL** — memo lanes, approval gates, `COVER SHEET` jackpot, red-stamp bumpers.
- **TONER LOW** — black-powder drain lanes, cyan/magenta/yellow bumpers, printer-error hurry-up mode.
- **FAX WIZARD 3000** — modem squeal lanes, thermal-paper ramps, `SEND / RECEIVE / JAM` targets.
- **MEETING CANCELED** — calendar-grid inserts, coffee-ring bumpers, `AGENDA` word bank.
- **INBOX ZERO** — envelope bumpers, unread-count rollovers, spam drains, `REPLY ALL` danger lane.

Underlay rules:

- Treat the ANSI layer as art first, collision second.
- Default opacity should be low, roughly 12-28%, with separate glow strength. In dark mode it can glow a little more; in light mode it must stay quieter.
- Use blend/additive glow around ANSI strokes, but avoid washing document text. If the document text contrast drops below a legibility threshold, auto-dim or mask the underlay behind text.
- Allow per-table controls: `underlayOpacity`, `glowStrength`, `scanlineStrength`, `textProtection`, and `themeFollowsSystem`.
- Creators can promote visual regions from the underlay into bumpers, rails, drains, gates, or inserts.
- A table can still use a live/captured office document as the playfield, but ANSI underlays provide authored “toy table” themes when a screenshot is too plain.
- Boss Key must hide any flashy underlay instantly.
- Licensing/provenance must be stored with imported ANSI art; built-in DACK originals can be generated as project assets.

This is also a good home for animated score/reel text: large, rotating, strobing words that feel like the Brickbat haiku ticker grew a backglass.

### Open decisions to test empirically

- Hand-rolled pinball solver vs. Godot `RigidBody2D`: start hand-rolled, compare only after flippers exist.
- Ball count cap: likely 3 for readability, same as Brickbat.
- Text collision default: for Pinball, probably `bounce` in pinball zones and `sensor/pass-through` elsewhere.
- Flipper input: keyboard-only first; mouse/touch later if it proves helpful.
- Nudge: delayed until basic ball survival works, then add light nudges with tilt warnings.
- Raised ramp layers: later; fake it with guided paths first.

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
