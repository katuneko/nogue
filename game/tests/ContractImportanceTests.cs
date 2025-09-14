using NUnit.Framework;
using Nogue.Gameplay.Contracts;

public class ContractImportanceTests
{
    [Test]
    public void ImportanceIncreasesAsDeadlineApproaches()
    {
        float far  = ContractImportance.Compute(daysLeft:10, requiredRemaining:10, expectedOutputWindow:20);
        float near = ContractImportance.Compute(daysLeft:2,  requiredRemaining:10, expectedOutputWindow:20);
        Assert.Less(far, near);
    }

    [Test]
    public void ImportanceIncreasesWithShortage()
    {
        float ok   = ContractImportance.Compute(daysLeft:3, requiredRemaining:5, expectedOutputWindow:10);
        float bad  = ContractImportance.Compute(daysLeft:3, requiredRemaining:10, expectedOutputWindow:5);
        Assert.Less(ok, bad);
    }
}

