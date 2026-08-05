# DACK Object Attribute Model

- **Status:** Normative schema direction; RAD coverage is partial
- **Coordinate authority:** [DACK Level Snapshot and Package Format](DACK-Level-Snapshot-Format.md)
- **Card authority:** [ADR-0014](adr/ADR-0014-card-definition-instance-and-dependency-contract.md)
- **Related engineering plan:** [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md)

## Purpose

DACK objects need a shared attribute vocabulary before the editor grows too many one-off controls.

The principle:

> Every object has common presentation, collision, behavior, and source-binding attributes. Specific game families add specialized attributes on top.

This lets a ladder, flipper, tank, ant, checkpoint, hidden switch, tower, bumper, word target, and RPG monster all live in one editor/inspector model without becoming one giant hardcoded form.

## Cards as the reusable unit

DACK should expose this model through cards rather than raw object schemas.

Cards can be small ingredients:

- sprite card;
- animation card;
- behavior / AI card;
- radar / awareness range;
- physics card;
- projectile / weapon card;
- explosion / effect card;
- sound card;
- text rule card;
- spawn rule card;
- source-binding card.
- pinball art card (board skin, logo, backglass, rail, bumper, insert, apron, or typography).
- Brickbat art card (ANSI target table, target-wall frame, logo, paddle/ball skin, bonus banner, or score typography).

Cards can also be finished composites:

- player card;
- enemy card;
- spawn point card;
- pinball part card;
- tower card;
- pickup card;
- word-object card;
- complete playset preset card.

The useful recursion is: composed objects become cards too. A creator can build a `Flying Fireball Dragon` from sprite, animation, AI, projectile, effect, sound, and text-rule cards; then that finished enemy becomes a single card that can be placed in a level, dropped onto a spawn point, used in a wave, forked, or shared.

This keeps the inspector precise while keeping the builder playful. The raw attributes below still matter, but the creator should usually encounter them as editable fields inside cards.

## Units and Coordinate Rules

Saved attributes must be self-describing and must follow the level coordinate contract.

- World position, endpoints, rectangles, radii, thickness, collision padding, ranges of motion, and authored paths use native `snapshot-pixels`.
- Position is measured from the Snapshot's top-left; positive X points right and positive Y points down.
- Runtime zoom, window fit, monitor placement, and camera transforms do not alter saved values.
- Asset frame rectangles and pivots use asset-local pixels. A placed card/instance owns the transform from asset-local space into `snapshot-pixels`.
- Time uses seconds. Rates should say what they are per second, such as `speedPixelsPerSecond` or `turnDegreesPerSecond`.
- Saved angles use degrees, clockwise in screen space, with `0` pointing right. Runtime systems may convert to radians internally.
- Opacity, normalized phase, confidence, and weight values use the closed range `0.0-1.0`.
- Text-relative perception may use `textUnits`, but every Snapshot must record the reference text metrics used to convert those values to pixels. A generic field named only `range` is transitional and must become a typed/suffixed property.
- Whole-number creator caps, such as shot toughness and spawner counts, remain integers in serialized data.

Names in the current RAD such as `speed`, `range`, and `rotation` are accepted migration aliases only. The versioned schema should prefer explicit unit suffixes so Inspector labels, validation, save files, and runtime code cannot silently disagree.

## Defaults and Safe Ranges

Defaults belong to the most specific applicable preset/card, but the schema supplies safe fallbacks and hard validation bounds. A field may use a narrower range for a particular object family.

| Attribute | Schema fallback | Initial creator range | Notes |
| --- | ---: | ---: | --- |
| `opacity` | `1.0` | `0.0-1.0` | Editor-only handles can remain visible independently. |
| `scaleMultiplier` | `1.0` | `0.25-4.0` | Quick UI emphasizes `0.5x`, `1x`, and `2x`. |
| `rotationDegrees` | `0` | normalized to `-180..180` | Continuous rotors store angular speed separately. |
| `gravityScale` | `1.0` | `-4.0-4.0` | Zero means unaffected; negative is an explicit inverted-gravity choice. |
| `health` / `shotToughness` | `1` | `1-9` | Matches the simple regular-shot vocabulary. |
| `weaponPower` | `1` | `1-9` | Removes that many toughness points per hit. |
| `radarRangeTextUnits` | `28` | `1-100` | Converted through Snapshot text metrics; `0` may explicitly disable perception. |
| `spawnIntervalSeconds` | `5` | `1-10` | Small whole numbers in the simple Inspector. |
| `burstCount` | `1` | `1-10` | Subject to active-actor budget. |
| `maxActive` | `3` | `1-10` | Hard cap per spawn rule in the first builder. |
| `cooldownSeconds` | `0` | `0-600` | Specialized presets should present a useful narrower slider. |

