using System;
using System.IO;

namespace Nogue.Core
{
    // Resolves CONTENT_ROOT -> actual absolute paths.
    public static class ContentPaths
    {
        public static string ContentRoot()
        {
            var env = Environment.GetEnvironmentVariable("CONTENT_ROOT");
            if (!string.IsNullOrEmpty(env)) return Path.GetFullPath(env);
            // default to ./content relative to current working directory
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "content"));
        }

        public static string EventsMergedPath() => Path.Combine(ContentRoot(), "events.yaml");
        public static string TiersPath() => Path.Combine(ContentRoot(), "tiers");
        public static string IdsRegistryPath() => Path.Combine(ContentRoot(), "ids.yaml");
        public static string DirectorScoringPath() => Path.Combine(ContentRoot(), "director", "Scoring.json");
        public static string EventsJsonPath() => Path.Combine(ContentRoot(), "events.json");
        public static string TiersJsonPath() => Path.Combine(ContentRoot(), "tiers.json");
        public static string DevicesJsonPath() => Path.Combine(ContentRoot(), "devices.json");
        public static string CropsJsonPath() => Path.Combine(ContentRoot(), "crops.json");
        public static string ContractsJsonPath() => Path.Combine(ContentRoot(), "contracts.json");
        public static string BudgetJsonPath() => Path.Combine(ContentRoot(), "director", "Budget.json");
    }
}
