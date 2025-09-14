using Nogue.Gameplay.Director;

namespace Nogue.Gameplay.Events
{
    public sealed class EventCandidate : IEventCandidate
    {
        public string Id { get; init; } = string.Empty;
        public EventType Type { get; init; } = EventType.Micro;
        public double BaseDanger { get; init; }
        public double Pedagogy { get; init; }
        public string NoveltyKey { get; init; } = string.Empty;
        public double RepetitionPenalty { get; init; }
        public bool IsContractCritical { get; init; }
    }
}

