using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Building_UndergroundDeepWaterPump : Building_UndergroundWaterPump, IBuilding_WaterNet
    {
        private MapComponent_WaterGrid waterGrid;
        public override MapComponent_WaterGrid WaterGrid
        {
            get
            {
                if (this.waterGrid == null)
                {
                    this.waterGrid = this.Map.GetComponent<MapComponent_DeepWaterGrid>();
                    if (this.waterGrid == null)
                    {
                        Log.Error("waterGrid is null");
                    }
                }
                return this.waterGrid;
            }
        }
    }
}
