#!/usr/bin/env python3
"""
Merge event fragments under content/events/ into a single content/events.yaml.
Rules:
 - Source files: common.yaml, T1.yaml .. T6.yaml (if exist)
 - Event `id` must be unique across all files (CI failure if duplicate)
 - If an event lacks `tier_min`/`tier_max`, infer from filename:
     common -> tier_min=1
     Tx     -> tier_min=int(x)
 - Output includes a DO NOT EDIT header comment.
"""
from pathlib import Path
import sys
from typing import List, Dict, Any

try:
    import yaml  # type: ignore
except Exception:
    print("[merge_events] Missing dependency: pyyaml", file=sys.stderr)
    sys.exit(2)

ROOT = Path(__file__).resolve().parents[2]
EVENTS_DIR = ROOT / "content" / "events"
OUTPUT_YAML = ROOT / "content" / "events.yaml"
OUTPUT_JSON = ROOT / "content" / "events.json"


def load_yaml(path: Path):
    text = path.read_text(encoding="utf-8")
    data = yaml.safe_load(text)
    if data is None:
        return []
    if not isinstance(data, list):
        raise ValueError(f"{path} must be a YAML list of events")
    return data


def infer_tier_from_name(name: str) -> int:
    if name == "common.yaml":
        return 1
    if name.startswith("T") and name[1].isdigit():
        try:
            return int(name[1])
        except Exception:
            pass
    return 1


def main() -> int:
    if not EVENTS_DIR.exists():
        print(f"[merge_events] No events dir: {EVENTS_DIR}")
        return 0

    order = ["common.yaml"] + [f"T{i}.yaml" for i in range(1, 7)]
    files = [EVENTS_DIR / f for f in order if (EVENTS_DIR / f).exists()]
    if not files:
        print("[merge_events] No event fragments found. Skipping.")
        return 0

    merged: List[Dict[str, Any]] = []
    seen_ids = set()

    for f in files:
        tier_default = infer_tier_from_name(f.name)
        for e in load_yaml(f):
            if not isinstance(e, dict) or "id" not in e:
                raise ValueError(f"Invalid event entry in {f}: {e}")
            eid = e["id"]
            if eid in seen_ids:
                raise SystemExit(f"[merge_events] Duplicate event id: {eid} in {f}")
            seen_ids.add(eid)
            e.setdefault("tier_min", tier_default)
            merged.append(e)

    header = (
        "# GENERATED FILE — DO NOT EDIT\n"
        "# Source fragments: content/events/common.yaml, T1.yaml .. T6.yaml\n"
    )
    OUTPUT_YAML.write_text(header + yaml.safe_dump(merged, allow_unicode=True, sort_keys=False), encoding="utf-8")
    # Also emit JSON array for lightweight loading in Unity
    import json
    OUTPUT_JSON.write_text(json.dumps(merged, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"[merge_events] Wrote {OUTPUT_YAML} and {OUTPUT_JSON} ({len(merged)} events)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
