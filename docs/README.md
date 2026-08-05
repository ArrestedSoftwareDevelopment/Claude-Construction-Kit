# DACK Documentation Map

- **Review baseline:** 2026-08-05
- **Purpose:** separate product intent, binding decisions, data contracts, implementation order, current RAD behavior, and idea banks so every note does not become an equal-priority promise.

DACK is now large enough that “the docs” are a small system. Read them by authority and responsibility rather than chronology. A document may be accurate but non-normative; a toolkit inventory does not schedule itself merely by containing a compelling feature.

## Authority Order

When two documents disagree, use this order:

1. An accepted ADR controls the narrow decision it records. A later ADR must explicitly supersede an earlier one.
2. [`../Desktop-Arena-Design-Doc.md`](../Desktop-Arena-Design-Doc.md) controls enduring product intent, safety, historical positioning, top-level architecture, and release phases.
3. A focused document explicitly labeled **Normative contract** controls its own schema or interaction boundary—for example level coordinates, card resolution, or capture state.
4. [`DACK-Optimization-and-Refactoring-Plan.md`](DACK-Optimization-and-Refactoring-Plan.md) controls current engineering order, provisional budgets, and exit gates. It does not silently redefine the product or a serialized contract.
5. Active UI/tool documents elaborate their assigned responsibility. The ownership table below resolves overlaps.
6. Toolkit and genre notes are design inventories and stress tests, not delivery commitments.
7. [`../dack/README.md`](../dack/README.md) describes what a tester should be able to run in the current RAD build; it does not define the target architecture.

When implementation and target design differ, keep both statements but label them **Implemented RAD** and **Target contract**. Do not use an unqualified “currently” for either.

## Decision Ownership

| Concern | Authoritative owner | Supporting detail |
| --- | --- | --- |
| Product identity, safety, historical niche, phase gates | [Primary Design Document](../Desktop-Arena-Design-Doc.md) | Construction-kit inspiration note |
| Session/navigation invariants and draw-layer ownership | [ADR-0010](adr/ADR-0010-session-preserving-ui-navigation.md) | GUI Architecture and UI Redesign |
| Module dependencies, command ownership, threading, asynchronous publication | [ADR-0011](adr/ADR-0011-core-adapters-and-session-command-model.md) | Optimization/Refactoring Plan |
| Source/baseline/intake/analysis/level/clone/run/variant ownership | [ADR-0012](adr/ADR-0012-snapshot-analysis-clone-state-separation.md) | Level Format and Live Capture Plan |
| Native-pixel clone rendering, dirty updates, and large-page memory bounds | [ADR-0013](adr/ADR-0013-tile-backed-native-pixel-clone-rendering.md) | Live Capture and Optimization Plan |
| Current implementation order and performance budgets | [Optimization/Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md) | RAD README and benchmark fixtures |
| Shell anatomy, menus, workspaces, modals, cards/shelves, two-monitor UX | [UI Redesign](DACK-UI-Redesign-Proposal.md) | GUI Architecture |
| View-family and preset taxonomy | [Top-Level Menu Plan](DACK-Top-Level-Menu-Plan.md) | UI Redesign renders that taxonomy |
| Play/Build/Understand behavior, responsive rules, compositing, HUD | [GUI Architecture](DACK-GUI-Architecture.md) | ADR-0010 |
| Source/capture lifecycle and scene-understanding pipeline | [Live Capture and Understanding Plan](DACK-Live-Capture-and-Understanding-Plan.md) | ADR-0009 and fixture matrix |
| Level/Snapshot/package serialization and coordinates | [Level Snapshot Format](DACK-Level-Snapshot-Format.md) | Object Attribute Model |
| Card definition/instance/override/dependency resolution | [ADR-0014](adr/ADR-0014-card-definition-instance-and-dependency-contract.md) | Object Attribute Model and UI Redesign Inspector rules |
| Object units and family-specific attributes | [Object Attribute Model](DACK-Object-Attribute-Model.md) | Card contract and toolkit notes |
| Simulation clock, mode semantics, and physics authority | [ADR-0015](adr/ADR-0015-simulation-clock-and-physics-authority.md) | Optimization Plan and toolkit physics notes |
| Asset rights and shipping eligibility | [`dack/assets/ASSET_PROVENANCE.md`](../dack/assets/ASSET_PROVENANCE.md) | Asset intake/audit documents |
| Runnable controls, known limitations, smoke test | [`dack/README.md`](../dack/README.md) | Current source tree |

