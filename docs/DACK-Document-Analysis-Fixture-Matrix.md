# DACK Document-Analysis Fixture Matrix

**Status:** Active test-planning fixture set  
**Baseline:** August 2026  
**Source folder:** `raw base assets/Test Screenshots and Text files/`  
**Authority:** This document defines the named source fixtures and expected analysis behaviors. The implementation sequence and performance gates remain in [DACK Optimization and Refactoring Plan](DACK-Optimization-and-Refactoring-Plan.md).

## Why These Fixtures Matter

The folder is a compact model of DACK's actual source problem. It contains sparse desktop icons, nested application chrome, spreadsheet grids, charts, colored headings, anti-aliased text, empty regions, fixed-width ASCII art, and a moving browser capture. A detector that performs well only on dark paragraph text is not sufficient for the construction kit.

These files are local development fixtures. They are not automatically public-build or hub-export content. Their purpose is to make rectangle discovery, icon discovery, background estimation, grid mapping, text geometry, OCR prioritization, and temporal resampling reproducible.

## Fixture Inventory

| Fixture | Source form | Primary analysis jobs | Expected DACK interpretation |
| --- | --- | --- | --- |
| `Desktop.png` | Sparse desktop screenshot with a large uniform orange field, a few icons, labels, and shortcut arrows | Background-region detection, icon/label pairing, tiny-object discovery, whitespace/HUD placement | One dominant background zone; each icon is a candidate object with a nearby text label; empty space is nonphysical and suitable for HUD placement |
| `Codex.png` | Dense Git client window with menu/ribbon, tabs, lists, diff panes, colored additions/deletions, and nested rectangles | Window/panel hierarchy, toolbar/icon discovery, colored text, selected state, divider/scrollbar detection | Outer window, header, navigation, content panes, diff rows, and controls are separate regions; colored diff text is still text/ink, not background |
| `Gantt Chart.png` | Excel window with ribbon, project table, month/day grid, colored bars, legend, headings, and large blank margins | Rectangular/grid discovery, chart bars, table headers, UI chrome, color-role classification, empty-space scoring | Spreadsheet cells and timeline bands become candidate grid cells/routes; bars are colored objects; ribbon and worksheet remain separate layers |
| `Spreadsheet Data.png` | Stylized loan calculator with merged colored headers, form rows, chart, legend, and a payment table | Merged-cell rectangles, anti-aliased colored text, chart/object bounds, table row/column inference | Header bands, form fields, chart, legend, and table are distinct regions; light text is discoverable without a true-black assumption |
| `Raw Spreadsheet.png` | Mostly empty Excel worksheet with faint gridlines, toolbar, formula bar, and large blank cell area | Faint-line/grid detection, UI/icon discovery, empty-region classification, native coordinate mapping | Worksheet grid is available as an optional rectangular overlay; blank cells remain traversable/empty unless the creator promotes them |
| `Dragon1.txt` | Large fixed-width ASCII/ANSI-style dragon art | Text-grid parsing, glyph occupancy, monospaced cell size, opaque underlay rendering | Preserve rows/columns and whitespace; allow glyphs to become walls, terrain, or decorative underlay without OCR |
| `DragonBallzLogo.txt` | Compact fixed-width ASCII logo | Small text-grid parsing, connected glyph groups, title/logo classification | Treat as a glyph block/logo object with cell bounds; do not confuse it with a paragraph or a sequence of gameplay letters |
| `KingDiamongANSIFile.ANS` | Classic ANSI art: ESC/CSI color and cursor controls, extended IBM-PC glyph bytes, CRLF rows, and a `SAUCE00` metadata record | ANSI parser, CP437/extended-glyph decoding, 132-column layout, color-state tracking, cursor movement, SAUCE metadata, opaque underlay rendering | Preserve control-driven colors and cell positions; expose the art as a fixed terminal canvas/backglass layer while retaining optional per-cell inspection; never treat escape sequences as gameplay text |
| `ChromeAnim.mp4` | Animated browser/UI capture | Temporal source updates, stable-region tracking, frame deduplication, OCR queue cancellation, geometry diffs | Track persistent window/UI regions across frames; analyze changed regions only; cancel stale OCR when the source frame changes |

## Required Detection Layers

Each fixture should produce one cached analysis product with these independently inspectable layers:

1. **Source and background:** native pixel bounds, monitor/DPI metadata when known, dominant/local background zones, gradients, and background confidence.
2. **Structural rectangles:** windows, panels, toolbars, tables, charts, merged bands, cells, gutters, margins, and scrollbars. Nested rectangles must retain parent/child relationships and z-order.
3. **Visual objects:** icons, shortcut arrows, buttons, badges, legend swatches, chart bars, thumbnails, and other compact non-text components. Each candidate needs bounds, role confidence, and a stable ID.
4. **Text geometry:** glyph, word, line, heading, paragraph, label, and fixed-width cell candidates. Detection must include colored, light, anti-aliased, and partially erased text.
5. **Grid model:** inferred rectangular cells for spreadsheets and optional fixed-width cells for ASCII. Hex grids are creator-generated overlays, not something the screenshot detector should hallucinate from ordinary UI.
6. **Semantic meaning:** optional local OCR/UIA labels bound to existing region IDs. OCR can name a region; it must not create a second incompatible geometry model.
7. **Mutation/collision masks:** the exact active mask used by gameplay. Erasure, scoring, collision, icon interaction, and background replacement must query the same region identity.

