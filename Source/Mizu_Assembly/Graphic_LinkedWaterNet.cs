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

            Building_Valve valve = thing as Building_Valve;
            if (valve != null)
            {
                if (!valve.IsOpen || !valve.GetConnectVecs().Contains(c))
                {
                    return false;
                }
            }

            bool isFound = false;
            foreach (var net in comp.Manager.Nets)
            {
                foreach (var t in net.Things)
                {
                    valve = t as Building_Valve;
                    if (valve != null && !valve.IsOpen)
                    {
                        continue;
                    }

                    if (t == thing)
                    {
                        if (t.OccupiedRect().Contains(c))
                        {
                            isFound = true;
                            goto LinkFound;
                        }
                    }
                    else
                    {
                        if (t.OccupiedRect().Contains(c) && t.IsConnectedTo(thing))
                        {
                            isFound = true;
                            goto LinkFound;
                        }
                    }
                }
            }

        LinkFound:
            return GenGrid.InBounds(c, parent.Map) && isFound;
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            foreach (var current in parent.OccupiedRect())
            {
                Vector3 vector = current.ToVector3ShiftedWithAltitude(AltitudeLayer.WorldDataOverlay);
                Printer_Plane.PrintPlane(layer, vector, Vector2.one, base.LinkedDrawMatFrom(parent, current), 0f, false, null, null, 0.01f);
            }
        }
    }
}
