using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Building_WaterBox : Building_WaterNetWorkTable, IBuilding_WaterNet, IBuilding_DrinkWater
    {
        private List<float> graphicThreshold = new List<float>()
        {
            0.05f,
            0.35f,
            0.65f,
            0.95f,
            100f,
        };

        private int graphicIndex = 0;
        private int prevGraphicIndex = 0;

        public override Graphic Graphic
        {
            get
            {
                return MizuGraphics.LinkedWaterBoxes[this.graphicIndex].GetColoredVersion(MizuGraphics.WaterBoxes[this.graphicIndex].Shader, this.DrawColor, this.DrawColorTwo);
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
            if (this.TankComp.StoredWaterType == WaterType.Undefined || this.TankComp.StoredWaterType == WaterType.NoWater) return false;

            // タンクの水量が十分にある
            return this.TankComp.StoredWaterVolume >= p.needs.water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public bool CanDrawFor(Pawn p)
        {
            if (this.TankComp == null) return false;
            if (this.TankComp.StoredWaterType == WaterType.Undefined || this.TankComp.StoredWaterType == WaterType.NoWater) return false;

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

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.graphicIndex, "graphicIndex");
            this.prevGraphicIndex = this.graphicIndex;
        }

        public override void Tick()
        {
            base.Tick();

            this.prevGraphicIndex = this.graphicIndex;
            if (this.TankComp == null)
            {
                this.graphicIndex = 0;
                return;
            }

            for (int i = 0; i < this.graphicThreshold.Count; i++)
            {
                if (this.TankComp.StoredWaterVolumePercent < this.graphicThreshold[i])
                {
                    this.graphicIndex = i;
                    break;
                }
            }

            if (this.graphicIndex != this.prevGraphicIndex)
            {
                this.DirtyMapMesh(this.Map);
            }
        }
    }
}
