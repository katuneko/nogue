using NUnit.Framework;
using System.Collections.Generic;
using Nogue.Gameplay.World;
using Nogue.Gameplay.Events;
using Nogue.Gameplay.Director;

public class NightResolverBudgetTests
{
    [Test]
    public void NightResolver_ConsumesBudget_OnResolvedEvents()
    {
        var world = new WorldState(tier: 1, k: 3, apRemaining: 10, epsilon: 0.0, reservedContract: 0);
        // Initialize budgets with defaults
        var cfg = new BudgetConfig();
        world.InitializeBudgets(cfg);

        // Build two candidates with only Funds losses
        var a = new EventCandidate
        {
            Id = "a",
            Type = EventType.Micro,
            BaseDanger = 0,
            Pedagogy = 0,
            NoveltyKey = "na",
            RepetitionPenalty = 0,
            IsContractCritical = false,
            ContractImportance = 0,
            LossProfile = new LossProfile(0,0,0.03,0,0)
        };
        var b = new EventCandidate
        {
            Id = "b",
            Type = EventType.Micro,
            BaseDanger = 0,
            Pedagogy = 0,
            NoveltyKey = "nb",
            RepetitionPenalty = 0,
            IsContractCritical = false,
            ContractImportance = 0,
            LossProfile = new LossProfile(0,0,0.05,0,0)
        };

        // Before consumption, candidate B should not exceed (0.05 <= 0.07 default day funds)
        Assert.IsFalse(world.ExceedsDamageBudget(b));

        // Resolve A at full severity -> consume 0.03 funds
        var night = new NightResolver(world);
        night.ResolveAndConsumeBudgets(new[]{ new ResolvedEvent(a, ResolutionOutcome.Fail) });

        // Now remaining funds ~0.04, so B should exceed
        Assert.IsTrue(world.ExceedsDamageBudget(b));
    }
}

