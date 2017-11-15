using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Faucet : Building_WorkTable, IBuilding_WaterNetBase
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
                vecs.Add(this.Position + this.Rotation.FacingCell);
                return vecs;
            }
        }
    }
}
