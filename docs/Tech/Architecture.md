# Architecture Overview (Draft)

- Engine target: Unity (URP/2D Tilemap, DOTS-ready).
- Layering:
  - Core: grid/tags/RNG/time/serialize
  - Gameplay: actions/cascade/director/automation/contracts/genetics
  - Presentation: UI overlays and hints (read-only to gameplay)
- Content: data tables in YAML under `content/`
- Dependency rules:
  - `presentation` reads `gameplay`; `gameplay` depends only on `core`/`ecs`.
  - Content changes require no recompilation (hot reload planned).

## Content Path Resolution

- Authoritative source: `content/` at repo root (tool-first).
- Runtime env var: `CONTENT_ROOT` (default: `content/`).
- Build step copies `CONTENT_ROOT` to `game/content/` (or symlinks it) for Unity `StreamingAssets` consumption.
- Events are authored in `content/events/*.yaml` and merged into `content/events.yaml` by `tools/ContentTools/merge_events.py` (generated file; do not edit).

## Director Implementation (MVP)

- `game/gameplay/Director/` contains a data-agnostic selector based on the spec.
- Inputs are provided via `IWorldState` and `IEventCandidate` interfaces.
- Reserved slot `contract_critical: 1` is honored for Tier >= 4.
- The implementation is deterministic once wired to a world RNG; for now it uses a fixed fallback RNG for ε-injection.
