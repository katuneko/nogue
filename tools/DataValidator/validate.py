#!/usr/bin/env python3
import sys
import json
import os
from pathlib import Path

try:
    import yaml  # type: ignore
    from jsonschema import validate, Draft7Validator  # type: ignore
except Exception as e:
    print("[validator] Missing deps?", e)
    sys.exit(2)

ROOT = Path(__file__).resolve().parents[2]
SCHEMA_PATH = Path(__file__).resolve().with_name("schema.json")


def load_yaml_files(paths):
    files = []
    for p in paths:
        if not p.exists():
            continue
        for ext in ("*.yml", "*.yaml"):
            files.extend(p.rglob(ext))
    return files


def main():
    if not SCHEMA_PATH.exists():
        print(f"[validator] schema not found: {SCHEMA_PATH}")
        return 0

    schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
    tiers_schema = schema.get("properties", {}).get("tiers")
    events_schema = schema.get("properties", {}).get("events")

    targets = []
    content_dir = ROOT / "content"
    tiers_dir = content_dir / "tiers"
    events_file = content_dir / "events.yaml"

    errors = 0

    # Validate tiers/*.yaml
    tier_files = load_yaml_files([tiers_dir])
    for tf in tier_files:
        try:
            data = yaml.safe_load(tf.read_text(encoding="utf-8")) or {}
            if tiers_schema:
                Draft7Validator(tiers_schema).validate(data)
            print(f"[validator] OK: {tf}")
        except Exception as e:
            errors += 1
            print(f"[validator] ERROR in {tf}: {e}")

    # Validate events.yaml if present
    if events_file.exists():
        try:
            data = yaml.safe_load(events_file.read_text(encoding="utf-8")) or []
            if events_schema:
                Draft7Validator(events_schema).validate(data)
            print(f"[validator] OK: {events_file}")
        except Exception as e:
            errors += 1
            print(f"[validator] ERROR in {events_file}: {e}")

    if not tier_files and not events_file.exists():
        print("[validator] No content to validate. Skipping.")
        return 0

    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())

