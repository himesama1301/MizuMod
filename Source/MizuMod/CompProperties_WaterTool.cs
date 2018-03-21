using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public class CompProperties_WaterTool : CompProperties
    {
        public enum UseWorkType : byte
        {
            Undefined = 0,
            Mop,
            Nurse,
            WaterFarm,
        }

        public List<UseWorkType> useWorkType = new List<UseWorkType>();
        public List<WorkTypeDef> supplyWorkType = new List<WorkTypeDef>();
        public float maxWaterVolume = 1f;

        public CompProperties_WaterTool() : base(typeof(CompWaterTool)) { }
        public CompProperties_WaterTool(Type compClass) : base(compClass) { }
    }
}
