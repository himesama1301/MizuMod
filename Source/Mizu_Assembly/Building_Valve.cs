using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Valve : Building_WaterNet, IBuilding_WaterNet
    {
        private CompFlickable flickableComp;

        private bool lastSwitchIsOn = true;

        public override Graphic Graphic
        {
            get
            {
                if (flickableComp == null)
                {
                    return base.Graphic;
                }
                return this.flickableComp.CurrentGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.flickableComp = base.GetComp<CompFlickable>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.lastSwitchIsOn, "lastSwitchIsOn");
        }

        public override void Tick()
        {
            base.Tick();

            if (lastSwitchIsOn != this.flickableComp.SwitchIsOn)
            {
                lastSwitchIsOn = this.flickableComp.SwitchIsOn;
                this.WaterNetManager.RefreshWaterNets();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (!FlickUtility.WantsToBeOn(this))
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append(MizuStrings.InspectValveClosed);
            }
            return stringBuilder.ToString();
        }

        public override void CreateConnectors()
        {
            this.Connectors.Clear();
            this.Connectors.Add(this.Position + this.Rotation.FacingCell);
            this.Connectors.Add(this.Position + this.Rotation.FacingCell * (-1));
        }
    }
}
