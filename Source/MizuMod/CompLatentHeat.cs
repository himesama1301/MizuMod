using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompLatentHeat : ThingComp
    {
        public CompProperties_LatentHeat Props
        {
            get
            {
                return (CompProperties_LatentHeat)this.props;
            }
        }

        private float latentHeatAmount;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<float>(ref this.latentHeatAmount, "latentHeatAmount");
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (DebugSettings.godMode)
            {
                if (stringBuilder.ToString() != string.Empty)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append("LatentHeatAmount:" + this.latentHeatAmount.ToString("F2"));
            }
            //if (stringBuilder.ToString() != string.Empty)
            //{
            //    stringBuilder.AppendLine();
            //}
            //stringBuilder.Append(MizuStrings.InspectWaterFlowInput + ": " + this.InputWaterFlow.ToString("F2") + " L/day");
            //stringBuilder.Append(string.Concat(new string[]
            //{
            //    "(",
            //    MizuStrings.GetInspectWaterTypeString(this.InputWaterType),
            //    ")",
            //}));

            return stringBuilder.ToString();
        }
    }
}
