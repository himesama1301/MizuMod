using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_DrawingWaterSpot : Building_WorkTable, IBuilding_DrinkWater
    {
        public bool IsActivated
        {
            get
            {
                return true;
            }
        }

        public WaterPreferability WaterPreferability
        {
            get
            {
                return this.Map.terrainGrid.TerrainAt(this.Position).ToWaterType().ToWaterPreferability();
            }
        }

        public int DrinkWorkAmount
        {
            get
            {
                return 2000;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public bool CanDrinkFor(Pawn p)
        {
            // 常時使用可能
            return true;
        }

        public void DrawWater(float amount)
        {
            // 何もしない
        }
    }
}
