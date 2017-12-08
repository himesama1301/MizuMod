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
        public GSNeedWater needWater = new GSNeedWater();
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
    }

    public class GSNeedWater
    {
        public float fallPerTickBase = 1.33E-05f;
        public float borderBase = 0.3f;
        public float dehydrationSeverityPerDay = 0.1f;
    }
}
