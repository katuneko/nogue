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

Game content (devices/crops/contracts)

- Source YAML: `content/devices.yaml`, `content/crops.yaml`, `content/contracts.yaml`.
- Export JSON for runtime: `python tools/ContentTools/export_content_json.py` → writes `content/{devices,crops,contracts}.json`.

Director scoring config

- Defaults live in `content/director/Scoring.json`.
- Tier-specific overrides come from `content/tiers/T*.yaml` under `director.{K,epsilon,reserved_slots}`.

Damage budget config

- Defaults live in `content/director/Budget.json` (`beta` per category and difficulty coefficients).

Docs

- Design extract: `docs/GDD/Fieldcrawler_T1-T6_spec.md`
- Tech overview: `docs/Tech/Architecture.md`

## 開発者向けガイド

このセクションはリポジトリの開発者向けの実務情報をまとめています。詳細は各ディレクトリの README と `docs/` を参照してください。

### プロジェクト構成

- `game/core`, `game/gameplay`: C# のゲームプレイロジック（Unity 互換）。
- `game/tests`: NUnit テスト（`*Tests.cs`）。
- `content/`: YAML が単一の真実。生成された JSON は同階層に出力。
- `tools/`: Python ユーティリティ（バリデータ、エクスポーター）。
- `docs/`: 設計・アーキテクチャノート。
- `.github/workflows/`: コンテンツ検証と（条件付き）Unity テストの CI。

### セットアップと基本コマンド

- Python 仮想環境と依存関係
  - `python -m venv .venv && source .venv/bin/activate && pip install -r tools/DataValidator/requirements.txt`
- イベント断片のマージ（`content/events/` → `content/events.yaml` / `.json`）
  - `python tools/ContentTools/merge_events.py`
- ティア / デバイス / 作物 / 契約の JSON エクスポート
  - `python tools/ContentTools/export_tiers_json.py`
  - `python tools/ContentTools/export_content_json.py`
- コンテンツ検証（スキーマ/参照/整合性）
  - `python tools/DataValidator/validate.py`
- Unity プロジェクトへのコンテンツ同期
  - `CONTENT_ROOT=content python tools/ContentTools/sync_content.py`

メモ: `content/events.yaml` は生成物です。直接編集せず、`content/events/` 配下の断片を編集してからマージしてください。

### コンテンツ作業フロー

- 変更は YAML をソースに行い、JSON は常にスクリプトで再生成します。
- 生成物（`content/*.json`, `content/events.yaml` など）は手編集しないでください。
- 変更前後で `validate.py` を実行し、CI の `Validate Content` が通ることを確認します。

### テスト

- フレームワーク: NUnit（`game/tests`）。テストファイルは `*Tests.cs` サフィックス。
- ランダム性は固定シードで決定的に保ちます。境界ケース（`core` の数理、ディレクター/予算ロジック）を重視。
- Unity テストは `game/ProjectSettings/ProjectVersion.txt` が存在する場合にのみ CI で実行されます（`unity-tests.yml`）。ローカルでは Unity Test Runner を使用してください。

### コーディング規約

- 一般: LF、最終改行あり、行末空白なし（`.editorconfig` 準拠）。
- インデント: C# は 4 スペース、YAML/JSON は 2 スペース。
- C#: `System` の `using` を先頭に、複数行ブロックは必ず波括弧。型/メソッドは `PascalCase`、ローカル/フィールドは `camelCase`。1 ファイル 1 公開型でファイル名と一致。
- コンテンツ ID: `snake_case` の ASCII、変更不可（不変）。`content/ids.yaml` に登録。ティアは `content/tiers/T#.yaml`。イベント断片は `content/events/{common,T1..T6}.yaml`。

### コミット / プルリクエスト

- コミットは簡潔・命令形（例: `content: add T3 wind tweaks`）。
- PR には要約、関連 Issue、変更の意図を記載。`Validate Content` CI を通してください。ゲームプレイ/UI 変更にはスクリーンショット/ログを添付。`.github/PULL_REQUEST_TEMPLATE.md` に従い、レビューは `CODEOWNERS` によりリクエストされます。

### セキュリティと設定

- 機密情報はコミットしないでください。
- 生成された JSON は編集せず、必ずスクリプトで再生成します。
- 必要に応じて `CONTENT_ROOT` を用いて Unity へ別のコンテンツルートを指示できます。
