using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Graphic_LinkedWaterNet : Graphic_Linked
    {
        public Graphic_LinkedWaterNet() : base()
        {
        }

        public Graphic_LinkedWaterNet(Graphic subGraphic) : base(subGraphic)
        {
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            IBuilding_WaterNet thing = parent as IBuilding_WaterNet;

            bool foundWaterNetBase = false;
            foreach (var net in thing.WaterNetManager.Nets)
            {
                foreach (var t in net.Things)
                {
                    if (thing.IsConnectedTo(t))
                    {
                        if (t.OccupiedRect().Contains(c))
                        {
                            foundWaterNetBase = true;
                            goto FoundWaterNetBase;

                        }
                    }
                }
            }

            FoundWaterNetBase:

            return GenGrid.InBounds(c, parent.Map) && foundWaterNetBase;
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return new Graphic_LinkedWaterNet(this.subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
            {
                data = this.data
            };
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            Printer_Plane.PrintPlane(layer, parent.TrueCenter(), Vector2.one, this.LinkedDrawMatFrom(parent, parent.Position), 0f, false, null, null, 0.01f);
        }
    }
}
