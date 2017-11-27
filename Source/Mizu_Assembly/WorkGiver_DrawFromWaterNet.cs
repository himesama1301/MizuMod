using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromWaterNet : WorkGiver_DrawWater
    {
        protected override Job CreateJobIfSatisfiedWaterCondition(IBillGiver giver, GetWaterRecipeDef recipe, Bill bill)
        {
            var thing = giver as Thing;
            if (thing == null) return null;

            var workTable = giver as Building_WaterNetWorkTable;
            if (workTable == null) return null;
            if (!recipe.needWaterTypes.Contains(workTable.StoredWaterType)) return null;
            if (workTable.StoredWaterVolume < recipe.needWaterVolume) return null;

            return new Job(MizuDef.Job_DrawFromWaterNet, thing) { bill = bill };
        }
    }
}
