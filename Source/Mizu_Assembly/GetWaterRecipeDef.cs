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
        public List<WaterType> needWaterTypes = null;
        public float needWaterVolume = 1.0f;
    }
}
