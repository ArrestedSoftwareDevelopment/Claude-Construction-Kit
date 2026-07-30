from __future__ import annotations

import json
import struct
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = PROJECT_ROOT / "raw base assets" / "Legacy Collection" / "Legacy Collection"
ASSETS_ROOT = SOURCE_ROOT / "Assets"
OUTPUT_ROOT = PROJECT_ROOT / "dack" / "assets" / "quarantine" / "legacy-collection-prep"
CATALOG_PATH = OUTPUT_ROOT / "legacy-collection-catalog.json"
SUMMARY_PATH = OUTPUT_ROOT / "legacy-collection-summary.md"
BUNDLE_MANIFEST_PATH = OUTPUT_ROOT / "legacy-collection-bundles.json"


IMAGE_EXTENSIONS = {".png", ".gif"}


def png_size(path: Path) -> tuple[int, int] | None:
    with path.open("rb") as handle:
        header = handle.read(24)
    if len(header) < 24 or not header.startswith(b"\x89PNG\r\n\x1a\n"):
        return None
    return struct.unpack(">II", header[16:24])


def gif_size(path: Path) -> tuple[int, int] | None:
    with path.open("rb") as handle:
        header = handle.read(10)
    if len(header) < 10 or header[:6] not in {b"GIF87a", b"GIF89a"}:
        return None
    return struct.unpack("<HH", header[6:10])


def image_size(path: Path) -> tuple[int, int] | None:
    try:
        if path.suffix.lower() == ".png":
            return png_size(path)
        if path.suffix.lower() == ".gif":
            return gif_size(path)
    except OSError:
        return None
    return None


def category_guess(relative: Path) -> str:
    text = "/".join(part.lower() for part in relative.parts)
    name = relative.stem.lower()

    if any(token in text for token in ("explosion", "magic", "effect", "vfx", "spell", "impact")):
        return "effects"
    if any(token in text for token in ("projectile", "bullet", "fireball", "laser", "shot")):
        return "projectiles"
    if any(token in text for token in ("character", "enemy", "monster", "player", "creature", "dragon", "guard", "soldier", "worker", "hero", "npc")):
        return "characters"
    if any(token in text for token in ("ship", "space", "tank", "vehicle", "car", "plane")):
        return "vehicles"
    if any(token in text for token in ("tile", "terrain", "platform", "block", "wall", "floor", "ground")):
        return "tiles_and_surfaces"
    if any(token in text for token in ("item", "object", "coin", "gem", "pickup", "prop", "chest", "key")):
        return "objects_and_pickups"
    if any(token in text for token in ("ui", "button", "icon", "cursor", "font")):
        return "ui"
    if any(token in name for token in ("idle", "run", "walk", "jump", "attack", "death", "hit", "hurt", "shoot", "climb")):
        return "animation_parts"
    return "uncategorized"


def animation_hint(relative: Path, width: int | None, height: int | None, sibling_count: int) -> str:
    text = "/".join(part.lower() for part in relative.parts)
    name = relative.stem.lower()
    if "spritesheet" in text or "sprite sheet" in text or "sheet" in name:
        return "spritesheet"
    if any(token in name for token in ("idle", "run", "walk", "jump", "attack", "death", "hit", "hurt", "shoot", "climb", "fly")):
        return "named_animation_strip"
    if sibling_count >= 6 and relative.suffix.lower() in IMAGE_EXTENSIONS:
        return "image_sequence_candidate"
    if width and height and width >= height * 3:
        return "horizontal_strip_candidate"
    if width and height and height >= width * 3:
        return "vertical_strip_candidate"
    return "single_asset_or_unknown"


def bundle_root(relative: Path) -> Path:
    parts = relative.parts
    lower = [part.lower() for part in parts]

    if len(parts) >= 2 and lower[0] == "explosions and magic":
        return Path(*parts[:2])

    if len(parts) >= 3 and lower[1] == "characters":
        if len(parts) >= 5 and lower[0] == "tinyrpg" and lower[2] == "battle sprites" and lower[3] == "living pack 1":
            return Path(*parts[:5])
        if len(parts) >= 4 and lower[0] == "tinyrpg" and lower[2] == "battle sprites":
            return Path(*parts[:4])
        return Path(*parts[:3])

    if len(parts) >= 4 and lower[0] == "warped" and lower[1] == "misc":
        return Path(*parts[:4])

    if len(parts) >= 3 and lower[1] == "environments":
        return Path(*parts[:3])

    if len(parts) >= 3 and lower[1] in {"objects", "items", "misc"}:
        return Path(*parts[:3])

    return relative.parent


