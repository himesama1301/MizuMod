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
        public bool enableResetBaseRegenRate = false;
        public FloatRange resetBaseRegenRateRangeForShallow;
        public FloatRange resetBaseRegenRateRangeForDeep;
    }

    public class GSNeedWater
    {
        public float fallPerTickBase = 1.33E-05f;
        public float borderBase = 0.3f;
        public float dehydrationSeverityPerDay = 0.1f;
    }
}
