# 0. デザイン原則（再掲・短縮／固定仕様の反映）

* **不動点**：

  1. 1アクション＝局所タグ更新、タグ次元は固定。
  2. 圃場（Patch）単位の独立性＋橋エッジ次数制限（⽔/風/生態 各≤2）。
  3. 1日AP（手数）は固定。提示イベント数Kは**難易度で2〜4に固定（既定3）**し、ラン中は不変。
  4. 被害予算・再出現ペナルティ・“対処可能性優先（solvableNow）”ディレクター。
  5. 勝敗：契約達成で勝ち、資金ショック/疫病暴走で負け（資金<0はグレース2日）。
* **スケール**：圃場数は2^Lで増やす（指数）。各ティアで新メソシステムを1つだけ追加（線形）。
* **体感負荷の一定化**：自動化と設計で圃場の維持APを段階的に半減し、圃場数の指数増と相殺して総APは概ね一定に収束（下限APを設ける）。

---

## 0.1 固定仕様（初期既定値の要約）

- プラットフォーム/エンジン：PC（Windows10+/macOS13+）。Unity（URP/2D Tilemap、Entities/DOTS想定）。
- グリッド/近傍：Tile=1m、Patch=16×16 tiles。`r=1`はマンハッタン（4近傍）。風/胞子はMoore（8近傍）。
- AP基準：`AP_base=10`/Patch/日、`AP_min=1`。維持APは乗算合成（詳細は4.3/1.6）。
- K（提示件数）：難易度で固定（易2/標準3/難4）。
- 被害予算：カテゴリ別日次・季節枠。超過はスコア減衰＋夜間クランプ（詳細は1.6/7）。
- 再現性：xoshiro128++、階層Seed、Q16.16固定小数点で決定的再現。
- UI（MVP）：タグ変化オーバーレイ、「今日の3件」＋根拠、風/水レイヤ切替、余波スタック（上位3）。

# 1. データモデル（基盤仕様）

## 1.1 タグ（最小集合）

* **環境**：`湿度(0..5), 温度帯(寒/温/暑), pH(酸/中/アルカリ), 肥沃度NPK(0..5), 日照(0..5)`
* **物理**：`高さ(-2..+2), 透水性(0..2), 可燃性(0..2), 風影(0..2)`
* **生態**：`花粉源(0..2), 害虫餌(0..2), 天敵棲み家(0..2), 菌根友好度(-1..+2), 被覆(裸地/草/マルチ)`
* **状態**：`清潔/汚染(0..2), 病原圧(0..5), 種子バンク(0..5)`

> **実装ルール**：新システムは**このタグに対する作用式**のみを追加。タグ次元は**増やさない**。

### 内部表現の補足
- 離散値（0..5 等）は整数、連続量はQ16.16固定小数点で保持。
- カテゴリ（温度帯など）はEnum→係数へ写像して計算。

## 1.2 作物（Prototype）

* `根深度(浅/中/深), 乾燥耐性(0..2), 耐寒(0..2), 耐暑(0..2), N要求(0..2), pH適性(酸/中/アル), 菌根親和(0..2), 花期(早/普/遅), 交配型(自/他/混), 成長日数(ターン換算)`

## 1.3 圃場（Patch）とグラフ

* `Patch{id, tiles[], devices[], crops[], local_budget(AP), risk, tension, contractsLocal[], edges{water[], wind[], bio[]}}`
* 橋エッジは各レイヤで**最大2**。水は高→低に流れ、風は風上→風下、⽣態は回廊で接続。候補>2の場合は `score = cap_norm × (1/(1+dist)) × stability` の上位2を採用（stabilityは過去事故率の逆数）。

## 1.4 行為（Action）

* `耕す/播種/水やり/除草/火入れ/マルチ/溝掘り/畝立て/設備設置/設備操作/収穫/出荷/交渉/修理`
* **更新原子**：`apply(Action) -> ΔTag(Patch, r=1), enqueue(余波)`

### APコスト（初期値）
- 耕す2、播種1、水やり1、除草1、収穫3、出荷1、溝掘り2、防風林設置3、マルチ1、火入れ2、修理2。
- 装置補正：該当工程のAP×(1-α_device)。

## 1.5 余波（Cascade）

* タグ変化によって生じる**二次結果**（例：湿度↑→ナメクジ候補↑）。
* 表示は**上位3連鎖**のみをUIに明示（認知負荷固定）。内部計算は**最大深さ5**。深度4は効果0.5倍、深度5は遮断。

