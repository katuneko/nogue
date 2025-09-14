using System.Collections.Generic;
using Nogue.Gameplay.Director;
using Nogue.Gameplay.Events;

namespace Nogue.Gameplay.World
{
    public static class LossPredictor
    {
        public static Dictionary<BudgetCat, float> ForCandidate(IEventCandidate c, float severity = 1.0f)
        {
            var lp = DefaultByType(c);
            if (c is EventCandidate ec)
            {
                var l = ec.LossProfile;
                lp = new LossProfileFloat
                {
                    Yield = (float)l.Yield,
                    Quality = (float)l.Quality,
                    Funds = (float)l.Funds,
                    Equipment = (float)l.Equipment,
                    Pathogen = (float)l.Pathogen
                };
            }
            return new Dictionary<BudgetCat, float>
            {
                { BudgetCat.Yield, lp.Yield * severity },
                { BudgetCat.Quality, lp.Quality * severity },
                { BudgetCat.Funds, lp.Funds * severity },
                { BudgetCat.Equipment, lp.Equipment * severity },
                { BudgetCat.Pathogen, lp.Pathogen * severity },
            };
        }

        private static LossProfileFloat DefaultByType(IEventCandidate c)
        {
            return c.Type switch
            {
                EventType.Micro => new LossProfileFloat { Quality = 0.10f, Pathogen = 0.20f },
                EventType.Meso => new LossProfileFloat { Yield = 0.05f, Quality = 0.08f, Funds = 0.02f, Pathogen = 0.10f },
                EventType.Macro => new LossProfileFloat { Yield = 0.20f, Quality = 0.08f, Funds = 0.10f, Equipment = 0.03f, Pathogen = 0.10f },
                _ => new LossProfileFloat(),
            };
        }

        private struct LossProfileFloat
        {
            public float Yield, Quality, Funds, Equipment, Pathogen;
        }
    }
}

