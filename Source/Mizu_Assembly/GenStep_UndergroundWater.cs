using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public abstract class GenStep_UndergroundWater : GenStep
    {
        private const float BaseMapArea = 250f * 250f;

        public void GenerateUndergroundWaterGrid(Map map, MapComponent_WaterGrid waterGrid, int basePoolNum, int minWaterPoolNum, float baseRainFall, float basePlantDensity, float literPerCell, IntRange poolCellRange)
        {
            float rainRate = map.TileInfo.rainfall / baseRainFall;
            float areaRate = map.Area / BaseMapArea;
            float plantRate = map.Biome.plantDensity / basePlantDensity;

            int waterPoolNum = Mathf.RoundToInt(basePoolNum * rainRate * areaRate * plantRate);

            Log.Message(string.Format("rain={0},area={1},plant={2},num={3}", rainRate.ToString("F3"), areaRate.ToString("F3"), plantRate.ToString("F3"), waterPoolNum));
            if (plantRate > 0.0f)
            {
                waterPoolNum = Mathf.Max(waterPoolNum, minWaterPoolNum);
            }

            for (int i = 0; i < waterPoolNum; i++)
            {
                IntVec3 result;
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith(5, (c) => !waterGrid.GetCellBool(map.cellIndices.CellToIndex(c)), map, out result))
                {
                    int numCells = poolCellRange.RandomInRange;
                    UndergroundWaterPool pool = new UndergroundWaterPool(waterGrid, numCells * literPerCell, WaterType.NormalWater);
                    pool.ID = i + 1;
                    waterGrid.AddWaterPool(pool, GridShapeMaker.IrregularLump(result, map, numCells));
                }
            }

            waterGrid.ModifyPoolGrid();
        }
    }
}
