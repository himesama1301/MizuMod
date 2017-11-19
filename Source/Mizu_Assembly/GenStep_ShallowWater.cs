using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class GenStep_ShallowWater : GenStep
    {
        private const float BasePoolNum = 30.0f;
        private const float BaseMapArea = 250f * 250f;
        private const float BaseRainFall = 1000f;
        private const float BasePlantDensity = 0.25f;
        private const int MinWaterPoolNum = 3;

        public override void Generate(Map map)
        {
            float rainRate = map.TileInfo.rainfall / BaseRainFall;
            float areaRate = map.Area / BaseMapArea;
            float plantRate = map.Biome.plantDensity / BasePlantDensity;

            int waterPoolNum = Mathf.RoundToInt(BasePoolNum * rainRate * areaRate * plantRate);

            Log.Message(string.Format("rain={0},area={1},plant={2},num={3}", rainRate.ToString("F3"), areaRate.ToString("F3"), plantRate.ToString("F3"), waterPoolNum));
            if (plantRate > 0.0f)
            {
                waterPoolNum = Mathf.Max(waterPoolNum, 3);
            }

            MapComponent_ShallowWaterGrid waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            for (int i = 0; i < waterPoolNum; i++)
            {
                IntVec3 result;
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(5, (c) => !waterGrid.GetCellBool(map.cellIndices.CellToIndex(c)), map, out result))
                {
                    int numCells = (new IntRange(30, 100)).RandomInRange;
                    UndergroundWaterPool pool = new UndergroundWaterPool(numCells * 1000);
                    pool.ID = i + 1;
                    waterGrid.AddWaterPool(pool, GridShapeMaker.IrregularLump(result, map, numCells));
                }
            }

            waterGrid.ModifyPoolGrid();
        }
    }
}
