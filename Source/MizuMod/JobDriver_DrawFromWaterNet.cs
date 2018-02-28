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
        private Building_WaterNetWorkTable WorkTable
        {
            get
            {
                return this.job.GetTarget(BillGiverInd).Thing as Building_WaterNetWorkTable;
            }
        }
        private WaterNet WaterNet
        {
            get
            {
                if (this.WorkTable == null) return null;

                return this.WorkTable.InputWaterNet;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            if (!base.TryMakePreToilReservations()) return false;

            if (this.WorkTable == null) return false;

            if (this.WaterNet == null) return false;

            return true;
        }

        protected override void SetFailCondition()
        {
        }

        protected override Thing FinishAction()
        {
            var targetWaterType = WaterType.NoWater;

            if (this.Ext.canDrawFromFaucet)
            {
                // 蛇口の場合
                targetWaterType = this.WaterNet.StoredWaterTypeForFaucet;
            }
            else
            {
                // 自分自身の場合
                targetWaterType = this.WorkTable.TankComp.StoredWaterType;
            }

            // 水道網の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(targetWaterType);
            if (waterThingDef == null) return null;

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return null;

            // 水道網から水を減らす
            if (this.Ext.canDrawFromFaucet)
            {
                // 蛇口の場合
                this.WaterNet.DrawWaterVolumeForFaucet(compprop.waterVolume * this.Ext.getItemCount);
            }
            else
            {
                // 自分自身の場合
                this.WorkTable.TankComp.DrawWaterVolume(compprop.waterVolume * this.Ext.getItemCount);
            }

            // 水を生成
            var createThing = ThingMaker.MakeThing(waterThingDef);
            if (createThing == null) return null;

            // 個数設定
            createThing.stackCount = this.Ext.getItemCount;
            return createThing;
        }
    }
}
