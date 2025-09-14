using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nogue.Gameplay.World
{
    public sealed class TierDirectorOverrides
    {
        public int? K { get; set; }
        public double? Epsilon { get; set; }
        public int? ReservedContract { get; set; }
    }

    public static class TierConfigLoader
    {
        public static TierDirectorOverrides LoadDirectorForTier(string tiersJsonPath, int tier)
        {
            var result = new TierDirectorOverrides();
            if (!File.Exists(tiersJsonPath)) return result;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(tiersJsonPath));
                string key = $"T{tier}";
                if (doc.RootElement.TryGetProperty(key, out var tnode))
                {
                    if (tnode.TryGetProperty("director", out var dnode))
                    {
                        if (dnode.TryGetProperty("K", out var k)) result.K = k.GetInt32();
                        if (dnode.TryGetProperty("epsilon", out var eps)) result.Epsilon = eps.GetDouble();
                        if (dnode.TryGetProperty("reserved_slots", out var rs) && rs.TryGetProperty("contract_critical", out var cc))
                            result.ReservedContract = cc.GetInt32();
                    }
                }
            }
            catch { /* ignore in skeleton */ }
            return result;
        }
    }
}

