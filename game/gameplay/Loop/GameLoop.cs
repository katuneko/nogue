using System.Collections.Generic;
using UnityEngine;
using Nogue.Core;
using Nogue.Gameplay.World;
using Nogue.Gameplay.Director;
using Nogue.Gameplay.Events;
using Nogue.Gameplay.Contracts;

namespace Nogue.Gameplay.Loop
{
    // Minimal, Editor-friendly loop to run one in-game day and log why picks were shown.
    public sealed class GameLoop : MonoBehaviour
    {
        [Header("Runtime")]
        [SerializeField] private bool autoResolveTop = true;
        [SerializeField] private Difficulty difficulty = Difficulty.Standard;
        [SerializeField] private int initialTier = 1;
        [SerializeField] private int dailyAP = 4; // decided value for Standard/T1

        [SerializeField] private Nogue.Presentation.UI.TodayTray? tray;

        private WorldState? _world;
        private Director? _director;
        private Xoshiro128PlusPlus? _rng;

        void Start()
        {
            Init();
            RunOneDay();
        }

        public void Init()
        {
            // 1) Resolve content paths
            string scoringPath = ContentPaths.DirectorScoringPath();
            string budgetPath = ContentPaths.BudgetJsonPath();
            string eventsJsonPath = ContentPaths.EventsJsonPath();
            string tiersJsonPath = ContentPaths.TiersJsonPath();
            string contractsJsonPath = ContentPaths.ContractsJsonPath();

            // 2) Load configs/content
            var scoring = DirectorConfigLoader.LoadOrDefault(scoringPath);
            var budgetCfg = BudgetConfig.LoadOrDefault(budgetPath);
            var tierOv = TierConfigLoader.LoadDirectorForTier(tiersJsonPath, initialTier);
            var contracts = ContractsLoader.Load(contractsJsonPath);

            // Apply tier overrides (tier takes precedence)
            int k = tierOv.K ?? scoring.K;
            double epsilon = tierOv.Epsilon ?? scoring.Epsilon;
            int reservedContract = Mathf.Max(scoring.ReservedSlots.ContractCritical, tierOv.ReservedContract ?? 0);

            // 3) Build world
            _rng = new Xoshiro128PlusPlus(12345UL); // placeholder seed; can be injected
            _world = new WorldState(
                tier: initialTier,
                k: k,
                apRemaining: dailyAP,
                epsilon: epsilon,
                reservedContract: reservedContract,
                inventory: WorldInventory.Default(),
                patchCount: 1,
                difficulty: difficulty);
            _world.InitializeBudgets(budgetCfg);
            _world.InitContracts(contracts);
            _world.BeginDay();

            // 4) Director from config (with overrides applied)
            var cfgEffective = new DirectorConfig
            {
                Difficulty = scoring.Difficulty,
                Weights = scoring.Weights,
                Epsilon = epsilon,
                K = k,
                ReservedSlots = new ReservedSlotsConfig { ContractCritical = reservedContract }
            };
            _director = Director.FromConfig(cfgEffective);
        }

        public void RunOneDay()
        {
            if (_world == null || _director == null) { Debug.LogWarning("[GameLoop] Init not completed."); return; }

            // A) Load event DTOs and adapt to candidates with world context
            var dtos = EventsLoader.LoadDTOs(ContentPaths.EventsJsonPath());
            var candidates = new List<IEventCandidate>(dtos.Count);
            foreach (var dto in dtos)
                candidates.Add(EventAdapter.ToCandidate(dto, _world));

            // B) Select K items
            var shown = _director.Select(candidates, _world);
            _world.MarkShown(shown);
            LogShown(shown);

            // Optionally display in Editor IMGUI tray
            if (tray != null)
            {
                var list = new List<IEventCandidate>(shown);
                var reasons = new List<string>(shown.Count);
                for (int i = 0; i < shown.Count; i++)
                {
                    var e = shown[i];
                    bool damped = _world.ExceedsDamageBudget(e);
                    bool solv = _world.IsSolvableNow(e);
                    reasons.Add($"danger={e.BaseDanger:F2}, pedagogy={e.Pedagogy:F2}, contractImp={e.ContractImportance:F2}, solvable={solv}, budget={(damped ? "damped" : "ok")}");
                }
                tray.SetItems(list, reasons);
                tray.OnPickIndex = idx =>
                {
                    if (idx < 0 || idx >= shown.Count) return;
                    var picked = shown[idx];
                    var outcome = ResolveOneAutomatically(picked);
                    float sev = EventResolution.InferSeverityFromOutcome(picked, outcome);
                    _world.OnEventResolved(picked, sev);
                };
            }

            // C) Player phase (temporary: auto-resolve top-1 if enabled)
            if (autoResolveTop && shown.Count > 0)
            {
                var picked = shown[0];
                var outcome = ResolveOneAutomatically(picked);
                float sev = EventResolution.InferSeverityFromOutcome(picked, outcome);
                _world.OnEventResolved(picked, sev);
            }

            // D) Night: growth tick etc.
            _world.NightlyGrowthTick();

            // E) Advance day and reset daily budget next morning
            _world.AdvanceDay();
            _world.BeginDay();
        }

        private ResolutionOutcome ResolveOneAutomatically(IEventCandidate c)
        {
            if (_world == null) return ResolutionOutcome.Success;
            // Minimal heuristic: solvable → Success, otherwise Partial
            return _world.IsSolvableNow(c) ? ResolutionOutcome.Success : ResolutionOutcome.SuccessPartial;
        }

        private void LogShown(IReadOnlyList<IEventCandidate> shown)
        {
            if (_world == null) return;
            Debug.Log($"[Director] Day {_world.Day} — picks={shown.Count}, K={_world.K}");
            for (int i = 0; i < shown.Count; i++)
            {
                var e = shown[i];
                bool damped = _world.ExceedsDamageBudget(e);
                bool solv = _world.IsSolvableNow(e);
                string reason = $"danger={e.BaseDanger:F2}, pedagogy={e.Pedagogy:F2}, contractImp={e.ContractImportance:F2}, solvable={solv}, budget={(damped ? "damped" : "ok")}";
                Debug.Log($"  {i + 1}. {e.Id} [{e.Type}] {(e.IsContractCritical ? "[CRITICAL]" : "")} :: {reason}");
            }
        }
    }
}
