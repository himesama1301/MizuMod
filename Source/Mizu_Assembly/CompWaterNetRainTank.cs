using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompWaterNetRainTank : CompWaterNetTank
    {
        protected virtual float DesiredWaterFlow
        {
            get
            {
                return base.Props.baseWaterFlow;
            }
        }

        public float RainCharge
        {
            get
            {
                return this.Props.baseRainCharge;
            }
        }

        public new CompProperties_RainTank Props
        {
            get
            {
                return (CompProperties_RainTank)this.props;
            }
        }

        public virtual float WaterFlow { get; set; }

        public override bool CanSupplyFromWaterNet
        {
            get
            {
                return false;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.UpdateDesiredWaterFlow();
        }

        public void UpdateDesiredWaterFlow()
        {
            if (this.IsBrokenDown || !this.SwitchIsOn || !this.PowerOn || this.StoredWaterVolume <= 0.0f)
            {
                this.WaterFlow = 0f;
            }
            else
            {
                this.WaterFlow = this.DesiredWaterFlow;
            }
        }

        public override string CompInspectStringExtra()
        {
            string str;
            str = MizuStrings.InspectWaterFlowOutput + ": " + this.WaterFlow.ToString("#####0") + " WaterVolume/day";

            string baseStr = base.CompInspectStringExtra();
            if (!string.IsNullOrEmpty(baseStr))
            {
                str += "\n" + baseStr;
            }
            return str;
        }
    }
}
