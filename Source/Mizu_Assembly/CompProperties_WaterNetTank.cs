using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompProperties_WaterNetTank : CompProperties_WaterNet
    {
        public float maxWaterVolume = 0f;
        public bool showBar = true;
        public float drainWaterFlow = 1000.0f;
        public int flatID = -1;

        public CompProperties_WaterNetTank() : base(typeof(CompWaterNetTank)) { }
        public CompProperties_WaterNetTank(Type compClass) : base(compClass) { }
    }
}