Movement speed, impulse, elasticity, friction, damping, damage radius, and similar physics values do not have one meaningful universal default. Their card or toolkit preset must define a default, creator-facing range, and safety ceiling. Missing specialized values fall back to that preset—not to an unrelated game's numbers.

## Card Inheritance and Instance Overrides

A reusable card is a definition; an object placed in a level is an instance of that definition.

### Definition ownership and edit authority

Cards have three distinct edit-authority tiers. The UI and save format must never blur them:

1. **Built-in or third-party definition:** shipped by DACK or admitted from an external pack. It is immutable in place and carries `editorLocked: true`. A creator may place it, override a placed instance, or use **Fork Card** to copy it into the project, but cannot rewrite the source definition or its provenance.
2. **Project-owned definition:** created by the user or forked into the current project. It carries `editorLocked: false` and may be opened and edited as a reusable definition. Changing it is a shared operation because every non-overridden instance that references it may change.
3. **Placed instance:** a level-owned reference to a definition plus a sparse override patch and runtime identity. It is independently movable, scalable, tintable, configurable, and duplicable without modifying the reusable definition.

`editorLocked` describes definition-authoring authority only. It does not disable the card in play, prevent instance overrides, or replace license/export policy. A locked third-party definition can still be unsafe for redistribution; provenance and distribution state remain separate fields.

The normal Inspector always edits **This Instance**. Definition editing is an explicit context change, not an accidental consequence of changing a field.

Inspector commands:

- **Apply to Definition:** copy the selected instance override(s) into an editable project-owned definition, then remove those now-redundant overrides from the selected instance. Before committing, show how many placed instances inherit each affected field and how many have protecting overrides. Apply to Definition is unavailable for `editorLocked` definitions until the creator forks them.
- **Reset to Card:** remove the selected field/group override and immediately show the resolved inherited value. This never rewrites the definition.
- **Fork Card:** create a new project-owned definition with a new stable `cardId`, exact resolved starting values, and optional `derivedFromId`/`derivedFromVersion` provenance; retarget only the selected instance unless the creator explicitly selects a broader scope.
- **Open Definition:** leave the level-instance Inspector context and open the reusable definition editor. Its header must say whether the definition is built-in, third-party, or project-owned and show the number of affected instances before shared edits are saved.

Every shared-definition operation must name its scope in plain language: `This Instance`, `This Definition`, or an explicitly selected set. Destructive propagation must not hide behind a generic Save button.

### Resolution and sparse overrides

Resolution order is deterministic:

1. schema fallback;
2. game-family/toolkit defaults;
3. referenced card values, including its composed ingredient cards;
4. placed-instance overrides;
5. temporary runtime state, which is saved only when the level's persistence policy asks for it.

Rules:

- A placed instance stores `cardId` and `cardVersion` plus only the fields it overrides.
- Renaming or moving a card does not break instances because identity is ID-based.
- Editing a project-owned shared card updates inheriting instances; an overridden field keeps its instance value.
- The Inspector must distinguish inherited values from overrides and offer **Reset to Card** per field or group.
- Nested ingredient-card overrides are namespaced by stable slot ID so a projectile's `speedPixelsPerSecond` cannot accidentally overwrite its owner's movement speed.
- Replacing a card preserves compatible instance overrides, reports incompatible ones, and never silently discards authored values.
- A fully detached/forked card receives a new stable ID and may retain `derivedFromId` for provenance.

### Composition slots, dependency safety, and version pins

A composite card owns typed, stable composition slots. A slot's `slotId` is independent of its display label, shelf position, and current ingredient. Examples include `visual.body`, `animation.primary`, `behavior.movement`, `weapon.primary`, `projectile.primary`, `effect.impact`, `effect.death`, and `audio.fire`. Renaming "Primary Shot" or reordering a shelf therefore cannot break bindings or misapply an override.

- Slot replacement is allowed only when the incoming card satisfies the slot's declared type/capability contract.
- Instance overrides address `slotId` plus a field path, never a display name or array index.
- Missing or incompatible dependencies load as visible disabled placeholders with repair choices; they do not disappear and do not silently substitute an unrelated card.
- Card composition is a directed acyclic graph. Self-reference and any transitive dependency cycle are rejected at edit, save, import, and publish time with the complete cycle path. Runtime resolution also keeps a bounded recursion guard so malformed external content cannot hang the app.
- A project-owned definition may explicitly track a newer compatible version during authoring, but every level save records the exact version resolved at save time. A creator must approve version advancement when it could affect placed instances.
- Published/shareable levels pin exact card and asset versions or embed immutable copies. Floating `latest` dependencies are not valid for published playsets.
- Forking begins from an exact resolved dependency graph. Subsequent upstream changes do not alter the fork unless the creator explicitly rebases it through a reviewed diff.

