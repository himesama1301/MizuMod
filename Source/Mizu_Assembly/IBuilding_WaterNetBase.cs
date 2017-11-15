using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    interface IBuilding_WaterNetBase
    {
        bool IsActivatedForWaterNet { get; }
        List<IntVec3> ConnectVecs { get; }
    }
}
