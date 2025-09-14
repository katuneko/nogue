using System.Collections.Generic;
using Nogue.Gameplay.Events;

namespace Nogue.Gameplay.World
{
    public sealed class NightResolver
    {
        private readonly WorldState _world;
        public NightResolver(WorldState world) { _world = world; }

        public void ResolveAndConsumeBudgets(IEnumerable<ResolvedEvent> todaysResolved)
        {
            foreach (var e in todaysResolved)
            {
                float sev = EventResolution.InferSeverityFromOutcome(e.Candidate, e.Outcome);
                _world.OnEventResolved(e.Candidate, sev);
            }
        }
    }

    public readonly struct ResolvedEvent
    {
        public readonly EventCandidate Candidate;
        public readonly ResolutionOutcome Outcome;
        public ResolvedEvent(EventCandidate c, ResolutionOutcome o) { Candidate = c; Outcome = o; }
    }
}