## Schema and Validation Contract

- Each `.dacklevel` format version has a machine-readable JSON Schema covering discriminators, required fields, types, units, ranges, stable-ID references, and object-family extensions.
- Common attributes live in one base schema; specialized attributes are selected by explicit `kind`/component discriminators rather than accepted as an unvalidated property bag.
- Save validates before replacing the last good file. Load validates before creating runtime objects.
- Invalid required data blocks load/save with a readable path and suggested repair. Unsafe numeric values (`NaN`, infinity, negative sizes, out-of-range counts) are never admitted.
- Creator-entered values may be clamped only when the Inspector shows the applied limit immediately. File loading must report a repair/migration; it must not silently change authored data.
- Unknown optional fields from a newer minor version should be retained when practical during a load/save round-trip. Unknown required kinds/components remain disabled and visible as repairable placeholders rather than disappearing.
- Cross-reference validation checks that card, actor, asset, route, trigger, source-region, and effect IDs exist and are of a compatible type.
- Defaults are materialized by resolution, not duplicated into every instance. Export may flatten resolved values for a self-contained published pack while retaining provenance and version pins.
- Schema migrations are explicit, version-to-version, testable transformations. Current RAD aliases are imported once and written back using canonical names.

## Common attribute groups

### 1. Identity

- name
- type / kind
- family: Side View, Overhead, Ball/Table, Paddle/Clearing, Grid/Text, Route/Flow, Ambient
- preset / behavior template
- tags
- source/provenance
- creation source: creator / imported / toolkit-starter
- starterGenerated flag and starter preset ID
- `editorLocked`: definition-authoring lock for built-in/third-party cards; it does not disable placed instances or replace export/licensing policy

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

#### Visibility and Stealth behavior

Presentation visibility answers **whether and how something is drawn**. Stealth answers **how a gameplay actor can be perceived and interacted with while concealed**. Stealth is a behavior/card channel with state and rules; it must not be encoded as a low-opacity sprite, `visible = false`, `disabled`, or `editor-only`.

Minimum stealth vocabulary:

- `stealthMode`: none, always concealed, fade by distance, reveal on proximity, reveal on attack, reveal on damage, timed phase/blink, or camouflage against the source;
- `visibleOpacity` and `concealedOpacity`, plus bounded fade-in/fade-out times;
- reveal triggers, reveal range, reveal duration, cooldown, and whether allied or hostile sensors can reveal it;
- `collisionWhileConcealed`;
- `targetableWhileConcealed`;
- `canDealDamageWhileConcealed`;
- `canTakeDamageWhileConcealed`;
- shadow, projectile, carried-object, nameplate, and effect visibility policies while concealed;
- creator-selected **tells** such as an outline, displaced text/pixels, footprints, dust/ripple, shadow, punctuation trail, warning glyph, or positional sound;
- an `accessibleCue` that communicates the same threat/state without relying only on opacity, color, fine detail, stereo sound, flashing, or rapid motion.

Stealth transitions should emit ordinary state events (`concealed`, `revealing`, `revealed`, `concealing`) so AI, targeting, HUD, sound, effects, save/load, and replay consume one result rather than independently guessing from sprite alpha. Collision and damage remain deterministic even when rendering is reduced or disabled. Reduced-motion/no-flash settings substitute a steady outline, glyph, contrast shape, or other non-strobing cue.

The performant default is event- or interval-driven perception using geometry already owned by the simulation. Source-aware camouflage may sample a coarse cached region when state changes; it must not rescan the full playfield or perform per-pixel matching every frame.

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

Enemy / NPC additions:

- radar / awareness range, measured in text units
- tracking enabled
- patrol range
- edge/gap reversal
- platform/elevator/conveyor/slide awareness
- ladder/route usage capability
- intelligence tier, which can simply map to larger radar and better path choices at first

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

### Text-surface policy

Text interaction is two-sided: the actor declares what it can do, and the source/text region declares what kind of surface it is. Neither side should infer a universal rule from the fact that pixels look like letters.

Surface policy options:

- `ignore`: no collision or climb support;
- `solid-platform`: the text block behaves as a platform, normally colliding on its top/support edge while preserving its visual text;
- `solid-block`: the full text-region hull is a solid obstacle;
- `climbable`: an actor with `canClimbText` may attach and move along the detected text surface;
- `crawlable`: dense/single-spaced rows permit the dedicated crawl animation and vertical text traversal;
- `destructible`: projectiles/tools can mutate the text but actors do not climb it unless separately enabled;
- `hybrid`: creator-defined combinations, such as solid top edge plus climbable face.