## 1.6 固定パラメータと式（初期）

- 維持AP：`AP_patch = max(AP_min, AP_base × ∏(1 - α_device) × design_factor)`（`AP_base=10, AP_min=1`）。
  - 代表α：スプリンクラー0.50、帯マルチ0.50、物流自動化0.50、生態自走0.50。
- ティア目標：成熟済みPatchの維持APは概ね `AP_base × 2^{-(L-1)}` を目安（下限1）。
- 被害予算（カテゴリcat）：`B_day(cat) = β_cat × √P × V`、`B_season(cat) = 6×B_day(cat)`
  - β：収量0.10、品質0.10、資金0.07、設備0.03、病原圧は日次+2まで。
  - V：難易度係数（易1.2/標準1.0/難0.8）。
- 価格係数：`f(q) = 1 + 1.2×(q/100)^1.5`（上限2.2）。UIグレード：C:0–59、B:60–79、A:80–100。
- 風減衰：`intensity_next = 0.6 × intensity_here × seasonCoeff`（季節±20%）。
- PRNG：xoshiro128++。`RunSeed → DaySeed → EventSeed` の階層。

---

# 2. ティア別 仕様書（T1〜T6）

> 表では、**指数（圃場数）**と**線形（新システム1つ）**、**自動化倍率**、**提示イベントK**、**成功/失敗条件**、**具体コンテンツ**を列挙。
> 目標は**実装の迷いを減らす**こと。数値は初期指標（調整可）。

## T1：単圃場・基礎タグ

* **圃場数**：1
* **新システム（線形）**：なし（タグ学習に集中）
* **提示イベントK**：**難易度で固定**（易2/標準3/難4）
* **自動化倍率**：1.0（なし）
* **行為**：耕す/播種(麦・豆・葉菜)/水やり/除草/マルチ/火入れ/収穫/出荷
* **代表イベント**（ミクロ）：病兆（うどんこ前兆/葉色変化）/受粉チャンス（葉菜は低）/小雨/小相場波。
* **マクロ**：豪雨OR干ばつ（どちらか片方のみ）。
* **契約**：単発小口（量少/品質C以上/納期緩め）。
* **勝ち**：2件以上の契約を納期内達成。
* **負け**：資金<0が**2日連続**（初日は緊急融資イベント発生） or 病原圧5が一定日数継続。
* **楽しい要素**：

  * “1手＝世界がわずかに動く”納得感。
  * 余波スタックで**因果が見える**。
* **開発注記**：T1は**UI/手触りの完成度**を最優先。タグ変更のオーバーレイは必須。

## T2：用水（溝・貯留・透水性）

* **圃場数**：2（指数）
* **新システム**：水利（溝/堤/貯留槽/透水性差）
* **自動化倍率**：0.5（スプリンクラー解禁、α=0.5）
* **提示イベントK**：3
* **代表イベント**：表土流亡（斜面・豪雨）/貯留槽の溢水/水脈発見。
* **契約**：小口複数（品質C〜B、納期普通）。
* **勝ち**：3件の契約達成＋赤字日なし。
* **楽しい要素**：**地形を読む**快感（溝設計で圃場が安定）。
* **UI**：水の流向矢印、透水性ヒートマップ。

## T3：風/障壁（防風林・風路）

* **圃場数**：4
* **新システム**：風（防風林、風窓、胞子飛散、乾燥）
* **自動化倍率**：0.5（継続、帯マルチα=0.5により**実質0.25相当**の圃場も）
* **提示イベントK**：3
* **代表イベント**：風下の病原圧上昇/花粉飛散ボーナス/乾燥障害。
* **契約**：品質B指定の定量（期日タイト）。
* **勝ち**：風害期に品質B以上で2件完走。
* **楽しい要素**：**風を“設計”して味方にする**。
* **UI**：風向レイヤ、風影可視化。

## T4：市場拡張（定期契約・品質係数）

* **圃場数**：8
* **新システム**：定期契約、品質係数（品質→単価の逓増）
* **自動化倍率**：0.25（自動出荷ルート/納品カレンダー）
* **提示イベントK**：3（**契約関連を優先提示**）
* **代表イベント**：契約先の緊急増量/代替品提案/違約ペナルティ警告。
* **契約**：週次納品（3週連続、欠品0）。
* **勝ち**：定期3週無欠品＋黒字。
* **楽しい要素**：**理解→品質→お金**が直結。意思決定が“経営”に昇華。
* **UI**：「今日やるべき3件」自動抽出、品質期待値プレビュー。

