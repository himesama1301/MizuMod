using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class CompWaterNetValve : CompWaterNetBase
    {
        public override string CompInspectStringExtra()
        {
            string str = string.Empty;

            if (DebugSettings.godMode)
            {
                IntVec3 curPos = this.parent.Position;
                IntVec3 frontPos = this.parent.Position + this.parent.Rotation.FacingCell;
                IntVec3 backPos = this.parent.Position + this.parent.Rotation.FacingCell * (-1);

                str = string.Format("Pos={0},Front={1},Back={2}", curPos, frontPos, backPos);
            }

            string baseStr = base.CompInspectStringExtra();
            if (!string.IsNullOrEmpty(baseStr))
            {
                str += "\n" + baseStr;
            }
            return str;
        }

        public override void PrintForGrid(SectionLayer sectionLayer)
        {
            Building_Valve valve = this.parent as Building_Valve;
            if (valve != null && valve.IsOpen)
            {
                base.PrintForGrid(sectionLayer);
            }
        }
    }
}
