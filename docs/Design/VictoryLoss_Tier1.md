# Victory / Loss Conditions (Tier1)

最終更新: 2025-09-14

`requirements_tier1.md` セクション6の数値を機械可読の擬似コードで定義します。

---

## パラメータ

- `REQUIRED_CONTRACTS = 2`
- `NEGATIVE_FUNDS_GRACE_DAYS = 2`  // 3日目で敗北
- `PATHOGEN_LOSS_LEVEL = 5`
- `PATHOGEN_LOSS_DAYS = 3`

---

## ステート拡張（最小）

```pseudocode
WorldState {
  day: int
  funds: int
  contractsCompleted: int
  pathogenStreak: int   // 連続日数（当日病原圧==5で+1、そうでなければ0にリセット）
  negativeFundsStreak: int // 連続日数（当日資金<0で+1、そうでなければ0）
  gameOver: bool
  victory: bool
}
```

---

## 判定ロジック

```pseudocode
function checkVictory(world):
  if world.contractsCompleted >= REQUIRED_CONTRACTS and world.funds >= 0:
    world.victory = true
    return true
  return false

function checkLoss(world):
  if world.negativeFundsStreak > NEGATIVE_FUNDS_GRACE_DAYS: // 3日目
    world.victory = false
    return true
  if world.pathogenStreak >= PATHOGEN_LOSS_DAYS: // 3日連続
    world.victory = false
    return true
  return false

function nightlyUpdateStreaks(world, patch):
  // 呼び出しタイミング: 夜間解決の最後
  if world.funds < 0:
    world.negativeFundsStreak += 1
  else:
    world.negativeFundsStreak = 0

  if patch.tags.pathogen == PATHOGEN_LOSS_LEVEL:
    world.pathogenStreak += 1
  else:
    world.pathogenStreak = 0
```

---

## 備考

- 緊急融資イベントは `negativeFundsStreak == 1` 時に発火（文言警告のみでも可）。
- 多Patch化後は、病原圧の最大値（または重み付き平均）でStreak計測に拡張する。

