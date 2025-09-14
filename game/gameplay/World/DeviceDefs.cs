using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Nogue.Gameplay.World
{
    public sealed class DeviceDef
    {
        public string Id = string.Empty;
        public Dictionary<ApCat, float> ApSavings = new();
    }

    public static class DeviceDefsLoader
    {
        public static Dictionary<string, DeviceDef> Load(string devicesJsonPath)
        {
            var map = new Dictionary<string, DeviceDef>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(devicesJsonPath)) return map;
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(devicesJsonPath));
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        var id = el.TryGetProperty("id", out var pid) ? (pid.GetString() ?? string.Empty) : string.Empty;
                        if (string.IsNullOrEmpty(id)) continue;
                        var def = new DeviceDef { Id = id };
                        if (el.TryGetProperty("ap_savings", out var aps) && aps.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in aps.EnumerateObject())
                            {
                                if (TryMapApCat(prop.Name, out var cat))
                                {
                                    def.ApSavings[cat] = (float)prop.Value.GetDouble();
                                }
                            }
                        }
                        map[id] = def;
                    }
                }
            }
            catch { /* ignore */ }
            return map;
        }

        private static bool TryMapApCat(string key, out ApCat cat)
        {
            switch (key)
            {
                case "tilling": cat = ApCat.Tilling; return true;
                case "seeding": cat = ApCat.Seeding; return true;
                case "watering": cat = ApCat.Watering; return true;
                case "weeding": cat = ApCat.Weeding; return true;
                case "harvest": cat = ApCat.Harvest; return true;
                case "shipping": cat = ApCat.Shipping; return true;
                case "trenching": cat = ApCat.Trenching; return true;
                case "windbreak": cat = ApCat.Windbreak; return true;
                case "mulch": cat = ApCat.Mulch; return true;
                case "burn": cat = ApCat.Burn; return true;
                case "repair": cat = ApCat.Repair; return true;
                default: cat = ApCat.Watering; return false;
            }
        }
    }

    public sealed class DeviceInstance
    {
        public string Id { get; }
        public Dictionary<ApCat, float> ApSavings { get; } = new();
        public DeviceInstance(string id, Dictionary<ApCat, float> savings)
        { Id = id; ApSavings = savings; }
    }

    public sealed class PatchState
    {
        public List<DeviceInstance> Devices { get; } = new();
    }
}

