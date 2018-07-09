using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class CompProperties_WaterNetTank : CompProperties_WaterNet
    {
        public enum DrawType : byte
        {
            Undefined = 0,
            Self,
            Faucet,
        }

        public float maxWaterVolume = 0f;
        public bool showBar = true;
        public int flatID = -1;
        public List<DrawType> drawTypes = new List<DrawType>()
        {
            DrawType.Faucet,
        };

        public CompProperties_WaterNetTank() : base(typeof(CompWaterNetTank)) { }
        public CompProperties_WaterNetTank(Type compClass) : base(compClass) { }
    }
}
