# Tier1 Game Loop Specification

最終更新: 2025-09-14

目的: Tier1 における1日サイクル（朝→昼→夜→判定→翌日）を、入出力・前後条件と擬似コードで明示する。

---

## フェーズ概要（時系列）

1) 朝（Initialise & Candidate Build）
- 入力: `WorldState`（日付/資金/AP/RNG）、`PatchState.Tags`、在庫、契約一覧
- 手順:
  - `updateWeather()`（簡易: 乱数駆動で季節揺らぎ、Tier1は影響軽微）
  - `collectCandidates()`（病兆、受粉チャンス、小雨、相場波、契約期限接近 など）
  - `directorSelect(pool, K)` でK件を選抜（`K=2/3/4` by 難易度、`solvableNow` で調整）
- 出力: `shown[K]`（スコア順リスト）

2) 昼（Player Actions）
- 入力: `AP_base=10`、`shown[K]`、プレイヤー入力
- 手順:
  - `AP=10` から、行為（耕す/播種/灌水/収穫）を実行
  - 実行ごとに `apply(action) → TagDelta（r=1） → propagateLocalCascade()`
  - `updateShownEvents()` で状況変化に応じた再スコア（軽量）
- 終了条件: `AP<=0` またはプレイヤーが行為終了
- 出力: `ActionLog`、更新後の `PatchState.Tags` / 在庫

3) 夜（Resolution）
- 手順:
  - `runAutomation()`（Tier1では効果なし、将来拡張のフックのみ）
  - `resolveContracts()` 在庫から契約へ自動充当
  - `applyNightlyGrowth()` 作物の成長/劣化（Tier1は簡易）
  - `enforceDamageBudget()`（Tier1は軽微・警告のみ）

4) 判定（Victory / Loss Check）
- 条件は `docs/Design/VictoryLoss_Tier1.md` を参照

5) 日付前進（Advance Day）
- `day += 1`、`AP=10` にリセット、`DaySeed` を更新

---

## 擬似コード

```pseudocode
function DayTick(world):
  seed = world.dailySeed
  updateWeather(world, seed)

  candidates = collectCandidates(world)
  shown = directorSelect(candidates, K=world.K, world)

  AP = world.dailyAP  // = 10
  while AP > 0 and player.hasActions():
    action = player.chooseAction(shown, world.UIHints)
    AP -= action.cost
    apply(action)                 // r=1, TagDelta
    propagateLocalCascade(action) // 深さ<=5（UIは3）
    updateShownEvents(shown, world)

  runAutomation(world)           // Tier1: No-op（将来拡張）
  resolveContracts(world)        // 在庫→契約充当
  applyNightlyGrowth(world)
  enforceDamageBudget(world)     // Tier1: 警告のみ

  if checkVictory(world) or checkLoss(world):
    world.gameOver = true
  else:
    advanceDay(world)
```

---

## 前後条件と失敗時の扱い

- 前提: `content/tiers/T1.yaml` がKや解禁に整合。欠損はValidatorで検出。
- 例外: 入力不正/行為不可はユーザー向けにメッセージ表示（Tier1は最小実装で可）。

