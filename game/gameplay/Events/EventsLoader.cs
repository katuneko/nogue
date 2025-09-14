using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nogue.Gameplay.Events
{
    public static class EventsLoader
    {
        public static List<EventDTO> LoadDTOs(string eventsJsonPath)
        {
            var list = new List<EventDTO>();
            if (!File.Exists(eventsJsonPath)) return list;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(eventsJsonPath));
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        var dto = new EventDTO
                        {
                            id = el.GetProperty("id").GetString() ?? string.Empty,
                            type = el.TryGetProperty("type", out var t) ? (t.GetString() ?? "micro") : "micro",
                            novelty_key = el.TryGetProperty("novelty_key", out var nk) ? (nk.GetString() ?? "") : "",
                            base_danger = el.TryGetProperty("score", out var sc) && sc.TryGetProperty("base_danger", out var bd) ? bd.GetDouble() : (el.TryGetProperty("base_danger", out var bd2) ? bd2.GetDouble() : 0.0),
                            pedagogy = el.TryGetProperty("score", out var sc2) && sc2.TryGetProperty("pedagogy", out var pg) ? pg.GetDouble() : (el.TryGetProperty("pedagogy", out var pg2) ? pg2.GetDouble() : 0.0),
                            repetition_penalty = el.TryGetProperty("score", out var sc3) && sc3.TryGetProperty("repetition_penalty", out var rp) ? rp.GetDouble() : (el.TryGetProperty("repetition_penalty", out var rp2) ? rp2.GetDouble() : 0.0),
                            contract_critical = el.TryGetProperty("score", out var sc4) && sc4.TryGetProperty("contract_boost", out var cb) ? cb.GetDouble() > 1.0 : (el.TryGetProperty("contract_critical", out var cc) && cc.GetBoolean()),
                            tier_min = el.TryGetProperty("tier_min", out var tm) ? tm.GetInt32() : 1,
                            tier_max = el.TryGetProperty("tier_max", out var tmax) ? tmax.GetInt32() : 6,
                        };
                        if (el.TryGetProperty("loss_profile", out var lp))
                        {
                            dto.loss_profile = new LossProfileDTO
                            {
                                yield = lp.TryGetProperty("yield", out var y) ? y.GetDouble() : 0.0,
                                quality = lp.TryGetProperty("quality", out var q) ? q.GetDouble() : 0.0,
                                funds = lp.TryGetProperty("funds", out var f) ? f.GetDouble() : 0.0,
                                equipment = lp.TryGetProperty("equipment", out var eq) ? eq.GetDouble() : 0.0,
                                pathogen = lp.TryGetProperty("pathogen", out var p) ? p.GetDouble() : 0.0,
                            };
                        }
                        list.Add(dto);
                    }
                }
            }
            catch { /* ignore parse errors in skeleton */ }
            return list;
        }
    }
}
