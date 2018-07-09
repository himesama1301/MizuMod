using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class CompProperties_DestroyByTime : CompProperties
    {
        public int destroyTicks = 1;

        public CompProperties_DestroyByTime() : base(typeof(CompDestroyByTime)) { }
        public CompProperties_DestroyByTime(Type compClass) : base(compClass) { }
    }
}
