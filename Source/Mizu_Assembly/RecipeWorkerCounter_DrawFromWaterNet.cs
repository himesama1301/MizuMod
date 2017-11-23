using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class RecipeWorkerCounter_DrawFromWaterNet : RecipeWorkerCounter
    {
        public GetWaterRecipeDef GetWaterRecipe
        {
            get
            {
                return (GetWaterRecipeDef)this.recipe;
            }
        }

        private List<ThingDef> waterDefs;

        public override bool CanCountProducts(Bill_Production bill)
        {
            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            if (this.waterDefs == null)
            {
                // 水アイテム定義リストの生成(初回のみ)
                this.waterDefs = new List<ThingDef>();
                foreach (var waterType in this.GetWaterRecipe.needWaterTypes)
                {
                    var thingDef = MizuUtility.GetWaterThingDefFromWaterType(waterType);
                    if (thingDef != null)
                    {
                        this.waterDefs.Add(thingDef);
                    }
                }
            }
            int num = 0;
            for (int i = 0; i < this.waterDefs.Count; i++)
            {
                num += bill.Map.resourceCounter.GetCount(this.waterDefs[i]);
            }
            return num;
        }

        public override string ProductsDescription(Bill_Production bill)
        {
            return null;
        }
    }
}
