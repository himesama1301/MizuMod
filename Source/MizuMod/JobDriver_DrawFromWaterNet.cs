using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrawFromWaterNet : JobDriver_DrawWater
    {
        private Building_WaterNetWorkTable workTable;
        private WaterNet waterNet;

        public override bool TryMakePreToilReservations()
        {
            if (!base.TryMakePreToilReservations()) return false;

            this.workTable = this.job.GetTarget(BillGiverInd).Thing as Building_WaterNetWorkTable;
            if (this.workTable == null) return false;

            this.waterNet = workTable.InputWaterNet;
            if (this.waterNet == null) return false;

            return true;
        }

        protected override void SetFailCondition()
        {
        }

        protected override Thing FinishAction()
        {
            var targetWaterType = WaterType.NoWater;

            if (ext.canDrawFromFaucet)
            {
                // 蛇口の場合
                targetWaterType = this.waterNet.StoredWaterTypeForFaucet;
            }
            else
            {
                // 自分自身の場合
                targetWaterType = this.workTable.TankComp.StoredWaterType;
            }

            // 水道網の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(targetWaterType);
            if (waterThingDef == null) return null;

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return null;

            // 水道網から水を減らす
            if (ext.canDrawFromFaucet)
            {
                // 蛇口の場合
                this.waterNet.DrawWaterVolumeForFaucet(compprop.waterVolume * ext.getItemCount);
            }
            else
            {
                // 自分自身の場合
                this.workTable.TankComp.DrawWaterVolume(compprop.waterVolume * ext.getItemCount);
            }

            // 水を生成
            var createThing = ThingMaker.MakeThing(waterThingDef);
            if (createThing == null) return null;

            // 個数設定
            createThing.stackCount = ext.getItemCount;
            return createThing;
        }
    }
}