Line spacing is a classification signal, not an automatic gameplay law. A preset may suggest `single/tight` rows as a continuous crawl or fence face and `double/loose` rows as separated ledges or ladder-required gaps, but the creator can override the result per block or region. The Inspector should expose detected spacing, confidence, gap tolerance, and the chosen policy in plain language.

The player/enemy profile must also expose `canClimbText`, `canCrawlText`, `canUseLadders`, and the required animation labels independently. A character with climb frames but a disabled capability must remain grounded; a capability without a mapped animation should show a diagnostic rather than silently failing.

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
- text surface policy: ignore / solid-platform / solid-block / climbable / crawlable / hybrid
- line-spacing mode: detected / tight / loose / creator override
- text-block support edge and gap tolerance
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
- visibility/stealth behavior card
- reveal triggers/range/duration and accessible concealment tell
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
- stalker/ambusher with conceal/reveal states
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

Enemy Spawn Point is a dedicated editor-only flag for generating enemies from authored locations. It should be treated as a marker/flag variant rather than a freeform script object.

Suggested starter attributes:

- `spawnGraphic`: optional visible editor graphic for the flag itself; hidden during play unless the creator explicitly exposes it.
- `enemyPool`: one or more assignable enemy types/sprites the point can spawn.
- `spawnIntervalSeconds`: small whole number, 1-10.
- `burstCount`: small whole number, 1-10, usually defaulting much lower.
- `maxActive`: small whole number, hard capped at 10 per sprite/enemy type.
- `spawnSpeedMultiplier`: small whole number or constrained multiplier applied to the spawned actors.
- `spawnDirection`: left, right, up, down, toward player, away from player, random, or along route.
- `aiPreset`: optional behavior card applied to spawned enemies.
- `activeOnlyInRegion`: optional camera/playfield/proximity gate so dormant spawners cost little.
- `cooldownAfterClear`: optional pause after the last spawned enemy is defeated.

Design guardrail: spawn points are intentionally bounded. DACK should feel like a construction kit, not a hidden performance trap. The creator-facing controls should prefer little integer choices, clear caps, and obvious defaults such as "one enemy every five seconds, max three active."

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
- Enemy Spawn Point exists as a first-pass editor-only placeable marker. In the current Inspector, `Speed/force` is temporarily reused as spawn interval, `Thickness` as burst count, and `Range` as max active; all are clamped to 1-10 until the shared schema supplies real spawn fields to the selected-instance Inspector and Actors/Logic workspaces.

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
- enemy spawn point flag
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

The RAD now proves a useful **placed actor instance** Inspector slice. Right-clicking an actor in Build opens a movable floating Inspector with name, AI mode, radar, toughness, projectile ability, text-aware movement, scale, tint, opacity, play visibility, shadow, facing, projectile/effect card slots, **Duplicate Instance**, and an early **Fork Card** action. These are direct instance edits in practice, actor values are included in RAD level save/load, and repeated placements are independent.

The RAD also has a narrower world-object Inspector and direct handles for position, A/B endpoints, body dragging, speed/force, thickness/collision pad, elevator range, ramp/slide direction, reversible conveyors, tint, opacity, and editor-only start/switch/checkpoint/spawn markers. Enemy spawn interval/burst/max-active still reuse generic numeric fields as a temporary bridge.

This is not yet the complete model above:

- actor/world-object data is still represented by prototype fields and switch-based UI rather than one versioned component schema;
- **Fork Card** is an early sprite/model detachment and does not yet create the complete persisted project-owned definition/dependency graph described here;
- **Apply to Definition**, **Reset to Card**, **Open Definition**, inherited/overridden field indicators, affected-instance counts, and `editorLocked` enforcement are not implemented;
- stable composition `slotId` values, exact dependency version pins, cycle detection, placeholder repair, and schema migration are not implemented in RAD persistence;
- `Visible in play`, opacity, collision flags, and editor-only markers exist, but the distinct stealth state machine, reveal rules, targeting/damage policies, tells, and accessible cues are planned rather than present;
- the full presentation-mode distinction (invisible-active versus editor-only versus disabled), source-binding editor, material/collision profiles, per-actor text capability set, and sprite/text/graphic/hybrid presentation control remain incomplete.

The required implementation boundary is therefore not more one-off Inspector controls. It is a schema-backed resolved-card view shared by actor and world-object Inspectors, with instance override patches as the default write target and the four explicit definition commands above.
