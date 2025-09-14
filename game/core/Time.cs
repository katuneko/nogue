using System;

namespace Nogue.Core
{
    public enum Season { Spring = 0, Summer = 1, Autumn = 2, Winter = 3 }

    public sealed class GameClock
    {
        public int Day { get; private set; } = 1;   // 1..30 (example)
        public int Year { get; private set; } = 1;
        public Season Season { get; private set; } = Season.Spring;

        public void AdvanceDay()
        {
            Day++;
            if (Day > 30)
            {
                Day = 1;
                Season = (Season)(((int)Season + 1) % 4);
                if (Season == Season.Spring) Year++;
            }
        }
    }
}

