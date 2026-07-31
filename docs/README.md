# DACK Documentation Map

This folder separates product decisions, implementation plans, data contracts, and idea banks so the prototype can grow without every note becoming an equal-priority promise.

## Authority Order

When two documents disagree, use this order:

1. [`../Desktop-Arena-Design-Doc.md`](../Desktop-Arena-Design-Doc.md) — product intent, safety rules, core architecture, and release phases.
2. [`DACK-Optimization-and-Refactoring-Plan.md`](DACK-Optimization-and-Refactoring-Plan.md) — current engineering sequence, performance budgets, and refactoring gates.
3. The focused architecture/data documents below — detailed UX and serialized-model decisions.
4. Toolkit and genre concept notes — design inventory and experiments, not delivery commitments.
5. [`../dack/README.md`](../dack/README.md) — how to run and test the current RAD build.

An ADR overrides older prose for the narrow decision it records. A later ADR should explicitly supersede an earlier one rather than silently contradict it.

## Status Language

Every substantial future documentation pass should use these labels consistently:

- **Implemented:** present in the current prototype and expected to run.
- **Proven:** demonstrated in the RAD, but not yet productized or generalized.
- **In progress:** actively being consolidated or completed.
- **Planned:** accepted direction with a place in the roadmap.
- **Exploratory:** a useful idea that has not earned a delivery commitment.
- **Deferred:** intentionally outside the current phase.

“Current” should be paired with a date or milestone when it might become stale. “Next” should appear primarily in the optimization/refactoring plan rather than being repeated differently across several files.

## Core Documents

| Document | Role | Current emphasis |
| --- | --- | --- |
| [`../Desktop-Arena-Design-Doc.md`](../Desktop-Arena-Design-Doc.md) | Primary product design | Safe clone, office-native construction kits, semantic text, live desktop, delivery phases |
| [`DACK-Optimization-and-Refactoring-Plan.md`](DACK-Optimization-and-Refactoring-Plan.md) | Engineering control document | Stabilization, UI shoring, profiling, decomposition, performance gates |
| [`DACK-GUI-Architecture.md`](DACK-GUI-Architecture.md) | Product UI architecture | Play/Build/Understand, tabbed cockpit, shelf, inspector, responsive layout |
| [`DACK-Top-Level-Menu-Plan.md`](DACK-Top-Level-Menu-Plan.md) | Information architecture | View/control families, presets, contextual pages |
| [`DACK-Sprite-Studio-Mini-App.md`](DACK-Sprite-Studio-Mini-App.md) | Character/asset authoring UX | Pick, slice, animate, bind, attack, effects, sounds, boxes |
| [`DACK-Brickbat-Builder.md`](DACK-Brickbat-Builder.md) | Brickbat rules/builder contract | Three-ball state, targets, mutation, bonuses, laser, HUD |
| [`DACK-Asset-Audit-and-Sprite-Animator.md`](DACK-Asset-Audit-and-Sprite-Animator.md) | Asset intake and importer plan | Provenance, source-specific import profiles, curated defaults |
| [`DACK-Level-Snapshot-Format.md`](DACK-Level-Snapshot-Format.md) | Level/package contract | Immutable source, Snapshot cache, placed objects, mutation variants |
| [`DACK-Object-Attribute-Model.md`](DACK-Object-Attribute-Model.md) | Shared object vocabulary | Cards, common attributes, actor/enemy/toolkit extensions |

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

## Documentation Maintenance Rules

- Put enduring product intent in the primary design document.
- Put implementation order, performance findings, and refactoring gates in the optimization plan.
- Put detailed fields and UI behavior in the focused architecture documents.
- Put a feature’s live controls in the RAD README only after the feature is implemented.
- Record exact asset rights and shipping status in `dack/assets/ASSET_PROVENANCE.md`; design notes are not license evidence.
- Prefer one link to the authoritative section over copying the same roadmap into multiple files.
- Preserve useful experiments, but label them **Exploratory** when they are not part of the current release path.
- Update the “prototype baseline” tables after a meaningful milestone rather than appending an unbounded diary of every tweak.
