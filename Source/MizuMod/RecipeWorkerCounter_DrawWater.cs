using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class RecipeWorkerCounter_DrawWater : RecipeWorkerCounter
    {
        public GetWaterRecipeDef GetWaterRecipe
        {
            get
            {
                return (GetWaterRecipeDef)this.recipe;
            }
        }

        public override bool CanCountProducts(Bill_Production bill)
        {
            var ext = bill.recipe.GetModExtension<DefExtension_WaterRecipe>();
            if (ext == null) return false;

            var building = bill.billStack.billGiver as IBuilding_DrinkWater;
            if (building == null) return false;

            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            var ext = bill.recipe.GetModExtension<DefExtension_WaterRecipe>();
            var building = bill.billStack.billGiver as IBuilding_DrinkWater;

            var waterDef = MizuUtility.GetWaterThingDefFromWaterType(building.WaterType);
            int numOfWater = bill.Map.resourceCounter.GetCount(waterDef);

            return numOfWater;
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return null;
        }
    }
}
