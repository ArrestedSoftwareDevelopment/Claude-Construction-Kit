"""Prepare local-only scaled candidates from the VerzatileDev pinball pack.

This is intentionally a *prep* script, not an importer. It reads from the ignored
raw asset vault and writes to the ignored quarantine folder. Curated/admitted
runtime assets should be copied into dack/assets/third_party only after a human
selects a small subset and updates ASSET_PROVENANCE.md.
"""

from __future__ import annotations

import json
from dataclasses import asdict, dataclass
from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "raw base assets" / "PinBall_By_VerzatileDev" / "PinBall_By_VerzatileDev"
DEST = ROOT / "dack" / "assets" / "quarantine" / "pinball-verzatiledev-prep"


@dataclass(frozen=True)
class PreparedAsset:
    source: str
    output: str
    source_size: tuple[int, int]
    output_size: tuple[int, int]
    tier: str
    scale: float


def scaled_size(width: int, height: int, longest_edge: int) -> tuple[int, int]:
    if max(width, height) <= longest_edge:
        return width, height

    scale = longest_edge / max(width, height)
    return max(1, round(width * scale)), max(1, round(height * scale))


def save_scaled(source: Path, dest: Path, longest_edge: int, tier: str) -> PreparedAsset:
    with Image.open(source) as image:
        image = image.convert("RGBA")
        original_size = image.size
        new_size = scaled_size(image.width, image.height, longest_edge)
        scale = new_size[0] / image.width
        if new_size != image.size:
            image = image.resize(new_size, Image.Resampling.LANCZOS)

        dest.parent.mkdir(parents=True, exist_ok=True)
        image.save(dest)

    return PreparedAsset(
        source=str(source.relative_to(ROOT)),
        output=str(dest.relative_to(ROOT)),
        source_size=original_size,
        output_size=new_size,
        tier=tier,
        scale=scale,
    )


def main() -> int:
    if not SOURCE.exists():
        print(f"Missing source folder: {SOURCE}")
        return 1

    outputs: list[PreparedAsset] = []
    pngs = sorted(SOURCE.rglob("*.png"))

    for source in pngs:
        relative = source.relative_to(SOURCE)
        is_sheet_or_background = source.parent == SOURCE

        if is_sheet_or_background:
            # Useful as local preview/reference, still too coarse for admitted runtime art.
            tier_plan = [("preview-1024", 1024), ("thumb-256", 256)]
        else:
            # Individual pieces get editor/runtime candidate sizes.
            tier_plan = [("candidate-512", 512), ("thumb-128", 128)]

        for tier, longest_edge in tier_plan:
            dest = DEST / tier / relative
            outputs.append(save_scaled(source, dest, longest_edge, tier))

    manifest = {
        "source_pack": "PinBall_By_VerzatileDev",
        "source_root": str(SOURCE.relative_to(ROOT)),
        "output_root": str(DEST.relative_to(ROOT)),
        "note": (
            "Local-only scaled candidates. Do not ship or commit from quarantine; "
            "curate a small subset into dack/assets/third_party and update provenance first."
        ),
        "assets": [asdict(asset) for asset in outputs],
    }

    DEST.mkdir(parents=True, exist_ok=True)
    (DEST / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(f"Prepared {len(outputs)} scaled files into {DEST}")
    print(f"Wrote manifest: {DEST / 'manifest.json'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
