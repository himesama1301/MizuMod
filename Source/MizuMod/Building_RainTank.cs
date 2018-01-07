using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_RainTank : Building_WaterNetWorkTable, IBuilding_WaterNet, IBuilding_DrinkWater
    {
        public override WaterType OutputWaterType
        {
            get
            {
                return WaterType.RawWater;
            }
        }

        public WaterType WaterType
        {
            get
            {
                if (this.TankComp == null) return WaterType.Undefined;

                return this.TankComp.StoredWaterType;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (this.TankComp == null) return true;
                if (this.TankComp.StoredWaterVolume <= 0f) return true;
                return false;
            }
        }

        public bool CanDrinkFor(Pawn p)
        {
            if (p.needs == null || p.needs.water() == null) return false;
            if (this.TankComp == null) return false;

            // 手が使用可能で、タンクの水量が十分にある
            return p.CanManipulate() && this.TankComp.StoredWaterVolume >= p.needs.water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public bool CanDrawFor(Pawn p)
        {
            if (this.TankComp == null) return false;

            var waterItemDef = MizuDef.List_WaterItem.First((def) => def.GetCompProperties<CompProperties_WaterSource>().waterType == this.TankComp.StoredWaterType);
            var compprop = waterItemDef.GetCompProperties<CompProperties_WaterSource>();

            // 汲める予定の水アイテムの水の量より多い
            return p.CanManipulate() && this.TankComp.StoredWaterVolume >= compprop.waterVolume;
        }

        public void DrawWater(float amount)
        {
            if (this.TankComp == null) return;
            this.TankComp.DrawWaterVolume(amount);
        }
    }
}
