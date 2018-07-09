using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_GroundWaterPump : Building_WaterNet, IBuilding_WaterNet
    {
        public override WaterType OutputWaterType
        {
            get
            {
                return this.Map.terrainGrid.TerrainAt(this.Position).ToWaterType();
            }
        }
    }
}
