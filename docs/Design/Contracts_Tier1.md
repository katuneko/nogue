# Contracts — Tier1 Minimal Spec

最終更新: 2025-09-14

目的: Tier1で扱う最小の契約仕様を、既存 `Nogue.Gameplay.Contracts` のDTO/Stateに合わせて定義。

---

## 対象

- 種別: `single` のみ（`weekly`/`custom` は後回し）
- 品質: `quality_min = "C"` 固定（Tier1 は品質計算を簡略化）
- 期限: `deadline.in_days = 3..5`（易5/標準4/難3）
- 数量: 少量（例: 5〜10）— 実数はコンテンツ側で調整

---

## DTO（JSON例）

```json
{
  "id": "single_leafy_small",
  "type": "single",
  "product": "leafy",
  "qty": 8,
  "quality_min": "C",
  "deadline": { "in_days": 4 },
  "pricing": { "base": 1200.0 },
  "penalties": { "late_per_day": 0.0, "cancel_after_days": 0 },
  "credit": { "delta_on_success": 0, "delta_on_cancel": 0 }
}
```

---

## ランタイム挙動

- 受注: `ContractsState.Add(dto, startDay=today)` で登録。
- 期限: `DaysLeft(id)` は `deadline.in_days - 経過日数` を返す。
- 必要量: `RequiredRemainingNow(id)` が当日必要数量を返す（single は残総量）。
- 納品: 夜間の `resolveContracts()` で在庫→契約へ自動充当し、`OnDelivered(id, qty)` を呼ぶ。
- 達成: `qty >= dto.qty && quality >= C` を満たした時点で完了→報酬を資金へ反映。

---

## 報酬（Tier1）

- 価格は `pricing.base` の固定額を、そのまま資金へ加算（品質逓増・遅延ペナルティは未使用）。

---

## 失敗・キャンセル

- 期限超過: Tier1では自動キャンセルしない（在庫保持のまま未達成）。
- 勝敗: キャンセル有無に関わらず、勝利条件は「契約達成数≥2かつ資金≥0」。

