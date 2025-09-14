using System;
using System.Collections.Generic;

namespace Nogue.Gameplay.World
{
    public enum ApCat { Tilling, Seeding, Watering, Weeding, Harvest, Shipping, Trenching, Windbreak, Mulch, Burn, Repair }

    public static class ApEstimator
    {
        private static readonly Dictionary<ApCat, int> Base = new()
        {
            {ApCat.Tilling,2},{ApCat.Seeding,1},{ApCat.Watering,1},{ApCat.Weeding,1},
            {ApCat.Harvest,3},{ApCat.Shipping,1},{ApCat.Trenching,2},{ApCat.Windbreak,3},
            {ApCat.Mulch,1},{ApCat.Burn,2},{ApCat.Repair,2}
        };

        public static int Estimate(ApCat cat, PatchState patch, float designFactor = 1.0f)
        {
            float baseAp = Base[cat];
            float mult = 1.0f;

            // Device multiplicative savings
            foreach (var dev in patch.Devices)
            {
                if (dev.ApSavings.TryGetValue(cat, out float a))
                    mult *= (1.0f - MathF.Max(0f, MathF.Min(0.95f, a)));
            }
            mult *= designFactor;
            int ap = (int)MathF.Ceiling(baseAp * mult);
            return Math.Max(1, ap);
        }
    }
}

