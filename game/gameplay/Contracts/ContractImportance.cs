using System;

namespace Nogue.Gameplay.Contracts
{
    public static class ContractImportance
    {
        // daysLeft: 残日, requiredRemaining: 今回必要量, expectedOutputWindow: 予測生産量
        public static float Compute(int daysLeft, int requiredRemaining, int expectedOutputWindow, int buffer = 2, float tau = 3f)
        {
            float dl = MathF.Max(0, daysLeft - buffer);
            float urgency = MathF.Exp(-dl / tau);                  // 0..1
            float shortage = (expectedOutputWindow <= 0)
                ? 1.2f
                : MathF.Max(1f, (float)requiredRemaining / MathF.Max(1f, expectedOutputWindow));
            float imp = urgency * MathF.Min(1.5f, shortage);
            if (imp < 0) imp = 0; if (imp > 1) imp = 1;
            return imp;
        }
    }
}

