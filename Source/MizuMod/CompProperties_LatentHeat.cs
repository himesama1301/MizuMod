using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompProperties_LatentHeat : CompProperties
    {
        public ThingDef changedThingDef;

        public CompProperties_LatentHeat() : base(typeof(CompLatentHeat)) { }
        public CompProperties_LatentHeat(Type compClass) : base(compClass) { }
    }
}