## Status Language

Every substantial future documentation pass should use these labels consistently:

- **Implemented:** present in the current prototype and expected to run.
- **Proven:** demonstrated in the RAD, but not yet productized or generalized.
- **In progress:** actively being consolidated or completed.
- **Planned:** accepted direction with a place in the roadmap.
- **Exploratory:** a useful idea that has not earned a delivery commitment.
- **Deferred:** intentionally outside the current phase.
- **Normative:** binding for its stated boundary even when implementation is incomplete.
- **Historical / superseded:** retained as rationale or evidence but no longer directs new work.

“Current” should be paired with a date or milestone when it might become stale. “Next” should appear primarily in the optimization/refactoring plan rather than being repeated differently across several files.

## Core Documents

| Document | Role | Current emphasis |
| --- | --- | --- |
| [DACK-UI-Redesign-Proposal.md](DACK-UI-Redesign-Proposal.md) | Accepted unified shell specification; implementation in progress | File menu, stable task workspaces, family contributions, Cards/Shelves, modeless Inspector, session/window ownership |
| [`../Desktop-Arena-Design-Doc.md`](../Desktop-Arena-Design-Doc.md) | Primary product design | Safe clone, office-native construction kits, semantic text, live desktop, delivery phases |
| [`DACK-Optimization-and-Refactoring-Plan.md`](DACK-Optimization-and-Refactoring-Plan.md) | Engineering control document | Stabilization, UI shoring, profiling, decomposition, performance gates |
| [`DACK-GUI-Architecture.md`](DACK-GUI-Architecture.md) | Product UI architecture | Play/Build/Understand, tabbed cockpit, shelf, inspector, responsive layout |
| [`DACK-Top-Level-Menu-Plan.md`](DACK-Top-Level-Menu-Plan.md) | Game taxonomy | View/control families, presets, and the Cards/tools they contribute to stable task workspaces |
| [`DACK-Sprite-Studio-Mini-App.md`](DACK-Sprite-Studio-Mini-App.md) | Character/asset authoring UX | Pick, slice, animate, bind, attack, effects, sounds, boxes |
| [`DACK-Brickbat-Builder.md`](DACK-Brickbat-Builder.md) | Brickbat rules/builder contract | Three-ball state, targets, mutation, bonuses, laser, HUD |
| [`DACK-Asset-Audit-and-Sprite-Animator.md`](DACK-Asset-Audit-and-Sprite-Animator.md) | Asset intake and importer plan | Provenance, source-specific import profiles, curated defaults |
| [`DACK-Kenney-All-in-One-Intake.md`](DACK-Kenney-All-in-One-Intake.md) | Kenney bundle intake and priority map | Audio-first curation, non-isometric queue, Sound Card contract, CC0 packaging policy |
| [`DACK-Document-Analysis-Fixture-Matrix.md`](DACK-Document-Analysis-Fixture-Matrix.md) | Screenshot/text/video analysis fixtures | Rectangle, icon, grid, background, OCR, mutation, and temporal test expectations |
| [`DACK-Live-Capture-and-Understanding-Plan.md`](DACK-Live-Capture-and-Understanding-Plan.md) | Live Desktop and scene-understanding plan | Capture contracts, coordinate spaces, incremental updates, geometry/text passes, and safety gates |
| [`DACK-Document-Geometry-and-Pagination-Plan.md`](DACK-Document-Geometry-and-Pagination-Plan.md) | Document-native transforms and page flow | Rotated/slanted text blocks, attached ladders and spawn routes, multi-page Word levels, scroll capture and reconciliation |
| [`DACK-Level-Snapshot-Format.md`](DACK-Level-Snapshot-Format.md) | Level/package contract | Immutable baselines and analyses, authored level data, mutation variants, recovery, safe packs |
| [`DACK-Object-Attribute-Model.md`](DACK-Object-Attribute-Model.md) | Shared object vocabulary | Cards, common attributes, actor/enemy/toolkit extensions |
| [`DACK-Construction-Kit-Inspiration.md`](DACK-Construction-Kit-Inspiration.md) | Reference analysis and design guardrails | SEUCK/ACS lessons, document-native mechanics, definitions vs placed instances |

