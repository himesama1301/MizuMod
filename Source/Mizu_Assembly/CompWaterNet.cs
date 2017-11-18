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
        protected CompBreakdownable breakdownableComp;
        protected CompPowerTrader powerComp;

        protected bool IsBrokenDown
        {
            get
            {
                return this.parent.IsBrokenDown();
            }
        }

        protected bool WantsToBeOn
        {
            get
            {
                return FlickUtility.WantsToBeOn(this.parent);
            }
        }

        protected bool PowerOn
        {
            get
            {
                return (this.powerComp == null || this.powerComp.PowerOn);
            }
        }

        public virtual bool IsActivated
        {
            get
            {
                return !this.IsBrokenDown && this.WantsToBeOn && this.PowerOn;
            }
        }

        public CompProperties_WaterNet Props
        {
            get
            {
                return (CompProperties_WaterNet)this.props;
            }
        }

        public IBuilding_WaterNet WaterNetBuilding
        {
            get
            {
                return this.parent as IBuilding_WaterNet;
            }
        }

        public MapComponent_WaterNetManager WaterNetManager
        {
            get
            {
                return this.parent.Map.GetComponent<MapComponent_WaterNetManager>();
            }
        }

        public WaterNet WaterNet = null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            this.powerComp = this.parent.GetComp<CompPowerTrader>();
        }

        public override string CompInspectStringExtra()
        {
            string str = string.Empty;

            if (DebugSettings.godMode)
            {
                str = string.Format("Water Net ID = {0}", this.WaterNet != null ? this.WaterNet.ID.ToString() : "null");
                str += "\nWater Type = " + (this.WaterNet != null ? this.WaterNet.WaterType.ToString() : "null");
            }

            string baseStr = base.CompInspectStringExtra();
            if (!string.IsNullOrEmpty(baseStr))
            {
                str += "\n" + baseStr;
            }
            return str;
        }
    }
}
