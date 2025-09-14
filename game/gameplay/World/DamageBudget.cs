using System;
using System.Collections.Generic;

namespace Nogue.Gameplay.World
{
    public enum BudgetCat { Yield, Quality, Funds, Equipment, Pathogen }

    public sealed class DamageBudget
    {
        public readonly Dictionary<BudgetCat, float> MaxDay = new();
        public readonly Dictionary<BudgetCat, float> RemDay = new();
        public readonly Dictionary<BudgetCat, float> MaxSeason = new();
        public readonly Dictionary<BudgetCat, float> RemSeason = new();

        public void Init(int patchCount, float betaYield, float betaQuality, float betaFunds, float betaEquipment, float betaPathogen, float difficultyCoef)
        {
            float rootP = MathF.Sqrt(MathF.Max(1, patchCount));
            void Set(BudgetCat cat, float beta)
            {
                float maxD = beta * rootP * difficultyCoef;
                MaxDay[cat] = RemDay[cat] = maxD;
                float maxS = 6f * maxD;
                MaxSeason[cat] = RemSeason[cat] = maxS;
            }
            Set(BudgetCat.Yield, betaYield);
            Set(BudgetCat.Quality, betaQuality);
            Set(BudgetCat.Funds, betaFunds);
            Set(BudgetCat.Equipment, betaEquipment);
            Set(BudgetCat.Pathogen, betaPathogen);
        }

        public void ResetDay()
        {
            foreach (var cat in MaxDay.Keys)
                RemDay[cat] = MaxDay[cat];
        }

        public bool WouldExceed(Dictionary<BudgetCat, float> predicted)
        {
            foreach (var kv in predicted)
            {
                var cat = kv.Key; var val = kv.Value;
                if (!RemDay.ContainsKey(cat) || !RemSeason.ContainsKey(cat)) continue;
                if (val > RemDay[cat] || val > RemSeason[cat]) return true;
            }
            return false;
        }

        public void Consume(Dictionary<BudgetCat, float> actual)
        {
            foreach (var kv in actual)
            {
                var cat = kv.Key; var val = kv.Value;
                if (!RemDay.ContainsKey(cat) || !RemSeason.ContainsKey(cat)) continue;
                RemDay[cat] = MathF.Max(0f, RemDay[cat] - val);
                RemSeason[cat] = MathF.Max(0f, RemSeason[cat] - val);
            }
        }

        public float Remaining(BudgetCat cat) => RemDay.TryGetValue(cat, out var v) ? v : 0f;
    }
}

