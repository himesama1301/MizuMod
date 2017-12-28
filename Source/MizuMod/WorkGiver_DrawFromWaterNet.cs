using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromWaterNet : WorkGiver_DrawWater
    {
        protected override Job CreateJobIfSatisfiedWaterCondition(IBillGiver giver, GetWaterRecipeDef recipe, Bill bill)
        {
            var thing = giver as Thing;
            if (thing == null) return null;

            var workTable = giver as Building_WaterNetWorkTable;
            if (workTable == null || workTable.InputWaterNet == null) return null;

            // レシピの要求する水質と現在の水質が合わなければダメ
            if (!recipe.needWaterTypes.Contains(workTable.InputWaterNet.StoredWaterType)) return null;

            // 入力水道網の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(workTable.InputWaterNet.WaterType);
            if (waterThingDef == null) return null;

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return null;

            // 水の量が足りなければダメ
            if (workTable.InputWaterNet.StoredWaterVolume < compprop.waterVolume * recipe.getItemCount) return null;

            return new Job(MizuDef.Job_DrawFromWaterNet, thing) { bill = bill };
        }
    }
}
