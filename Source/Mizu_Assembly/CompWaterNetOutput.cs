using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompWaterNetOutput : CompWaterNet
    {
        public new CompProperties_WaterNetOutput Props
        {
            get
            {
                return (CompProperties_WaterNetOutput)this.props;
            }
        }

        protected virtual float MaxOutputWaterFlow
        {
            get
            {
                return this.Props.maxOutputWaterFlow;
            }
        }

        public float OutputWaterFlow { get; private set; }
        public WaterType OutputWaterType { get; private set; }

        private CompWaterNetTank tankComp = null;
        private bool HasTank
        {
            get
            {
                return tankComp != null;
            }
        }
        private bool TankIsEmpty
        {
            get
            {
                return !this.HasTank || tankComp.StoredWaterVolume <= 0.0f;
            }
        }

        public override bool IsActivated
        {
            get
            {
                return base.IsActivated && (!this.HasTank || !this.TankIsEmpty);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.OutputWaterType = WaterType.NoWater;
            tankComp = this.parent.GetComp<CompWaterNetTank>();
        }

        public override void CompTick()
        {
            base.CompTick();

            this.UpdateOutputWaterStatus();
        }

        public void UpdateOutputWaterStatus()
        {
            if (!this.IsActivated)
            {
                // 機能していない
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            if (!this.HasTank)
            {
                // 貯蔵機能なし=ポンプ
                this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                this.OutputWaterFlow = this.MaxOutputWaterFlow;
                return;
            }

            if (this.TankIsEmpty)
            {
                // 貯蔵機能あり、中身なし
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            bool foundNotFullTank = false;
            foreach (var t in this.WaterNetBuilding.WaterNet.Things)
            {
                CompWaterNetTank tankComp = t.GetComp<CompWaterNetTank>();
                CompWaterNetInput inputComp = t.GetComp<CompWaterNetInput>();

                if (t == this.WaterNetBuilding)
                {
                    continue;
                }
                if (inputComp == null || inputComp.InputType != CompProperties_WaterNetInput.InputType.WaterNet)
                {
                    continue;
                }
                if (tankComp == null || tankComp.AmountCanAccept == 0.0f)
                {
                    continue;
                }

                foundNotFullTank = true;
                break;
            }

            if (foundNotFullTank)
            {
                // 貯蔵機能あり、中身あり、水道網の中に満タンでないタンクあり
                this.OutputWaterType = tankComp.StoredWaterType;
                this.OutputWaterFlow = this.MaxOutputWaterFlow;
            }
            else
            {
                // 貯蔵機能あり、中身あり、水道網の中に満タンでないタンクなし
                this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                this.OutputWaterFlow = 0;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(MizuStrings.InspectWaterFlowOutput + ": " + this.OutputWaterFlow.ToString("#####0") + " WaterVolume/day");
            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Format("({0})", this.OutputWaterType));
            }

            return stringBuilder.ToString();
        }
    }
}