## T5：生態網（天敵回廊・受粉ネット）

* **圃場数**：16
* **新システム**：生態橋（天敵/受粉回廊）※各ノード次数≤2
* **自動化倍率**：0.125（放し飼い最適化、捕食圧の自然制御）
* **提示イベントK**：3
* **代表イベント**：回廊切断/過剰捕食/受粉ボーナスの波。
* **契約**：品質係数高倍率（A到達で単価跳ねる）。
* **勝ち**：品質Aを一定出荷＋契約無欠品。
* **楽しい要素**：**自然が“働く”盤面づくり**の醍醐味。
* **UI**：回廊可視化、捕食圧ヒートマップ。

## T6：遺伝（交配・系統固定）

* **圃場数**：32
* **新システム**：交配/系統固定（遺伝タグの組合せで新特性）
* **自動化倍率**：0.125（農場はほぼ自走、プレイヤーは**設計者**へ）
* **提示イベントK**：3（**研究/試験区優先**）
* **代表イベント**：偶発特性出現/近縁交配リスク/市場の特注。
* **契約**：特注（“短茎×耐乾”の麦を指定期で納品）
* **勝ち**：特注契約達成＋新系統確立（図鑑登録）。
* **楽しい要素**：**知の勝利**。失敗が次ランの武器（知識図鑑/種子ライブラリ）。
* **UI**：交配プラン3択、期待特性の可視化（確率/影響タグ）。

---

# 3. 主要“楽しさ”の担保ポイント（ティア横断）

* **今日の3手**：ディレクターがK件提示（難易度で2〜4、既定3）。**迷わず着手できる**。
* **余波スタック（上位3連鎖）**：行為→結果→次の機会が**見える**。
* **対処可能性保証**：手持ちリソースで解けるイベントが基本。解けない場合は**準備イベント→本番**の二段階で出す。
* **被害予算**：理不尽な事故の**最大値を制限**。
* **橋次数制限**：連鎖の爆発的拡大を物理的に抑止。

---

# 4. 疑似コード（自動化・イベント選抜・一部コア）

## 4.1 日次更新ループ（概略）

```pseudocode
function DayTick(world):
    seed = world.dailySeed
    updateWeather(world, seed)

    // 1) 自然系更新（各Patchローカル）
    for patch in world.patches:
        applyPassiveFlows(patch)        // 蒸発, 浸透, 胞子拡散(制限)
        resolveBridges(patch, world)    // 水/風/生態の橋で限定伝播
        decayStates(patch)              // 被覆劣化, 病原自然減衰

    // 2) イベント候補の収集
    candidates = collectCandidates(world) // 病兆, 受粉, 契約, 来訪 など

    // 3) ディレクターで提示イベントKの選抜（被害予算超過候補は減衰）
    shown = directorSelect(candidates, K=world.K, world=world)

    // 4) プレイヤーフェーズ（AP固定）
    AP = world.dailyAP
    while AP > 0 and player.hasActions():
        action = player.chooseAction(shown, world.UIHints)
        AP -= action.cost
        apply(action)                   // r=1の局所タグ更新
        propagateLocalCascade(action)   // b分岐上限、上位3連鎖のみ可視化
        updateShownEvents(shown, world) // 状況変化に合わせ再スコア

    // 5) 夜間解決（収穫/繁殖/契約判定/自動化稼働）
    for patch in world.patches:
        runAutomation(patch)            // 自動化倍率に応じて維持AP代替
        resolveContracts(patch, world.market)
        applyNightlyGrowth(patch)
    
    enforceDamageBudget(world)          // 被害上限を最終チェック（超過は自動薄め）
    advanceDay(world)
```

## 4.2 イベント・ディレクター（“楽しさ重視”の核）

