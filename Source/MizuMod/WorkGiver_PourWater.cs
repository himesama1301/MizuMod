using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_PourWater : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            var baseJob = base.JobOnThing(pawn, thing, forced);
            if (baseJob == null) return null;

            var comp = thing.TryGetComp<CompWaterNetTank>();
            if (comp == null) return null;
            if (comp.AmountCanAccept <= 0f) return null;

            return new Job(MizuDef.Job_PourWater)
            {
                targetA = baseJob.targetA,
                targetB = baseJob.targetB,
                targetC = baseJob.targetC,
                targetQueueA = baseJob.targetQueueA,
                targetQueueB = baseJob.targetQueueB,
                count = baseJob.count,
                countQueue = baseJob.countQueue,
                bill = baseJob.bill,
            };
        }
    }
}
