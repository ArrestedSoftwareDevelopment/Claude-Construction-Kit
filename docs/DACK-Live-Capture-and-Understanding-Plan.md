# DACK Live Capture and Scene Understanding Plan

**Status:** Active architecture; provisional geometry profiler implemented; manual-refresh source policy is normative; reviewed 2026-08-05
**Authority:** Companion to [ADR-0009](adr/ADR-0009-shared-snapshot-environment-map.md), [ADR-0012](adr/ADR-0012-snapshot-analysis-clone-state-separation.md), and [ADR-0013](adr/ADR-0013-tile-backed-native-pixel-clone-rendering.md)
**Scope:** Windows desktop, monitor, window, region, Snapshot, and incremental scene analysis

DACK's live-desktop promise is not “put sprites on a screenshot.” It is:

> capture a safe, native-resolution view of a real desktop or document; understand enough of its structure to make it playable; preserve that understanding as a reusable Snapshot; and update only what actually changed.

The capture and understanding services therefore form the substrate for every playset. Platformer, Brickbat, Pinball, RPG, Snake/Maze, and Live Desktop Mode must query the same source geometry, text records, icons, windows, whitespace, and mutation state.

## 1. Source contract

All source kinds implement one session-facing contract:

```text
ISourceProvider
  Describe() -> SourceDescriptor
  Capabilities -> SourceCapabilities
  CaptureFullAsync(request, cancellation) -> SourceFrame
  RequestRefreshAsync(activeBaseline, cancellation) -> RefreshCandidate
  CaptureChangesAsync(previousSequence, cancellation) -> CapturedDelta  # only when declared
  StopAsync() -> void
```

`SourceCapabilities` declares full capture, incremental/delta capture, cursor include/exclude, UIA evidence, occlusion behavior, minimum-update cadence, permission/consent model, and window/monitor/region support. Callers never infer that every provider can perform the same operation. Admission of a `SourceFrame` into an immutable `SnapshotBaseline` is an Application/Snapshot-repository use case, not a `Freeze()` side effect owned by the provider.

Initial providers:

- `ImageSource` — an imported image or existing framegrab.
- `TextGridSource` — fixed-width ASCII/ANSI cells with native row/column geometry.
- `MonitorSource` — a selected physical monitor at native capture resolution.
- `WindowSource` — a selected top-level window and its client/non-client bounds.
- `RegionSource` — a creator-selected rectangle within a monitor or window.
- `LiveDesktopSource` — a read-only stream composed from a monitor/window provider.

The provider never edits the desktop, sends keystrokes, closes windows, or writes to a document. Input routing is a separate, opt-in service owned by Play/Build mode. Boss Key teardown must be able to stop or hide every provider immediately.

### 1.1 Capture metadata

Every captured frame records:

- source kind and stable local source ID;
- monitor, window, or region identity when available;
- physical pixel size and capture rectangle;
- Windows DPI scale, desktop coordinate origin, orientation, and color format;
- graphics-adapter/output identity, including negative desktop coordinates and independent monitor rotation;
- channel order, bit depth, alpha/premultiplication, SDR/HDR/transfer-function/color-profile metadata, and the normalization profile used for analysis;
- capture timestamp, monotonic sequence, and content hash;
- cursor policy (`excluded`, `included`, or `unknown`);
- occlusion/minimization/permission state;
- parent window and child-region hints from Windows APIs when available;
- source privacy/local-only state and provenance note.

The pixel buffer remains the authority for visual play. Window metadata and UI Automation (UIA) text are evidence layers, not substitutes for the captured image.

Analysis uses one documented CPU-normalized pixel contract (initially 8-bit premultiplied/unpremultiplied policy made explicit, RGBA channel order, sRGB/SDR working space). The original capture metadata is retained. HDR/wide-gamut inputs require an explicit previewable tone/color normalization rather than silent clipping; a future high-fidelity path may preserve more precision, but collision/erasure/background restoration must use the same normalized pixels the creator approved.

### 1.2 Coordinate systems

DACK must never pass an unqualified `Vector2` between capture, analysis, editor, and gameplay. Every point/rectangle declares its space:

