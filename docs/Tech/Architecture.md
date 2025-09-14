# Architecture Overview (Tier1 Refresh)

- Engine target: Unity (URP/2D Tilemap, DOTS-ready). Tier1は非Unityランタイムでも検証可能（.NET + NUnit）。
- Layering:
  - Core: grid/tags/RNG/time/serialize
  - Gameplay: actions/cascade/director/automation/contracts/genetics
  - Presentation: UI overlays and hints (read-only to gameplay)
- Content: data tables in YAML under `content/`
- Dependency rules:
  - `presentation` reads `gameplay`; `gameplay` depends only on `core`/`ecs`.
  - Content changes require no recompilation (hot reload planned).

## Tier1 Snapshot（コンポーネント境界とデータフロー）

- 含む: `core`（Tags/RNG/Time）, `gameplay`（Actions/Cascade/Director/Contracts minimal）, `tests`（NUnit）
- 含まない: Automation/Devices、Bridge（水/風/生態）、Genetics、品質逓増、Weekly契約
- データフロー:
  1) 起動: `WorldState` 初期化（RunSeed/DaySeed、AP=10、資金、PatchState.Tags）
  2) 朝: `EventsLoader`/候補収集 → `Director.Select(K)` → `shown[K]`
  3) 昼: `Actions` 実行 → `TagDelta` 適用 → `Cascade`（深さ<=5, 可視3）
  4) 夜: `ContractsState` へ自動充当 → 成長・予算チェック → 勝敗判定

## Unity Bridging（現状）

- Unity プロジェクト未作成時は `tools/ContentTools/sync_content.py` で `content/` を別プロジェクトへ同期可能。
- 将来の `game/ProjectSettings` 追加後は Unity Test Runner と CI `unity-tests.yml` を有効化。

## Content Path Resolution

- Authoritative source: `content/` at repo root (tool-first).
- Runtime env var: `CONTENT_ROOT` (default: `content/`).
- Build step copies `CONTENT_ROOT` to `game/content/` (or symlinks it) for Unity `StreamingAssets` consumption.
- Events are authored in `content/events/*.yaml` and merged into `content/events.yaml` by `tools/ContentTools/merge_events.py` (generated file; do not edit).
  - The tool also emits `content/events.json` for runtime loading.

## Director Implementation (MVP)

- `game/gameplay/Director/` contains a data-agnostic selector based on the spec.
- Inputs are provided via `IWorldState` and `IEventCandidate` interfaces.
- Reserved slot `contract_critical: 1` is honored for Tier >= 4（Tier1では未使用）。
- The implementation is deterministic once wired to a world RNG; for now it uses a fixed fallback RNG for ε-injection.

## Scoring Config (Externalized)

- Authoritative defaults: `content/director/Scoring.json`
  - keys: `weights.{danger,pedagogy,novelty,diversity,contract}`, `epsilon`, `K`, `reserved_slots.contract_critical`
- Tier overrides: `content/tiers/T*.yaml -> director.{K,epsilon,reserved_slots}`
- Merge rule: Tier overrides take precedence over Scoring defaults.

## Damage Budget

- Defaults in `content/director/Budget.json` (`beta` per category and `difficulty_coef`).
- Runtime class: `game/gameplay/World/DamageBudget.cs` with day/season caps and consumption.
- Predictor: `game/gameplay/World/LossPredictor.cs` uses event loss_profile or defaults by type.
- World wires: `WorldState.InitializeBudgets/BeginDay/OnEventResolved`.

## AP Estimation

- Device definitions from `content/devices.{yaml,json}` (key `ap_savings`), loaded by `DeviceDefsLoader`.
- Estimator: `game/gameplay/World/ApEstimator.cs` → `AP_base × ∏(1-α_device) × design_factor` (ceil, min 1).
- `solvableNow` uses AP estimates with a depth-2 plan search and threshold on expected tag improvement.
