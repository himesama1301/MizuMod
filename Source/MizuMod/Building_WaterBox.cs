using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Building_WaterBox : Building_WaterNetWorkTable, IBuilding_WaterNet, IBuilding_DrinkWater
    {
        public override Graphic Graphic
        {
            get
            {
                if (this.TankComp == null) return MizuGraphics.LinkedWaterBox0.GetColoredVersion(MizuGraphics.WaterPipe.Shader, this.DrawColor, this.DrawColorTwo);
                if (this.TankComp.StoredWaterVolumePercent <= 0.05f) return MizuGraphics.LinkedWaterBox0.GetColoredVersion(MizuGraphics.WaterBox0.Shader, this.DrawColor, this.DrawColorTwo);
                if (this.TankComp.StoredWaterVolumePercent <= 0.35f) return MizuGraphics.LinkedWaterBox1.GetColoredVersion(MizuGraphics.WaterBox1.Shader, this.DrawColor, this.DrawColorTwo);
                if (this.TankComp.StoredWaterVolumePercent <= 0.65f) return MizuGraphics.LinkedWaterBox2.GetColoredVersion(MizuGraphics.WaterBox2.Shader, this.DrawColor, this.DrawColorTwo);
                if (this.TankComp.StoredWaterVolumePercent <= 0.95f) return MizuGraphics.LinkedWaterBox3.GetColoredVersion(MizuGraphics.WaterBox3.Shader, this.DrawColor, this.DrawColorTwo);

                return MizuGraphics.LinkedWaterBox4.GetColoredVersion(MizuGraphics.WaterBox4.Shader, this.DrawColor, this.DrawColorTwo);
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

            // タンクの水量が十分にある
            return this.TankComp.StoredWaterVolume >= p.needs.water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public void DrawWater(float amount)
        {
            if (this.TankComp == null) return;
            this.TankComp.DrawWaterVolume(amount);
        }
    }
}
