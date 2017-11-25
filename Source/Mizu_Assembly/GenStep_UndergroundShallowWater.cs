using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class GenStep_UndergroundShallowWater : GenStep_UndergroundWater
    {
        public override void Generate(Map map)
        {
            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            this.GenerateUndergroundWaterGrid(
                map,
                waterGrid,
                MizuDef.GlobalSettings.shallowWaterLayer.basePoolNum,
                MizuDef.GlobalSettings.shallowWaterLayer.minWaterPoolNum,
                MizuDef.GlobalSettings.shallowWaterLayer.baseRainFall,
                MizuDef.GlobalSettings.shallowWaterLayer.basePlantDensity,
                MizuDef.GlobalSettings.shallowWaterLayer.waterVolumePerCell,
                MizuDef.GlobalSettings.shallowWaterLayer.poolCellRange);
        }
    }
}
