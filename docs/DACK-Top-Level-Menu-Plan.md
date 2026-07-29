# DACK Top-Level Menu Plan: Game Types by View and Control Family

## Core decision

DACK should not put every named genre in one flat top-level list.

The top-level menu should first ask:

> What kind of view/control/physics family are you building?

Then it should offer presets underneath that family.

This is cleaner than a giant list of `Platformer / Brickbat / Pinball / Combat / RPG / Racing / Snake / Tower Defense / Casual / ...` because many named genres share the same underlying camera, collision, input, and editor tools.

For example:

- Combat, driving, planes, spaceships, RPG walkers, animals, insects, workers, and swarms are all **Overhead**.
- Pitfall-style horizontal platforming, Crazy Climber-style vertical play, side-view shooting, digging, ladders, ramps, slides, and conveyors are all **Side View**.
- Pinball, marble-table toys, pachinko, bumpers, ramps, flippers, rollovers, drains, and score inserts are all **Ball / Table Physics**.
- Brickbat, Pong-like paddles, word clearing, letter clearing, target walls, and literary score games are all **Paddle / Clearing**.
- Rogue/Hack maps, Snake/Maze, BBS terminal maps, ASCII/glyph maps, spreadsheet tile maps, and word-goal grids are all **Grid / Text**.
- Racing routes, Frogger-like crossings, tower-defense lanes, process diagrams, patrol paths, and wave routes are all **Route / Flow**.

Named game references remain extremely useful, but they belong one level down as presets and examples, not as the spine of the whole UI.

## Proposed top strip

When the Cockpit is open, the compact top strip should eventually read like this:

```text
Source  |  Snapshot  |  Play / Build / Understand  |  View Family  |  Preset  |  Word Sense  |  Boss  |  ×
```

### Current implementation note

The proof-of-concept top strip should stay deliberately small while the contextual shelves absorb the growing tool vocabulary.

Current top-strip responsibilities:

- Switch active playset/view family: Platformer, Brickbat, Pinball, Overhead.
- Reset the current playfield state when explicitly requested.
- Show/hide the Cockpit.
- Trigger the Boss Key.

Mode-specific tools should move into the relevant page/shelf instead of accumulating on the top strip. The Platformer shelf now uses this first-pass category model:

- **Session:** Save Level, Load Level, Enter Play Mode / Return to Editor.
- **Build Tools:** Ladder, Ramp, Slide, Conveyor, Elevator.
- **Route / Logic:** Start, Checkpoint, Goal, Hidden Switch.
- **Player Rules:** Safety Floor, Gun.
- **Enemy Rules:** Enemy AI, Enemy Track, Enemy Shots.
- **Text Rules:** Text Terrain, Text Crawl, Shot Text Damage.
- **Reset:** Clear Placed Parts.

This same pattern should be reused for Brickbat, Pinball, Overhead, and future families: the top strip answers "where am I and how do I hide/reset it?", while the contextual shelf answers "what can I build or tune here?"

### Source

- Desktop
- Monitor
- Window
- Region
- Image
- Text Grid
- Live Desktop
- Later: supported source clone/import

### Snapshot

- Capture Snapshot
- Re-snapshot
- Reset Clone
- Save Variant
- Compare Source
- Export Pack

### Mode

- Play
- Build
- Understand

### View Family

- Side View
- Overhead
- Ball / Table Physics
- Paddle / Clearing
- Grid / Text
- Route / Flow
- Ambient / Desktop Toybox

### Preset

Changes based on the selected View Family.

### Word Sense

- Off
- Lazy Local
- Full Page Prep
- Status: unavailable / reading / N words known

### Boss

Always visible as a safety affordance when DACK UI is visible. The keyboard Boss Key remains separate and instant.

### Close gadget

The menu/Cockpit needs a visible close gadget in the upper-right, even though Esc is the reliable shortcut. The close gadget hides the ordinary menu/editor overlay; it is not the Boss Key and should not imply panic/privacy mode.

Recommended behavior:

- `×` closes/hides the Cockpit.
- `Esc` toggles the Cockpit.
- `Ctrl+Alt+B` remains the stronger Boss Key.
- Mouse pointer becomes visible while the Cockpit is open and returns to gameplay policy when it closes.

## View families and presets

## Coverage audit

The current family model covers the major 2D game-construction spaces DACK is likely to need:

