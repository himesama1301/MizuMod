using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class RecipeWorkerCounter_AnyWater : RecipeWorkerCounter
    {
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
                foreach (ThingDef current in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (current.thingCategories != null && current.thingCategories.Contains(MizuDef.ThingCategory_Waters))
                    {
                        this.waterDefs.Add(current);
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
            return MizuDef.ThingCategory_Waters.label;
        }
    }
}
