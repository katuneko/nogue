using System;
using System.Collections.Generic;
using System.Linq;

namespace Nogue.Gameplay.Director
{
    public sealed class Director
    {
        public sealed record ScoreRow(IEventCandidate Evt, double Score);

        public sealed class Weights
        {
            public double Danger = 0.45;
            public double Pedagogy = 0.25;
            public double Novelty = 0.15;
            public double Diversity = 0.10;
            public double Contract = 0.05;
            public double BudgetPenalty = 0.3; // multiplier if exceeds budget
        }

        private readonly Weights _w;
        private readonly double _epsilonDefault;
        private readonly int _kDefault;
        private readonly int _reservedContractDefault;

        public Director(Weights? w = null, double epsilonDefault = 0.08, int kDefault = 3, int reservedContractDefault = 0)
        {
            _w = w ?? new Weights();
            _epsilonDefault = epsilonDefault;
            _kDefault = kDefault;
            _reservedContractDefault = reservedContractDefault;
        }

        public static Director FromConfig(DirectorConfig cfg)
        {
            var w = new Weights
            {
                Danger = cfg.Weights.Danger,
                Pedagogy = cfg.Weights.Pedagogy,
                Novelty = cfg.Weights.Novelty,
                Diversity = cfg.Weights.Diversity,
                Contract = cfg.Weights.Contract
            };
            return new Director(w, cfg.Epsilon, cfg.K, cfg.ReservedSlots.ContractCritical);
        }

        public List<IEventCandidate> Select(IReadOnlyList<IEventCandidate> candidates, IWorldState world)
        {
            if (candidates == null || candidates.Count == 0) return new List<IEventCandidate>();

            // 1) Score all
            var scored = new List<ScoreRow>(candidates.Count);
            foreach (var c in candidates)
            {
                double danger = Clamp01(c.BaseDanger);
                double pedagogy = Clamp01(c.Pedagogy);
                double novelty = 1.0 - Clamp01(c.RepetitionPenalty) - (world.HasSeenNovelty(c.NoveltyKey) ? 0.25 : 0.0);
                novelty = Math.Max(0, novelty);
                double diversity = 1.0; // actual diversity boost applied later against partial selection
                // Contract importance boosts: map 0..1 -> 1.0..1.2
                double contract = 1.0 + (Clamp01(c.ContractImportance) * 0.2);

                double solvable = world.IsSolvableNow(c) ? 1.0 : 0.4;
                double baseScore = (_w.Danger * danger) + (_w.Pedagogy * pedagogy) + (_w.Novelty * novelty) + (_w.Contract * contract);
                double s = baseScore * solvable;
                if (world.ExceedsDamageBudget(c)) s *= _w.BudgetPenalty;
                scored.Add(new ScoreRow(c, s));
            }

            // 2) Reserved slots (e.g., contract critical) — simple preselection by category
            var selected = new List<IEventCandidate>();
            int reserveContract = Math.Max(world.GetReservedSlots("contract_critical"), _reservedContractDefault);
            if (reserveContract > 0)
            {
                var pool = scored.Where(r => r.Evt.IsContractCritical)
                                  .OrderByDescending(r => r.Score)
                                  .Take(reserveContract)
                                  .Select(r => r.Evt);
                selected.AddRange(pool);
            }

            // 3) Diversity-aware greedy fill for remaining slots
            int k = world.K > 0 ? world.K : _kDefault;
            int slots = Math.Max(0, k - selected.Count);
            var remaining = new List<ScoreRow>(scored.Where(r => !selected.Contains(r.Evt)));

            // helper: prefer different event types
            bool ContainsType(EventType t) => selected.Any(e => e.Type == t);
            double DiversityBonus(IEventCandidate e)
            {
                // Boost if this type not yet in selection
                return ContainsType(e.Type) ? 1.0 : 1.1; // small boost
            }

            for (int i = 0; i < slots && remaining.Count > 0; i++)
            {
                var pick = remaining
                    .OrderByDescending(r => r.Score * DiversityBonus(r.Evt))
                    .First();
                selected.Add(pick.Evt);
                remaining.RemoveAll(r => r.Evt == pick.Evt);
            }

            // 4) ε-greedy novelty injection: replace last slot with highest unseen occasionally
            double eps = world.DirectorEpsilon > 0 ? world.DirectorEpsilon : _epsilonDefault;
            if (selected.Count > 0 && eps > 0)
            {
                var unseen = scored.Where(r => !world.HasSeenNovelty(r.Evt.NoveltyKey))
                                   .OrderByDescending(r => r.Score)
                                   .Select(r => r.Evt)
                                   .FirstOrDefault();
                if (unseen != null && RandomShared(world).NextDouble() < eps)
                {
                    selected[selected.Count - 1] = unseen;
                }
            }

            // Clip to K
            if (selected.Count > k)
                selected = selected.Take(k).ToList();

            return selected;
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        // Lightweight deterministic random hook point; to be wired to world RNG later.
        private static readonly System.Random _fallback = new System.Random(12345);
        private static System.Random RandomShared(IWorldState _) => _fallback;
    }
}
