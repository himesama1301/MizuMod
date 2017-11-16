using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public enum WaterPreferability : byte
    {
        Undefined = 0,
        NeverDrink = 1,
        TerrainWater = 2,
        SeaWater = 3,
        MudWater = 4,
        RainWater = 5,
        NormalWater = 6,
        ClearWater = 7,
    }
}
