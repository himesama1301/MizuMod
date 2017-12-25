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

        public WaterPreferability WaterPreferability
        {
            get
            {
                if (this.InputWaterNet == null) return WaterPreferability.Undefined;

                return this.InputWaterNet.WaterType.ToWaterPreferability();
            }
        }

        public int DrinkWorkAmount
        {
            get
            {
                return 400;
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

            // 手が使用可能で、入力水道網の水量が十分にある
            return p.CanManipulate() && this.InputWaterNet.StoredWaterVolume >= p.needs.water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public void DrawWater(float amount)
        {
            if (this.InputWaterNet == null) return;
            this.InputWaterNet.DrawWaterVolume(amount);
        }
    }
}
