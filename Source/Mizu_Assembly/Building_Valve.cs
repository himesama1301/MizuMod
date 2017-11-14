using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Valve : Building
    {
        private CompFlickable flickableComp;
        private CompWaterNetValve valveComp;

        private bool lastIsOpen = true;

        public bool IsOpen
        {
            get
            {
                return FlickUtility.WantsToBeOn(this);
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return this.flickableComp.CurrentGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.flickableComp = base.GetComp<CompFlickable>();
            this.valveComp = base.GetComp<CompWaterNetValve>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.lastIsOpen, "lastIsOpen");
        }

        public override void Tick()
        {
            base.Tick();

            if (lastIsOpen != IsOpen)
            {
                lastIsOpen = IsOpen;
                valveComp.Manager.RefreshWaterNets();
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
                stringBuilder.Append("VentClosed".Translate());
            }
            return stringBuilder.ToString();
        }
    }
}
