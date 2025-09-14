# Cascade Rules — Tier1

最終更新: 2025-09-14

Tier1 の余波（Cascade）挙動を数値で固定します。

---

## ルール（決定）

- 深さ上限: 5（深さ=1 は直接効果後の一次反応）
- 減衰: 深さ4で×0.5、深さ5で遮断（0として切り捨て）
- 分岐上限（b）: 各ノードあたり最大3件まで展開（スコア上位を優先）
- 優先度キュー: `danger` と `pedagogy` の合成スコアで降順処理
- 再入抑止: 同一キー（例: 同一病兆）の再発は1日1回まで（デバウンス）
- 可視化: UIには上位3件のみ通知（タイトル+アイコン+簡易根拠）

---

## 擬似コード

```pseudocode
function propagateLocalCascade(action):
  queue = PriorityQueue()
  pushChildren(action.effects, depth=1)
  visible = []

  while queue.notEmpty():
    node = queue.pop() // highest score
    if node.depth >= 5: continue
    apply(node.tagDelta)
    if node.visibleScoreTop3Candidate():
      visible.add(node)
      if visible.count == 3: markVisibleFull()
    if node.depth == 4: node.scale(0.5)
    for child in expand(node):
      if child.depth <= 4 and not debounced(child):
        queue.push(child)

  return visible.take(3)
```

---

## テスト観点

- 最大深さ=5 を越えないこと（6以降は反映されない）。
- 分岐>3 が来ても上位3のみ展開されること。
- UI通知は最大3件で安定し、順序がスコア準拠であること。

