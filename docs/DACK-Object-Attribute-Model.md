# DACK Object Attribute Model

## Purpose

DACK objects need a shared attribute vocabulary before the editor grows too many one-off controls.

The principle:

> Every object has common presentation, collision, behavior, and source-binding attributes. Specific game families add specialized attributes on top.

This lets a ladder, flipper, tank, ant, checkpoint, hidden switch, tower, bumper, word target, and RPG monster all live in one editor/inspector model without becoming one giant hardcoded form.

## Common attribute groups

### 1. Identity

- name
- type / kind
- family: Side View, Overhead, Ball/Table, Paddle/Clearing, Grid/Text, Route/Flow, Ambient
- preset / behavior template
- tags
- source/provenance

### 2. Presentation

- visible in play
- visible in editor
- opacity
- tint/color
- palette slot
- sprite / text / graphic / hybrid / invisible
- outline/glow
- animation set
- scale
- rotation
- z-layer
- label/caption
- Boss Key safe presentation

Important distinction:

- **Invisible but active:** hidden trigger, invisible collision, invisible checkpoint.
- **Editor-only:** visible while building, absent during play.
- **Disabled:** ignored by play and editor except for selection/re-enable.

These must not collapse into one checkbox.

### 3. Geometry and handles

- position
- start/end points
- width/height
- radius
- thickness / collision pad
- rotation
- spline/control points
- arc start/end
- range of motion
- snap/bind mode
- draggable body
- endpoint handles
- special handles: flipper sweep, bumper radius, route node, spawn direction

### 4. Collision and material

- collision enabled
- solid / pass-through / climbable / bouncy / sticky / damaging / sensor
- collision layer
- actor categories affected
- material: paper, metal, rubber, glass, tar, water, fire, text, UI chrome
- elasticity
- friction
- hardness / health
- ricochet policy
- one-way policy
- edge cleanup / mask tolerance for captured pixels

### 5. Motion and physics

- speed
- acceleration
- gravity scale
- direction
- reversible
- loop / ping-pong
- phase
- range
- path/rail binding
- table tilt
- thrust / drag / inertia
- turn rate / angular velocity
- throttle / acceleration model
- damping
- local gravity source / gravity sensitivity
- wrap / bounce / kill boundary policy
- max speed
- conveyor force
- elevator timing
- platform carry behavior

Ball / Table Physics additions:

- ball radius
- table tilt vector
- elasticity / restitution
- rolling damping
- max ball speed
- flipper rest angle
- flipper active angle
- flipper sweep speed
- flipper return speed
- flipper strength
- bumper impulse
- bumper cooldown
- plunger charge
- plunger launch direction
- drain policy
- ball-save timer
- one-way gate direction
- rollover lit state
- mission group / word bank
- ramp layer: flat / raised / wireform
- table nudge strength
- tilt warning count

### 6. Gameplay role

- role: start, midpoint, end, checkpoint, switch, trigger, objective, spawn, target, hazard, pickup, door, key
- win/loss contribution
- score value
- cooldown
- activation count
- reset behavior
- save/respawn behavior
- persistence policy
- mutation policy

### 7. Source binding

- bound source object: word, letter, line, icon, window, cell, color region, manually placed point
- binding mode: bound / offset / detached-but-linked / free
- text face / graphic face / hybrid face
- OCR/Word Sense label
- confidence
- re-snapshot rebinding status
- clone-only mutation state

### 8. Per-actor capability flags

Text and document behavior must be per actor/projectile/obstacle, not universal.

Examples:

- treats text as solid
- can climb text
- can crawl through text
- can destroy text
- can dig text
- ignores text
- reads semantic words
- triggers word effects
- can pass through windows
- collides with icons
- affected by hidden triggers
- visible to enemies
- makes noise / leaves trail

This lets a spider climb text, a ghost ignore it, a drill destroy it, a tank ricochet off it, and a player treat it as optional terrain.

## Player attributes

Common:

