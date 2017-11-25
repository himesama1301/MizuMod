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
        public GSShallowWaterLayer shallowWaterLayer = new GSShallowWaterLayer();
        public GSDeepWaterLayer deepWaterLayer = new GSDeepWaterLayer();
    }

    public class GSForDebug
    {
        public float needWaterReduceRate = 1.0f;
    }

    public class GSNeedWater
    {
        public float fallPerTickBase = 1.33E-05f;
        public float borderBase = 0.3f;
        public float dehydrationSeverityPerDay = 0.1f;
    }

    public class GSShallowWaterLayer
    {
        public int basePoolNum = 30;
        public float baseRainFall = 1000f;
        public float basePlantDensity = 0.25f;
        public int minWaterPoolNum = 3;
        public IntRange poolCellRange = new IntRange(30, 100);
        public float waterVolumePerCell = 10.0f;
        public float baseRegenRate = 5000.0f;
    }

    public class GSDeepWaterLayer
    {
        public int basePoolNum = 30;
        public float baseRainFall = 1000f;
        public float basePlantDensity = 0.25f;
        public int minWaterPoolNum = 10;
        public IntRange poolCellRange = new IntRange(50, 100);
        public float waterVolumePerCell = 30.0f;
        public float baseRegenRate = 5000.0f;
    }
}
