using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class PlaceWorker_IceWorker : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            var def = checkingDef as ThingDef;
            if (def == null) return false;

            bool cond_building = false;
            var terrainLoc = map.terrainGrid.TerrainAt(loc);
            if (terrainLoc.IsIce())
            {
                cond_building = true;
            }

            bool cond_interaction = true;
            if (def.hasInteractionCell)
            {
                var terrainInteraction = map.terrainGrid.TerrainAt(ThingUtility.InteractionCellWhenAt(def, loc, rot, map));
                if (terrainInteraction.passability != Traversability.Standable)
                {
                    cond_interaction = false;
                }
            }

            return (cond_building && cond_interaction);
        }
    }
}
