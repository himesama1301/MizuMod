using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class Building_UndergroundWaterPump : Building_WaterNet, IBuilding_WaterNet
    {
        public override WaterType OutputWaterType
        {
            get
            {
                return WaterType.NormalWater;
            }
        }
    }
}
