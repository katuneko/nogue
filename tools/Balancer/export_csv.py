#!/usr/bin/env python3
"""
Minimal stub for future balancing exports.
Reads YAML under content/ and prints a flat CSV for quick inspection.
"""
from pathlib import Path
import sys
try:
    import yaml  # type: ignore
except Exception:
    print("[export_csv] Missing dependency: pyyaml", file=sys.stderr)
    sys.exit(2)

ROOT = Path(__file__).resolve().parents[2]
CONTENT = ROOT / "content"

def main():
    tiers_dir = CONTENT / "tiers"
    rows = ["tier,automation_multiplier,K,epsilon,systems,devices"]
    for yml in sorted(tiers_dir.glob("*.y*ml")):
        data = yaml.safe_load(yml.read_text(encoding="utf-8")) or {}
        tier = yml.stem
        mult = (data.get("automation") or {}).get("multiplier")
        director = data.get("director") or {}
        K = director.get("K")
        eps = director.get("epsilon")
        systems = ";".join((data.get("unlock") or {}).get("systems") or [])
        devices = ";".join((data.get("unlock") or {}).get("devices") or [])
        rows.append(f"{tier},{mult},{K},{eps},{systems},{devices}")
    print("\n".join(rows))

if __name__ == "__main__":
    main()

