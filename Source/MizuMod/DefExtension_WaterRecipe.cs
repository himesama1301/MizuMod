using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class DefExtension_WaterRecipe : DefModExtension
    {
        public enum RecipeType : byte
        {
            Undefined = 0,
            DrawFromTerrain,
            DrawFromWaterPool,
            DrawFromWaterNet,
            PourWater,
        }

        public RecipeType recipeType = RecipeType.Undefined;
        public List<WaterTerrainType> needWaterTerrainTypes = null;
        public List<WaterType> needWaterTypes = null;
        public int getItemCount = 1;
    }
}
