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
- max speed
- conveyor force
- elevator timing
- platform carry behavior

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

- movement model: platformer, overhead tank, overhead car, thrust/space, grid step, click-to-move
- speed / acceleration / friction
- gravity / jump / climb / crawl / swim / fly
- health/lives
- weapon/tool loadout
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

Grid/Text:

- grid speed
- turn-based/realtime
- inventory
- fog/reveal radius

## Enemy / NPC attributes

Common:

- faction/team
- behavior preset
- perception radius
- line of sight
- hearing/noise sensitivity
- memory duration
- patrol route
- target priority
- attack pattern
- flee/chase/scatter state
- health/damage/resistance
- text capability flags
- source binding / spawn marker
- loot/drop behavior
- visual style/tint/opacity

Behavior families:

- direct chaser
- ambusher
- flanker/pincer
- erratic/patrol-biased
- turret/guard
- wanderer/forager
- flee-from-player
- swarm/follow-leader
- worker/carry/build/repair
- RPG talk/shop/quest

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

