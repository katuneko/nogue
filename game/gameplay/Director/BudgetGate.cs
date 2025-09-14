using Nogue.Gameplay.Events;

namespace Nogue.Gameplay.Director
{
    public sealed class DamageBudget
    {
        public double YieldRemaining { get; set; } = 1.0;
        public double QualityRemaining { get; set; } = 1.0;
        public double FundsRemaining { get; set; } = 1.0;
        public double EquipmentRemaining { get; set; } = 1.0;
        public double PathogenRemaining { get; set; } = 1.0;
    }

    public static class BudgetGate
    {
        public static bool WouldExceed(EventCandidate c, DamageBudget budget)
        {
            var lp = c.LossProfile;
            if (lp.Yield > 0 && budget.YieldRemaining < lp.Yield) return true;
            if (lp.Quality > 0 && budget.QualityRemaining < lp.Quality) return true;
            if (lp.Funds > 0 && budget.FundsRemaining < lp.Funds) return true;
            if (lp.Equipment > 0 && budget.EquipmentRemaining < lp.Equipment) return true;
            if (lp.Pathogen > 0 && budget.PathogenRemaining < lp.Pathogen) return true;
            return false;
        }
    }
}

