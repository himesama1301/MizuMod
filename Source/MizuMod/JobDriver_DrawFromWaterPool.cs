using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrawFromWaterPool : JobDriver_DrawWater
    {
        private UndergroundWaterPool pool;

        public override bool TryMakePreToilReservations()
        {
            if (!base.TryMakePreToilReservations()) return false;

            var waterGrid = this.TargetThingA.Map.GetComponent<MapComponent_ShallowWaterGrid>();
            if (waterGrid == null) return false;

            this.pool = waterGrid.GetPool(this.TargetThingA.Map.cellIndices.CellToIndex(this.job.GetTarget(BillGiverInd).Thing.Position));
            if (this.pool == null) return false;

            return true;
        }

        protected override void SetFailCondition()
        {
        }

        protected override Thing FinishAction()
        {
            // 地下水脈の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(this.pool.WaterType);
            if (waterThingDef == null) return null;

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return null;

            // 地下水脈から水を減らす
            this.pool.CurrentWaterVolume = Mathf.Max(0, pool.CurrentWaterVolume - compprop.waterVolume * recipe.getItemCount);

            // 水を生成
            var createThing = ThingMaker.MakeThing(waterThingDef);
            if (createThing == null) return null;

            // 個数設定
            createThing.stackCount = recipe.getItemCount;
            return createThing;
        }

        //public override string GetReport()
        //{
        //    return base.GetReport() + "!!!" + this.CurToilIndex.ToString();
        //}
    }
}
