using Nogue.Gameplay.Director;
using Nogue.Gameplay.Contracts;

namespace Nogue.Gameplay.Events
{
    public static class EventAdapter
    {
        public static EventCandidate ToCandidate(EventDTO d)
        {
            var type = d.type switch
            {
                "micro" => EventType.Micro,
                "meso" => EventType.Meso,
                "macro" => EventType.Macro,
                "contract" => EventType.Meso, // treat as meso for diversity purposes
                _ => EventType.Micro
            };

            return new EventCandidate
            {
                Id = d.id,
                Type = type,
                BaseDanger = Clamp01(d.base_danger),
                Pedagogy = Clamp01(d.pedagogy),
                NoveltyKey = d.novelty_key ?? string.Empty,
                RepetitionPenalty = Clamp01(d.repetition_penalty),
                IsContractCritical = d.contract_critical,
                ContractImportance = 0.0,
                LossProfile = d.loss_profile != null
                    ? new LossProfile(
                        Clamp01(d.loss_profile.yield),
                        Clamp01(d.loss_profile.quality),
                        Clamp01(d.loss_profile.funds),
                        Clamp01(d.loss_profile.equipment),
                        Clamp01(d.loss_profile.pathogen))
                    : LossProfile.Zero,
                SolvableTags = d.solvable_tags ?? System.Array.Empty<string>(),
            };
        }

        public static EventCandidate ToCandidate(EventDTO d, IWorldState world)
        {
            var c = ToCandidate(d);
            // Contract importance wiring (optional paths safe)
            if ((d.type == "contract") && !string.IsNullOrEmpty(d.contract_id) && world.Contracts != null)
            {
                int daysLeft = world.Contracts.DaysLeft(d.contract_id);
                int req = world.Contracts.RequiredRemainingNow(d.contract_id);
                int exp = world.Contracts.ExpectedOutputWithin(d.product ?? string.Empty, daysLeft, world.ForecastExpectedOutput);
                float imp = ContractImportance.Compute(daysLeft, req, exp);
                c = new EventCandidate
                {
                    Id = c.Id,
                    Type = c.Type,
                    BaseDanger = c.BaseDanger,
                    Pedagogy = c.Pedagogy,
                    NoveltyKey = c.NoveltyKey,
                    RepetitionPenalty = c.RepetitionPenalty,
                    IsContractCritical = imp >= 0.70f || c.IsContractCritical,
                    ContractImportance = imp,
                    LossProfile = c.LossProfile,
                    SolvableTags = c.SolvableTags,
                };
            }
            return c;
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    }
}
