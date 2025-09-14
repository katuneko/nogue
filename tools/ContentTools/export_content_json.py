#!/usr/bin/env python3
"""
Export devices.yaml, crops.yaml, contracts.yaml to JSON for runtime loading.
Writes: content/devices.json, content/crops.json, content/contracts.json
"""
from pathlib import Path
import json
import sys

try:
    import yaml  # type: ignore
except Exception:
    print("[export_content_json] Missing dependency: pyyaml", file=sys.stderr)
    sys.exit(2)

ROOT = Path(__file__).resolve().parents[2]
CONTENT = ROOT / "content"


def dump_yaml_to_json(yaml_path: Path, json_path: Path):
    if not yaml_path.exists():
        print(f"[export_content_json] WARN: {yaml_path} not found")
        return
    data = yaml.safe_load(yaml_path.read_text(encoding="utf-8"))
    json_path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[export_content_json] Wrote {json_path}")


def main() -> int:
    dump_yaml_to_json(CONTENT / "devices.yaml", CONTENT / "devices.json")
    dump_yaml_to_json(CONTENT / "crops.yaml", CONTENT / "crops.json")
    dump_yaml_to_json(CONTENT / "contracts.yaml", CONTENT / "contracts.json")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

