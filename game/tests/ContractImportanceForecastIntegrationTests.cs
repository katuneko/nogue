using NUnit.Framework;
using Nogue.Gameplay.World;
using Nogue.Gameplay.Contracts;
using Nogue.Gameplay.Events;

public class ContractImportanceForecastIntegrationTests
{
    [Test]
    public void EventAdapter_Importance_Lowers_WhenForecastMeetsRequirement()
    {
        var world = new WorldState(tier: 1, k: 3, apRemaining: 10, epsilon: 0.0, reservedContract: 0);

        var contract = new ContractDTO
        {
            Id = "c-1",
            Type = "single",
            Product = "lettuce",
            Quantity = 3,
            Deadline = new DeadlineDTO { InDays = 3 }
        };
        world.InitContracts(new[]{ contract });

        var dto = new EventDTO
        {
            id = "evt-contract-1",
            type = "contract",
            contract_id = "c-1",
            product = "lettuce",
            base_danger = 0.0,
            pedagogy = 0.0,
            repetition_penalty = 0.0
        };

        // Without forecast: higher importance
        var cNoForecast = EventAdapter.ToCandidate(dto, world);
        double impNo = cNoForecast.ContractImportance;

        // With forecast equal to requirement within days-left
        world.Debug_AddPlanting("lettuce", daysToHarvest: 3, units: 3);
        var cWithForecast = EventAdapter.ToCandidate(dto, world);
        double impYes = cWithForecast.ContractImportance;

        Assert.Less(impYes, impNo);
    }
}

