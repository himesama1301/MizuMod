using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_RainTank : Building_WaterNetWorkTable, IBuilding_WaterNet
    {
        public override WaterType OutputWaterType
        {
            get
            {
                return WaterType.RawWater;
            }
        }
    }
}
