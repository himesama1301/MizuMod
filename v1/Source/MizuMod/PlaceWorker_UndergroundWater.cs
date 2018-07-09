using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public abstract class PlaceWorker_UndergroundWater : PlaceWorker
    {
        public abstract MapComponent_WaterGrid WaterGrid { get; }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            this.WaterGrid.MarkForDraw();
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            ThingDef def = checkingDef as ThingDef;
            if (def == null)
            {
                return false;
            }

            int curID = 0;

            for (int x = 0; x < def.Size.x; x++)
            {
                for (int z = 0; z < def.Size.z; z++)
                {
                    IntVec3 relVec = (new IntVec3(x, 0, z)).RotatedBy(rot);
                    IntVec3 curVec = loc + relVec;

                    int poolID = this.WaterGrid.GetID(map.cellIndices.CellToIndex(curVec));
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
