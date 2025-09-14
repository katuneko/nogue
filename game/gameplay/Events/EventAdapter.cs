using Nogue.Gameplay.Director;

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
            };
        }

        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    }
}

