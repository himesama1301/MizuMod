using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_ShallowWater : PlaceWorker
    {
        private const float MinFertility = 0.5f;
        private const float MinDistance = 20.0f;
        private const float MinDistanceSquared = MinDistance * MinDistance;

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            Map visibleMap = Find.VisibleMap;
            MapComponent_ShallowWaterGrid waterGrid = visibleMap.GetComponent<MapComponent_ShallowWaterGrid>();
            if (waterGrid != null)
            {
                waterGrid.MarkForDraw();
            }
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            ThingDef def = checkingDef as ThingDef;
            if (def == null)
            {
                return false;
            }

            int curID = 0;

            // 肥沃度チェック
            for (int x = 0; x < def.Size.x; x++)
            {
                for (int z = 0; z < def.Size.z; z++)
                {
                    IntVec3 relVec = (new IntVec3(x, 0, z)).RotatedBy(rot);
                    IntVec3 curVec = loc + relVec;

                    int poolID = waterGrid.GetID(map.cellIndices.CellToIndex(curVec));
                    if (poolID == 0)
                    {
                        return false;
                    }

                    if (curID == 0)
                    {
                        curID = poolID;
                    }
                    else if (curID != poolID)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
