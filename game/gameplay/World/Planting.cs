namespace Nogue.Gameplay.World
{
    public sealed class Planting
    {
        public string ProductId { get; init; } = string.Empty;
        public int DaysToHarvest { get; set; }
        public int ExpectedUnits { get; init; } = 1;
    }
}

