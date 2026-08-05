# DACK Construction-Kit Inspiration and Document-Native Design Rules

**Status:** Active historical/design guardrail; reviewed 2026-08-05
**Authority:** Document-native product test and reference analysis; the primary design document controls scope and the optimization plan controls order.

The `raw base assets/Inspiration` folder is a useful visual reminder of what made classic construction kits memorable. The Shoot-'Em-Up Construction Kit and Adventure Construction Set references are not merely palette or nostalgia references. They show a complete authoring grammar: a creator chooses a reusable definition, edits a small set of understandable parameters, places or sequences instances, and can test the result immediately.

DACK should borrow that clarity while making the document, image, window, and desktop the actual material of play. If the source document can be removed and the result still feels identical, we have drifted toward a generic SEUCK/ACS imitation rather than DACK.

## The lineage and the gap DACK fills

The historical construction sets solved a human problem before “no-code” became a product category: they replaced programming syntax with a small, legible world of parts and parameters. Their original manuals show the pattern directly:

- [Pinball Construction Set](https://smalltalkzoo.computerhistory.org/users/Dan/uploads/PinballConstructionSet.pdf) makes parts, placement, gravity/physics adjustment, play, and saved tables one toy-like loop.
- [Adventure Construction Set](https://www.mocagh.org/ea/acsuk-manual.pdf) separates play from named construction workrooms, supports reusable map/creature/thing material, and can auto-construct a useful starting adventure.
- [SEUCK](https://manualzz.com/doc/html/28942555/shoot--em-up-construction-kit-user-manual) exposes distinct sprite, object, background, sound, player, attack-wave, level, test, and storage utilities with no programming prerequisite.

Modern creative tools refine the same structure:

| Modern pattern | Evidence | DACK rule |
| --- | --- | --- |
| Definition versus placed instance | [GameMaker objects/instances](https://manual.gamemaker.io/beta/en/Additional_Information/Objects_vs_Instances.htm) and Room Editor | A Card is a reusable definition; a playfield placement is an independently identified instance. |
| Visible local override with Apply/Revert | [Unity prefab-instance Inspector](https://docs.unity3d.com/ja/current/Manual/prefab-instance-inspector-reference.html) | Instance overrides are marked and can Reset to Card, Apply to Definition, or Fork Card deliberately. |
| Selection-specific properties and bulk edit | [Unreal Details Panel](https://dev.epicgames.com/documentation/en-us/unreal-engine/level-editor-details-panel-in-unreal-engine) and [Property Matrix](https://dev.epicgames.com/documentation/en-us/unreal-engine/property-matrix-in-unreal-engine) | One Inspector follows selection; a later Level Contents view and property matrix handle repeated actors/objects. |
| Consolidated component properties and compatible swaps | [Figma component properties](https://help.figma.com/hc/en-us/articles/5579474826519-Explore-component-properties) | Composed Cards expose selected bindings and common values in one place; Slots filter compatible Card replacements. |
| Task-oriented layouts over one document | [Blender Workspaces](https://docs.blender.org/manual/en/latest/interface/window_system/workspaces.html) | Build, Understand, and Sprite Studio are views over one session, not separate copies of the level. |

DACK's historical niche is the missing combination: **classic construction-set immediacy, modern definition/instance discipline, and a read-only visible office surface as the native material.** Microsoft described [Ribbon Hero](https://blogs.microsoft.com/ai/ready-to-play-with-microsoft-clippy/) as a casual game for discovering Office commands; DACK's office relationship is different. It does not teach or operate Word, Excel, or another host. It compiles the visible work into a clone-safe world and lets the creator decide what that world means.

This distinction should survive every refactor. The reusable engine systems can be conventional; the authored experience must keep source bindings, interpretation, and clone mutation close enough to see and play with.

## What the references reveal

### 1. The editor is a set of small, named workrooms

The reference menus separate sprites, objects, backgrounds, sound effects, player limits, attack waves, levels, front-end presentation, test play, and storage. Each page has a clear purpose and a small vocabulary of controls.

DACK should preserve this rhythm in the Cockpit:

- **Source / Understanding:** capture, Snapshot, document regions, OCR, window boundaries, and provenance.
- **Scene / Terrain:** text platforms, image regions, grids, paths, ramps, ladders, and invisible geometry.
- **Actors / Objects:** reusable character and object definitions, then placed instances.
- **Rules / Waves:** movement, collision, AI, damage, projectiles, goals, spawns, and intensity.
- **Presentation / Audio:** palettes, shadows, ANSI underlays, effects, sounds, and HUD policy.
- **Test / Snapshot / Storage:** F6 test loop, save/load, immutable source clone, and package preview.

The page changes; the session, source clone, selection, and document understanding do not.

### 2. Definitions and placed instances are different things

The references show a current object or sprite being edited separately from the list of objects used in a level. DACK needs the same distinction:

- An **asset definition** is a reusable actor, projectile, explosion, tile, palette, sound, or behavior profile.
- A **placed instance** is one use of that definition in a particular Snapshot, with its own position, scale, facing, attributes, and overrides.
- Repeated shelf clicks or card drops always create new instances. A level may contain many copies of the same enemy or element.
- Editing a definition offers an explicit choice: update this instance only, or update the shared profile and show affected instances.

This is the basis for a real game builder rather than a screen full of one-off decorations.

### 3. Small parameter sheets beat hidden complexity

The reference controls use compact integers, toggles, and short labels: speed, points, hits to kill, fire direction, bullet speed, delay, collision, and whether a player or enemy is enabled. DACK should continue to expose the same kind of bounded controls:

- Prefer small whole-number ranges and named presets.
- Show the live result beside the control: path, projectile ray, collision volume, score effect, or text region.
- Keep advanced attributes behind an expandable card rather than adding permanent shelf sprawl.
- Make every parameter reversible and Snapshot-saveable.

The creator should be able to understand a character or object by reading its card aloud.

### 4. Palette and frame editing are authoring, not asset administration

The sprite and block editor references make the low-resolution grid visible. They provide a palette, mirror/slide controls, copy/flood/undo, frame or object numbers, and a clear current target. This supports DACK's live-linked sidebar pad and Sprite Studio:

- Always show the actual frame or block being edited at a readable scale.
- Keep frame numbers, labels, sequence ranges, and facing in one source of truth.
- Treat palette profiles and transparent color as part of the actor's identity.
- Preserve the source orientation; display transforms such as facing and reverse are explicit and non-destructive.
- Let the same actor definition feed Platformer, Brickbat, Pinball, Overhead, and later RPG playsets.

### 5. Map editing carries meaning, not just paint

The Adventure Construction Set terrain screen exposes not only pictures but also whether a terrain is open and who can travel through it. That is a direct precedent for DACK's text-aware world model.

Every discovered or painted region should be able to carry separate policies:

- presentation: native text, recolored text, graphic replacement, opaque ANSI, or invisible;
- traversal: solid, climbable, crawlable, slippery, conveyor, gap, hazard, or pass-through;
- mutation: protected, destructible, harvestable, word target, letter target, or blast-reactive;
- semantics: heading, paragraph, icon, pillbox, button, window, code block, or named goal;
- permissions: player only, enemy only, projectile only, or shared.

The document is therefore both the map and the material legend.

### 6. Narrative and status belong inside the playfield language

The adventure reference uses a map, a character, a status/narrative band, and location or mission text as one composition. DACK should use the same idea without covering the source unnecessarily:

- OCR-discovered words may become goals, taunts, bonuses, mission fragments, or haiku-like score history.
- A document heading can become a level title, a room name, a wave label, or a boss introduction.
- Reclaimed white space can host score and status, but the HUD should remain visually subordinate to the source text.
- In live desktop mode, application changes and window activity can become narrative events rather than generic timers.

### 7. Test and storage are part of the creative loop

The references make `TEST GAME` and `STORAGE` first-class menu destinations. DACK's equivalent is:

- `F6` toggles Build/Edit and Play without losing selection or the active document mutation.
- Snapshot freezes the admitted source pixels and capture identity used by the test. Its matching Intake Recipe and immutable Analysis Revision preserve creator guidance and derived interpretation without pretending that either is part of the pixel baseline.
- The Level references those immutable products and owns accepted corrections, Card bindings, placed instances, rules, routes, and authoring policy. The Working Clone and Run State remain separate so test-play damage cannot silently rewrite authored truth.
- Save/load restores the complete session aggregate while retaining those ownership boundaries; named mutation branches are Variants, not mutated Snapshots.
- Publish shows the clone and metadata-scrubbing preview; it never edits or packages the original implicitly.

## DACK's document-native oath

Every toolkit should pass these checks:

1. The source document, image, window, or desktop capture remains identifiable while building and testing.
2. At least one core mechanic uses a discovered or author-defined document feature: text, whitespace, paragraph geometry, icon, window boundary, color region, or desktop activity.
3. The creator can inspect what the document contributed through Understanding mode.
4. The source and Snapshot Baseline remain immutable; only the Working Clone, Region Runtime State, and explicitly saved Variant branch change.
5. A generic replacement asset is allowed, but it is labeled as a freeform overlay rather than pretending to be document-derived.
6. Removing the source layer should make the level materially less interesting. If it does not, the playset needs a stronger document rule.

## Implications for current work

- Keep the two-level role/family → individual asset picker; it is the compact modern equivalent of the old editor's current-object selector.
- Keep repeated placement additive and randomized initially, then let creators drag, align, duplicate, and save exact instances.
- Make source-region anchors visible in Build mode and hidden in Play mode.
- Add document-derived presets to every genre page: text terrain for Platformer, words/regions for Brickbat, paragraph lanes and ANSI underlays for Pinball, headings/icons for Overhead, and glyph/word rules for RPG/Snake/Maze.
- Treat ANSI/ASCII art as an intentional presentation layer with provenance, not as a generic background texture.
- Use the same parameter-sheet language for actors, projectiles, explosions, terrain, spawns, waves, and goals.

## Reference handling

The files in `raw base assets/Inspiration` are internal visual references. They should guide interaction and information architecture, not be redistributed as DACK content unless their provenance and license are independently confirmed.
