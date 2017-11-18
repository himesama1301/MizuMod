using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompProperties_WaterNetInput : CompProperties_WaterNet
    {
        public enum InputType : byte
        {
            Undefined = 0,
            WaterNet,
            Rain,
        }

        public enum InputWaterFlowType : byte
        {
            Undefined = 0,
            Constant,
            Any,
        }

        public float maxInputWaterFlow = float.MaxValue;
        public InputType inputType = InputType.WaterNet;
        public InputWaterFlowType inputWaterFlowType = InputWaterFlowType.Any;

        public CompProperties_WaterNetInput() : base(typeof(CompWaterNetInput)) { }
        public CompProperties_WaterNetInput(Type compClass) : base(compClass) { }
    }
}