- movement model: platformer, overhead tank, overhead car, atmospheric flyer, thrust/space, lunar lander, grid step, click-to-move
- speed / acceleration / friction
- gravity / jump / climb / crawl / swim / fly
- health/lives
- weapon/tool loadout
- weapon power: how many enemy toughness points one hit removes
- current checkpoint/start marker
- collision profile
- text capability flags
- sprite/animation set
- tint/opacity/costume
- input bindings
- camera/HUD follow rules

Side View:

- jump height/count
- coyote time
- climb speed
- ladder/text crawl behavior
- slope handling
- fall/death policy
- projectile/dig verbs

Overhead:

- rotate speed
- drive/steer/thrust model
- aim mode
- turning radius
- traction/drift
- line of sight
- interaction radius

Flying/space movement should be a separate set of reusable physics cards:

- **Atmospheric Flyer:** throttle, turn rate, drag, climb/dive, altitude band, stall-ish slowdown, terrain/cloud/lane collision.
- **Thrust-Space:** rotate, thrust, coast/inertia, damping, max velocity, screen wrap/bounce, independent turret aim, asteroid/debris collision.
- **Lunar Lander:** rotate, main/side thrusters, fuel, gravity, descent-rate safety, landing-leg/contact tolerance, landing pad zones, crash/explosion rules.
- **Localized Gravity:** gravity well point/region, pull strength, falloff, safe radius, orbit/slingshot behavior, per-actor gravity sensitivity, kill zone.
- **Spacewar Arena:** local sun/star gravity, wraparound edges, player/enemy thrust ships, projectile inheritance of ship velocity, ricochet or wrap shots.

Grid/Text:

- grid speed
- turn-based/realtime
- inventory
- fog/reveal radius

## Enemy / NPC attributes

Common:

- character name
- faction/team
- locomotion class: grounded, flying, swimming, crawling, climbing, turret/static, vehicle/ball
- behavior preset
- perception radius
- line of sight
- hearing/noise sensitivity
- memory duration
- patrol route
- home/anchor point
- target priority
- attack pattern
- projectile profile
- flee/chase/scatter state
- health/damage/resistance
- text capability flags
- source binding / spawn marker
- manual placement / authored position
- scale multiplier or explicit size
- loot/drop behavior
- visual style/tint/opacity

Behavior families:

- direct chaser
- ambusher
- flanker/pincer
- erratic/patrol-biased
- turret/guard
- defend point/area/object
- hovering flyer / sine-wave flyer
- wanderer/forager
- flee-from-player
- swarm/follow-leader
- flock/horde
- worker/carry/build/repair
- RPG talk/shop/quest

Flock/Horde is the group-enemy behavior card for Robotron-inspired arenas, bullet-hell mobs, insect swarms, office gremlins, and rescue/escort panic rooms.

Suggested attributes:

- `spawnBudget`: maximum simultaneous actors in this group.
- `groupRadius`: how widely the group spreads from its home, leader, or target.
- `cohesionWeight`: how strongly members stay near the group center.
- `separationWeight`: how strongly members avoid overlapping each other.
- `targetWeight`: how strongly members chase, orbit, flee, or guard a target.
- `leaderMode`: none, follow-leader, protect-leader, split-on-leader-death.
- `panicRadius`: distance at which the group scatters, charges, or changes state.
- `damageMode`: contact, projectile, area pulse, harmless escort, rescue target.
- `bulletPattern`: none, aimed, radial, spiral, burst, lane, random spray.
- `rangeUnits`: threat radius in text units.
- `cooldownSeconds`: group or per-member firing/spawn cadence.

This should be edited like a rule card, not a node graph: drag Flock/Horde onto an enemy type, then tune the handful of values that matter for the current preset.

Defend is the area-loyal behavior card. It differs from patrol/chase because the enemy's primary commitment is to a protected point, region, object, route node, word, or marker.

Suggested attributes:

- `defendTarget`: marker/object/word/region being defended, such as Goal, Checkpoint, Hidden Switch, tower-defense objective, door, treasure, or NPC.
- `defendRadius`: area the defender tries to hold.
- `pursuitRadius`: how far the defender may chase before returning.
- `returnSpeed`: urgency when retreating back to the defend target.
- `alertRadius`: distance at which the player or enemy wave wakes the defender.
- `lineOfSightRequired`: whether the defender must see the intruder before engaging.
- `failurePolicy`: what happens if the target is touched/destroyed/stolen/reached.
- `teamFilter`: who the defender attacks, ignores, escorts, or protects.
- `stance`: hold position, orbit target, patrol perimeter, block lane, intercept intruders, bodyguard.

