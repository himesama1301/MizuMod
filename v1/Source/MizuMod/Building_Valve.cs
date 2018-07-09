using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    // バルブの場合、スイッチON/OFF⇒バルブの開閉(水を通すかどうか)
    public class Building_Valve : Building_WaterNet, IBuilding_WaterNet
    {
        private bool lastSwitchIsOn = true;

        public override bool HasInputConnector
        {
            get
            {
                return base.HasInputConnector && this.SwitchIsOn;
            }
        }

        public override bool HasOutputConnector
        {
            get
            {
                return base.HasOutputConnector && this.SwitchIsOn;
            }
        }

        public override bool IsActivatedForWaterNet
        {
            get
            {
                return base.IsActivatedForWaterNet && this.SwitchIsOn;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (this.flickableComp == null) return base.Graphic;

                return this.flickableComp.CurrentGraphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.lastSwitchIsOn, "lastSwitchIsOn");
        }

        public override void Tick()
        {
            base.Tick();

            if (lastSwitchIsOn != this.SwitchIsOn)
            {
                lastSwitchIsOn = this.SwitchIsOn;
                this.WaterNetManager.UpdateWaterNets();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (!this.SwitchIsOn)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append(MizuStrings.InspectValveClosed.Translate());
            }
            return stringBuilder.ToString();
        }

        public override void CreateConnectors()
        {
            this.InputConnectors.Clear();
            this.OutputConnectors.Clear();

            this.InputConnectors.Add(this.Position + this.Rotation.FacingCell);
            this.InputConnectors.Add(this.Position + this.Rotation.FacingCell * (-1));

            this.OutputConnectors.Add(this.Position + this.Rotation.FacingCell);
            this.OutputConnectors.Add(this.Position + this.Rotation.FacingCell * (-1));
        }
    }
}
