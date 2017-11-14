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
            ThingWithComps thing = parent as ThingWithComps;
            CompWaterNetBase comp = thing.GetComp<CompWaterNetBase>();

            bool foundWaterNetBase = false;
            foreach (var net in comp.Manager.Nets)
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

            //for (int i = 0; i < 4; i++)
            //{
            //    IntVec3 intVec = parent.Position + GenAdj.CardinalDirections[i];

            //    CompWaterNetBase comp = parent.TryGetComp<CompWaterNetBase>();
            //    bool foundWaterNetBase = false;
            //    foreach (var net in comp.Manager.Nets)
            //    {
            //        foreach (var t in net.Things)
            //        {
            //            foundWaterNetBase = t.OccupiedRect().Contains(intVec);
            //            goto FoundWaterNetBase;
            //        }
            //    }

            //    FoundWaterNetBase:

            //    if (intVec.InBounds(parent.Map) && foundWaterNetBase && !intVec.GetTerrain(parent.Map).layerable)
            //    {
            //        bool containsPipe = intVec.GetThingList(parent.Map).Any((t) =>
            //        {
            //            return t.TryGetComp<CompWaterNetPipe>() != null;
            //        });

            //        if (!containsPipe)
            //        {
            //            Printer_Plane.PrintPlane(layer, intVec.ToVector3ShiftedWithAltitude(parent.def.Altitude), Vector2.one, this.LinkedDrawMatFrom(parent, intVec), 0f, false, null, null, 0.01f);
            //        }
            //    }
            //}
        }
    }
}
