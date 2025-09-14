using System.Collections.Generic;

namespace Nogue.Gameplay.World
{
    public sealed class WorldInventory
    {
        public int Funds { get; set; }
        private readonly HashSet<string> _devices = new HashSet<string>();

        public bool HasDevice(string id) => _devices.Contains(id);
        public void AddDevice(string id) => _devices.Add(id);

        public static WorldInventory Default()
        {
            var w = new WorldInventory { Funds = 100 };
            return w;
        }

        public bool CanAfford(params SolvableNow.ActionPlan[] plans)
        {
            int total = 0;
            foreach (var p in plans)
            {
                if (p == null) continue;
                total += p.Funds;
                if (p.DeviceRequired != null && !_devices.Contains(p.DeviceRequired)) return false;
            }
            return total <= Funds;
        }
    }
}
