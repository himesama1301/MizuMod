using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_UndergroundShallowWater : PlaceWorker_UndergroundWater
    {
        private MapComponent_WaterGrid waterGrid;
        public override MapComponent_WaterGrid WaterGrid
        {
            get
            {
                if (this.waterGrid == null)
                {
                    Map visibleMap = Find.VisibleMap;
                    this.waterGrid = visibleMap.GetComponent<MapComponent_ShallowWaterGrid>();
                }
                return this.waterGrid;
            }
        }

    }
}
