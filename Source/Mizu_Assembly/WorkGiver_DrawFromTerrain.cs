using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromTerrain : WorkGiver_DrawWater
    {
        protected override Job CreateJobIfSatisfiedWaterCondition(IBillGiver giver, GetWaterRecipeDef recipe, Bill bill)
        {
            var thing = giver as Thing;
            if (!recipe.needWaterTerrainTypes.Contains(thing.Map.terrainGrid.TerrainAt(thing.Position).GetWaterTerrainType())) return null;

            return new Job(MizuDef.Job_DrawFromTerrain, thing) { bill = bill };
        }
    }
}