## Fixture-Specific Acceptance Checks

### Desktop and sparse-icon checks

- The orange field is recognized as background across the page, not as thousands of tiny empty objects.
- Recycle Bin, Git Bash, HandBrake, DaVinci Resolve, and O+ Connect are separate icon candidates.
- Shortcut arrows do not become independent gameplay objects by default, but remain available as a sub-component for icon classification.
- Labels are associated with their nearest icon only when alignment and distance support that relationship.
- HUD placement prefers the large blank area without changing source resolution.

### Application and spreadsheet checks

- Outer window chrome is not merged with the document/workbook canvas.
- Toolbar buttons, tabs, scrollbars, and chart legends are icon/control candidates, not paragraph text.
- Gridlines can be faint and still form a rectangular grid model without becoming solid terrain.
- Merged header bands, chart bars, table rows, and colored cells receive separate bounds.
- Light or colored text is included in ink masks and OCR candidates.
- The detector reports background zones per region; a single global white/gray assumption is invalid.

### ASCII checks

- Row/column spacing is preserved exactly enough to map glyphs to cells.
- Whitespace is meaningful and must not be trimmed away before geometry is saved.
- A logo or art block can be classified as one opaque display object while its cells remain inspectable.
- CP437/box-drawing and ordinary ASCII remain readable when OCR is disabled.

### ANSI `.ANS` checks

`.ANS` is not simply a text file with a different extension. The classic format is a terminal recording: printable bytes are interleaved with ANSI escape sequences, usually CSI/SGR commands for color, intensity, cursor movement, clearing, and positioning. Extended IBM-PC bytes commonly represent line-art and shading glyphs. This fixture also carries a `SAUCE00` record with title/author/group metadata and a declared 132-column canvas.

DACK's first ANSI reader should:

- decode the byte stream with an explicit code-page policy (CP437 first, with a visible fallback if unavailable);
- execute a bounded subset of CSI/SGR commands into a terminal-cell buffer rather than drawing directly into the gameplay texture;
- preserve foreground/background color, bright/blink state where supported, cursor movement, and CR/LF semantics;
- keep the terminal canvas at its native cell geometry and render it as an opaque/dimmed underlay or backglass;
- parse SAUCE as provenance/display metadata, not as proof of redistribution rights;
- impose limits on rows, columns, escape-sequence count, and render time so a hostile or malformed file cannot stall the editor;
- allow a creator to promote selected cells/regions into bumpers, rails, targets, walls, or decorative art without requiring OCR.

The King Diamond fixture is a parser and color-state golden test. Its recorded title/author/group should be shown as local metadata only; licensing remains a separate provenance decision.

### Temporal checks

- `ChromeAnim.mp4` is treated as a sequence of source frames, not a single import image.
- Unchanged regions retain stable IDs across frames.
- Changed rectangles are analyzed incrementally and coalesced before publication.
- OCR jobs for old frames are cancelled or discarded when the active source hash changes.
- A frame with a transient menu, tooltip, or selection does not permanently rewrite the Snapshot without creator confirmation.

## Golden Outputs to Record

For each fixture, store a small reviewable golden record rather than a full pixel-perfect segmentation:

- source dimensions and hash;
- dominant/local background samples;
- structural rectangle count and parent/child relationships;
- icon/object candidate boxes and confidence;
- text candidate counts by glyph/word/line/heading;
- grid origin, cell size, and confidence when applicable;
- OCR labels and provider/version only where available;
- expected playable collision/mutation regions;
- accepted false positives and creator overrides;
- analysis duration, allocations, and dirty-region sizes.

Golden records should be versioned by analysis algorithm. A detector improvement may change a result, but it must produce an explicit golden-record update and a visible before/after review.

## Test Order

1. `Desktop.png` — background and sparse icons.
2. `Spreadsheet Data.png` — colored text, merged rectangles, chart/table structure.
3. `Raw Spreadsheet.png` — faint gridlines and empty-space behavior.
4. `Gantt Chart.png` — dense grid, bars, legends, and nested application chrome.
5. `Codex.png` — diff colors, controls, panes, and hierarchy.
6. `Dragon1.txt` and `DragonBallzLogo.txt` — fixed-width text-grid path.
7. `KingDiamongANSIFile.ANS` — ANSI byte/control parser and terminal-cell renderer.
8. `ChromeAnim.mp4` — temporal/live-source analysis after Snapshot analysis is stable.

The first five fixtures should be part of the R0/R2 golden-data suite. ASCII and ANSI fixtures belong in the Grid/Text, BBS, and Pinball backglass smoke levels. The video belongs in the Live Desktop spike and must not become a prerequisite for the static screenshot loop.
