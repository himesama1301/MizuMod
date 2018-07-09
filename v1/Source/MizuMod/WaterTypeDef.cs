using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class WaterTypeDef : Def
    {
        public WaterType waterType = WaterType.Undefined;
        public WaterPreferability waterPreferability = WaterPreferability.Undefined;
        public List<ThoughtDef> thoughts = null;
        public List<HediffDef> hediffs = null;
        public float foodPoisonChance = 0.0f;
    }
}