| Space | Meaning |
| --- | --- |
| `DesktopPx` | Physical Windows desktop coordinates across monitors |
| `SourcePx` | Native pixels in the captured monitor/window/region |
| `SnapshotPx` | Frozen native pixels in a Snapshot; normally identical to `SourcePx` |
| `PlayfieldPx` | Runtime coordinates over the Snapshot |
| `EditorUi` | Cockpit/Inspector/Sprite Studio layout coordinates |
| `TextUnits` | Creator-facing units derived from recorded font/cell metrics |

Transforms must preserve the source-to-Snapshot mapping. If a creator intentionally resizes or crops, DACK creates a new Snapshot transform and records the rebinding; it does not silently stretch a previous level.

## 2. Capture lifecycle

### 2.1 Select and preview

1. Show a monitor/window/region picker with a live thumbnail and exact pixel dimensions.
2. Mark likely sensitive regions and explain that visible content may remain even after metadata scrubbing.
3. Preview cursor policy, occlusion behavior, and the proposed crop before capture.
4. Capture a full native-resolution frame and create a temporary immutable working clone.

The user should see the source at 1:1 before choosing a playset. The playset does not determine the capture crop.

### 2.1.1 Intake Workbench: human-assisted preprocessing

After native capture and before Snapshot publication, DACK opens an optional **Intake Workbench**. It lets the creator contribute visual knowledge without destructively painting the source or replacing automatic analysis.

The Workbench provides independent guide layers:

- `None`, adjustable rectangular grid, and adjustable hex grid;
- rectangular, polygonal, and freeform regions;
- draggable dividers, edge segments, and later path/curve guides;
- foreground/text, background, object, and ignore/exclusion seed points;
- roles such as document background, text block, solid geometry, UI chrome, playable area, protected region, destructible region, HUD-safe whitespace, and separate page/level;
- commands such as `treat as one object`, `split along this edge`, `repeat this rule by cell`, and `ignore this toolbar`.

A rectangular grid records origin, cell width/height, rows/columns, rotation, and snapping. A hex grid records origin, radius, pointy/flat orientation, rotation, and bounds. These are guides and optional gameplay topology; they never resample or overwrite Snapshot pixels. Ordinary UI must not be forced into a grid when `None` is the more truthful interpretation.

Workbench authorship lives in an independent, versioned `IntakeRecipe`, because guides and seeds may exist before any detector region exists. Each guide has a stable ID, Snapshot binding/transform, geometry, role/label, authority, accepted state, and undo history. Creator evidence outranks detector guesses, survives Save/Load, and can be mapped/reviewed during an explicit source refresh.

Dragging a guide draws immediate geometry at editor frame rate. Cheap approximate feedback may update continuously, but expensive segmentation/analysis is debounced, cancelable, and limited to affected tiles/regions; pointer motion does not launch an unbounded detector job. A candidate Analysis revision previews masks, collision, words, objects, and background restoration, and becomes active only through an explicit Accept command.

An **analysis guide grid** helps intake; a **gameplay topology grid** is authored level geometry. One may seed the other through an explicit command, but they are not automatically the same object or authority.

The intended intake sequence is:

```text
Capture -> native-pixel calibration -> optional grid -> regions/edges/seeds
        -> live analysis preview -> creator correction -> lazy OCR enrichment
        -> admit SnapshotBaseline + IntakeRecipe + selected AnalysisRevision
        -> derive PlayfieldProfile
```

### 2.2 Snapshot admission and level ownership

`Capture Snapshot` admits an immutable `SnapshotBaseline` containing:

- the approved native image buffer and content hash;
- capture metadata, color normalization, transforms, and source provenance;
- a stable Snapshot ID and creation/admission record.

The level separately owns the selected `IntakeRecipe` and `AnalysisRevision` references, accepted creator corrections/source bindings, Cards/instances, rules, routes, presentation policy, and mutation policy. The Working Clone, Region Runtime State, and Run State remain separate mutable products. Completed OCR/UIA labels are versioned enrichment cache records bound to Analysis region IDs, not in-place changes to the immutable baseline.

