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
        private bool lastIsActivatedForWaterNet;

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
                return this.WaterNetBuilding.SwitchIsOn;
            }
        }

        protected bool PowerOn
        {
            get
            {
                return this.WaterNetBuilding.PowerOn;
            }
        }

        public virtual bool IsActivated
        {
            get
            {
                return this.WaterNetBuilding.IsActivated;
            }
        }

        public virtual bool IsActivatedForWaterNet
        {
            get
            {
                return this.WaterNetBuilding.IsActivatedForWaterNet;
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

        protected CompWaterNetInput InputComp
        {
            get
            {
                return this.WaterNetBuilding.InputComp;
            }
        }
        protected CompWaterNetOutput OutputComp
        {
            get
            {
                return this.WaterNetBuilding.OutputComp;
            }
        }
        protected CompWaterNetTank TankComp
        {
            get
            {
                return this.WaterNetBuilding.TankComp;
            }
        }

        public CompWaterNet() : base()
        {

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.lastIsActivatedForWaterNet, "lastIsActivatedForWaterNet");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            this.lastIsActivatedForWaterNet = this.IsActivatedForWaterNet;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (this.lastIsActivatedForWaterNet != this.IsActivatedForWaterNet)
            {
                this.lastIsActivatedForWaterNet = this.IsActivatedForWaterNet;
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
