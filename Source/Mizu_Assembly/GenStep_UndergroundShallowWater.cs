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
