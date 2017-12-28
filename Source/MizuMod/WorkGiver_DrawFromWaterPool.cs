using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromWaterPool : WorkGiver_DrawWater
    {
        protected override Job CreateJobIfSatisfiedWaterCondition(IBillGiver giver, GetWaterRecipeDef recipe, Bill bill)
        {
            var thing = giver as Thing;
            var waterGrid = thing.Map.GetComponent<MapComponent_ShallowWaterGrid>();
            var pool = waterGrid.GetPool(thing.Map.cellIndices.CellToIndex(thing.Position));

            // 入力水道網の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(pool.WaterType);
            if (waterThingDef == null) return null;

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return null;

            if (!recipe.needWaterTypes.Contains(pool.WaterType)) return null;
            if (pool.CurrentWaterVolume < compprop.waterVolume * recipe.getItemCount) return null;

            return new Job(MizuDef.Job_DrawFromWaterPool, thing) { bill = bill };
        }
    }
}
