# DACK Space, Air, Tank, and Artillery Concepts

- **Status:** Exploratory concept note; non-normative
- **Planning role:** Idea bank for future toolkit/card proposals, not an approved scope or implementation sequence
- **Promotion path:** Validate a small playable slice, then move accepted contracts into the relevant normative design note and [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md)

This note captures candidate broad-family playsets beyond the proven Platformer, Brickbat, Pinball, and early Overhead experiments. The common theme is that the document is no longer only a wall, floor, or brick field. It can become sky, space, terrain, resources, hazards, and mission language.

The source document and Snapshot Baseline remain protected. Any fading, harvesting, cratering, sucking, erasing, or exploding happens against the Working Clone/Variant branch.

Everything below is a candidate until tested. Names, defaults, controls, shelf groupings, physics rules, and module boundaries may change; this note must not override the Snapshot contract, object schema, active toolkit plans, or measured optimization work.

## 1. Document-as-starfield mode

Space games let DACK become surreal without losing the office-document premise. The document can dim into a starfield, with selected letters, punctuation, icons, and OCR words still peering through as stars, coordinates, mines, cargo, or constellations.

Core visual rules:

- The cloned document image can fade toward black, navy, or deep purple.
- Individual letters can remain visible as twinkling stars.
- Punctuation becomes tiny bright stars, sparks, mines, or navigation points.
- Headings and subheadings can become brighter constellations.
- Icons, pillboxes, buttons, and other non-text UI objects can become planets, stations, gates, pickups, bunkers, or nebula markers.
- Dark mode should invert gracefully: instead of fading white paper into space, the existing dark background can deepen while light text becomes the natural star layer.

The important design idea: the document should feel like it is still there, not replaced by a generic backdrop. The player is flying through the meaning and structure of the page.

## 2. Non-destructive word and letter harvesting

Not every text interaction should damage the page. For mining, salvage, escort, and puzzle modes, the player can harvest a copy of the text token while leaving the visible document intact.

Useful variants:

- Letter mining: collect target letters as resources, ammunition, fuel, shield charge, or score.
- Word harvesting: collect OCR words as cargo, spells, objectives, or strange haiku-score readouts.
- Semantic mining: optional later layer where nouns, verbs, numbers, capitalized words, dates, or names produce different rewards.
- Safe extraction: the word glows, duplicates into the ship/tool, then returns to normal.
- Destructive extraction: arcade option where the word is pulled loose, exploded, erased, cratered, or transformed.

Default behavior should be non-destructive for creator trust. Destructive harvesting becomes a per-mode, per-object, or per-powerup option.

## 3. Black hole / gravity well mechanic

A black hole is a perfect DACK object because it can affect text, actors, projectiles, pickups, and the player with one reusable physics idea.

Behavior:

- Pulls nearby loose letters, whole words, projectiles, enemies, particles, and possibly the player.
- Can make letters detach from the document and orbit before being consumed.
- Can threaten target words, forcing the player to rescue them before they disappear.
- Can be visible, invisible until triggered, timed, growing, shrinking, or player-created as a weapon.
- Can use a "meaningful casualty" display: swallowed words appear in the score/haiku strip before vanishing.

Counterplay:

- Tractor beam: pull words away from the gravity well.
- Anchor tool: pin important words temporarily.
- Shield bubble: protect a paragraph or UI region.
- Gravity bomb: create a temporary counter-well.
- Rescue route: collect endangered words and deposit them at a station.

This can support frantic arcade play, puzzle rescue, mining, and boss encounters.

## 4. Space combat playsets

Space is the natural home for inertia, rotation, thrust, gravity, mining, and word salvage.

Candidate modes:

- Asteroid field: letters, punctuation, and UI objects become drifting debris.
- Space War duel: local gravity wells pull ships and shots.
- Lunar Lander: land on document shelves, headings, UI bars, or drawn pads.
- Defender-style rescue: enemies abduct letters/words; the player retrieves them.
- Document Invasion: Space Invaders / Galaxian / Galaga-style fixed-screen defense where the document, text rows, or word fleets advance downward toward the player.
- Robotron/bullet-hell office space: swarm/horde behavior with dense projectile fields.
- Mining run: harvest target words/letters while avoiding damaging protected text.
- Convoy escort: protect a moving word, sentence, cargo pod, or office icon.