The original desktop, file, window, and document remain outside DACK's write boundary. A later recapture creates a new candidate baseline and an explicit region/guide lineage proposal; it never mutates the old baseline or silently reuses derived IDs.

### 2.3 Continuous live (future opt-in)

Live Desktop begins with one full frame, then uses bounded incremental work:

1. Acquire a new frame or OS-reported dirty rectangles.
2. Tile/hash the source buffer and coalesce adjacent changed tiles.
3. Expand each dirty rectangle by detector-specific padding (anti-alias, glyph, border, and shadow margins).
4. Re-run background/geometry/text analysis only inside the expanded regions.
5. Reconcile new candidates against stable region IDs using role, bounds, overlap, and local evidence.
6. Publish one coherent frame/Analysis/environment transaction to gameplay and UI; never show new pixels with old collision as if they agree.
7. Cancel or discard OCR work tied to an obsolete frame sequence.

No capture, analysis, OCR, or texture upload may block input or the gameplay frame loop. If the source changes faster than analysis can keep up, DACK shows the last coherent analysis and marks the source `updating` rather than mixing partial worlds.

### 2.4 Manual-refresh source policy (default)

The default desktop/document workflow is **capture once, edit many, refresh explicitly**. DACK does not poll the source for gameplay, run dirty-region analysis because a window moved, or silently replace the playfield while the creator is working.

The session states are:

```text
Uncaptured -> Previewing -> Captured -> SnapshotReady -> Editing/Playing
                                      \-> RefreshPending -> DiffReady
                                      \-> ApplyRefresh/Rebind -> SnapshotReady
                                      \-> DiscardRefresh -> Editing/Playing
```

Initial capture performs the expensive work once: acquire a full native-resolution `SourceFrame`, admit an immutable `SnapshotBaseline`, create a DACK-owned tiled `WorkingClone`, run the versioned `IntakeRecipe`/`AnalysisRevision` pipeline, build one indexed `EnvironmentMap` view, and publish a coherent level revision every playset can query.

While in `Editing` or `Playing`, the source frame, analysis, OCR cache, and region IDs remain stable. Runtime mutations affect only the clone. Lazy OCR may continue against the current Snapshot, but it is not a source refresh and never triggers a full-image rescan.

If DACK can cheaply observe that the selected window moved, resized, closed, or changed, it may show a non-invasive `Source changed - Refresh available` badge. This advisory must not capture pixels, invalidate gameplay, or alter the clone.

`Refresh Source` is a deliberate creator command in the shared File/Source command surface, with review in Understand/Intake. It pauses Play if necessary, retains the current clone, captures a temporary `RefreshCandidate`, analyzes it off the active game path, and shows a diff of added, moved, deleted, and uncertain regions. The creator chooses `Apply as New Snapshot`, `Rebind and Apply`, or `Discard`.

Applying creates a new Snapshot/version while preserving the prior Snapshot and mutation history. Only source bindings with confident matches migrate automatically; uncertain ladders, routes, actors, and goals are flagged for review. Candidate OCR and analysis jobs carry the candidate source version and are discarded if the candidate is rejected or superseded.

### 2.5 Source policy choices

The creator chooses a source policy per playset:

- **Snapshot:** capture once; no refresh until the creator requests it.
- **Manual-refresh desktop:** initial capture plus explicit `Refresh Source`; this is the default for desktop/document play.
- **Continuous live:** optional future experiment using the incremental path above; it is not the default authoring or shipping path.
- **Solid-until-clear:** a disappearing source region remains physical until a confirmed replacement frame arrives.

Transient menus, tooltips, selection rectangles, cursor flashes, and ordinary document edits must not rewrite a Snapshot. They become part of a new candidate only after `Refresh Source` and creator approval.

### 2.6 Technical pipeline and ownership

The implementation should keep capture, understanding, editing, and play as separate services with explicit ownership:

```text
Windows capture backend
  -> immutable SourceFrame (BGRA/RGBA pixels + metadata + hash)
  -> Snapshot repository -> immutable SnapshotBaseline
  -> IntakeRecipe + Analysis service -> immutable AnalysisRevision
  -> EnvironmentMap index/view + RegionRuntimeState
  -> tile-backed WorkingClone (mutable pixels + mutation log)
  -> toolkit queries (collision, text, icons, whitespace, routes)
  -> simulation and renderer
```

The initial and refresh capture paths may use Desktop Duplication/DXGI for monitors and Windows.Graphics.Capture for user-consented windows/displays, with an explicit post-acquisition crop for creator-selected regions and a reviewed fallback for inaccessible surfaces. The backend owns native pixel acquisition and DPI/desktop transforms; it does not know about enemies, OCR words, or gameplay.

The Snapshot repository copies or references the captured buffer immutably, records the source hash and capture metadata, and assigns a Snapshot identity. The analysis service consumes baseline + Intake Recipe + algorithm versions and emits immutable derived records. The Working Clone is a separate tiled writable layer: Brickbat erasure, projectile damage, paragraph deformation, and creator visual mutations never write into `SourceFrame`, `SnapshotBaseline`, or `AnalysisRevision`. Shadows and ANSI underlays are normally composited presentation layers rather than baked clone mutations unless the creator explicitly promotes them.

Gameplay reads the cached Environment Map through spatial queries and subscribes to committed clone/Region Runtime State mutation events. It must never call the capture backend or run a whole-image detector during a frame. `RefreshCandidate` follows the same pipeline off to the side and becomes visible only after an Apply/Rebind command commits a new baseline/analysis/lineage transaction.

### 2.7 Windows capture backend policy

Use the platform APIs by capability, not by a false single-backend abstraction:

| Need | Preferred first path | Important behavior to preserve |
| --- | --- | --- |
| User-consented window/display selection | `Windows.Graphics.Capture` | system picker/consent, capture border where applicable, resize/close/device loss, cursor policy |
| Per-monitor high-rate/delta evidence | DXGI Desktop Duplication | output/adapter identity, rotation, move rectangles before dirty rectangles, cursor metadata, access loss |
| Static image/text/ANSI source | reviewed core decoder/parser | deterministic pixels/cells, no executable content, explicit limits |

Manual Snapshot capture may use GPU capture → CPU-normalized buffer → tiled Godot textures. Continuous live work must measure that path, dirty-tile CPU upload, and residency costs before considering shared native textures or GDExtension. Native code is justified by a measured transfer/API bottleneck, not by architectural fashion.

DACK-owned windows should be excluded from DACK capture where supported to prevent recursive feedback, and the picker/preview must reveal when exclusion is unavailable. Window display-affinity/exclusion flags are defense-in-depth presentation controls, not DRM or a guarantee against other capture software. Secure desktop/UAC, protected content, inaccessible/minimized surfaces, target close/replacement, access loss, and device removal fall back to the last coherent baseline and a clear recovery action.

## 3. Analysis pipeline

The first pass produces one immutable, versioned `AnalysisRevision` with independently inspectable layers. Each layer can add evidence to a shared derived region record; none creates a competing geometry model. Runtime active/deleted/damaged state is resolved separately through Region Runtime State.

### Pass A — normalize and tile

- Convert to a managed RGBA/luminance/chroma buffer.
- Preserve the admitted Snapshot Baseline pixels byte-stably and separately from the mutable Working Clone; this does not grant access to or package the originating file.
- Build multiscale tiles for dominant color, variance, edge density, and entropy.
- Record local background candidates per tile and per connected surface, not one global “white.”

### Pass B — background and whitespace

Background detection is a field, not a color constant:

- estimate dominant colors in tiles and connected surfaces;
- model gradients and theme bands (light page, dark editor, ANSI field, desktop wallpaper);
- classify ink by local contrast and chroma distance from that field;
- preserve large low-entropy regions as whitespace candidates;
- expose background confidence and alternate local regions in Understand mode.

This is the foundation for native legibility, score placement, text erasure, shadows, and “fall through the document” behavior.

### Pass C — structural geometry

Find rectangles and boundaries in stages:

