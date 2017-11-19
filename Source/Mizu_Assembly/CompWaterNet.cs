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
        private bool lastIsActivated;

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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.lastIsActivated, "lastIsActivated");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.lastIsActivated = this.IsActivated;
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            this.powerComp = this.parent.GetComp<CompPowerTrader>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (this.lastIsActivated != this.IsActivated)
            {
                this.lastIsActivated = this.IsActivated;
                foreach (var vec in this.WaterNetBuilding.OccupiedRect().ExpandedBy(1))
                {
                    this.WaterNetManager.map.mapDrawer.MapMeshDirty(vec, MapMeshFlag.Things);
                    this.WaterNetManager.map.mapDrawer.MapMeshDirty(vec, MapMeshFlag.Buildings);
                }
                this.WaterNetManager.RequestUpdateWaterNet();
            }
        }
    }
}