## Toolkit and Concept Notes

These documents are design inventories. They help identify shared mechanics and stress the core architecture, but their individual features do not outrank the staged roadmap.

| Document | Scope |
| --- | --- |
| [`DACK-Pinball-and-Overhead-Combat-Startup.md`](DACK-Pinball-and-Overhead-Combat-Startup.md) | Pinball physics/parts and first overhead combat family |
| [`DACK-Space-Air-Tank-Artillery-Concepts.md`](DACK-Space-Air-Tank-Artillery-Concepts.md) | Space, air, tank, artillery, harvesting, gravity, and invasion ideas |

## Architecture Decision Records

| ADR | Decision |
| --- | --- |
| [`adr/ADR-0007-live-linked-sprite-pad.md`](adr/ADR-0007-live-linked-sprite-pad.md) | Keep a constrained, live-linked in-context pixel pad |
| [`adr/ADR-0008-captured-text-platforms.md`](adr/ADR-0008-captured-text-platforms.md) | Captured text is baseline platform terrain |
| [`adr/ADR-0009-shared-snapshot-environment-map.md`](adr/ADR-0009-shared-snapshot-environment-map.md) | Generalize captured-page analysis into one stable, indexed environmental model |
| [`adr/ADR-0010-session-preserving-ui-navigation.md`](adr/ADR-0010-session-preserving-ui-navigation.md) | Keep playset/source/mutations intact across Play/Build/Cockpit/Studio/Boss navigation |
| [`adr/ADR-0011-core-adapters-and-session-command-model.md`](adr/ADR-0011-core-adapters-and-session-command-model.md) | Keep domain state and analysis independent of Godot/Windows; mutate one session through commands and versioned transactions |
| [`adr/ADR-0012-snapshot-analysis-clone-state-separation.md`](adr/ADR-0012-snapshot-analysis-clone-state-separation.md) | Give source, baseline, intake recipe, analysis, level, clone, region/run state, variants, and packs distinct ownership |
| [`adr/ADR-0013-tile-backed-native-pixel-clone-rendering.md`](adr/ADR-0013-tile-backed-native-pixel-clone-rendering.md) | Bound mutation/upload/memory cost with a tile-backed 1:1 clone renderer |
| [`adr/ADR-0014-card-definition-instance-and-dependency-contract.md`](adr/ADR-0014-card-definition-instance-and-dependency-contract.md) | Separate immutable/project definitions from placed overrides and make nested Card resolution deterministic |
| [`adr/ADR-0015-simulation-clock-and-physics-authority.md`](adr/ADR-0015-simulation-clock-and-physics-authority.md) | Advance one world on one bounded fixed clock and use Godot-first physics with a measured Pinball rate |

## Documentation Maintenance Rules

- Put enduring product intent in the primary design document.
- Put implementation order, performance findings, and refactoring gates in the optimization plan.
- Put detailed fields and UI behavior in the focused architecture documents.
- Put a feature’s live controls in the RAD README only after the feature is implemented.
- Record exact asset rights and shipping status in `dack/assets/ASSET_PROVENANCE.md`; design notes are not license evidence.
- Prefer one link to the authoritative section over copying the same roadmap into multiple files.
- Preserve useful experiments, but label them **Exploratory** when they are not part of the current release path.
- Update the “prototype baseline” tables after a meaningful milestone rather than appending an unbounded diary of every tweak.
- When a defect named in an ADR or plan is fixed, preserve it as historical rationale and mark the resolution date; do not continue describing it as present tense.
- New architecture claims must state ownership, thread/loop affinity, persistence boundary, invalidation/version rule, and failure behavior—not just a class name.
- Every mouse-only durable action needs a keyboard/menu route in the UI specification. Every new animated or flashing effect needs reduced-motion/strobe behavior.
