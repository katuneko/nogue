using System;

namespace Nogue.Gameplay.Events
{
    [Serializable]
    public sealed class EventDTO
    {
        public string id = string.Empty;
        public string type = "micro"; // micro|meso|macro|contract (optional)
        public string novelty_key = string.Empty;
        public double base_danger = 0.0;
        public double pedagogy = 0.0;
        public double repetition_penalty = 0.0;
        public bool contract_critical = false;
        public string[] solvable_tags = Array.Empty<string>();
        public int tier_min = 1;
        public int tier_max = 6;
        public LossProfileDTO? loss_profile = null;
    }

    [Serializable]
    public sealed class LossProfileDTO
    {
        public double yield = 0.0;
        public double quality = 0.0;
        public double funds = 0.0;
        public double equipment = 0.0;
        public double pathogen = 0.0;
    }
}
