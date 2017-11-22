using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DoBillDrawing : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job baseJob = base.JobOnThing(pawn, thing, forced);
            if (baseJob == null)
            {
                return null;
            }

            if (thing.Map.terrainGrid.TerrainAt(thing.Position).GetWaterTerrainType() == WaterTerrainType.NoWater)
            {
                return null;
            }

            return new Job(MizuDef.Job_DoBillDrawing, thing)
            {
                targetQueueB = baseJob.targetQueueB,
                countQueue = baseJob.countQueue,
                haulMode = baseJob.haulMode,
                bill = baseJob.bill,
            };
        }
    }
}
