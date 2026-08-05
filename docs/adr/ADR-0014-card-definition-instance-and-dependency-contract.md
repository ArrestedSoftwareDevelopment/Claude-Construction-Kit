# ADR-0014: Card Definition, Instance, Override, and Dependency Contract

- **Status:** Accepted
- **Date:** 2026-08-05
- **Decision owners:** DACK project
- **Related:** ADR-0011; `DACK-Object-Attribute-Model.md`; `DACK-UI-Redesign-Proposal.md`; `DACK-Level-Snapshot-Format.md`

## Context

Cards are DACK's reusable construction grammar: players, enemies, projectiles, effects, sounds, AI behaviors, world parts, rules, and complete levels can be composed and reused. The prototype also proves that editing a selected actor directly on the playfield is faster than making a round trip through a separate builder.

Without a strict definition/instance contract, however, a local tweak can unexpectedly change every actor, a built-in asset can be overwritten, a nested Card can form a dependency cycle, or a published level can resolve to different content later.

## Decision

### 1. Three ownership classes

1. **Built-in or third-party Card Definition:** immutable in place. The creator may place it, override an Instance, or use Fork Card to copy it into project ownership.
2. **Project-owned Card Definition:** editable reusable definition with a stable ID and version/revision. Definition edits are explicit authored commands.
3. **Placed Instance:** independently identified placement containing transform/state plus a sparse override patch against one resolved Card Definition.

The selection-driven Inspector edits the placed Instance by default. It never silently enters definition-edit mode.

### 2. Explicit authoring actions

- **Reset to Card** removes selected overrides after previewing the resolved result.
- **Apply to Definition** is available only for a project-owned definition, shows the number and scope of affected instances, validates dependencies, and commits as one undoable transaction.
- **Fork Card** creates a new project-owned definition and optionally rebinds the selected Instance in the same transaction.
- **Open Definition** enters the appropriate owned workspace while preserving the current session and return context.
- **Make Unique** is friendly wording for Fork + rebind where that phrasing better matches the selected object.

Cancel restores the pre-preview value. Slider scrubbing and direct manipulation may preview continuously but commit one command per gesture.

### 3. Typed Slots and stable identity

A composed Card exposes stable component Slot IDs. Each Slot declares:

- accepted Card kinds/interfaces and optional capability constraints;
- cardinality and required/optional policy;
- default binding and parameter surface;
- whether an Instance may override the binding;
- fallback behavior when unresolved.

Slot identity does not derive from display order or label. Renaming or reordering a Slot cannot orphan an override.

### 4. Dependency graph and resolution

- Card dependencies form a directed acyclic graph. The editor rejects a binding that would introduce a direct or transitive cycle and explains the path.
- Authoring may follow a compatible local catalog reference, but a saved release/published pack pins or embeds the exact resolved definition and dependency versions.
- Resolution is deterministic and produces a hashable resolved Card. List order is presentation unless the schema explicitly declares ordered semantics.
- Missing or incompatible dependencies load as visible disabled placeholders that preserve IDs, serialized fields, provenance, and repair actions. They never silently substitute a different Card.
- `CardCatalog` is the canonical definition resolver. Asset and actor-profile repositories are adapters/projections over it, not competing stores of Card truth.

### 5. Invalidation classes

Every property/Slot schema declares the smallest invalidation class required by a committed change:

- presentation only;
- simulation/runtime reconstruction;
- collision/environment re-index;
- asset compile/thumbnail;
- persistence/package validation.

Definition edits invalidate all dependent resolved Cards and affected Instances through the dependency index. They do not trigger source capture or whole-page analysis unless a field explicitly changes accepted source interpretation.

## Consequences

### Positive

- Direct playfield editing remains safe and fast.
- Shared definitions are powerful without producing invisible global edits.
- Cards can nest into actors, levels, and packs while remaining deterministic and testable.
- Published playsets do not drift when a catalog changes.
- Inspector, Shelf, compact picker, Sprite Studio, and persistence share one vocabulary.

### Tradeoffs

- The editor needs affected-instance previews, dependency indexing, cycle diagnostics, version migration, and unresolved placeholders.
- Applying a definition edit is more deliberate than changing an ordinary local property.
- Exact release pins increase package metadata and may duplicate small definitions across packs.

## Validation

1. Editing a selected actor changes only that Instance until Apply to Definition is explicitly chosen.
2. Applying to a project-owned definition reports affected Instances, updates them deterministically, and undoes as one transaction.
3. Built-in/third-party definitions cannot be overwritten; Fork Card creates a project-owned result with provenance.
4. A cycle attempt is rejected with the complete dependency path and leaves the project unchanged.
5. Renaming/reordering Slots preserves overrides by stable Slot ID.
6. A missing pinned Card loads as a repairable placeholder and round-trips without data loss.
7. A published pack resolves to the same Card graph and hashes on a clean machine.
