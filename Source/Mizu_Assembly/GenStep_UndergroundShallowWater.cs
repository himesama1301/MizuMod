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
        private const int BasePoolNum = 30;
        private const float BaseRainFall = 1000f;
        private const float BasePlantDensity = 0.25f;
        private const int MinWaterPoolNum = 3;
        private static readonly IntRange PoolCellRange = new IntRange(30, 100);
        private const float WaterVolumePerCell = 10.0f;

        public override void Generate(Map map)
        {
            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            this.GenerateUndergroundWaterGrid(map, waterGrid, BasePoolNum, MinWaterPoolNum, BaseRainFall, BasePlantDensity, WaterVolumePerCell, PoolCellRange);
        }
    }
}