```pseudocode
function directorSelect(candidates, K, world):
    // スコア設計：危険度×（対処可能性）× 新規性 × 学習価値 × 多様性
    scored = []
    for c in candidates:
        danger = clamp(estimateDanger(c), 0, 1)
        solvable = isSolvableNow(c, world) ? 1.0 : 0.4   // 解けない候補は弱めて保持
        novelty  = 1.0 - repetitionPenalty(c, world)     // 同種連発抑制
        pedagogy = estimateLearningValue(c, world)        // 相互作用の“発見”に繋がるか
        diversity= typeDiversityBoost(c, scored)          // 種類が被らないよう加点
        contract = isContractCritical(c) ? 1.2 : 1.0      // 納期圧の優先
        score = (0.45*danger + 0.25*pedagogy + 0.15*novelty + 0.10*diversity + 0.05*contract) * solvable
        // 被害予算に触れる場合は減衰
        if exceedsDamageBudget(c, world): score *= 0.3
        scored.append((c, score))
    
    // 新鮮味の注入（ε-greedy的）：低確率で未体験タイプを1枠差し替え
    top = topK(scored, K)
    if random()<world.directorEpsilon:
        unseen = highestUnseen(scored, world.knowledge)
        if unseen: top[-1] = unseen
    
    return top
```

### solvableNow（定義）
```
function isSolvableNow(candidate, world):
    // 資源：必要APとアイテム/資金が即時満たせるか
    if totalAPNeeded(candidate) > world.APRemaining: return false
    if !resourcesAvailable(candidate): return false
    if !systemsUnlocked(candidate): return false
    // 納期：本日＋翌日以降の必要手数の見込みが期限内か
    if isContract(candidate) and !deadlineFeasible(candidate, horizon=2): return false
    // 簡易探索（深さ2）で成功確率≥0.7
    return greedyLookaheadSuccessProb(candidate, depth=2) >= 0.7
```

## 4.3 自動化（圃場の維持APを逓減）

```pseudocode
function runAutomation(patch):
    for device in patch.devices:
        if device.isOperational():
            effect = device.tickEffect()       // 例：湿度+1, 雑草-1 等
            applyTagEffect(patch, effect)
            device.consumeUpkeep()             // 維持コスト（資金/肥料/電力）
        else:
            maybeFailureEvent(device)          // 故障はミクロイベント候補へ

    // 自動化倍率に基づく維持評価（乗算合成）
    requiredAP = estimateManualAP(patch)
    effectiveFactor = (∏(1 - device.alpha) * patch.designFactor * world.tierAutomationFactor)
    patch.local_budget = max(world.AP_min, world.AP_base * effectiveFactor)
```

## 4.4 橋エッジの制限伝播

```pseudocode
function resolveBridges(patch, world):
    for edge in patch.bridges.water[0..1]:   // 最大2
        flowWater(patch, edge.to, limitedBy=heightDiff, capacity=edge.cap, decay=0.5)
    for edge in patch.bridges.wind[0..1]:
        spreadSpores(patch, edge.to, limit=world.windLimit, decay=0.6*seasonCoeff(world))  // 1ホップで減衰
    for edge in patch.bridges.bio[0..1]:
        migratePredators(patch, edge.to, cap=edge.cap, returnNextDay=true)       // 天敵の移動
```

## 4.5 交配（T6）

```pseudocode
function breed(parentA, parentB, envTags):
    // 遺伝タグの再配列（シンプル）：共有強特性を優先、環境シナジーを補正
    traits = recombine(parentA.traits, parentB.traits)
    synergy = evaluateEnvSynergy(traits, envTags)            // 乾燥地なら乾耐性に微ブースト
    if random()<mutationRate(traits, envTags):
        traits = applyBeneficialMutation(traits)             // 確率は低いが“気持ちよい”成功率
    return makeSeed(traits)
```

---

# 5. コンテンツ仕様（抜粋テーブル）

## 5.1 作物プロトタイプ（例）

| 名称 | 成長 | 根深度 | 乾燥耐性 | N要求 | pH適性 | 菌根 | 花期 | 交配 | 特色       |
| -- | -- | --- | ---- | --- | ---- | -- | -- | -- | -------- |
| 麦  | 中  | 中   | 1    | 1   | 中    | 1  | 普  | 混  | 風/乾燥と相性良 |
| 豆  | 中  | 中   | 0    | 0   | 中    | 2  | 普  | 自  | N固定で肥沃度↑ |
| 葉菜 | 早  | 浅   | 0    | 2   | 中    | 1  | 早  | 自  | 湿度↑で病気↑  |

## 5.2 設備（自動化）

