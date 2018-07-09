using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompProperties_WaterNetOutput : CompProperties_WaterNet
    {
        public enum OutputWaterFlowType : byte
        {
            Undefined = 0,
            Constant,
            Any,
        }

        public float maxOutputWaterFlow = 0.0f;
        public WaterType forceOutputWaterType = WaterType.Undefined;
        public OutputWaterFlowType outputWaterFlowType = OutputWaterFlowType.Constant;

        public CompProperties_WaterNetOutput() : base(typeof(CompWaterNetOutput)) { }
        public CompProperties_WaterNetOutput(Type compClass) : base(compClass) { }
    }
}
