# DACK Brickbat Builder

**Status:** Proven play loop; builder/productization in progress  
**Last updated:** July 2026 RAD plateau  
**Authority:** Canonical Brickbat rules and builder vocabulary; engineering sequence remains in [`DACK-Optimization-and-Refactoring-Plan.md`](DACK-Optimization-and-Refactoring-Plan.md)

## Identity

**Brickbat** is DACK’s document-native paddle/target-clearing family. It began as an arcade proof, but its product identity is broader:

- ordinary visible letters, words, headings, icons, pillboxes, cells, actors, or accepted regions become targets;
- the cloned document itself is the board;
- clearing reveals the sampled regional background and changes shared terrain;
- optional Word Sense turns destruction into literary bonuses, found poems, word goals, and semantic missions;
- effects can range from quiet office markup to full analog-neon overkill;
- the creator decides which targets bounce, break, pass through, transform, score, or persist.

Historical paddle-and-brick games are useful mechanical references. Product-facing UI should use **Brickbat**, **Paddle / Clearing**, **target**, **letter**, **word**, and **region** rather than another game’s title.

## Canonical Ball/Reserve State Machine

The standard game owns **three total served balls**.

1. **New Game**
   - Restore the configured new-game Snapshot/variant.
   - Set `ballsRemainingToServe = 3`.
   - Clear transient balls, bonuses, cooldowns, and round score.
   - Serve one ball and decrement the reserve to `2`.
2. **Ball In Play**
   - One served ball begins active.
   - A multiball bonus may create additional active balls up to the configured on-screen cap (default/cap: `3`).
   - Bonus-created balls do **not** consume the reserve.
3. **Partial Loss**
   - If one active ball is lost while another remains, play continues.
   - No reserve ball is served while any active ball remains.
4. **All Active Balls Lost**
   - If `ballsRemainingToServe > 0`, serve exactly one and decrement the reserve.
   - Preserve score, destroyed targets, clone mutations, OCR labels, word ticker, and applicable power-up state.
5. **Final Loss**
   - If no active ball remains and `ballsRemainingToServe == 0`, the run ends.
   - The board remains visibly deformed until the creator/player explicitly chooses **New Game**, **Restore Snapshot**, or another configured reset action.

Switching to another playset does not implicitly restore the page. Platformer, Pinball, Snake/Maze, or another toolkit may inherit Brickbat’s holes. Reset/persistence policy is an explicit level setting.

## Target Model

Brickbat targets are stable environmental/object IDs, not rectangles rediscovered every frame.

Target sources:

- letter/glyph;
- word;
- line, heading, or paragraph;
- icon, button, pillbox, cell, or detected UI region;
- actor/enemy/object card;
- creator-painted region;
- semantic label or category;
- target group/bank.

Each target can define:

- presentation and authority;
- collision mode: `Bounce`, `PassThrough`, `Destroy`, `Damage`, `Transform`, `TriggerOnly`, or conditional;
- hit points;
- score/base multiplier;
- background/mutation recipe;
- Word Sense label/category;
- group/bank and completion event;
- bonus deck;
- effect/sound profile;
- persistence policy.

Letter mode removes one accepted glyph region. Word mode removes one accepted word region. A hit must never erase an unrelated adjacent word merely because their bounding boxes touch.

## Text Clearing

Text clearing uses the shared Snapshot environmental model:

1. Query nearby target IDs through the spatial index.
2. Select the exact target according to the current granularity/recipe.
3. Mark it inactive immediately so collision and every playset agree.
4. Fill/mask its bounded anti-aliased component using the cached regional background model.
5. Clean only the small padded dirty region.
6. Commit visual mutations at most once per simulation frame.
7. Record an undoable/persistable mutation.

Small leftover components below the configured “period-sized” threshold may fade or be cleaned, but cleanup must remain bounded so it cannot eat valid punctuation or nearby light text.

## Paddle and Deflection

Builder controls:

- bottom, top, left, or right paddle;
- paddle length/thickness;
- mouse/direct, keyboard, assisted, or AI movement;
- acceleration/smoothing;
- edge clamp and dead zone;
- flat, curved, sticky, magnetic, split, or temporary side paddle;
- deflection curve by impact point;
- minimum/maximum outgoing angle;
- spin/english and speed gain;
- ball speed tiers and maximum speed.

The default deflection should give deliberate control: center hits return a straighter angle; edge hits produce stronger lateral angles; a minimum vertical/horizontal component prevents boring near-flat or near-vertical loops.

Any document content behind the paddle/drain/serve zone may be neutralized by an explicit reversible **preflight mutation**. The area below a bottom paddle (or beyond the equivalent drain edge) may receive a solid/pattern cover so no source-derived gravity/collision is implied there.

## Multiball

