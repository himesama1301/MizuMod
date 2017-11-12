using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DoBillFaucet : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job baseJob = base.JobOnThing(pawn, thing, forced);
            if (baseJob != null && baseJob.bill != null && baseJob.bill.recipe != null)
            {
                FaucetRecipeDef recipe = baseJob.bill.recipe as FaucetRecipeDef;
                CompWaterNet comp = thing.TryGetComp<CompWaterNet>();
                if (comp != null && recipe != null && recipe.needWaterVolume <= comp.WaterNet.WaterVolume)
                {
                    Job job = new Job(MizuDef.Job_DoBillFaucet, (Thing)thing);
                    job.targetQueueB = baseJob.targetQueueB;
                    job.countQueue = baseJob.countQueue;
                    job.haulMode = baseJob.haulMode;
                    job.bill = baseJob.bill;
                    return job;
                }
            }
            return null;
        }
    }
}