1. accumulate horizontal/vertical edge evidence from luminance and chroma changes;
2. join collinear runs with gap tolerance;
3. propose rectangles from aligned borders, fills, padding, and repeated spacing;
4. classify parent/child nesting and z-order;
5. reconcile against OS window/UIA bounds when they exist;
6. retain uncertain candidates as evidence instead of forcing a gameplay object.

Candidate structural roles include desktop regions, windows, title bars, menus, toolbars, tabs, panels, tables, chart areas, cards, gutters, scrollbars, buttons, input fields, and document columns.

### Pass D — visual components

Connected components and repeated layout patterns identify icons, shortcut arrows, badges, pillboxes, legend swatches, chart bars, thumbnails, and decorative objects.

Each candidate stores:

- bounds and optional pixel mask;
- component area, aspect ratio, fill/edge statistics, and repetition group;
- nearby label candidates and alignment score;
- role hypotheses and confidence;
- whether it is safe to promote into a gameplay object.

Shortcut arrows, caret marks, scroll thumbs, and tiny anti-alias fragments remain sub-components unless the creator promotes them.

### Pass E — text geometry

Text detection must work before OCR and must not assume black ink:

1. classify ink against the local background using luminance, chroma, and neighborhood contrast;
2. run small morphological cleanup that preserves thin stems and punctuation;
3. derive glyph/component boxes from connected ink with anti-alias tolerance;
4. group glyphs by baseline, height, gap, and line spacing;
5. group lines into words/runs, paragraphs, headings, labels, and fixed-width cells;
6. retain exact masks and adjacent-pixel color sets for mutation/erasure;
7. assign `textRole` and confidence without requiring a recognized word.

The detector must recognize light subheaders, colored text, anti-aliased text, disabled UI labels, and partially erased text. A word record may exist with `recognizedText = null`; fallback labels such as `WORD-042` keep gameplay functional when OCR is disabled.

### Pass F — grids and paths

Grid inference is conservative:

- detect repeated row/column spacing in tables, spreadsheets, pixel editors, and fixed-width text;
- save a rectangular grid only when spacing and alignment confidence are high;
- preserve ASCII/ANSI cell geometry exactly from its parser;
- let creators add rectangular/hex grids, curves, parabolas, and route overlays explicitly;
- never hallucinate a hex grid from ordinary UI rectangles.

Grid cells, path points, and curves become source-bound geometry only after the creator accepts or paints them.

### Pass G — OCR and UIA enrichment

OCR is a lazy naming service, not the geometry engine:

- queue candidates nearest the active ball, player, projectile ray, goal, or selected word region;
- prefer headings, labels, and likely bonus words when they are gameplay-relevant;
- use UIA/native labels when the provider exposes them, binding them to existing region IDs;
- cancel stale jobs when the source hash or region version changes;
- retain fallback IDs and geometry when OCR is unavailable or disabled.

### Pass H — playfield profiling and recommendations

The interpreter produces a **PlayfieldProfile** after geometry analysis. This is an affordance vector, not a single app/file label. Initial dimensions include text density, horizontal continuity, vertical connectivity, grid regularity, open-space ratio, background confidence, destructibility, object repetition, corridor/path evidence, and HUD-safe space. Later rectangle, icon, UIA, and OCR evidence enriches the same profile.

Each data-driven `ToolkitDescriptor` declares preferred affordances, negative evidence/contraindications, prerequisites, minimum confidence, suggested construction recipes, and explanation templates. The service reports **compatibility score** separately from **evidence confidence**; neither is presented as a calibrated probability. It may truthfully report “No strong recommendation.” Source-app hints are weak evidence, never authority.

DACK ranks several plausible game types, explains the strongest evidence, and identifies what must be added—for example ladders between text rows, a pinball table boundary, a tower-defense route, or racing checkpoints. Recommendations use plain labels such as **Strong fit**, **Good fit**, and **Experimental**. `Preview` creates a reversible noncommitted draft; `Apply` is the explicit command that selects a family or creates starter construction. Merely visiting Understand, switching task workspace, or selecting a family/preset never changes the level.

