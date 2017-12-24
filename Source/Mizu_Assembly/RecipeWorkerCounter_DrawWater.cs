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
            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            int num = 0;
            foreach (var waterDef in MizuDef.List_WaterItem)
            {
                num += bill.Map.resourceCounter.GetCount(waterDef);
            }
            return num;
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return null;
        }
    }
}
