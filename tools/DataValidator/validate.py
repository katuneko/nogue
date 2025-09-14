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

    content_dir = ROOT / "content"
    tiers_dir = content_dir / "tiers"
    events_file = content_dir / "events.yaml"
    ids_file = content_dir / "ids.yaml"

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

    # Validate IDs registry and cross-check
    if ids_file.exists():
        try:
            ids = yaml.safe_load(ids_file.read_text(encoding="utf-8")) or {}
            # Basic structure expectations
            for key in ("systems", "devices", "events"):
                if key not in ids:
                    raise ValueError(f"ids.yaml missing key: {key}")

            registry = {k: set(v or []) for k, v in ids.items()}

            # Cross-check: tiers -> systems/devices must be registered
            for tf in load_yaml_files([tiers_dir]):
                data = yaml.safe_load(tf.read_text(encoding="utf-8")) or {}
                unlock = (data.get("unlock") or {})
                for s in (unlock.get("systems") or []):
                    if s not in registry.get("systems", set()):
                        raise ValueError(f"Unregistered system in {tf.name}: {s}")
                for d in (unlock.get("devices") or []):
                    if d not in registry.get("devices", set()):
                        raise ValueError(f"Unregistered device in {tf.name}: {d}")

            # Cross-check: events.yaml -> events must be registered
            if events_file.exists():
                data = yaml.safe_load(events_file.read_text(encoding="utf-8")) or []
                ev_ids = {e.get("id") for e in data if isinstance(e, dict)}
                missing = ev_ids - registry.get("events", set())
                if missing:
                    raise ValueError(f"Unregistered event IDs: {sorted(missing)}")

            print(f"[validator] OK: {ids_file}")
        except Exception as e:
            errors += 1
            print(f"[validator] ERROR in {ids_file}: {e}")

    if not tier_files and not events_file.exists():
        print("[validator] No content to validate. Skipping.")
        return 0

    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