The first implemented profiler is deliberately provisional. It derives geometry-only recommendations from the existing text platforms, glyph/word/line candidates, background coherence, and repeated target sizes. It currently ranks Side-View Platformer, Brickbat, Pinball, Maze/Snake, Tower Defense/Escort, and Racing/Route, exposes the best three in Understand mode, and does not require OCR. Its percentages are uncalibrated compatibility scores. Rectangle/icon hierarchy, calibrated guide evidence, negative evidence, and source-family hints will replace weak heuristics as those passes come online; profiler feature/schema version is part of the cache key and diagnostics.

## 4. Shared region model

Every region record should be able to answer the same questions for every playset:

```text
RegionRecord
  id, parentId, snapshotId, analysisRevision
  bounds, mask, zOrder
  kind: background | window | panel | icon | glyph | word | line | paragraph | gridCell | path | control | unknown
  evidence: pixels | edges | layout | UIA | OCR | creator
  confidence, localBackground, palette
  recognizedText?, textRole?, glyph/cell metrics?
  proposed traversal, collision, mutation, presentation policies
  evidence provenance
```

Accepted creator corrections/source bindings live in the Level Definition and reference region IDs. `RegionRuntimeState(regionId)` carries active/deleted/damaged/current-presentation values for the current Working Clone/Variant. This keeps detector evidence immutable while allowing every playset to observe the same current world.

The resolved environment service exposes:

- a spatial index by `SourcePx`/`SnapshotPx`;
- accepted hierarchy and stable-ID lineage/reconciliation results;
- resolved active/deleted/mutated region state through the current Region Runtime State;
- source-to-playfield transforms;
- nearest whitespace/background queries for HUD and effects;
- evidence and correction history for Understand mode.

The mutation service and tile-backed Clone Renderer own dirty pixel updates/uploads and publish the committed region-state delta back to this resolved view; the Environmental Map does not rewrite clone pixels itself.

Collision, erasure, scoring, OCR targeting, shadow grounding, and save/load all query this shared model.

## 5. Understand mode

Before Play, the creator can inspect the analysis without opening a debugging tool:

- toggle background, rectangle, icon, text, grid, and path layers;
- click a region to see its evidence, local background, confidence, ID, and proposed role;
- accept, reject, merge, split, recolor, or rebind a region;
- preview the exact mutation mask that Brickbat, Pinball, or a projectile would use;
- see which pixels are source-derived, creator-authored, or runtime-mutated;
- view capture age, frame sequence, analysis version, and stale OCR state.

Manual creator decisions outrank automatic detection and survive Snapshot save/load.

## 6. Windows-first implementation sequence

### Spike LC-0 — deterministic capture

- Capture a selected monitor, window, and region into a native-resolution `SnapshotImageSource`.
- Record DPI, desktop/source transforms, cursor policy, timestamps, hashes, and provenance.
- Show a 1:1 preview and create a clone without touching the original.

### Spike LC-1 — analysis review surface

- Feed existing screenshot fixtures through the shared analysis product.
- Draw rectangles, text masks, icons, backgrounds, and whitespace in Understand mode.
- Save a golden record and a versioned analysis cache.
- Expose the Intake Workbench guide layers and persist creator corrections.
- Show a PlayfieldProfile with ranked recommendations, reasons, and required additions.

### Spike LC-2 — incremental live source

- Compare a sequence of monitor/window frames.
- Tile/hash changed regions, debounce transient UI, reconcile stable IDs, and publish dirty-region transactions.
- Prove that stale OCR cannot overwrite a newer frame.

### Spike LC-3 — gameplay binding

- Connect Platformer text terrain, Brickbat targets, Pinball plow regions, and HUD whitespace queries to the same environment map.
- Freeze on Play and restore Build state with `F6`.
- Exercise Boss Key and two-monitor focus ownership.

## 7. Acceptance gates

The design is ready to move beyond the spike when:

