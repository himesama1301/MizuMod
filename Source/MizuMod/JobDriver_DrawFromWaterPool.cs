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
        private MapComponent_ShallowWaterGrid waterGridInt;
        private MapComponent_ShallowWaterGrid WaterGrid
        {
            get
            {
                if (this.waterGridInt == null)
                {
                    this.waterGridInt = this.TargetThingA.Map.GetComponent<MapComponent_ShallowWaterGrid>();
                }
                return this.waterGridInt;
            }
        }

        private UndergroundWaterPool poolInt;
        private UndergroundWaterPool Pool
        {
            get
            {
                if (this.poolInt == null)
                {
                    this.poolInt = this.WaterGrid.GetPool(this.TargetThingA.Map.cellIndices.CellToIndex(this.job.GetTarget(BillGiverInd).Thing.Position));
                }
                return this.poolInt;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            if (!base.TryMakePreToilReservations()) return false;

            if (this.WaterGrid == null) return false;

            if (this.Pool == null) return false;

            return true;
        }

        protected override void SetFailCondition()
        {
        }

        protected override Thing FinishAction()
        {
            // 地下水脈の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(this.Pool.WaterType);
            if (waterThingDef == null) return null;

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return null;

            // 地下水脈から水を減らす
            this.Pool.CurrentWaterVolume = Mathf.Max(0, this.Pool.CurrentWaterVolume - compprop.waterVolume * this.Ext.getItemCount);

            // 水を生成
            var createThing = ThingMaker.MakeThing(waterThingDef);
            if (createThing == null) return null;

            // 個数設定
            createThing.stackCount = this.Ext.getItemCount;
            return createThing;
        }

        //public override string GetReport()
        //{
        //    return base.GetReport() + "!!!" + this.CurToilIndex.ToString();
        //}
    }
}
