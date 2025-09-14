using System;
using System.IO;
using System.Text.Json;

namespace Nogue.Gameplay.World
{
    public sealed class BudgetConfig
    {
        public float BetaYield { get; set; } = 0.10f;
        public float BetaQuality { get; set; } = 0.10f;
        public float BetaFunds { get; set; } = 0.07f;
        public float BetaEquipment { get; set; } = 0.03f;
        public float BetaPathogen { get; set; } = 2.0f;

        public float CoefEasy { get; set; } = 1.2f;
        public float CoefStandard { get; set; } = 1.0f;
        public float CoefHard { get; set; } = 0.8f;

        public static BudgetConfig LoadOrDefault(string path)
        {
            var cfg = new BudgetConfig();
            try
            {
                if (!File.Exists(path)) return cfg;
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                var root = doc.RootElement;
                if (root.TryGetProperty("beta", out var beta))
                {
                    if (beta.TryGetProperty("yield", out var y)) cfg.BetaYield = (float)y.GetDouble();
                    if (beta.TryGetProperty("quality", out var q)) cfg.BetaQuality = (float)q.GetDouble();
                    if (beta.TryGetProperty("funds", out var f)) cfg.BetaFunds = (float)f.GetDouble();
                    if (beta.TryGetProperty("equipment", out var e)) cfg.BetaEquipment = (float)e.GetDouble();
                    if (beta.TryGetProperty("pathogen", out var p)) cfg.BetaPathogen = (float)p.GetDouble();
                }
                if (root.TryGetProperty("difficulty_coef", out var dc))
                {
                    if (dc.TryGetProperty("easy", out var ce)) cfg.CoefEasy = (float)ce.GetDouble();
                    if (dc.TryGetProperty("standard", out var cs)) cfg.CoefStandard = (float)cs.GetDouble();
                    if (dc.TryGetProperty("hard", out var ch)) cfg.CoefHard = (float)ch.GetDouble();
                }
            }
            catch { /* ignore */ }
            return cfg;
        }

        public float CoefFor(Difficulty d) => d switch
        {
            Difficulty.Easy => CoefEasy,
            Difficulty.Standard => CoefStandard,
            Difficulty.Hard => CoefHard,
            _ => CoefStandard
        };
    }
}

