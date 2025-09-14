using System.Collections.Generic;
using Nogue.Gameplay.Events;
using Nogue.Core;

namespace Nogue.Gameplay.World
{
    public static class SolvableNow
    {
        public sealed record ActionPlan(ApCat Cat, string? DeviceRequired = null, float TagDelta = 0f, string? Id = null);

        public static bool Evaluate(EventCandidate c, WorldState world, WorldInventory inv, PatchState? patch = null, float theta = 0.5f)
        {
            patch ??= BuildPatchFromInventory(inv);
            var cand = Suggest(c);
            if (cand.Count == 0) return true; // nothing required

            int apRemain = world.APRemaining;

            bool Feasible(ActionPlan a)
            {
                if (a.DeviceRequired != null && !inv.HasDevice(a.DeviceRequired)) return false;
                int ap = ApEstimator.Estimate(a.Cat, patch);
                return ap <= apRemain && inv.CanAfford(a);
            }

            // depth-1
            foreach (var a in cand)
                if (Feasible(a) && System.MathF.Abs(a.TagDelta) >= theta) return true;

            // depth-2 sequences
            for (int i = 0; i < cand.Count; i++)
            for (int j = i + 1; j < cand.Count; j++)
            {
                var a = cand[i]; var b = cand[j];
                int apA = ApEstimator.Estimate(a.Cat, patch);
                int apB = ApEstimator.Estimate(b.Cat, patch);
                if (apA + apB > apRemain) continue;
                float delta = a.TagDelta + b.TagDelta;
                if (System.MathF.Abs(delta) >= theta && inv.CanAfford(a, b)) return true;
            }

            return false;
        }

        public static List<ActionPlan> Suggest(Director.IEventCandidate c)
        {
            var list = new List<ActionPlan>();
            // Use solvable_tags when provided
            if (c is EventCandidate ec && ec.SolvableTags.Length > 0)
            {
                foreach (var tag in ec.SolvableTags)
                {
                    if (MapTagToPlan(tag, out var plan)) list.Add(plan);
                }
                return list;
            }
            // Heuristics
            if (c.Id.Contains("disease") || c.NoveltyKey.Contains("disease"))
            {
                list.Add(new ActionPlan(ApCat.Windbreak, null, -0.4f, "ventilate"));
                list.Add(new ActionPlan(ApCat.Mulch, "mulch_belt", -0.2f, "remove_excess_mulch"));
            }
            else if (c.Id.Contains("pollination"))
            {
                list.Add(new ActionPlan(ApCat.Windbreak, null, +0.0f, "open_wind_path"));
            }
            return list;
        }

        private static bool MapTagToPlan(string tag, out ActionPlan plan)
        {
            switch (tag)
            {
                case "open_path":
                case "remove_windbreak":
                    plan = new ActionPlan(ApCat.Windbreak, null, -0.4f, tag); return true;
                case "apply_herb":
                    plan = new ActionPlan(ApCat.Repair, null, -0.3f, tag); return true;
                case "water":
                    plan = new ActionPlan(ApCat.Watering, "sprinkler", +1.0f, tag); return true;
                case "apply_mulch":
                    plan = new ActionPlan(ApCat.Mulch, "mulch_belt", -0.3f, tag); return true;
                default:
                    plan = new ActionPlan(ApCat.Weeding, null, -0.2f, tag); return true;
            }
        }

        private static PatchState BuildPatchFromInventory(WorldInventory inv)
        {
            var patch = new PatchState();
            var defs = DeviceDefsLoader.Load(ContentPaths.DevicesJsonPath());
            foreach (var kv in defs)
            {
                if (!inv.HasDevice(kv.Key)) continue;
                patch.Devices.Add(new DeviceInstance(kv.Key, kv.Value.ApSavings));
            }
            return patch;
        }
    }
}
