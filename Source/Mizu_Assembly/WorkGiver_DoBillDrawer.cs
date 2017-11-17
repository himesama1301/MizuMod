using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DoBillDrawer : WorkGiver_DoBill
    {
        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Job baseJob = base.JobOnThing(pawn, thing, forced);
            if (baseJob == null || baseJob.bill == null || baseJob.bill.recipe == null)
            {
                return null;
            }

            DrawerRecipeDef recipe = baseJob.bill.recipe as DrawerRecipeDef;
            if (recipe == null)
            {
                return null;
            }

            if (recipe.needWaterTerrainType == thing.Map.terrainGrid.TerrainAt(thing.Position).GetWaterTerrainType())
            {
                return baseJob;
            }
            return null;
        }
    }
}