| Family | Covers |
| --- | --- |
| **Side View** | platformers, climbers, side shooters, digging games, side-view obstacle courses |
| **Overhead** | Combat/tanks, driving, planes/spaceships, RPG/adventure actors, animals/insects, swarms |
| **Ball / Table Physics** | pinball, pachinko/plinko, marble tables, bumper toys |
| **Paddle / Clearing** | Brickbat, Pong-like games, text clearing, target walls, catch/deflect games |
| **Grid / Text** | roguelikes, Snake/Maze, terminal/BBS games, spreadsheet tile maps, word quests |
| **Route / Flow** | racing, Frogger-like crossings, tower defense, patrol paths, conveyor/factory flows |
| **Ambient / Desktop Toybox** | tiny workers, office wildlife, ambient Word War, decorative/reactive desktop life |

Likely future specialty presets can fit without new top-level menus:

- **Fixed-screen shooter / gallery shooter:** Paddle/Clearing or Overhead depending on movement.
- **Missile defense / interception:** Overhead or Route/Flow, with protected-object markers.
- **Territory / Surround / light-cycle:** Grid/Text or Route/Flow depending on grid vs spline trails.
- **Puzzle / casual sorting:** Grid/Text, Route/Flow, or Ambient depending on source grammar.
- **Rhythm/timing:** usually Route/Flow, because the core authoring object is a timed lane/event stream.

So the top-level taxonomy is not "complete forever," but it is broad enough that new ideas should usually enter as presets, not as new top-level categories.

### 1. Side View

Side View is the family for left/right/up/down play where gravity, floors, ladders, ramps, slides, conveyors, ledges, falls, jumps, and side-view projectiles matter.

Presets:

- **Horizontal Platformer:** Pitfall-like left/right traversal.
- **Vertical Climber:** Crazy Climber / Mario vertical traversal, ladders, single-spaced text crawl.
- **Side Shooter:** run/jump/shoot; document text and UI objects as terrain/cover.
- **Digging / Lode Runner:** dig holes, tunnel through text, create/repair terrain.
- **Crawl / Fence Text:** text rows become climb/crawl surfaces if enabled.
- **Office Obstacle Course:** simple casual traversal using document/window geometry.

Primary shelves:

- Platforms
- Ladders
- Ramps
- Slides
- Conveyors
- Elevators
- Markers/Flags
- Enemies
- Projectiles
- Dig tools
- Hazards
- Pickups

Important rules:

- Text Terrain on/off
- Text Crawl on/off
- Text Destruction on/off
- Per-actor/per-projectile text capability flags
- Gravity
- Safety floor
- Fall/death policy

### 2. Overhead

Overhead is a camera/control family, not one game.

Presets:

- **Combat / Tanks:** rotate, drive, shoot, ricochet, hide, duel.
- **Driving:** steer, accelerate, brake/reverse, drift, follow roads or tracks.
- **Planes / Atmospheric Flying:** steer/bank, throttle, climb/dive, drag, stall-ish slowdown, altitude bands, clouds/terrain as lanes.
- **Spaceships / Thrust:** rotate, thrust, coast/inertia, wrap/bounce, shoot, local gravity wells, orbit hazards, Spacewar-style arenas.
- **Lunar Lander:** rotate, thrust, conserve fuel, manage descent rate, touch down on safe pads, crash into document terrain.
- **RPG / Adventure:** 8-way or click-to-move, interact, pick up, open, talk/fight.
- **Animals / Insects:** crawl, wander, forage, flee, swarm, follow trails, climb over/around text.
- **Horde / Robotron-like:** player-centered arena pressure, flock/chase groups, rescue/escort variants, projectile floods, and panic-room survival.
- **Office Creatures / Workers:** inspect, patrol, carry, build, repair, emote, fight, respond to live document/desktop events.

Primary shelves:

- Player spawns
- Enemy/NPC spawns
- Start / midpoint / goal markers
- Patrol routes
- Cover regions
- Ricochet walls
- Doors/gates
- Pickups/items
- Objectives
- Safe zones
- Line-of-sight tools
- Scent/trail/swarm fields
- Flock/horde emitters
- Bullet-pattern emitters
- Trigger/marker flags

Important rules:

- Movement model: tank / car / atmospheric flyer / thrust-space / walk / crawl / swarm / flock
- Actor collision profile
- Projectile ricochet / pierce / explode
- Line of sight
- Cover
- Per-actor text behavior
- AI behavior blocks

Flying/space physics need their own movement libraries:

- **Atmospheric flyer:** facing direction, throttle, turn rate, climb/dive, drag, optional lift/stall, altitude bands, ground/ceiling avoidance.
- **Thrust-space:** rotation, thrust vector, inertia, max velocity, damping/drag, wrap/bounce boundaries, rotate-to-aim vs independent turret aim.
- **Lunar lander:** gravity, main/side thrusters, fuel, landing pads, safe descent/tilt thresholds, crash explosions, terrain/debris collision.
- **Localized gravity:** point/region gravity wells, inverse-square-ish or constant pull, safe orbit radius, slingshot strength, black-hole/star hazards, per-actor gravity sensitivity.
- **Arena rules:** screen wrap, edge bounce, kill boundary, asteroid/word debris, ricochet shots, local sun/planet/source-word gravity.

First complete side-view level spine:

- `Start Point`: editor-visible spawn marker, hidden during play unless the creator chooses otherwise.
- `Checkpoint`: visible midpoint or recovery marker.
- `Goal`: visible end objective; useful immediately for platformers and later for racing, escort, maze, and tower-offense modes.
- `Enemy/NPC`: actor placed as blocker, guard, hazard, or moving puzzle piece.

Enemy setup should be a small set of composable choices rather than one giant AI page:

- Locomotion: grounded, flying, climbing, crawling, swimming, turret/static.
- Behavior: patrol, guard, chase, flee, wander, ambush, swarm, flock/horde, escort target, defend goal.
- Attack: contact only, projectile, beam/laser, text-destroying shot, area pulse, none.
- Text interaction: collide with text, ignore text, destroy text, climb text, tunnel text, seek/avoid OCR words.

Flock/Horde should be a behavior card, not a bespoke game. Core exposed knobs:

- group size / spawn budget;
- cohesion, separation, and player-attraction weights;
- leader/follow-leader vs pure boids;
- panic radius / scatter behavior;
- contact damage vs projectile emitters;
- bullet pattern: aimed, radial, spiral, burst, lane, random spray;
- friendly/escort mode for “rescue the office workers” variants.

Defend should be another core behavior card:

- defend a point, area, route node, word, object, NPC, or marker;
- hold, orbit, patrol perimeter, block lane, or intercept intruders;
- chase only within a pursuit radius, then return to the defended anchor;
- bind directly to Goal, Checkpoint, Hidden Switch, door, tower-defense base, escort target, or semantic word-object.

Sunny Dragon is the first practical test of this model: a flying guard that can block a route in platformer, side-view shooter, Brickbat bonus/hazard, pinball toy, or overhead modes without requiring new art.

### 3. Ball / Table Physics

This is the family for Pinball and other continuous ball-table toys.

Presets:

- **Pinball Table:** flippers, plunger, bumpers, rollovers, drain, jackpots.
- **Document Pinball:** gutters as lanes, headings/icons/pillboxes as bumpers/inserts.
- **BBS Backglass Pinball:** opaque textmode table art and score reels.
- **Pachinko / Plinko:** gravity drop, pegs, score bins.
- **Marble Table:** tilt, bumpers, holes, gates, ramps.

Primary shelves:

- Ball spawn
- Plunger lane
- Flippers
- Bumpers
- Rollovers
- Gates
- Ramps/rails
- Drains/outlanes
- Inserts/lights
- Jackpot/multiball logic
- Score reels/HUD
- Nudge/tilt rules

Important rules:

- Table gravity/tilt
- Elasticity
- Friction
- Ball count
- Multiball cap
- Flipper strength/return speed
- Drain/ball-save policy
- Lit/unlit target state
- Semantic word missions

### 4. Paddle / Clearing

This is the family currently proven by Brickbat.

Presets:

- **Brickbat:** clear letters/words with ball and paddle.
- **Word Bricks:** word-sized targets, OCR/found-poem ticker.
- **Letter Wall:** individual text-object clearing.
- **Pong-ish Duel:** two paddles, ricochet, defend sides.
- **Red Pen / Editor Mode:** document correction as arcade clearing.
- **Laser Column:** targeted beams and document mutation.

Primary shelves:

- Paddles
- Balls
- Target recipes
- Power-ups
- Lasers
- Score panels
- Word ticker
- Persistence/mutation policy

Important rules:

- Target grain: letter / word / line / icon / color / heading
- Ball count
- Paddle orientation
- Deflection
- Power-up deck
- Text erasure policy
- OCR priority
- Clone damage persistence

### 5. Grid / Text

Grid/Text is the family for terminal-style, glyph, tile, maze, and word-goal games.

Presets:

- **RPG / Roguelike:** Rogue/Hack-style dungeon maps.
- **Snake / Maze Chase:** collect, grow, flee/chase, tunnels, power states.
- **Life / Snake / Minefield Mashup:** cellular growth, moving snake/chain bodies, hidden mines/traps, revealed danger counts, and chain reactions across text cells.
- **BBS Terminal Mode:** opaque ASCII/ANSI/CP437 layer.
- **Spreadsheet Dungeon:** grid cells as tiles.
- **Word Quest:** seek/avoid/collect semantic words.

Primary shelves:

- Glyph legend
- Walls/floors
- Doors/keys
- Monsters/NPCs
- Items
- Stairs/exits
- Pellets
- Enemy homes
- Tunnels/wrap edges
- Fog/visibility
- Traps
- Cells/mines/life rules
- Inventory/rules

Important rules:

- Grid size / cell mapping
- Text/graphic/hybrid presentation
- Turn-based vs realtime
- Pathfinding
- Word goals
- Cellular birth/survival rules
- Mine reveal / flag / detonation rules
- Enemy chase/flee/scatter
- Item/inventory logic

The Life/Snake/Minefield mashup should be treated as an experimental throwback preset:

- **Life layer:** cells are born/survive/die by simple neighbor rules, optionally seeded from text density or OCR categories.
- **Snake layer:** the player or enemy chain moves through the grid, grows, sheds, tunnels, or consumes cells/words.
- **Minefield layer:** hidden cells contain traps, bonuses, or chain-reaction explosives; adjacent danger counts can be shown as numbers, glyph colors, or document annotations in the clone.
- **DACK twist:** words can become rule seeds: `BOMB`, `FOOD`, `WALL`, `LIFE`, `TARPIT`, `EXIT`, etc.

### 6. Route / Flow

Route/Flow is for games where paths, lanes, checkpoints, waves, traffic, and route conflict matter. The shared model is:

> actors attempt to traverse a route while placed tools, hazards, defenders, escorts, blockers, or helpers alter whether they survive/reach the objective.

Tower Defense, Tower Offense, escort, convoy, Frogger-like crossing, patrol planning, racing, and conveyor/factory sorting are different polarities of the same route system.

Presets:

- **Racing / Track:** start, finish, checkpoints, laps, boosts, hazards.
- **Crossing / Escort:** Frogger-like timing lanes, traffic bands, safe islands, carried platforms, convoy routes, and self-escort missions.
- **Tower Defense:** enemy waves follow routes; creator places towers, blockers, slows, and traps to protect an objective.
- **Tower Offense:** friendly waves/convoys follow routes; creator places escorts, healers, shields, breachers, decoys, or route modifiers to get units through defenses.
- **Process Defense:** defend a document/diagram/project flow.
- **Patrol Planner:** actors follow editable paths.
- **Conveyor / Factory:** flows, belts, gates, sorting.

Primary shelves:

- Paths/splines
- Lane bands
- Moving hazard streams
- Moving platform streams
- Safe islands
- Start/finish markers
- Checkpoints
- Lanes
- Towers
- Escort/guard units
- Blockers/barricades
- Spawns
- Objectives
- Protected targets
- Breach/reach targets
- Boosts/slows
- Heals/shields
- Gates/switches
- Route heatmaps

Important rules:

- Path direction
- Route polarity: defend / offend / escort / race / cross / sort
- Lane speed/direction
- Safe/unsafe timing windows
- Carried-platform behavior
- Route cost
- Lap/checkpoint order
- Wave schedule
- Wave allegiance
- Tower targeting
- Escort targeting
- Upgrade rules
- Dynamic path blocking
- Objective health / convoy health / breach progress
- Live document/desktop event triggers

### 7. Ambient / Desktop Toybox

Ambient is the first-run personality layer: the desktop simply wakes up.

Presets:

- **Tiny Workers:** entities inspect, carry, build, repair.
- **Office Wildlife:** insects/animals explore the desktop/document.
- **Little War / Word War Ambient:** low-stakes patrols/skirmishes.
- **Garden / Aquarium:** gentle growth and decorative activity.
- **Boss Key Safe Preview:** instantly quiet, harmless, work-looking state.

Primary shelves:

- Creature/worker spawns
- Patrol/forage zones
- Emotes
- Props
- Tiny tasks
- Ambient triggers
- Live event reactions
- Safe zones

Important rules:

- Intensity
- No-fail / engaged / siege
- Pause/detach
- Live desktop boundaries
- Activity event map
- Boss Key behavior

