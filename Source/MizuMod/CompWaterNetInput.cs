using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

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
        //public virtual CompProperties_WaterNetInput.InputType InputType
        //{
        //    get
        //    {
        //        return this.Props.inputType;
        //    }
        //}
        public virtual List<CompProperties_WaterNetInput.InputType> InputTypes
        {
            get
            {
                return this.Props.inputTypes;
            }
        }
        public virtual CompProperties_WaterNetInput.InputWaterFlowType InputWaterFlowType
        {
            get
            {
                return this.Props.inputWaterFlowType;
            }
        }
        public virtual List<WaterType> AcceptWaterTypes
        {
            get
            {
                return this.Props.acceptWaterTypes;
            }
        }
        public virtual float BaseRainFlow
        {
            get
            {
                return this.Props.baseRainFlow;
            }
        }
        public virtual float RoofEfficiency
        {
            get
            {
                return this.Props.roofEfficiency;
            }
        }
        public virtual int RoofDistance
        {
            get
            {
                return this.Props.roofDistance;
            }
        }

        // 水道網から流し込まれる水量
        // Maxを超えていることもある
        public float InputWaterFlow { get; set; }
        public WaterType InputWaterType { get; set; }

        private bool HasTank
        {
            get
            {
                return this.TankComp != null;
            }
        }
        public bool IsReceiving
        {
            get
            {
                // 水道網から入力するタイプで、現在の入力量が0ではない⇒入力中
                return this.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet) && this.InputWaterFlow > 0f;
            }
        }

        // 入力機能が働いているか
        public override bool IsActivated
        {
            get
            {
                bool isOK = base.IsActivated && this.IsActivatedForWaterNet && (!this.HasTank || this.TankComp.AmountCanAccept > 0.0f);
                if (this.InputWaterFlowType == CompProperties_WaterNetInput.InputWaterFlowType.Constant)
                {
                    isOK &= (this.InputWaterFlow >= this.MaxInputWaterFlow);
                }
                return isOK;
            }
        }

        public CompWaterNetInput() : base()
        {
            this.InputWaterType = WaterType.NoWater;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(MizuStrings.InspectWaterFlowInput + ": " + this.InputWaterFlow.ToString("F2") + " L/day");
            stringBuilder.Append(string.Concat(new string[]
            {
                "(",
                MizuStrings.GetInspectWaterTypeString(this.InputWaterType),
                ")",
            }));

            return stringBuilder.ToString();
        }
    }
}
