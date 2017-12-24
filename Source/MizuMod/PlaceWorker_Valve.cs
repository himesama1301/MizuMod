using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_Valve : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            IntVec3 intVecSouth = center + IntVec3.South.RotatedBy(rot);
            IntVec3 intVecNorth = center + IntVec3.North.RotatedBy(rot);
            GenDraw.DrawFieldEdges(new List<IntVec3> { intVecSouth }, Color.blue);
            GenDraw.DrawFieldEdges(new List<IntVec3> { intVecNorth }, Color.blue);
        }
    }
}
