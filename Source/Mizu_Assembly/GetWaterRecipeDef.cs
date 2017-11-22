using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class GetWaterRecipeDef : RecipeDef
    {
        public List<WaterTerrainType> needWaterTerrainTypes = null;
        public WaterType needWaterType = WaterType.NoWater;
        public float needWaterVolume = 1.0f;
    }
}
