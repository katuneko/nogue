# Fieldcrawler T1–T6 Spec (Extract)

本ドキュメントは `requirements.md` の固定仕様を実装者向けに抽出したメモです。詳細・根拠は `requirements.md` を参照してください。

- 不動点：局所タグ更新、橋次数≤2、Kは難易度で2–4固定（既定3）、被害予算あり。
- 自動化：乗算合成＋下限APで逓減。装置の不足/劣化/故障はイベント化。
- データ駆動：コンテンツは `content/` 配下（YAML/JSON）、ホットリロード前提。
- ディレクター：`solvableNow` 重視＋多様性、契約は優先。`epsilon` で新鮮味注入。

このリポジトリの暫定配置：`content/tiers/*.yaml` と `content/events.yaml` を用意し、`tools/DataValidator` でスキーマ検証します。

