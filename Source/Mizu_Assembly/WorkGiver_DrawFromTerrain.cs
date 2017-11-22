using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromTerrain : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job baseJob = base.JobOnThing(pawn, thing, forced);
            if (baseJob == null)
            {
                return null;
            }

            var getWaterRecipe = baseJob.bill.recipe as GetWaterRecipeDef;
            if (getWaterRecipe == null || getWaterRecipe.needWaterTerrainTypes == null)
            {
                return null;
            }

            if (!getWaterRecipe.needWaterTerrainTypes.Contains(thing.Map.terrainGrid.TerrainAt(thing.Position).GetWaterTerrainType()))
            {
                return null;
            }

            return new Job(MizuDef.Job_DrawFromTerrain, thing)
            {
                targetQueueB = baseJob.targetQueueB,
                countQueue = baseJob.countQueue,
                haulMode = baseJob.haulMode,
                bill = baseJob.bill,
            };
        }
    }
}
