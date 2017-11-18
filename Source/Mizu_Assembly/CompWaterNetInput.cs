using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompWaterNetInput : CompWaterNet
    {
        public new CompProperties_WaterNetInput Props
        {
            get
            {
                return (CompProperties_WaterNetInput)this.props;
            }
        }

        public virtual float MaxInputWaterFlow
        {
            get
            {
                return this.Props.maxInputWaterFlow;
            }
        }
        public virtual CompProperties_WaterNetInput.InputType InputType
        {
            get
            {
                return this.Props.inputType;
            }
        }
        public virtual CompProperties_WaterNetInput.InputWaterFlowType InputWaterFlowType
        {
            get
            {
                return this.Props.inputWaterFlowType;
            }
        }

        public float InputWaterFlow { get; set; }

        private CompWaterNetTank tankComp = null;
        private bool HasTank
        {
            get
            {
                return tankComp != null;
            }
        }
        private bool TankCanAccept
        {
            get
            {
                return !this.HasTank || tankComp.AmountCanAccept > 0.0f;
            }
        }

        public override bool IsActivated
        {
            get
            {
                bool isOK = base.IsActivated && this.TankCanAccept;
                if (this.InputWaterFlowType == CompProperties_WaterNetInput.InputWaterFlowType.Constant)
                {
                    isOK &= (this.InputWaterFlow >= this.MaxInputWaterFlow);
                }
                return isOK;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            tankComp = this.parent.GetComp<CompWaterNetTank>();
        }

        public override void CompTick()
        {
            base.CompTick();

            //this.UpdateInputWaterStatus();
        }

        public void UpdateInputWaterStatus()
        {
            if (!this.IsActivated)
            {
                // 機能していない
                this.InputWaterFlow = 0f;
                return;
            }

            if (!this.HasTank)
            {
                // 貯蔵機能なし=フィルター
                this.InputWaterFlow = this.MaxInputWaterFlow;
                return;
            }

            if (!this.TankCanAccept)
            {
                // 貯蔵機能あり、受け入れ不可
                this.InputWaterFlow = 0f;
            }
            else
            {
                // 貯蔵機能あり、受け入れ可
                this.InputWaterFlow = this.MaxInputWaterFlow;
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
            stringBuilder.Append(MizuStrings.InspectWaterFlowInput + ": " + this.InputWaterFlow.ToString("#####0") + " WaterVolume/day");

            return stringBuilder.ToString();
        }
    }
}
