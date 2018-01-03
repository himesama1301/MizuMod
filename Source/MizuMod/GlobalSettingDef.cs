using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class GlobalSettingDef : Def
    {
        public GSForDebug forDebug = new GSForDebug();
    }

    public class GSForDebug
    {
        public float needWaterReduceRate = 1.0f;
        public bool enableChangeWaterPoolType = false;
        public WaterType changeWaterPoolType = WaterType.NoWater;
        public bool enableChangeWaterPoolVolume = false;
        public float waterPoolVolumeRate = 1.0f;
        public bool enableResetRegenRate = false;
        public FloatRange resetBaseRegenRateRangeForShallow = new FloatRange(10.0f, 20.0f);
        public FloatRange resetBaseRegenRateRangeForDeep = new FloatRange(40.0f, 80.0f);
        public float resetRainRegenRatePerCellForShallow = 10.0f;
        public float resetRainRegenRatePerCellForDeep = 5.0f;
        public bool enableResetHiddenWaterSpot = false;
        public int resetHiddenWaterSpotBlockSizeX = 30;
        public int resetHiddenWaterSpotBlockSizeZ = 30;
        public int resetHiddenWaterSpotAllSpotNum = 100;
    }
}
