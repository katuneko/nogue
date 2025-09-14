#!/usr/bin/env python3
"""
Export tiers/*.yaml into a single JSON map `content/tiers.json` for runtime loading.
Output shape:
{
  "T1": {"director": {"K": 3, "epsilon": 0.08, "reserved_slots": {"contract_critical": 0}}, ...},
  ...
}
"""
from pathlib import Path
import json
import sys

try:
    import yaml  # type: ignore
except Exception:
    print("[export_tiers_json] Missing dependency: pyyaml", file=sys.stderr)
    sys.exit(2)

ROOT = Path(__file__).resolve().parents[2]
TIERS_DIR = ROOT / "content" / "tiers"
OUTPUT = ROOT / "content" / "tiers.json"


def load_yaml_files(path: Path):
    files = []
    for ext in ("*.yml", "*.yaml"):
        files.extend(sorted(path.glob(ext)))
    return files


def main() -> int:
    if not TIERS_DIR.exists():
        print(f"[export_tiers_json] No tiers dir: {TIERS_DIR}")
        return 0
    out = {}
    for yml in load_yaml_files(TIERS_DIR):
        data = yaml.safe_load(yml.read_text(encoding="utf-8")) or {}
        out[yml.stem] = data
    OUTPUT.write_text(json.dumps(out, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[export_tiers_json] Wrote {OUTPUT} ({len(out)} tiers)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

