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
            ThingDef thingDef = null;
            Designator_Build designator_build = Find.DesignatorManager.SelectedDesignator as Designator_Build;
            if (designator_build != null)
            {
                thingDef = designator_build.PlacingDef as ThingDef;
            }
            Designator_Install designator_install = Find.DesignatorManager.SelectedDesignator as Designator_Install;
            if (designator_install != null)
            {
                thingDef = designator_install.PlacingDef as ThingDef;
            }

            if (thingDef != null && typeof(IBuilding_WaterNet).IsAssignableFrom(thingDef.thingClass))
            {
                base.DrawLayer();
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
