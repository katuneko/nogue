using System;

namespace Nogue.Core
{
    public enum TemperatureBand { Cold = 0, Temperate = 1, Hot = 2 }
    public enum PHBand { Acidic = 0, Neutral = 1, Alkaline = 2 }
    public enum CoverType { Bare = 0, Grass = 1, Mulch = 2 }

    // Minimal tag vector (subset, extend as needed)
    public struct Tags
    {
        // Environment
        public int Humidity;           // 0..5
        public TemperatureBand Temp;   // 寒/温/暑
        public PHBand PH;              // 酸/中/アルカリ
        public int FertilityNPK;       // 0..5 (abstracted)
        public int Insolation;         // 0..5

        // Physical
        public int Height;             // -2..+2
        public int Permeability;       // 0..2
        public int Flammability;       // 0..2
        public int WindShadow;         // 0..2

        // Ecology
        public int Pollen;             // 0..2
        public int PestFeed;           // 0..2
        public int PredatorHabitat;    // 0..2
        public int MycorrhizaAffinity; // -1..+2
        public CoverType Cover;        // 裸地/草/マルチ

        // State
        public int Sanitation;         // 清潔/汚染 0..2 (0 clean)
        public int PathogenPressure;   // 0..5
        public int SeedBank;           // 0..5

        public void ApplyDelta(in TagDelta d)
        {
            Humidity = Clamp01to05(Humidity + d.Humidity);
            FertilityNPK = Clamp01to05(FertilityNPK + d.FertilityNPK);
            Insolation = Clamp01to05(Insolation + d.Insolation);
            Height = Math.Clamp(Height + d.Height, -2, 2);
            Permeability = Math.Clamp(Permeability + d.Permeability, 0, 2);
            Flammability = Math.Clamp(Flammability + d.Flammability, 0, 2);
            WindShadow = Math.Clamp(WindShadow + d.WindShadow, 0, 2);
            Pollen = Math.Clamp(Pollen + d.Pollen, 0, 2);
            PestFeed = Math.Clamp(PestFeed + d.PestFeed, 0, 2);
            PredatorHabitat = Math.Clamp(PredatorHabitat + d.PredatorHabitat, 0, 2);
            MycorrhizaAffinity = Math.Clamp(MycorrhizaAffinity + d.MycorrhizaAffinity, -1, 2);
            Sanitation = Math.Clamp(Sanitation + d.Sanitation, 0, 2);
            PathogenPressure = Clamp01to05(PathogenPressure + d.PathogenPressure);
            SeedBank = Clamp01to05(SeedBank + d.SeedBank);
            if (d.SetCover.HasValue) Cover = d.SetCover.Value;
            if (d.SetTemp.HasValue) Temp = d.SetTemp.Value;
            if (d.SetPH.HasValue) PH = d.SetPH.Value;
        }

        static int Clamp01to05(int v) => Math.Clamp(v, 0, 5);
    }

    public struct TagDelta
    {
        // Integer deltas (defaults = 0)
        public int Humidity, FertilityNPK, Insolation;
        public int Height, Permeability, Flammability, WindShadow;
        public int Pollen, PestFeed, PredatorHabitat, MycorrhizaAffinity;
        public int Sanitation, PathogenPressure, SeedBank;
        // Enum sets
        public CoverType? SetCover;
        public TemperatureBand? SetTemp;
        public PHBand? SetPH;
    }
}

