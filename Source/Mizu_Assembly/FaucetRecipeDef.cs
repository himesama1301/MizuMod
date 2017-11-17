using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class FaucetRecipeDef : RecipeDef
    {
        public WaterType needWaterType = WaterType.NoWater;
        public float needWaterVolume = 1.0f;
    }
}