## Marker / flag menu model

Markers should be a shared object family across every view family.

They should not appear as unrelated Start Point, Checkpoint, End Flag, Hidden Switch, etc.

One object model:

- role: start / midpoint / end / secret / switch / checkpoint / objective
- presentation: flag / checkbox / glow / text label / icon / invisible
- visibility: visible in play / editor-only
- binding: free point / source word / icon / window edge / route node / region
- behavior: spawn / respawn / save / trigger / win / route checkpoint / score / objective

Examples:

- Side View: start flag, checkpoint flag, end flag.
- Overhead: player spawn, enemy spawn, objective marker, hidden trigger.
- Pinball: ball spawn, drain, lit insert, jackpot trigger.
- Racing: start line, checkpoint, finish line.
- Tower Defense: wave spawn, route node, protected objective.
- RPG: stairs, secret door trigger, quest marker.

The UI should present this as **Markers & Logic**, with role and visibility checkboxes in the Inspector.

## Recommended menu hierarchy

```text
DACK
  Source
    Capture Desktop
    Capture Monitor
    Capture Window
    Capture Region
    Use Image
    Use Text Grid
    Live Desktop

  Snapshot
    Capture Snapshot
    Re-snapshot
    Reset Clone
    Save Variant
    Export Pack

  Mode
    Play
    Build
    Understand

  Game Type
    Side View
      Horizontal Platformer
      Vertical Climber
      Side Shooter
      Digging / Lode Runner
    Overhead
      Combat / Tanks
      Driving
      Planes / Spaceships
      RPG / Adventure
      Animals / Insects
      Office Creatures / Workers
    Ball / Table Physics
      Pinball
      Document Pinball
      Pachinko / Plinko
      Marble Table
    Paddle / Clearing
      Brickbat
      Word Bricks
      Letter Wall
      Pong-ish Duel
    Grid / Text
      RPG / Roguelike
      Snake / Maze Chase
      BBS Terminal Mode
      Spreadsheet Dungeon
    Route / Flow
      Racing / Track
      Crossing / Escort
      Tower Defense
      Tower Offense
      Process Defense
      Patrol Planner
    Ambient / Desktop Toybox
      Tiny Workers
      Office Wildlife
      Ambient Word War
      Garden / Aquarium

  Layers
    Source Clone
    Collision
    Text Boxes
    Word Sense Labels
    Markers & Logic
    Routes
    AI Heatmaps
    Mutations
    HUD Avoidance

  Safety
    Boss Key
    Pause / Detach
    Hide Pointer During Play
    Clone-Only Status
```

## RAD UI implication

The current always-on strip should remain small:

```text
[-]  SIDE  OVERHEAD  BALL  PADDLE  GRID  ROUTE  RESET  COCKPIT  BOSS
```

For the current prototype mappings:

- `SIDE` opens Platformer.
- `PADDLE` opens Brickbat.
- `BALL` opens Pinball placeholder.
- `OVERHEAD` opens Overhead placeholder.
- `GRID` and `ROUTE` can be next placeholders.

Inside the Cockpit, the family page chooses the exact preset and exposes shelf parts. This lets the top strip stay comprehensible while the construction kit grows.

## Two-monitor direction

Two-monitor design should become a first-class layout mode, not a hack around fullscreen.

Primary arrangement:

- **Monitor A: live work surface.** The real desktop/app/document stays native and usable.
- **Monitor B: DACK clone/editor/playfield.** DACK shows the cloned playfield, Cockpit, shelves, inspector, Understanding overlays, and test play.

Useful modes:

- **Work + clone:** user writes/edits on one monitor while the game runs on the cloned view on the other.
- **Live desktop theater:** one monitor is the office desktop; the other is the active game world reacting to it.
- **Editor + preview:** one monitor shows Build/Understand controls; the other stays close to pure Play.
- **Overview + detail:** one monitor shows a full battlefield; the other shows zoomed local action or selected-object editing.

Safety rules:

- Only explicitly selected monitors/windows/regions are captured.
- Boss Key clears/hides DACK on every monitor.
- The clone-only status remains visible in editor modes.
- Sensitive source previews should not appear in task-switch thumbnails where avoidable.

Menu implication:

```text
Layout
  Single Monitor
  Two Monitor: Work + Clone
  Two Monitor: Editor + Preview
  Two Monitor: Overview + Detail
```

This reinforces the product distinction: the real work stays untouched on one side; the living game clone plays on the other.
