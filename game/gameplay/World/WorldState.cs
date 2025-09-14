using System.Collections.Generic;
using Nogue.Gameplay.Director;
using Nogue.Gameplay.Contracts;
using Nogue.Core;

namespace Nogue.Gameplay.World
{
    public sealed class WorldState : IWorldState
    {
        public int Tier { get; private set; }
        public int K { get; private set; }
        public int APRemaining { get; private set; }
        public double DirectorEpsilon { get; private set; }
        public int Day => _clock.Day;
        public ContractsState? Contracts { get; private set; }

        private readonly HashSet<string> _seenNovelty = new HashSet<string>();
        private readonly Dictionary<string, int> _reserved = new Dictionary<string, int>();
        private readonly DamageBudget _budget = new DamageBudget();
        private readonly WorldInventory _inventory;
        private readonly GameClock _clock = new GameClock();

        public int PatchCount { get; private set; } = 1;
        public Difficulty Difficulty { get; private set; } = Difficulty.Standard;

        public WorldState(int tier, int k, int apRemaining, double epsilon, int reservedContract, WorldInventory? inventory = null, int patchCount = 1, Difficulty difficulty = Difficulty.Standard)
        {
            Tier = tier;
            K = k;
            APRemaining = apRemaining;
            DirectorEpsilon = epsilon;
            _reserved["contract_critical"] = reservedContract;
            _inventory = inventory ?? WorldInventory.Default();
            PatchCount = patchCount;
            Difficulty = difficulty;
        }

        public bool ExceedsDamageBudget(IEventCandidate e)
        {
            var predicted = LossPredictor.ForCandidate(e);
            return _budget.WouldExceed(predicted);
        }

        public bool IsSolvableNow(IEventCandidate e)
        {
            if (e is Events.EventCandidate ec)
                return SolvableNow.Evaluate(ec, this, _inventory, null, 0.5f);
            return true;
        }

        public void InitializeBudgets(BudgetConfig cfg)
        {
            float coef = cfg.CoefFor(Difficulty);
            _budget.Init(PatchCount, cfg.BetaYield, cfg.BetaQuality, cfg.BetaFunds, cfg.BetaEquipment, cfg.BetaPathogen, coef);
        }

        public void BeginDay() => _budget.ResetDay();

        public void OnEventResolved(IEventCandidate e, float severity = 1.0f)
        {
            var actual = LossPredictor.ForCandidate(e, severity);
            _budget.Consume(actual);
        }

        public bool HasSeenNovelty(string noveltyKey)
        {
            if (string.IsNullOrEmpty(noveltyKey)) return false;
            return _seenNovelty.Contains(noveltyKey);
        }

        public void MarkShown(IEnumerable<IEventCandidate> shown)
        {
            foreach (var c in shown)
                if (!string.IsNullOrEmpty(c.NoveltyKey)) _seenNovelty.Add(c.NoveltyKey);
        }

        public int GetReservedSlots(string category)
        {
            return _reserved.TryGetValue(category, out var v) ? v : 0;
        }

        // 簡易な期待生産フック（後で実装差替）
        public int ForecastExpectedOutput(string productId, int days) => 0;

        public void InitContracts(IEnumerable<ContractDTO> dtos)
        {
            Contracts = new ContractsState(() => Day);
            foreach (var d in dtos)
                Contracts.Add(d, startDay: Day);
        }
    }
}
