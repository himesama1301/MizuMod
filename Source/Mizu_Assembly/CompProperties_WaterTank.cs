using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompProperties_WaterTank : CompProperties_WaterNet
    {
        public float storedWaterVolumeMax = 1000f;

        public CompProperties_WaterTank()
        {
            this.compClass = typeof(CompWaterNetTank);
        }
    }
}
