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
                this.basePoolNum,
                this.minWaterPoolNum,
                this.baseRainFall,
                this.basePlantDensity,
                this.literPerCell,
                this.poolCellRange,
                this.baseRegenRateRange,
                this.rainRegenRatePerCell);
        }
    }
}
