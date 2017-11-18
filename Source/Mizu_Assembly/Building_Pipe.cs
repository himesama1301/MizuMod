using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Building_Pipe : Building_WaterNet, IBuilding_WaterNet
    {
        public override Graphic Graphic
        {
            get
            {
                if (this.Position.GetTerrain(base.Map).layerable)
                {
                    return MizuGraphics.LinkedWaterPipeClear;
                }
                if (this.def.costStuffCount >= 0)
                {
                    return MizuGraphics.LinkedWaterPipe.GetColoredVersion(MizuGraphics.WaterPipe.Shader, this.DrawColor, this.DrawColorTwo);
                }
                return MizuGraphics.LinkedWaterPipe;
            }
        }
    }
}
