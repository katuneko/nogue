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
        public double ContractImportance { get; init; }
        public LossProfile LossProfile { get; init; } = LossProfile.Zero;
        public string[] SolvableTags { get; init; } = System.Array.Empty<string>();
    }

    public readonly struct LossProfile
    {
        public readonly double Yield;
        public readonly double Quality;
        public readonly double Funds;
        public readonly double Equipment;
        public readonly double Pathogen;
        public LossProfile(double y, double q, double f, double e, double p)
        { Yield = y; Quality = q; Funds = f; Equipment = e; Pathogen = p; }
        public static readonly LossProfile Zero = new LossProfile(0,0,0,0,0);
    }
}
