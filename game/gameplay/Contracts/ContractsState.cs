using System;
using System.Collections.Generic;

namespace Nogue.Gameplay.Contracts
{
    public sealed class ContractsState
    {
        private readonly Dictionary<string, Runtime> _rt = new(StringComparer.OrdinalIgnoreCase);
        private readonly Func<int> _day;

        public ContractsState(Func<int> dayProvider)
        { _day = dayProvider; }

        public void Add(ContractDTO dto, int startDay)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Id)) return;
            var r = new Runtime { Dto = dto, StartDay = startDay, Delivered = 0, WeeksDelivered = 0 };
            _rt[dto.Id] = r;
        }

        public bool Has(string id) => _rt.ContainsKey(id);

        private int CurrentWeekIndex() => (_day() - 1) / 7;

        public int DaysLeft(string id)
        {
            if (!_rt.TryGetValue(id, out var r)) return int.MaxValue;
            int today = _day();
            if (r.Dto.Type == "single" || r.Dto.Type == "custom")
            {
                int deadline = (r.StartDay + (r.Dto.Deadline?.InDays ?? 0));
                return deadline - today;
            }

            // weekly：次の納品日までの日数
            int dow = ((today - 1) % 7) + 1; // 1..7
            int target = r.Dto.Schedule?.DayOfWeek ?? 1;
            int delta = (target - dow + 7) % 7;
            bool deliveredThisWeek = (r.LastDeliveredWeekIndex == CurrentWeekIndex());

            if (delta == 0)
                return deliveredThisWeek ? 7 : 0;
            return delta;
        }

        public int RequiredRemainingNow(string id)
        {
            if (!_rt.TryGetValue(id, out var r)) return 0;
            if (r.Dto.Type == "single" || r.Dto.Type == "custom")
            {
                int remaining = (r.Dto.Quantity - r.Delivered);
                return remaining < 0 ? 0 : remaining;
            }

            // weekly：次回納品分
            if ((r.Dto.Schedule?.Weeks ?? 0) > 0 && r.WeeksDelivered >= r.Dto.Schedule!.Weeks) return 0;
            return r.Dto.Quantity;
        }

        public void OnDelivered(string id, int qty)
        {
            if (!_rt.TryGetValue(id, out var r)) return;
            r.Delivered += Math.Max(0, qty);
            if (r.Dto.Type == "weekly")
            {
                r.WeeksDelivered += 1;
                r.LastDeliveredWeekIndex = CurrentWeekIndex();
            }
        }

        public int ExpectedOutputWithin(string productId, int days, Func<string, int, int>? fallbackForecast = null)
            => fallbackForecast?.Invoke(productId, days) ?? 0;

        private sealed class Runtime
        {
            public ContractDTO Dto = new ContractDTO();
            public int StartDay;
            public int Delivered;
            public int WeeksDelivered;
            public int LastDeliveredWeekIndex = -1;
        }
    }
}
