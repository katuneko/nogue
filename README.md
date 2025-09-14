# Nogue

Field management simulation prototype. See `requirements.md` for the full spec. This repo includes:

- Core design and balancing specs
- GitHub Actions for content schema validation and Unity tests
- Content validator (`tools/DataValidator`)

Getting started (WIP): Unity project scaffolding will be added under `game/`.

Quick content iteration

- Edit YAML under `content/` (e.g., `content/tiers/T3.yaml`, `content/events.yaml`).
- Author in fragments under `content/events/` and run `python tools/ContentTools/merge_events.py` to regenerate the merged `content/events.yaml` (generated; do not edit by hand).
- Validate locally: `python tools/DataValidator/validate.py`
- CI runs merge+validation on pushes/PRs that touch `content/**`.

Docs

- Design extract: `docs/GDD/Fieldcrawler_T1-T6_spec.md`
- Tech overview: `docs/Tech/Architecture.md`
