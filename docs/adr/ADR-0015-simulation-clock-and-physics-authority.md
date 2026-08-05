# ADR-0015: Simulation Clock and Physics Authority

- **Status:** Accepted; Pinball rate/CCD configuration requires measurement
- **Date:** 2026-08-05
- **Decision owners:** DACK project
- **Related:** ADR-0010; ADR-0011; `DACK-Optimization-and-Refactoring-Plan.md`; `DACK-Pinball-and-Overhead-Combat-Startup.md`

## Context

DACK combines platforming, projectiles, overhead movement, Pinball, text mutation, editor preview, multiple windows, and background analysis. Prototype code has sometimes advanced toolkit behavior from rendered-frame callbacks, while the Pinball note proposed a separate hand-written 120 Hz solver. Either path can make gameplay speed depend on rendering, duplicate simulation when a second view opens, or create two competing collision authorities.

The project already uses Godot 2D physics successfully and prefers to learn its limits before maintaining another physics engine.

## Decision

### 1. One clock and one world

- One authoritative fixed simulation clock advances one `SimulationWorld` for the active session, regardless of render frame rate or window count.
- Rendering may interpolate between committed simulation states. UI, HUD, diagnostics, thumbnails, and decorative effects update independently and may use lower or variable presentation cadences.
- Simulation never advances once per visible viewport. A second monitor is another view of the same committed tick.

The baseline semantic tick is 60 Hz. A measured toolkit may request a higher physics evaluation rate, but it remains one clock policy owned by the session—not a loop inside an overlay and never “N substeps per rendered frame.”

### 2. Bounded time behavior

- The accumulator has a configured maximum number of catch-up ticks and a maximum accepted frame delta.
- If the process falls behind, DACK reports the overrun and uses the selected quality/slow-frame policy; it never performs unbounded catch-up.
- Gameplay RNG is seeded, serialized where required, and separate from decorative/effect RNG.
- Deterministic order is: sample buffered input → pre-physics rules/AI → physics step → contacts/damage/environment mutation → post-step events/goals/scoring → atomic state publication → render interpolation.
- Per-tick rules, AI perception, path requests, contacts, and mutation work have budgets and deterministic overflow/defer rules.

### 3. Mode semantics

- **Play:** advances the simulation and accepts gameplay input.
- **Build:** does not advance an ordinary run; explicit preview/scrub tools may evaluate isolated deterministic samples without mutating Run State.
- **Freeze:** pauses advancement while preserving the exact current run and visual composition.
- **Stop/Reset Run:** ends transient Run State and reconstructs the configured Level/Variant start state.
- **Boss/Safety:** immediately mutes/releases/hides presentation and parks advancement/providers through a high-priority application command; restoration resumes or remains frozen according to the saved safety policy.

### 4. Godot-first physics authority

- Godot 2D bodies, areas, shapes, continuous-collision options, and direct-space queries are the first implementation for actors, projectiles, balls, flippers, bumpers, triggers, and authored world parts.
- Toolkit helpers may calculate paths, apply forces/impulses, resolve grid movement, or perform deterministic kinematic sweeps, but one object/contact pair has one collision authority in a given mode.
- A custom solver requires a measured failure of the Godot path, deterministic fixtures demonstrating the requirement, and a superseding/narrow ADR. DACK does not maintain a half-Godot/half-custom contact response by accident.
- Dense document geometry is queried through the indexed Environmental Map and promoted/batched collision representation. No ball, actor, or projectile scans every glyph on every substep.

### 5. Pinball measurement gate

The first Pinball comparison uses Godot physics with a `RigidBody2D`-style ball, continuous collision where appropriate, authored static/animatable parts, and the shared indexed text-plow query. Measure 60 Hz and 120 Hz physics policies against the same fixtures. Compare tunneling, flipper energy transfer, repeatability, CPU time, catch-up behavior, and creator-tunable feel before selecting the shipping rate.

If 120 Hz wins, the session activates that fixed physics policy while Pinball runs and restores the prior policy transactionally on exit. It still does not multiply work by render frames or view count.

## Consequences

### Positive

- Gameplay remains stable across fast/slow displays and one/two-window layouts.
- Freeze, Stop, Boss, save/checkpoint, replay, and diagnostics gain precise semantics.
- Godot remains the maintained physics platform until evidence justifies more code.
- Pinball can demand higher fidelity without becoming a separate application loop.
- Performance budgets can attribute simulation, physics, AI, environment queries, and rendering separately.

### Tradeoffs

- Existing frame-driven RAD behaviors must migrate gradually and receive regression fixtures.
- Fully bit-identical physics across engines/hardware is not promised; DACK targets deterministic event ordering and bounded, reproducible construction-kit behavior.
- A 120 Hz Pinball policy may increase CPU cost and therefore must earn its default on baseline hardware.

## Validation

1. Running one versus two visible views produces the same tick count, actor/projectile count, score, mutation sequence, and gameplay RNG stream.
2. A 30/60/144 Hz render test produces equivalent gameplay outcomes for a fixed input trace within declared physics tolerances.
3. Freeze and Boss stop advancement within their budgets and restore the intended prior state without a catch-up burst.
4. A long stall cannot trigger an unbounded simulation spiral.
5. Dense-text Pinball reports bounded spatial candidates per ball step and passes high-speed tunneling fixtures.
6. Switching into/out of a higher Pinball rate restores the session policy and does not duplicate contacts or impulses.
7. No gameplay object is advanced or collision-resolved by both Godot and an overlay-local solver in the same tick.

