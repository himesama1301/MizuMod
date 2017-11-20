using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class GenStep_UndergroundDeepWater : GenStep_UndergroundWater
    {
        private const int BasePoolNum = 30;
        private const float BaseRainFall = 1000f;
        private const float BasePlantDensity = 0.25f;
        private const int MinWaterPoolNum = 10;
        private static readonly IntRange PoolCellRange = new IntRange(50, 100);
        private const float WaterVolumePerCell = 30.0f;

        public override void Generate(Map map)
        {
            var waterGrid = map.GetComponent<MapComponent_DeepWaterGrid>();
            this.GenerateUndergroundWaterGrid(map, waterGrid, BasePoolNum, MinWaterPoolNum, BaseRainFall, BasePlantDensity, WaterVolumePerCell, PoolCellRange);
        }
    }
}
