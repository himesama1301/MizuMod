using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public abstract class CompWaterNet : ThingComp
    {
        public WaterNet WaterNet = null;

        protected CompFlickable flickableComp;
        protected CompBreakdownable breakdownableComp;
        protected CompPowerTrader powerComp;

        protected bool IsBrokenDown
        {
            get
            {
                return this.parent.IsBrokenDown();
            }
        }

        protected bool SwitchIsOn
        {
            get
            {
                return (this.flickableComp == null || this.flickableComp.SwitchIsOn);
            }
        }

        protected bool PowerOn
        {
            get
            {
                return (this.powerComp == null || this.powerComp.PowerOn);
            }
        }

        public CompProperties_WaterNet Props
        {
            get
            {
                return (CompProperties_WaterNet)this.props;
            }
        }

        public MapComponent_WaterNetManager Manager
        {
            get
            {
                return this.parent.Map.GetComponent<MapComponent_WaterNetManager>();
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.flickableComp = this.parent.GetComp<CompFlickable>();
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            this.powerComp = this.parent.GetComp<CompPowerTrader>();

            this.Manager.AddThing(this.parent);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            map.GetComponent<MapComponent_WaterNetManager>().RemoveThing(this.parent);
        }

        public override string CompInspectStringExtra()
        {
            string str = string.Empty;

            if (DebugSettings.godMode)
            {
                str = string.Format("Net ID = {0}", this.WaterNet.ID);
            }

            string baseStr = base.CompInspectStringExtra();
            if (!string.IsNullOrEmpty(baseStr))
            {
                str += "\n" + baseStr;
            }
            return str;
        }

        public void PrintForGrid(SectionLayer sectionLayer)
        {
            MizuGraphics.LinkedWaterNet.Print(sectionLayer, this.parent);
        }
    }
}
