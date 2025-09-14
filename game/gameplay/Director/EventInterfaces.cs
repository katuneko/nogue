using System;

using Nogue.Gameplay.Contracts;

namespace Nogue.Gameplay.Director
{
    public enum EventType { Micro, Meso, Macro }

    public interface IEventCandidate
    {
        string Id { get; }
        EventType Type { get; }
        // Scoring attributes (0..1 suggested, but not enforced here)
        double BaseDanger { get; }
        double Pedagogy { get; }
        string NoveltyKey { get; }
        double RepetitionPenalty { get; }

        // Flags and context
        bool IsContractCritical { get; }
        // Optional: 0..1 importance for contract urgency/shortage
        double ContractImportance { get; }
    }

    public interface IWorldState
    {
        int Tier { get; }
        int K { get; }                 // slots to show (2..4, default 3)
        int APRemaining { get; }
        double DirectorEpsilon { get; }
        bool ExceedsDamageBudget(IEventCandidate e);
        bool IsSolvableNow(IEventCandidate e);

        // Diversity / novelty memory
        bool HasSeenNovelty(string noveltyKey);

        // Reserved slots by category name (e.g., "contract_critical": 1)
        int GetReservedSlots(string category);

        // Contracts and production forecast hooks (optional)
        ContractsState? Contracts { get; }
        int ForecastExpectedOutput(string productId, int days);
    }
}
