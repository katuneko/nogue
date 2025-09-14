using NUnit.Framework;
using Nogue.Gameplay.Contracts;

public class ContractsStateWeeklyTests
{
    [Test]
    public void WeeklyContract_DueToday_Undelivered_ReturnsZero()
    {
        int today = 7; // Sunday if 1..7 with 7 as target
        var contracts = new ContractsState(() => today);
        var dto = new ContractDTO
        {
            Id = "weekly-1",
            Type = "weekly",
            Product = "lettuce",
            Quantity = 10,
            Schedule = new ScheduleDTO { Weeks = 10, DayOfWeek = 7 }
        };
        contracts.Add(dto, startDay: 1);

        int daysLeft = contracts.DaysLeft(dto.Id);
        Assert.AreEqual(0, daysLeft);
    }

    [Test]
    public void WeeklyContract_DueToday_AlreadyDelivered_ReturnsSeven()
    {
        int today = 7;
        var contracts = new ContractsState(() => today);
        var dto = new ContractDTO
        {
            Id = "weekly-2",
            Type = "weekly",
            Product = "lettuce",
            Quantity = 10,
            Schedule = new ScheduleDTO { Weeks = 10, DayOfWeek = 7 }
        };
        contracts.Add(dto, startDay: 1);
        contracts.OnDelivered(dto.Id, 10); // mark delivered this week

        int daysLeft = contracts.DaysLeft(dto.Id);
        Assert.AreEqual(7, daysLeft);
    }
}

