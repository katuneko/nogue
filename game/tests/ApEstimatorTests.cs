using NUnit.Framework;
using Nogue.Gameplay.World;
using System.Collections.Generic;

public class ApEstimatorTests
{
    [Test]
    public void SprinklerHalvesWatering()
    {
        var patch = new PatchState();
        patch.Devices.Add(new DeviceInstance("sprinkler", new Dictionary<ApCat, float>{{ApCat.Watering, 0.50f}}));

        int baseAp = ApEstimator.Estimate(ApCat.Watering, new PatchState());
        int withSpr = ApEstimator.Estimate(ApCat.Watering, patch);

        Assert.Greater(baseAp, 0);
        Assert.LessOrEqual(withSpr, baseAp/2 + 1); // 切上げ・下限1を考慮
    }
}

