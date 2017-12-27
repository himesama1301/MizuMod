using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public static class MizuGraphics
    {
        public static Texture2D Texture_ButtonIngest = ContentFinder<Texture2D>.Get("UI/Buttons/Ingest");

        // 水道管
        public static Graphic WaterPipeClear = GraphicDatabase.Get<Graphic_Single>("Things/Mizu_Clear", ShaderDatabase.Transparent);
        public static Graphic WaterPipe = GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterPipe", ShaderDatabase.Transparent);
        public static Graphic WaterNet = GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterNet", ShaderDatabase.MetaOverlay);

        public static Graphic_LinkedWaterNet LinkedWaterPipeClear = new Graphic_LinkedWaterNet(MizuGraphics.WaterPipeClear);
        public static Graphic_LinkedWaterNet LinkedWaterPipe = new Graphic_LinkedWaterNet(MizuGraphics.WaterPipe);
        public static Graphic_LinkedWaterNetOverlay LinkedWaterNetOverlay = new Graphic_LinkedWaterNetOverlay(MizuGraphics.WaterNet);

        // 水箱
        public static Graphic WaterBox0 = GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox0", ShaderDatabase.Transparent);
        public static Graphic WaterBox4 = GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox4", ShaderDatabase.CutoutComplex);

        public static Graphic_Linked LinkedWaterBox0 = new Graphic_Linked(MizuGraphics.WaterBox0);
        public static Graphic_Linked LinkedWaterBox4 = new Graphic_Linked(MizuGraphics.WaterBox4);

    }
}
