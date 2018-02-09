using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompLatentHeat : ThingComp
    {
        public CompProperties_LatentHeat Props
        {
            get
            {
                return (CompProperties_LatentHeat)this.props;
            }
        }

        public ThingDef ChangedThingDef
        {
            get
            {
                return this.Props.changedThingDef;
            }
        }

        private float latentHeatAmount;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<float>(ref this.latentHeatAmount, "latentHeatAmount");
        }

        public override void CompTickRare()
        {
            base.CompTick();

            this.latentHeatAmount += 1f;

            if (this.latentHeatAmount >= 4f)
            {
                var changedThing = ThingMaker.MakeThing(this.ChangedThingDef);
                changedThing.stackCount = this.parent.stackCount;

                var map = this.parent.Map;
                if (map != null)
                {
                    GenSpawn.Spawn(changedThing, parent.Position, map);
                    this.parent.Destroy(DestroyMode.Vanish);
                    return;
                }

                var owner = this.parent.holdingOwner;
                if (owner != null)
                {
                    owner.Remove(this.parent);
                    this.parent.Destroy(DestroyMode.Vanish);
                    if (owner.TryAdd(changedThing) == false)
                    {
                        Log.Error("failed TryAdd");
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (DebugSettings.godMode)
            {
                if (stringBuilder.ToString() != string.Empty)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append("LatentHeatAmount:" + this.latentHeatAmount.ToString("F2"));
            }
            //if (stringBuilder.ToString() != string.Empty)
            //{
            //    stringBuilder.AppendLine();
            //}
            //stringBuilder.Append(MizuStrings.InspectWaterFlowInput + ": " + this.InputWaterFlow.ToString("F2") + " L/day");
            //stringBuilder.Append(string.Concat(new string[]
            //{
            //    "(",
            //    MizuStrings.GetInspectWaterTypeString(this.InputWaterType),
            //    ")",
            //}));

            return stringBuilder.ToString();
        }
    }
}
