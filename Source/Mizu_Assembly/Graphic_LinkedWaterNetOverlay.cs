using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Graphic_LinkedWaterNetOverlay : Graphic_Linked
    {
        public Graphic_LinkedWaterNetOverlay() : base()
        {
            
        }

        public Graphic_LinkedWaterNetOverlay(Graphic subGraphic) : base(subGraphic)
        {

        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            bool isFound = false;
            IBuilding_WaterNet thing = parent as IBuilding_WaterNet;
            if (thing == null)
            {
                return false;
            }
            if (parent.OccupiedRect().Contains(c))
            {
                return GenGrid.InBounds(c, parent.Map);
            }
            if (!thing.HasConnector)
            {
                return false;
            }

            foreach (var net in thing.WaterNetManager.Nets)
            {
                foreach (var t in net.Things)
                {
                    if (t == thing)
                    {
                        continue;
                    }
                    if (!t.HasConnector)
                    {
                        continue;
                    }

                    if (t.IsConnectedOr(thing) && t.OccupiedRect().Contains(c))
                    {
                        isFound = true;
                    }
                }
                if (isFound) break;
            }

            return GenGrid.InBounds(c, parent.Map) && isFound;
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            foreach (var current in parent.OccupiedRect())
            {
                Vector3 vector = current.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
                Printer_Plane.PrintPlane(layer, vector, Vector2.one, base.LinkedDrawMatFrom(parent, current), 0f, false, null, null, 0.01f);
            }
        }
    }
}