Defend should bind naturally to DACK's marker vocabulary. A Start Point, Checkpoint, Goal, Hidden Switch, word-object, route node, door, NPC, pinball insert, or tower-defense base can all become defendable anchors.

First enemy slice:

- Sunny Dragon is the first imported animated enemy.
- Its source animation is flying/hovering, so it should default to `locomotion = flying`, `behavior = patrol/guard`, and `attack = none` until projectiles are explicitly enabled.
- Enemy actors can now be dragged directly on the playfield and scaled with the same 1/2x, 1x, 2x vocabulary used for actor sizing.
- The first implemented combat rule is intentionally simple: enemy contact kills the player, enemy shots can use partial health damage or instant death, and player shots reduce enemy shot toughness before awarding a defeat score.
- Enemy shot toughness is creator-facing: a value from 1-9 means "how many regular 1x shots this enemy can take." Player gun power is the matching multiplier/subtractor: a 2x gun removes two toughness points per hit.
- Sunny Dragon's first AI is a hover/patrol guard around its authored home position. Dragging the enemy redefines that home position; this lets creators block a route without opening a full AI graph.
- Enemy tracking is a basic toggle before a full AI editor: tracking on means patrol/facing/projectile aim bias toward the player; tracking off means patrol/guard motion with shots fired along current facing.
- Projectile/explosion assets should be assignable profiles, not hardcoded dots. The first imported profile is `explosion-b`: frame 0 is the projectile, frame 1 is the impact flash, and frames 2+ are the explosion bloom. A profile should eventually define projectile frame/range, impact frame/range, explosion frame/range, speed, radius, damage rules, text-destruction rules, sound, and credit/provenance.
- Explosions can cheaply affect document terrain without OCR: active letter regions inside the blast radius can be randomly erased, scored, and thrown as letter-shard visual effects. The current rule throws the same number of random letters as the blast destroys.
- Grounded enemies need terrain-following, gap handling, ladders/slopes permissions, and jump/drop rules.
- Flying enemies need altitude bands, patrol bounds, obstacle avoidance, and optional swoop/projectile behavior.
- Projectile-firing enemies need shot cadence, aim style, range, projectile collision profile, and whether their shots affect text/terrain.

First complete test level recipe:

1. Place a `Start Point`.
2. Place a visible `Checkpoint` / midpoint.
3. Place a visible `Goal`.
4. Add text terrain plus one or two authored tools such as a ladder, ramp, conveyor, or elevator.
5. Add Sunny Dragon as a flying guard between midpoint and goal.
6. Assign one simple AI preset: hover patrol, chase in radius, or guard goal.
7. Win condition: player reaches Goal after crossing the midpoint; reset condition: fall death or enemy contact.

## Obstacles and interactive world objects

Common:

- visible/invisible/editor-only
- solid/sensor/hazard
- material
- health/hardness
- activation rule
- reset/persistence
- source binding
- tint/opacity
- collision layer

Examples:

- platform
- ladder
- ramp
- slide
- conveyor
- elevator
- checkpoint/marker/flag
- hidden switch
- door/gate
- bumper
- flipper
- rollover
- drain
- racing checkpoint
- tower-defense route node
- RPG wall/door/trap
- semantic word-object such as `TARPIT`, `LADDER`, `JACKPOT`, `KEY`

## Current RAD implementation status

Implemented first-pass object attributes:

- position and A/B endpoints
- body dragging
- speed/force
- thickness/collision pad
- elevator range of motion
- ramp/slide direction normalization
- reversible conveyor direction
- global player gravity
- custom tint/color
- opacity
- editor-only start/switch/checkpoint-style markers

Next useful inspector controls:

- presentation mode: visible / invisible active / editor-only / disabled
- marker role dropdown: start / midpoint / end / switch / secret / objective
- source binding display
- per-object collision/material type
- per-actor text capability flags
- sprite/graphic/text/hybrid presentation toggle
