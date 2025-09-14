#!/usr/bin/env python3
"""
Copy authoritative content (CONTENT_ROOT or ./content) into game/content/.
This is a convenience for local runs until Unity build scripts take over.
"""
import os
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SRC = Path(os.environ.get("CONTENT_ROOT", ROOT / "content"))
DST = ROOT / "game" / "content"


def copytree(src: Path, dst: Path):
    if dst.exists():
        shutil.rmtree(dst)
    shutil.copytree(src, dst)


def main():
    if not SRC.exists():
        print(f"[sync_content] Source not found: {SRC}")
        return 1
    copytree(SRC, DST)
    print(f"[sync_content] Synced {SRC} -> {DST}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

