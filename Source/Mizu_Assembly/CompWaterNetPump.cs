using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class CompWaterNetPump : CompWaterNet
    {
        protected virtual float DesiredWaterFlow
        {
            get
            {
                return base.Props.baseWaterFlow;
            }
        }

        public virtual float WaterFlow { get; set; }

        public override void CompTick()
        {
            base.CompTick();
            this.UpdateDesiredWaterFlow();
        }

        public void UpdateDesiredWaterFlow()
        {
            if (this.IsBrokenDown || !this.SwitchIsOn || !this.PowerOn)
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
