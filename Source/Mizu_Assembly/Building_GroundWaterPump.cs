using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_GroundWaterPump : Building_Pump, IBuilding_Pump
    {
        public override void TickWaterType(CompWaterNetPump comp)
        {
            if (comp.WaterFlow > 0.0f)
            {
                TerrainDef terrain = this.Map.terrainGrid.TerrainAt(this.Position);

                if (terrain.IsSea())
                {
                    this.WaterType = WaterType.SeaWater;
                }
                else if (terrain.IsMarsh())
                {
                    this.WaterType = WaterType.MudWater;
                }
                else if (terrain.IsRiver() || terrain.IsLakeOrPond())
                {
                    this.WaterType = WaterType.NormalWater;
                }
            }
            else
            {
                this.WaterType = WaterType.NoWater;
            }
        }
    }
}