### Document Invasion variant

This is the cleanest fixed-screen space combat idea for DACK: the page itself becomes the invader formation.

Possible forms:

- Whole document descent: the cloned page slowly scrolls downward toward the player/base.
- Paragraph fleets: each paragraph becomes a formation that steps sideways, then drops.
- Word squadrons: OCR words or detected word blocks become enemy groups.
- Letter aliens: individual letters detach and attack, still shaped like their original glyphs before evolving into richer sprites.
- Header bosses: section headers become large boss enemies or mothership waves.
- Footnote dive-bombers: punctuation, bullets, numbers, and small UI artifacts break off into fast attackers.

Core rules:

- The player defends a bottom band, desk edge, or office-base line.
- Text can descend as a single pressure wall or as discrete attack rows.
- Some words/letters can be protected, harvested, or rescued instead of destroyed.
- Barricades can be made from remaining document chunks, visible shelf objects, or generated office junk.
- Enemy waves should be able to use simple formation motion first: left/right sweep, step down, fire, occasional dive.
- Later Galaga-like behavior can add capture beams, diving arcs, escorts, challenge stages, and returning formations.

Text-specific scoring:

- Destroyed letters score normally.
- Destroyed whole words feed the haiku/score strip.
- Saved words can score more than destroyed words.
- Forbidden words can punish or trigger chaos if shot.
- Bonus target words can glow briefly as the formation advances.

Builder tools:

- Invasion line / base line.
- Formation source: whole document, selected paragraph, selected region, OCR words, or placed enemy group.
- Advance speed.
- Drop distance.
- Fire rate.
- Dive-bomber toggle.
- Barricade generator.
- Protected-word and bonus-word rules.

This mode belongs under Overhead or Paddle/Clearing depending on movement. If the player only slides along the bottom and shoots upward, it is Paddle/Clearing-adjacent. If the player can move freely in a bottom arena or the waves include richer ships, it fits Overhead.

Shared physics vocabulary:

- Thrust
- Rotation
- Inertia
- Local gravity
- Wrap, bounce, clamp, or death-screen boundaries
- Tractor beam
- Projectile lifetime
- Projectile pierce/bounce/explode
- Gravity-affected projectiles
- Velocity-facing sprites
- Heading-frame sprites

Text interactions:

- Letters as stars/resources
- Words as mission targets
- Paragraphs as asteroid belts
- Icons as stations or bosses
- Headings as sectors
- Highlighted words as bonus routes

## 5. Air combat playsets

Air games sit between space and ground combat. They can use overhead or side-view rules, but usually need forward momentum, altitude/lane ideas, and readable target zones.

Candidate modes:

- River-raid office flyer: navigate through document columns and UI obstacles.
- 1942-style vertical shooter: text fields become cloud/terrain/target lanes.
- Side-view dogfight: headings, text rows, and app windows become altitude bands.
- Bombing run: drop projectiles onto target words, icons, or drawn structures.
- Rescue/escort: guide a vulnerable object across a hostile desktop.

Shared physics vocabulary:

- Forward speed
- Banking left/right
- Optional inertia
- Stall or minimum speed
- Wind
- Altitude lanes
- Bomb arcs
- Homing missiles
- Flak bursts
- Cloud/visibility overlays

Text interactions:

- Words can become targets, no-fly zones, supply drops, or civilians.
- UI pillboxes/icons can become hangars, radar sites, or bonus pads.
- Paragraphs can become terrain bands.
- White space can become safe air.

## 6. Tank and ground combat playsets

Tank combat is the first obvious "overhead office battlefield" mode. It also shares logic with maze, escort, tower defense, tower offense, and capture games.

Candidate modes:

- Combat-style tanks: two or more vehicles in document/UI mazes.
- Convoy escort: protect a moving unit through text corridors.
- Tower offense/defense: pathing, turrets, swarms, defended points.
- Capture-the-flag/checkpoint: start, midpoint, goal, invisible triggers.
- Minefield mashup: hidden hazards in text or whitespace.
- Life/Snake/Minefield hybrid: emergent movement plus word/resource objectives.

Shared physics vocabulary:

- Drive
- Reverse
- Turn
- Strafe, optional
- Turret aim independent of hull
- Projectile bounce
- Projectile pierce
- Explosion radius
- Armor/health
- Defend area
- Patrol path
- Flank
- Ambush
- Flee
- Horde/flock

Text interactions:

- Letters and words as destructible cover.
- Paragraph blocks as walls.
- UI icons as bunkers, depots, turrets, gates, or bonus objects.
- OCR words as objectives or forbidden targets.
- Gaps and whitespace as roads or danger zones.

## 7. SideView artillery / Scorched-Earth-like playset

SideView artillery belongs beside Platformer rather than Overhead. It can reuse the parabola/path editor and the text-terrain/cloned-document destruction systems.

Core idea:

Players or enemies fire arcing shots across a text-derived terrain field. The document becomes hills, cliffs, officescape terrain, or paragraph strata. Shots can crater the cloned playfield, throw letters, erase chunks, or expose pickups.

Candidate rules:

- Turn-based artillery.
- Real-time artillery.
- Wind and gravity settings.
- Adjustable shot angle/power.
- Visible parabola preview.
- Destructible terrain.
- Protected words/regions.
- Optional falling terrain fragments.
- Optional water/acid/void gutters.

Weapon vocabulary:

- Eraser shell: removes text/terrain cleanly.
- Whiteout bomb: paints a temporary blank zone.
- Highlighter flare: reveals hidden words or bonuses.
- Staple mortar: pins terrain or enemies.
- Footnote cluster: splits into small descending notes.
- Memo nuke: huge comic text effect, huge blast radius.
- Black-hole round: creates a temporary gravity well.
- Rubber-band shot: bounces from text or UI edges.

This mode should make DACK's text-destruction feel tactical instead of merely chaotic.

## 8. Shared reusable systems implied by these modes

These concepts should not become one-off code. They imply a small set of reusable modules.

Proposed modules:

- `HarvestableText`: marks letters/words as collectible, protected, destructive, or non-destructive.
- `LooseTextParticle`: makes a letter or word detach, drift, spin, orbit, explode, or return.
- `GravityWell`: pulls actors, projectiles, pickups, and loose text.
- `TractorBeam`: pulls target objects toward an actor without necessarily damaging them.
- `ProjectileArc`: shared ballistic/parabola logic for artillery, bombs, thrown objects, and grenades.
- `TerrainCrater`: mutates the cloned playfield and updates collision after a blast.
- `AltitudeLayer`: gives air games lanes, cloud layers, and target heights.
- `TurretController`: lets tanks, bunkers, ships, and enemies aim independently of movement.
- `HeadingFrameController`: chooses sprite frames based on travel/aim direction.
- `DefendBehavior`: gives enemies a point, region, object, or word to protect.
- `FlockBehavior`: supports swarms, hordes, bullet hell, Robotron-like pressure, and insect/animal groups.

## 9. Builder implications

The creator-facing toolkit should expose these as draggable/assignable pieces rather than technical systems.

Possible shelf categories:

- Space: ship, asteroid, station, gravity well, tractor beam, mineable word, wormhole, shield.
- Air: plane, cloud, flak, bomb target, wind zone, runway, altitude gate.
- Tanks/Ground: tank, turret, bunker, mine, convoy unit, defend zone, checkpoint, road.
- Artillery: cannon, target, wind, gravity, terrain brush, crater rule, weapon pack.
- Text Magic: harvest word, protect word, explode word, spin word, black-hole word, forbidden word.

The best version of this is still keyboard/mouse-first and office-user friendly: drag a thing, give it a few readable traits, press Play.
