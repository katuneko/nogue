using System.Collections.Generic;
using Nogue.Gameplay.Director;

namespace Nogue.Gameplay.World
{
    public sealed class WorldState : IWorldState
    {
        public int Tier { get; private set; }
        public int K { get; private set; }
        public int APRemaining { get; private set; }
        public double DirectorEpsilon { get; private set; }

        private readonly HashSet<string> _seenNovelty = new HashSet<string>();
        private readonly Dictionary<string, int> _reserved = new Dictionary<string, int>();

        public WorldState(int tier, int k, int apRemaining, double epsilon, int reservedContract)
        {
            Tier = tier;
            K = k;
            APRemaining = apRemaining;
            DirectorEpsilon = epsilon;
            _reserved["contract_critical"] = reservedContract;
        }

        public bool ExceedsDamageBudget(IEventCandidate e) => false; // TODO: wire to budget

        public bool IsSolvableNow(IEventCandidate e)
        {
            // TODO: heuristic/DFS. For now, assume true.
            return true;
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
    }
}