| 設備      | 解禁 | 効果          | 維持    | 余波         |
| ------- | -- | ----------- | ----- | ---------- |
| スプリンクラー | T2 | 湿度+1/日（r=1） | 水/資金  | 真菌圧+微（α=0.5）      |
| 帯マルチ    | T3 | 蒸発↓・雑草↓     | 資材    | 真菌圧↑（風で緩和、α=0.5） |
| 防風林     | T3 | 風影+1・乾燥↓    | 成長時間  | 胞子飛散↓/受粉↓  |
| 受粉箱     | T5 | 受粉率↑        | 砂糖/維持 | 天敵誘引↑      |

### 装置の不足/劣化/故障（運用ルール）
- 資源不足→停止。2日連続不足で劣化（効果-50%）。劣化3回で故障（イベント）。
- 故障率：日次1%（メンテ後3日間は0.2%）。修理：2AP＋資金、当日は効果無効。

## 5.3 イベント（型）

* **病兆（ミクロ）**：`if 病原圧>2 && 湿度>3 -> 対処:通風/薬草/ローテーション`
* **受粉チャンス（ミクロ）**：`if 花期 && 風/蜜蜂活発 -> 収量・品質↑`
* **豪雨（マクロ）**：`p(季節)に応じ発生、流亡→下流肥沃度↑/上流↓`
* **契約増量（メソ）**：`納期圧↑、単価↑`（ディレクターで優先度高）

---

# 6. システム設計書（役割分担・ファイル構成）

## 6.1 アーキテクチャ方針

* **エンジン**：Unity（URP/2D Tilemap、Entities/DOTS互換）。
* **ECS（Entity-Component-System）**ベース（パフォーマンス/柔軟性/テスト容易性）。
* **データ駆動**：タグ/作物/設備/イベントは外部定義（YAML/JSON）。
* **レイヤ分割**：

  * ① **Core**：グリッド/タグ/確率/時刻/シリアライズ。
  * ② **Gameplay**：行為/余波/ディレクター/自動化/契約/経済/遺伝。
  * ③ **Presentation**：UI/オーバーレイ/ツールチップ/サウンド。
  * ④ **Content**：数値・定義・テキスト。
  * ⑤ **DevOps**：テスト/バランスクラウド/リプレイ。

## 6.2 役割分担（人）

* **リードデザイナー**：不動点・KPI・ティア要件の維持。
* **システムデザイナー**：イベント型・契約型・設備の相互作用設計。
* **ゲームプレイエンジニア**：ECS実装、Action/Cascade、Director、Automation。
* **テクニカルデザイナー**：データパイプライン、定義テーブル、バランス自動検証。
* **UI/UX**：余波スタック、3件提示、レイヤ可視化。
* **アナリスト**：テレメトリ、KPI、被害予算の実測調整。

## 6.3 ルート構成（Unity/C#）

```
/docs
  /GDD/ Fieldcrawler_T1-T6_spec.md
  /Tech/ Architecture.md, Balancing_KPIs.md
/game
  /core
    Grid.cs                      // グリッド・座標・近傍r
    Tags.cs                      // タグベクトル型と合成/制限
    RNG.cs                       // 乱数、階層Seed（日/季節/イベント）
    Time.cs                      // 日/季節/年管理
    Serialize.cs
  /ecs
    Entity.cs, Component.cs, System.cs
  /gameplay
    Actions/
      Action.cs, ActionDefs.yaml       // 行為→タグΔの式
    Cascade/
      CascadeSystem.cs, Limits.cfg     // b分岐上限/表示3連鎖
    Patches/
      PatchGraph.cs, Bridges.cs        // 水/風/生態エッジと次数制限
    Automation/
      Devices.cs, AutomationSystem.cs
    Events/
      EventTypes.cs, EventPool.yaml
      Director.cs, Scoring.cfg         // スコア重み・ε
      DamageBudget.cs
    Crops/
      CropDefs.yaml, GrowthSystem.cs
    Market/
      Contracts.cs, Pricing.cs
    Genetics/
      Breeding.cs, Traits.yaml
  /presentation
    UI/
      EventTray.cs           // 今日の3件
      CascadeLog.cs          // 余波スタック（上位3連鎖）
      Overlays/
        MoistureOverlay.cs, WindOverlay.cs, RiskOverlay.cs
    Audio/, VFX/
  /content
    locales/ja-JP/*.json
    crops.yaml, devices.yaml, events.yaml, contracts.yaml
    tiers/T1.yaml ... T6.yaml         // ティアごとの解禁/倍率/候補
  /tests
    Unit/
      Test_DirectorScores.cs
      Test_DamageBudget.cs
      Test_BridgeDegree.cs
      Test_AutomationHalving.cs
    Sim/
      MonteCarlo_Balancing.cs|py      // 自動プレイでAP/K/連鎖深さを監視（ヘッドレス）
/tools
  DataValidator/ schema.json
  Balancer/ export_csv.py
```

