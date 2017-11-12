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
            CompWaterNet comp = parent.TryGetComp<CompWaterNet>();
            ThingWithComps linkThing = null;

            foreach (var net in comp.Manager.Nets)
            {
                foreach (var t in net.Things)
                {
                    foreach (var occupiedVec in t.OccupiedRect())
                    {
                        if (occupiedVec == c)
                        {
                            linkThing = t;
                            goto LinkFound;
                        }
                    }
                }
            }
        LinkFound:
            return GenGrid.InBounds(c, parent.Map) && (linkThing != null) && (linkThing.GetComp<CompWaterNet>() != null);
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            foreach (var current in GenAdj.OccupiedRect(parent))
            {
                Vector3 vector = current.ToVector3ShiftedWithAltitude(AltitudeLayer.WorldDataOverlay);
                Printer_Plane.PrintPlane(layer, vector, Vector2.one, base.LinkedDrawMatFrom(parent, current), 0f, false, null, null, 0.01f);
            }
        }
    }
}