def main() -> None:
    if not ASSETS_ROOT.exists():
        raise SystemExit(f"Missing Legacy Collection assets directory: {ASSETS_ROOT}")

    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)

    all_files = sorted(path for path in ASSETS_ROOT.rglob("*") if path.is_file())
    sibling_counts: dict[Path, int] = Counter(path.parent for path in all_files)
    extension_counts = Counter(path.suffix.lower() or "[no extension]" for path in all_files)

    top_level = []
    for folder in sorted(path for path in ASSETS_ROOT.iterdir() if path.is_dir()):
        folder_files = sorted(path for path in folder.rglob("*") if path.is_file())
        top_level.append(
            {
                "name": folder.name,
                "files": len(folder_files),
                "extensions": dict(sorted(Counter(path.suffix.lower() or "[no extension]" for path in folder_files).items())),
            }
        )

    image_entries = []
    category_counts = Counter()
    hint_counts = Counter()
    category_examples: dict[str, list[dict[str, object]]] = defaultdict(list)
    import_candidates = []
    bundle_entries: dict[str, dict[str, object]] = {}

    for path in all_files:
        relative = path.relative_to(ASSETS_ROOT)
        category = category_guess(relative)
        category_counts[category] += 1

        width = height = None
        if path.suffix.lower() in IMAGE_EXTENSIONS:
            size = image_size(path)
            if size:
                width, height = size

        hint = animation_hint(relative, width, height, sibling_counts[path.parent])
        hint_counts[hint] += 1

        if path.suffix.lower() in IMAGE_EXTENSIONS:
            entry = {
                "path": relative.as_posix(),
                "category": category,
                "animationHint": hint,
                "width": width,
                "height": height,
                "siblingFileCount": sibling_counts[path.parent],
            }
            image_entries.append(entry)
            if len(category_examples[category]) < 12:
                category_examples[category].append(entry)
            if hint != "single_asset_or_unknown" or category in {"characters", "effects", "projectiles", "vehicles"}:
                import_candidates.append(entry)

            root = bundle_root(relative)
            root_key = root.as_posix()
            bundle = bundle_entries.setdefault(
                root_key,
                {
                    "bundleRoot": root_key,
                    "displayName": root.name,
                    "categoryCounts": Counter(),
                    "animationHintCounts": Counter(),
                    "imageFiles": 0,
                    "spriteSheets": [],
                    "previews": [],
                    "sampleFiles": [],
                    "dimensions": Counter(),
                },
            )
            bundle["imageFiles"] += 1
            bundle["categoryCounts"][category] += 1
            bundle["animationHintCounts"][hint] += 1
            if width and height:
                bundle["dimensions"][f"{width}x{height}"] += 1
            if hint == "spritesheet":
                bundle["spriteSheets"].append(entry["path"])
            if "preview" in relative.stem.lower() or path.suffix.lower() == ".gif":
                bundle["previews"].append(entry["path"])
            if len(bundle["sampleFiles"]) < 8:
                bundle["sampleFiles"].append(entry["path"])

    import_priority = sorted(
        import_candidates,
        key=lambda item: (
            {
                "effects": 0,
                "projectiles": 1,
                "characters": 2,
                "vehicles": 3,
                "objects_and_pickups": 4,
                "tiles_and_surfaces": 5,
            }.get(str(item["category"]), 9),
            str(item["path"]).lower(),
        ),
    )
    import_candidates_by_category = {}
    for category in sorted(category_counts):
        category_items = [item for item in import_priority if item["category"] == category]
        if category_items:
            import_candidates_by_category[category] = category_items[:75]

    catalog = {
        "sourceRoot": str(SOURCE_ROOT),
        "assetsRoot": str(ASSETS_ROOT),
        "generatedUtc": datetime.now(timezone.utc).isoformat(),
        "totals": {
            "files": len(all_files),
            "imageFiles": len(image_entries),
            "directories": sum(1 for path in ASSETS_ROOT.rglob("*") if path.is_dir()),
        },
        "extensions": dict(sorted(extension_counts.items())),
        "topLevelFolders": top_level,
        "categoryCounts": dict(sorted(category_counts.items())),
        "animationHintCounts": dict(sorted(hint_counts.items())),
        "categoryExamples": category_examples,
        "importCandidates": import_priority[:300],
        "importCandidatesByCategory": import_candidates_by_category,
    }

    CATALOG_PATH.write_text(json.dumps(catalog, indent=2), encoding="utf-8")

    bundles = []
    for bundle in bundle_entries.values():
        categories = bundle["categoryCounts"]
        hints = bundle["animationHintCounts"]
        dims = bundle["dimensions"]
        primary_category = categories.most_common(1)[0][0] if categories else "uncategorized"
        quality = "reference"
        if bundle["spriteSheets"]:
            quality = "spritesheet_ready"
        elif hints.get("named_animation_strip", 0) >= 3 or hints.get("image_sequence_candidate", 0) >= 6:
            quality = "sequence_ready"
        elif bundle["imageFiles"] >= 3:
            quality = "manual_review"
        bundles.append(
            {
                "bundleRoot": bundle["bundleRoot"],
                "displayName": bundle["displayName"],
                "primaryCategory": primary_category,
                "quality": quality,
                "imageFiles": bundle["imageFiles"],
                "categoryCounts": dict(sorted(categories.items())),
                "animationHintCounts": dict(sorted(hints.items())),
                "commonDimensions": dict(dims.most_common(6)),
                "spriteSheets": bundle["spriteSheets"][:12],
                "previews": bundle["previews"][:12],
                "sampleFiles": bundle["sampleFiles"],
            }
        )

    bundles = sorted(
        bundles,
        key=lambda item: (
            {
                "spritesheet_ready": 0,
                "sequence_ready": 1,
                "manual_review": 2,
                "reference": 3,
            }.get(str(item["quality"]), 9),
            str(item["primaryCategory"]),
            str(item["bundleRoot"]).lower(),
        ),
    )
    BUNDLE_MANIFEST_PATH.write_text(
        json.dumps(
            {
                "sourceRoot": str(SOURCE_ROOT),
                "generatedUtc": catalog["generatedUtc"],
                "note": "Quarantine/prep bundle manifest. Promote assets explicitly into project assets after review.",
                "bundles": bundles,
            },
            indent=2,
        ),
        encoding="utf-8",
    )

    lines = [
        "# Legacy Collection Processing Summary",
        "",
        f"Source: `{SOURCE_ROOT}`",
        f"Generated: {catalog['generatedUtc']}",
        "",
        "## Inventory",
        "",
        f"- Files: {catalog['totals']['files']}",
        f"- Image files: {catalog['totals']['imageFiles']}",
        f"- Directories: {catalog['totals']['directories']}",
        "",
        "## Top-level folders",
        "",
        "| Folder | Files | PNG | Other notable extensions |",
        "| --- | ---: | ---: | --- |",
    ]

    for folder in top_level:
        extensions = dict(folder["extensions"])
        png_count = extensions.pop(".png", 0)
        notable = ", ".join(f"{ext}: {count}" for ext, count in extensions.items()) or "-"
        lines.append(f"| {folder['name']} | {folder['files']} | {png_count} | {notable} |")

    lines.extend(
        [
            "",
            "## Importer-facing buckets",
            "",
        ]
    )
    for category, count in sorted(category_counts.items()):
        lines.append(f"- {category}: {count} files")

    lines.extend(
        [
            "",
            "## Animation hints",
            "",
        ]
    )
    for hint, count in sorted(hint_counts.items()):
        lines.append(f"- {hint}: {count}")

    lines.extend(
        [
            "",
            "## Best next importer targets",
            "",
            "1. Effects and projectile sheets: easiest win for reusable impact, magic, laser, and Brickbat/Pinball spectacle.",
            "2. Character and vehicle sheets: feed the Sprite Studio preset workflow after frame slicing is confirmed.",
            "3. Objects/pickups and tiles: useful for shelves, but should be promoted only after we decide whether each object is decorative, collectible, solid, or hazardous.",
            "",
            "## Representative candidates",
            "",
        ]
    )

    for category in ("effects", "projectiles", "characters", "vehicles", "objects_and_pickups", "tiles_and_surfaces"):
        examples = category_examples.get(category, [])
        if not examples:
            continue
        lines.append(f"### {category}")
        lines.append("")
        for example in examples[:8]:
            dims = f"{example['width']}x{example['height']}" if example["width"] and example["height"] else "unknown size"
            lines.append(f"- `{example['path']}` - {dims}, {example['animationHint']}")
        lines.append("")

    lines.extend(
        [
            "## Bundle pass",
            "",
            f"- Candidate bundles: {len(bundles)}",
            f"- Spritesheet-ready bundles: {sum(1 for bundle in bundles if bundle['quality'] == 'spritesheet_ready')}",
            f"- Sequence-ready bundles: {sum(1 for bundle in bundles if bundle['quality'] == 'sequence_ready')}",
            "",
            "Top bundle candidates:",
            "",
        ]
    )

    for bundle in bundles[:18]:
        dimensions = ", ".join(f"{dim} ({count})" for dim, count in list(bundle["commonDimensions"].items())[:3]) or "mixed/unknown"
        lines.append(
            f"- `{bundle['bundleRoot']}` - {bundle['primaryCategory']}, {bundle['quality']}, "
            f"{bundle['imageFiles']} images, {dimensions}"
        )

    lines.append("")

    lines.extend(
        [
            "## Processing boundary",
            "",
            "This catalog is a quarantine/prep artifact. It does not promote the Legacy Collection into runtime assets by itself.",
            "Use it to choose explicit imports, assign credits/licenses, and create stable DACK sprite/object presets.",
            "",
        ]
    )

    SUMMARY_PATH.write_text("\n".join(lines), encoding="utf-8")
    print(SUMMARY_PATH)
    print(CATALOG_PATH)
    print(BUNDLE_MANIFEST_PATH)


if __name__ == "__main__":
    main()