### 6.4 依存ルール

* `presentation` → `gameplay` 読み取りのみ。`gameplay`は`presentation`に依存しない。
* `gameplay`は`core`/`ecs`にのみ依存。
* `content`の変更は**再コンパイル不要**（ホットリロード可）。

### 6.5 コンテンツ記述例（YAML抜粋）

```yaml
# tiers/T3.yaml
unlock:
  systems: ["wind", "windbreak", "mulch_belt"]
  devices: ["windbreak_tree", "mulch_belt"]
director:
  K: 3
  epsilon: 0.08
automation:
  multiplier: 0.5
events_weight_adjust:
  disease_downwind: +0.2
  pollination_bonus: +0.15
```

---

# 7. バランス・KPI・自動検証

## 7.1 KPI（各ティアで満たすべき閾値）

* **1日平均提示イベント**：K±1（=2〜4に収束）
* **平均連鎖深さ**：≤3（表示は上位3）
* **総AP**：T1比で±20%以内
* **失敗率**：標準難度で35〜50%（再挑戦動機）
* **“発見”ログ**：新規学習（図鑑登録）1日1件以上

## 7.2 自動検証（シミュレータ）

* ヘッドレスSim（1スレッド）で**ナイトリー1万ラン**（開発ローカルは1千ラン）を実行し、KPI分布を監視。
* 外れ値が出た場合、`Director`重み±15%、`β_cat`±10%の範囲で自動微調整（クリップ）。閾値超過はCI失敗。

---

# 8. “今日の3件”抽出ロジック（UI連携）

```pseudocode
function todaysThree(world):
    pool = []
    pool += criticalContractsDueSoon(world)           // 納期が近い
    pool += highRiskPatches(world)                    // 病原圧↑, 流亡リスク
    pool += highLeverageOpportunities(world)          // 小手数で大きな効果
    // スコア付け（ディレクター準拠）
    return directorSelect(pool, K=world.K /* 難易度で2〜4固定 */, world)
```

---

# 9. 実装者向け “迷わない”指針（チェックリスト）

* **新要素を入れるとき**

  * タグ式：`Δ湿度/Δ病原圧/…` を**r=1**で定義。
  * **橋次数の上限**に触れないか？
  * **イベント候補**は`solvableNow`を満たす文脈で湧くか？
  * **表示は上位3連鎖**だけで十分伝わるか？（内部は深さ5、4以降は弱体化）
* **イベント追加**

  * 型（ミクロ/メソ/マクロ）を決め、`danger/pedagogy/novelty`を初期値で設定。
  * 同種連発の**repetitionPenaltyキー**を必ず記述。
* **契約追加**

  * 価格は品質係数（A> B > C）に**逓増**で連動。
  * ディレクターの`contractBoost`を調整し“悩ましいけど解ける”に。

---

# 10. 付録：サンプル疑似データ（イベント）

```yaml
- id: "disease_mildew_hint"
  type: "micro"
  trigger: "humidity>=4 AND mulch>=1"
  effect: "pathogen+1 on patch; show_hint('風通し')"
  score:
    base_danger: 0.6
    pedagogy: 0.7
    novelty_key: "disease_mildew"
    repetition_penalty: 0.4
    solvable_tags: ["remove_windbreak","open_path","apply_herb"]
- id: "contract_weekly_leafy"
  type: "meso"
  trigger: "tier>=4"
  effect: "set_weekly_order(qty=10, quality>=B)"
  score:
    base_danger: 0.5
    pedagogy: 0.5
    novelty_key: "weekly_contract"
    contract_boost: 1.2
```

---

## まとめ（固定仕様反映版）

* **T1〜T6**で指数（圃場数）×線形（新メソシステム1つ）を積み上げ、**不動点**（局所タグ更新・難易度固定K・橋次数制限・被害予算）で**破綻を防止**。
* **ディレクター**は`solvableNow`と多様性重視、被害予算と整合。**今日の3手**が常に明確。
* **自動化**は乗算合成＋下限APで総APを一定化。装置の不足/劣化/故障もイベント駆動。
* **システム設計**はUnity/ECS＋データ駆動で役割分担を明確化、実装の迷いを最小化。
