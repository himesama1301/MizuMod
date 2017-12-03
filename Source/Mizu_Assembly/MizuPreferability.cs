using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public enum WaterPreferability : byte
    {
        Undefined = 0,
        NeverDrink = 10,
        TerrainWater = 20,
        SeaWater = 30,
        MudWater = 40,
        NaturalWater = 50,
        NormalWater = 60,
        ClearWater = 70,
    }
}
