using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Faucet : Building_WaterNetWorkTable, IBuilding_WaterNet, IBuilding_DrinkWater
    {
        public override void CreateConnectors()
        {
            this.InputConnectors.Clear();
            this.OutputConnectors.Clear();

            this.InputConnectors.Add(this.Position + this.Rotation.FacingCell);
            this.OutputConnectors.Add(this.Position + this.Rotation.FacingCell);
        }

        public WaterType WaterType
        {
            get
            {
                if (this.InputWaterNet == null) return WaterType.Undefined;

                return this.InputWaterNet.StoredWaterType;
            }
        }

        public float WaterVolume
        {
            get
            {
                if (this.InputWaterNet == null) return 0f;
                return this.InputWaterNet.StoredWaterVolume;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (this.InputWaterNet == null) return true;
                if (this.InputWaterNet.StoredWaterVolume <= 0f) return true;
                return false;
            }
        }

        public bool CanDrinkFor(Pawn p)
        {
            if (p.needs == null || p.needs.water() == null) return false;
            if (this.InputWaterNet == null) return false;
            if (this.InputWaterNet.StoredWaterTypeForFaucet == WaterType.Undefined || this.InputWaterNet.StoredWaterTypeForFaucet == WaterType.NoWater) return false;

            // 手が使用可能で、入力水道網の水量が十分にある
            return p.CanManipulate() && this.InputWaterNet.StoredWaterVolumeForFaucet >= p.needs.water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public bool CanDrawFor(Pawn p)
        {
            if (this.InputWaterNet == null) return false;

            var targetWaterType = this.InputWaterNet.StoredWaterTypeForFaucet;
            if (targetWaterType == WaterType.Undefined || targetWaterType == WaterType.NoWater) return false;

            var waterItemDef = MizuDef.List_WaterItem.First((def) => def.GetCompProperties<CompProperties_WaterSource>().waterType == targetWaterType);
            var compprop = waterItemDef.GetCompProperties<CompProperties_WaterSource>();

            // 汲める予定の水アイテムの水の量より多い
            return p.CanManipulate() && this.InputWaterNet.StoredWaterVolumeForFaucet >= compprop.waterVolume;
        }

        public void DrawWater(float amount)
        {
            if (this.InputWaterNet == null) return;
            this.InputWaterNet.DrawWaterVolumeForFaucet(amount);
        }
    }
}