- Maximum active balls: `3`.
- Default cooldown: approximately `30 seconds`.
- Multiball never consumes reserve balls.
- Ball speed may scale with active count: three faster than two, two faster than one, within a readable maximum.
- Effects and sound must not obscure the balls; active balls redraw on the gameplay-critical layer and may use high-contrast/inverted treatment while spectacle is active.

The builder may expose a smaller cap/cooldown, but ordinary presets should not exceed three.

## Laser

The literary/arcade laser is a reusable beam profile:

- roll strength `1–10`;
- map it to `10–100%` of the available travel distance and/or an authored deletion budget;
- arm briefly, then fire at a semi-random readable moment;
- aim along the paddle’s inward axis;
- focus toward an eligible nearby target column/row without snapping unpredictably across the board;
- erase/score intersected targets up to the strength budget;
- show charge, beam, impact, score, and sound as separate effect events.

Higher strength must reliably clear more eligible letters/words, not merely draw a longer cosmetic line. The builder can change the beam to score, reveal, recolor, transform, or damage rather than erase.

## Literary Bonus Deck

Starter bonuses:

- **Footnote:** widen the paddle and add a citation-tail effect.
- **Plot Twist:** sharply alter the next controlled deflection.
- **Second Draft:** save one miss and rewrite the ball into play.
- **Red Pen:** temporary pass-through text deletion.
- **Bookmark:** place a temporary relaunch/checkpoint position.
- **Alliteration:** chain compatible labeled words once Word Sense resolves them.
- **Marginalia:** create a temporary side paddle.
- **Quotation:** protect or duplicate a labeled word instead of deleting it.
- **Run-On Sentence:** temporary uninterrupted pass-through chain.
- **Thesaurus:** transform one semantic target/bonus category into another.

Geometry-only fallback names/effects are always available. OCR/Word Sense enriches the deck; it never gates play.

Power-ups are cards with:

- eligibility/rarity;
- cooldown and duration;
- stacking rule;
- target filter;
- gameplay commands;
- effect/sound profile;
- geometry-only fallback;
- Word Sense upgrade.

## Word Ticker and Found Poems

When a destroyed target has a label, push it into the HUD ticker. The default display keeps the most recent `3–5` words and scrolls older words out, creating short accidental phrases/haiku. Unknown targets use restrained fallbacks rather than flooding the ticker with `TEXT`.

The HUD:

- starts in detected whitespace;
- can be dragged/pinned in Build mode;
- fades/slides on ball approach;
- never becomes collision;
- preserves its position in the level;
- recomputes automatic placement only when its size, source/mutation generation, or avoidance map changes.

## Effects and Sound

Brickbat calls the shared effects/audio libraries:

- target flash, paddle tick, sparks, shock ring;
- word or component-letter shrapnel;
- score caption and combo typography;
- multiball bloom;
- laser charge/fire/impact;
- ball lost, serve, round banner, and game-over count-up.

Each burst may choose a strong solid color, scale, rotation, vector/spline motion, and palette family. Spectacle opacity/quantity adapts to quality and accessibility settings. Balls, paddle, targeting, and required HUD information remain readable above effects.

## Builder Page

Suggested collapsible sections:

1. **Session:** New Game, Test/Play, Restore Snapshot, save variant.
2. **Paddle:** side, size, response, deflection, control.
3. **Balls:** total served balls, active cap, speed tiers, launch randomization.
4. **Targets:** source/granularity, filters, hit points, collision/pass-through policy.
5. **Mutation:** erase/damage/transform recipe, blast cleanup, persistence.
6. **Bonuses:** bonus deck, rarity, cooldown, duration.
7. **Laser:** strength range, aim, delay, deletion/scoring rule.
8. **Scoring:** values, combos, word/line/bank completion, multipliers.
9. **Word Sense:** Off/Lazy/Prepared, goal/bonus categories, fallbacks.
10. **HUD / Effects / Sound:** layout, fade, theme, intensity, profiles.

Controls are descriptors feeding the shared Inspector/session state. Brickbat does not own global source capture, Boss Key, save implementation, effects drawing, OCR process management, or document analysis.

## Current Proof vs. Product Work

| Area | Status |
| --- | --- |
| Letter/word target creation and clone erasure | Proven |
| Three-ball reserve loop | Proven; formalized here |
| Multiball, laser, literary effects, word ticker, sound | Proven at RAD depth |
| Draggable/whitespace-aware HUD | Proven at RAD depth |
| Actors/enemies as destructible targets | Proven |
| Stable region IDs/spatial index/batched mutation | Planned in active optimization pass |
| Schema-driven builder page and target cards | Planned |
| Versioned persistence and replayable deterministic tests | Planned |
| Semantic missions/bonus decks beyond starter rules | Exploratory after Word Sense/product spine |

Implementation sequencing and performance gates are intentionally not duplicated here; follow the active optimization/refactoring plan.
