using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class CompProperties_WaterNet : CompProperties
    {
        public CompProperties_WaterNet() : base(typeof(CompWaterNet)) { }
        public CompProperties_WaterNet(Type compClass) : base(compClass) { }
    }
}
