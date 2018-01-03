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
        // デバッグ用
        public MapComponent_HiddenWaterSpot HiddenWaterSpot
        {
            get
            {
                return Find.VisibleMap.GetComponent<MapComponent_HiddenWaterSpot>();
            }
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            base.DrawGhost(def, center, rot);

            if (DebugSettings.godMode)
            {
                // デバッグ用
                this.HiddenWaterSpot.MarkForDraw();
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            ThingDef def = checkingDef as ThingDef;
            if (def == null)
            {
                return false;
            }

            bool cond_building = false;
            TerrainDef terrainLoc = map.terrainGrid.TerrainAt(loc);
            if (terrainLoc.IsSea() || terrainLoc.IsRiver() || terrainLoc.IsLakeOrPond() || terrainLoc.IsMarsh())
            {
                cond_building = true;
            }

            bool cond_interaction = true;
            if (def.hasInteractionCell)
            {
                TerrainDef terrainInteraction = map.terrainGrid.TerrainAt(ThingUtility.InteractionCellWhenAt(def, loc, rot, map));
                if (terrainInteraction.passability != Traversability.Standable)
                {
                    cond_interaction = false;
                }
            }

            return (cond_building && cond_interaction);
        }
    }
}
