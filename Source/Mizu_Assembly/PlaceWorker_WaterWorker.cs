using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_WaterWorker : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            ThingDef def = checkingDef as ThingDef;
            if (def == null)
            {
                return false;
            }

            bool cond_building = false;
            TerrainDef terrainLoc = this.Map.terrainGrid.TerrainAt(loc);
            if (terrainLoc.defName.Contains("Water") || terrainLoc.defName.Equals("Marsh"))
            {
                cond_building = true;
            }

            bool cond_interaction = false;
            TerrainDef terrainInteraction = this.Map.terrainGrid.TerrainAt(Thing.InteractionCellWhenAt(def, loc, rot, this.Map));
            if (terrainInteraction.passability == Traversability.Standable)
            {
                cond_interaction = true;
            }

            return (cond_building && cond_interaction);
        }
    }
}
