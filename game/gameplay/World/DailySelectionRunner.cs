using System.Collections.Generic;
using Nogue.Gameplay.Director;
using Nogue.Gameplay.Events;
using Nogue.Core;

namespace Nogue.Gameplay.World
{
    // Minimal runner to build candidates with world-aware adapter and run Director selection.
    public sealed class DailySelectionRunner
    {
        private readonly WorldState _world;
        private readonly Director.Director _director;

        public DailySelectionRunner(WorldState world, Director.Director director)
        {
            _world = world; _director = director;
        }

        public List<IEventCandidate> BuildCandidatesAndSelect()
        {
            var dtos = EventsLoader.LoadDTOs(ContentPaths.EventsJsonPath());
            var candidates = new List<IEventCandidate>(dtos.Count);
            foreach (var dto in dtos)
            {
                // world-aware overload so contract importance is wired in
                candidates.Add(EventAdapter.ToCandidate(dto, _world));
            }
            return _director.Select(candidates, _world);
        }
    }
}

