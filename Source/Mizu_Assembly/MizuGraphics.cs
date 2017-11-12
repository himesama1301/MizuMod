using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public static class MizuGraphics
    {
        public static Graphic WaterNet = GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterNet", ShaderDatabase.MetaOverlay);

        public static Graphic_LinkedWaterNet LinkedWaterNet = new Graphic_LinkedWaterNet(MizuGraphics.WaterNet);
    }
}
