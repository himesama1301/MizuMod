using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromWaterPool : WorkGiver_DrawWater
    {
        protected override Job CreateJobIfSatisfiedWaterCondition(IBillGiver giver, GetWaterRecipeDef recipe, Bill bill)
        {
            var thing = giver as Thing;
            var waterGrid = thing.Map.GetComponent<MapComponent_ShallowWaterGrid>();
            var pool = waterGrid.GetPool(thing.Map.cellIndices.CellToIndex(thing.Position));

            if (!recipe.needWaterTypes.Contains(pool.WaterType)) return null;
            if (pool.CurrentWaterVolume < recipe.needWaterVolume) return null;

            return new Job(MizuDef.Job_DrawFromWaterPool, thing) { bill = bill };
        }
    }
}
