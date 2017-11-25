using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class GenStep_UndergroundDeepWater : GenStep_UndergroundWater
    {
        public override void Generate(Map map)
        {
            var waterGrid = map.GetComponent<MapComponent_DeepWaterGrid>();
            this.GenerateUndergroundWaterGrid(
                map,
                waterGrid,
                MizuDef.GlobalSettings.deepWaterLayer.basePoolNum,
                MizuDef.GlobalSettings.deepWaterLayer.minWaterPoolNum,
                MizuDef.GlobalSettings.deepWaterLayer.baseRainFall,
                MizuDef.GlobalSettings.deepWaterLayer.basePlantDensity,
                MizuDef.GlobalSettings.deepWaterLayer.waterVolumePerCell,
                MizuDef.GlobalSettings.deepWaterLayer.poolCellRange);
        }
    }
}
