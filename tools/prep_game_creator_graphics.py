"""Prepare local-only sprite candidates from The Game Creator's Pack.

This is a prototype sprite-importer pass, not a runtime admission step. It reads
from the ignored raw asset vault, detects non-transparent sprite blobs, writes
cropped frame candidates to ignored quarantine, and records a manifest that can
drive the future DACK animation editor.
"""

from __future__ import annotations

import json
from dataclasses import asdict, dataclass
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "raw base assets" / "The Game Creator's Pack" / "The Game Creator's Pack" / "Graphic Pack"
DEST = ROOT / "dack" / "assets" / "quarantine" / "game-creators-pack-graphics-prep"


@dataclass(frozen=True)
class FrameCandidate:
    index: int
    rect: tuple[int, int, int, int]
    size: tuple[int, int]
    opaque_pixels: int
    output: str


@dataclass(frozen=True)
class SpriteSheetCandidate:
    source: str
    source_size: tuple[int, int]
    mode: str
    frame_count: int
    importer_hint: str
    frames: list[FrameCandidate]


def is_opaque(pixel: tuple[int, int, int, int]) -> bool:
    return pixel[3] > 8


def detect_components(image: Image.Image, min_pixels: int = 12) -> list[tuple[int, int, int, int, int]]:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    visited = [[False for _ in range(height)] for _ in range(width)]
    components: list[tuple[int, int, int, int, int]] = []

    for y in range(height):
        for x in range(width):
            if visited[x][y] or not is_opaque(pixels[x, y]):
                continue

            stack = [(x, y)]
            visited[x][y] = True
            min_x = max_x = x
            min_y = max_y = y
            count = 0

            while stack:
                cx, cy = stack.pop()
                count += 1
                min_x = min(min_x, cx)
                max_x = max(max_x, cx)
                min_y = min(min_y, cy)
                max_y = max(max_y, cy)

                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if nx < 0 or ny < 0 or nx >= width or ny >= height or visited[nx][ny]:
                        continue

                    if not is_opaque(pixels[nx, ny]):
                        continue

                    visited[nx][ny] = True
                    stack.append((nx, ny))

            if count >= min_pixels:
                components.append((min_x, min_y, max_x + 1, max_y + 1, count))

    return sorted(components, key=lambda rect: (rect[1], rect[0], rect[3], rect[2]))


def importer_hint(source: Path, frame_count: int) -> str:
    name = source.stem.lower()
    if "player" in name:
        return "horizontal-strip/blob-detect; group frames into locomotion clips manually"
    if "spritesheet" in name or "platformer" in name:
        return "mixed-sheet/blob-detect first, optional grid/manual grouping in editor"
    if frame_count == 1:
        return "single-sprite"
    return "loose-object-sheet/blob-detect"


def save_frame(source_image: Image.Image, source: Path, component: tuple[int, int, int, int, int], index: int) -> FrameCandidate:
    x0, y0, x1, y1, opaque_pixels = component
    pad = 1
    crop_rect = (
        max(0, x0 - pad),
        max(0, y0 - pad),
        min(source_image.width, x1 + pad),
        min(source_image.height, y1 + pad),
    )
    crop = source_image.convert("RGBA").crop(crop_rect)
    output = DEST / "frames" / source.stem / f"{index:03d}.png"
    output.parent.mkdir(parents=True, exist_ok=True)
    crop.save(output)
    return FrameCandidate(
        index=index,
        rect=crop_rect,
        size=crop.size,
        opaque_pixels=opaque_pixels,
        output=str(output.relative_to(ROOT)),
    )


def main() -> int:
    if not SOURCE.exists():
        print(f"Missing source folder: {SOURCE}")
        return 1

    sheets: list[SpriteSheetCandidate] = []
    for source in sorted(SOURCE.glob("*.png")):
        with Image.open(source) as image:
            components = detect_components(image)
            frames = [save_frame(image, source, component, i) for i, component in enumerate(components)]
            sheets.append(
                SpriteSheetCandidate(
                    source=str(source.relative_to(ROOT)),
                    source_size=image.size,
                    mode=image.mode,
                    frame_count=len(frames),
                    importer_hint=importer_hint(source, len(frames)),
                    frames=frames,
                )
            )

    manifest = {
        "source_pack": "The Game Creator's Pack / Graphic Pack",
        "source_root": str(SOURCE.relative_to(ROOT)),
        "output_root": str(DEST.relative_to(ROOT)),
        "note": (
            "Local-only sprite importer prototype output. Do not ship or commit from quarantine; "
            "curate approved runtime sheets/manifests into dack/assets/third_party first."
        ),
        "sheets": [asdict(sheet) for sheet in sheets],
    }

    DEST.mkdir(parents=True, exist_ok=True)
    (DEST / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(f"Analyzed {len(sheets)} PNG sheets.")
    print(f"Prepared {sum(sheet.frame_count for sheet in sheets)} frame candidates into {DEST}")
    print(f"Wrote manifest: {DEST / 'manifest.json'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
