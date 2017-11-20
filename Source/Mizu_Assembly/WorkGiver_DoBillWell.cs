using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DoBillWell : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job baseJob = base.JobOnThing(pawn, thing, forced);
            if (baseJob == null || baseJob.bill == null || baseJob.bill.recipe == null)
            {
                return null;
            }

            FaucetRecipeDef recipe = baseJob.bill.recipe as FaucetRecipeDef;
            if (recipe == null)
            {
                return null;
            }

            var waterGrid = thing.Map.GetComponent<MapComponent_ShallowWaterGrid>();
            if (waterGrid == null)
            {
                return null;
            }

            var pool = waterGrid.GetPool(thing.Map.cellIndices.CellToIndex(thing.Position));
            if (pool == null)
            {
                return null;
            }

            if (recipe.needWaterVolume > pool.CurrentWaterVolume)
            {
                return null;
            }

            Job job = new Job(MizuDef.Job_DoBillWell, (Thing)thing);
            job.targetQueueB = baseJob.targetQueueB;
            job.countQueue = baseJob.countQueue;
            job.haulMode = baseJob.haulMode;
            job.bill = baseJob.bill;
            return job;
        }
    }
}
