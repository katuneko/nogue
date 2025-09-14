using System;
using System.IO;
using System.Text.Json;

namespace Nogue.Gameplay.Director
{
    public sealed class DirectorConfig
    {
        public string Difficulty { get; set; } = "standard";
        public WeightsConfig Weights { get; set; } = new WeightsConfig();
        public double Epsilon { get; set; } = 0.08;
        public int K { get; set; } = 3;
        public ReservedSlotsConfig ReservedSlots { get; set; } = new ReservedSlotsConfig();
    }

    public sealed class WeightsConfig
    {
        public double Danger { get; set; } = 0.45;
        public double Pedagogy { get; set; } = 0.25;
        public double Novelty { get; set; } = 0.15;
        public double Diversity { get; set; } = 0.10;
        public double Contract { get; set; } = 0.05;
    }

    public sealed class ReservedSlotsConfig
    {
        public int ContractCritical { get; set; } = 0; // maps from contract_critical
    }

    public static class DirectorConfigLoader
    {
        public static DirectorConfig LoadOrDefault(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    // map snake_case to properties
                    using var doc = JsonDocument.Parse(json);
                    var cfg = new DirectorConfig();
                    var root = doc.RootElement;
                    if (root.TryGetProperty("difficulty", out var diff)) cfg.Difficulty = diff.GetString() ?? cfg.Difficulty;
                    if (root.TryGetProperty("epsilon", out var eps)) cfg.Epsilon = eps.GetDouble();
                    if (root.TryGetProperty("K", out var k)) cfg.K = k.GetInt32();
                    if (root.TryGetProperty("weights", out var w))
                    {
                        cfg.Weights = new WeightsConfig
                        {
                            Danger = w.TryGetProperty("danger", out var a) ? a.GetDouble() : 0.45,
                            Pedagogy = w.TryGetProperty("pedagogy", out var b) ? b.GetDouble() : 0.25,
                            Novelty = w.TryGetProperty("novelty", out var c) ? c.GetDouble() : 0.15,
                            Diversity = w.TryGetProperty("diversity", out var d) ? d.GetDouble() : 0.10,
                            Contract = w.TryGetProperty("contract", out var e) ? e.GetDouble() : 0.05,
                        };
                    }
                    if (root.TryGetProperty("reserved_slots", out var rs))
                    {
                        cfg.ReservedSlots = new ReservedSlotsConfig
                        {
                            ContractCritical = rs.TryGetProperty("contract_critical", out var ccrit) ? ccrit.GetInt32() : 0
                        };
                    }
                    return cfg;
                }
            }
            catch (Exception)
            {
                // ignore and fall through
            }
            return new DirectorConfig();
        }
    }
}

