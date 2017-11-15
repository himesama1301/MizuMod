using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_WaterNetBase : Building, IBuilding_WaterNetBase
    {
        public virtual bool IsActivatedForWaterNet
        {
            get
            {
                return true;
            }
        }

        public virtual List<IntVec3> ConnectVecs
        {
            get
            {
                List<IntVec3> vecs = new List<IntVec3>();
                CellRect rect = this.OccupiedRect().ExpandedBy(1);

                foreach (var cell in rect.EdgeCells)
                {
                    if (cell.x == rect.minX && cell.z == rect.minZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.minX && cell.z == rect.maxZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.maxX && cell.z == rect.minZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.maxX && cell.z == rect.maxZ)
                    {
                        continue;
                    }
                    vecs.Add(cell);
                }

                return vecs;
            }
        }
    }
}
