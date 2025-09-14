using NUnit.Framework;
using System.Collections.Generic;
using Nogue.Gameplay.Director;
using Nogue.Gameplay.Events;
using Nogue.Gameplay.World;

public class DirectorReservedSlotTests
{
    [Test]
    public void ContractCritical_IsAlwaysIncluded_WhenReservedSlotIsOne()
    {
        var world = new WorldState(tier: 4, k: 3, apRemaining: 10, epsilon: 0.0, reservedContract: 1);
        var director = new Director();

        var micro = new EventCandidate { Id = "m1", Type = EventType.Micro, BaseDanger = 0.9, Pedagogy = 0.9, NoveltyKey = "n1", RepetitionPenalty = 0.0, IsContractCritical = false };
        var meso  = new EventCandidate { Id = "m2", Type = EventType.Meso,  BaseDanger = 0.8, Pedagogy = 0.7, NoveltyKey = "n2", RepetitionPenalty = 0.0, IsContractCritical = false };
        var contr = new EventCandidate { Id = "c1", Type = EventType.Meso,  BaseDanger = 0.2, Pedagogy = 0.2, NoveltyKey = "n3", RepetitionPenalty = 0.0, IsContractCritical = true, ContractImportance = 1.0 };

        var picked = director.Select(new List<IEventCandidate>{ micro, meso, contr }, world);
        bool hasContract = false;
        foreach (var e in picked) if (e.Id == "c1") hasContract = true;
        Assert.IsTrue(hasContract, "契約クリティカルは予約1枠で必ず含まれるべき");
        Assert.AreEqual(3, picked.Count);
    }
}