1. A captured monitor/window/region can be frozen at native resolution with a complete, clone-only provenance record.
2. The same analysis exposes background, rectangles, icons, text geometry, whitespace, and optional grid candidates without separate toolkit scans.
3. Colored/light/anti-aliased text and mixed backgrounds remain discoverable.
4. Changed live regions update incrementally while stable IDs and mutations survive.
5. OCR can be disabled without disabling collision, erasure, scoring, save/load, or fallback text.
6. Understand mode makes false positives correctable by a creator.
7. Every playset can identify which source regions supplied its terrain, targets, routes, or bonuses.
8. Ten minutes of idle Editing/Playing produces no source capture, full-image analysis, or geometry invalidation.
9. A source change outside DACK produces at most a non-invasive advisory until the creator selects `Refresh Source`.
10. A refresh candidate can be previewed, applied as a new Snapshot with confident rebinding, or discarded without changing the active clone.
11. No action can modify the real desktop, source file, or original document.
12. A creator can correct intake with grids, regions, edges, and seed points without altering native pixels.
13. Recommendations explain their evidence, remain optional, and continue to function with OCR disabled.

## 8. Performance and safety guardrails

- Initial capture and first analysis are asynchronous and show progress; no modal wait on the creator's input.
- Future continuous-live analysis is bounded by changed-area budgets and drops decorative work before gameplay-critical collision or input; it is not part of the default manual-refresh path.
- Maximum capture dimensions, tile counts, region counts, OCR queue depth, and parser time are explicit settings.
- Malformed ANSI/control streams, unstable windows, inaccessible surfaces, and permission failures degrade to the last coherent Snapshot.
- Local-only is the default. Publishing requires clone preview, visible-content warning, provenance, and always-on metadata scrubbing.
- Refresh candidates use separate memory/temporary storage and are never published to gameplay until the creator applies them.

### 8.1 Job and publication contract

- One bounded scheduler owns priority lanes for capture, interaction-critical regional analysis, OCR, thumbnails, and asset compilation. Save staging has its own durability priority.
- Jobs deduplicate by cache key and carry session ID, baseline/candidate ID, recipe revision, input hash, algorithm/provider version, and cancellation token.
- Workers receive immutable buffers/records only and never access Godot scene/UI objects. Results enter a bounded main-thread commit queue.
- The commit boundary rejects stale identity/version and publishes coherent pixels + environment revision together. There is no interval where a new live frame pretends to match old collision.
- Queue depth, age, cancellations, stale drops, cache hit rate, analysis tiles, dirty/upload tiles, bytes owned, and publication latency appear in diagnostics/Activity Center.
- Boss/Return to Desktop immediately parks providers and suspends nonessential work without synchronously waiting for every worker to unwind.

### 8.2 Native-pixel and memory contract

The clone renderer follows [ADR-0013](adr/ADR-0013-tile-backed-native-pixel-clone-rendering.md): immutable and mutable tiles share Snapshot coordinates; dirty mutation tiles bound CPU/GPU uploads; only visible/preloaded tiles require GPU residency. A 4K RGBA buffer is roughly 32 MiB before clones, staging, analysis maps, masks, and textures, so diagnostics must name every full-size buffer and enforce the optimization plan's 1080p/4K memory guardrails.

At authoritative 1:1 view there is no interpolation, resampling, or unrecorded color conversion. Fit/Overview remains available as a clearly labeled view transform with pan/minimap support; it never rewrites Snapshot coordinates or claims pixel-authoritative inspection. Golden fixtures verify channel order/color normalization, source-to-display coordinate round-trips, half-open rectangle boundaries, collision/erasure mask identity, background restoration, and seam-free tiles.

### 8.3 Initial performance gates

- 1080p: capture preview within 1 s, provisional geometry/profile within 750 ms, coherent non-OCR analysis within 2 s on the recorded baseline.
- 4K/mixed DPI: coherent non-OCR analysis within 5 s with bounded buffers and progressive feedback.
- Workbench guide dragging: visible geometry under 50 ms; expensive analysis debounced and cancelable.
- Manual-refresh idle: no capture or whole-image analysis, and background CPU settles to the agreed quiet-office budget.
- Continuous-live experiments declare visual capture cadence separately from semantic analysis cadence and keep the last coherent environment when they fall behind.
