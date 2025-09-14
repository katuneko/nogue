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
    director_scoring = content_dir / "director" / "Scoring.json"
    devices_file = content_dir / "devices.yaml"
    crops_file = content_dir / "crops.yaml"
    contracts_file = content_dir / "contracts.yaml"

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

            # Devices
            if devices_file.exists():
                devs = yaml.safe_load(devices_file.read_text(encoding="utf-8")) or []
                for d in devs:
                    did = d.get("id")
                    if did not in registry.get("devices", set()):
                        raise ValueError(f"Unregistered device id: {did}")
                    # ranges
                    rel = (d.get("reliability") or {})
                    fr = rel.get("fail_rate_daily", 0)
                    if not (0 <= fr <= 1):
                        raise ValueError(f"device {did} fail_rate_daily out of range: {fr}")
            # Crops
            if crops_file.exists():
                crops = yaml.safe_load(crops_file.read_text(encoding="utf-8")) or []
                for c in crops:
                    cid = c.get("id")
                    if cid not in registry.get("crops", set()):
                        raise ValueError(f"Unregistered crop id: {cid}")
                    qw = (c.get("quality_weights") or {})
                    total = sum(float(qw.get(k, 0)) for k in qw.keys())
                    if not (0.99 <= total <= 1.01):
                        raise ValueError(f"crop {cid} quality_weights must sum to 1.0 (got {total})")
            # Contracts
            if contracts_file.exists():
                cons = yaml.safe_load(contracts_file.read_text(encoding="utf-8")) or []
                for ct in cons:
                    ctid = ct.get("id")
                    if ctid not in registry.get("contracts", set()):
                        raise ValueError(f"Unregistered contract id: {ctid}")
                    prod = ct.get("product")
                    if prod not in registry.get("crops", set()):
                        raise ValueError(f"contract {ctid} references unknown crop: {prod}")

            print(f"[validator] OK: {ids_file}")
        except Exception as e:
            errors += 1
            print(f"[validator] ERROR in {ids_file}: {e}")

    if not tier_files and not events_file.exists():
        print("[validator] No content to validate. Skipping.")
        return 0

    # Scoring config presence check
    if director_scoring.exists():
        try:
            json.loads(director_scoring.read_text(encoding="utf-8"))
            print(f"[validator] OK: {director_scoring}")
        except Exception as e:
            errors += 1
            print(f"[validator] ERROR in {director_scoring}: {e}")
    else:
        print("[validator] WARN: content/director/Scoring.json not found")

    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
