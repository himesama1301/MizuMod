using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    internal class SectionLayer_WaterNet : SectionLayer_Things
    {
        public SectionLayer_WaterNet(Section section) : base(section)
		{
            this.requireAddToMapMesh = false;
            this.relevantChangeTypes = MapMeshFlag.Buildings;
        }

        public override void DrawLayer()
        {
            Designator_Build designator = Find.DesignatorManager.SelectedDesignator as Designator_Build;
            if (designator != null)
            {
                ThingDef thingDef = designator.PlacingDef as ThingDef;
                if (thingDef != null && typeof(IBuilding_WaterNet).IsAssignableFrom(thingDef.thingClass))
                {
                    base.DrawLayer();
                }
            }
        }

        protected override void TakePrintFrom(Thing t)
        {
            IBuilding_WaterNet building = t as IBuilding_WaterNet;
            if (building != null)
            {
                building.PrintForGrid(this);
            }
        }
    }
}
