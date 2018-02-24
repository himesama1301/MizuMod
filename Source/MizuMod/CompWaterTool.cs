using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class CompWaterTool : ThingComp
    {
        public CompProperties_WaterTool Props
        {
            get
            {
                return (CompProperties_WaterTool)this.props;
            }
        }

        public List<CompProperties_WaterTool.UseWorkType> UseWorkType
        {
            get
            {
                return this.Props.useWorkType;
            }
        }
        public List<WorkTypeDef> SupplyWorkType
        {
            get
            {
                return this.Props.supplyWorkType;
            }
        }
        public float MaxWaterVolume
        {
            get
            {
                return this.Props.maxWaterVolume;
            }
        }

        private float storedWaterVolume;
        public float StoredWaterVolume
        {
            get
            {
                return this.storedWaterVolume;
            }
            set
            {
                this.storedWaterVolume = Mathf.Min(this.MaxWaterVolume, Mathf.Max(0f, value));
            }
        }

        private WaterType storedWaterType = WaterType.NoWater;
        public WaterType StoredWaterType
        {
            get
            {
                return this.storedWaterType;
            }
            set
            {
                this.storedWaterType = value;
            }
        }

        public float StoredWaterVolumePercent
        {
            get
            {
                return this.StoredWaterVolume / this.MaxWaterVolume;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref this.storedWaterVolume, "storedWaterVolume");
            Scribe_Values.Look(ref this.storedWaterType, "storedWaterType", WaterType.NoWater);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("StoredWaterVolumePercent:" + (StoredWaterVolumePercent * 100).ToString("F0") + "%");

            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Concat(new string[]
                {
                    "(",
                    StoredWaterVolume.ToString("F2"),
                    "/",
                    MaxWaterVolume.ToString("F2"),
                    ")",
                }));
            }

            stringBuilder.Append(string.Concat(new string[]
            {
                "(",
                MizuStrings.GetInspectWaterTypeString(this.StoredWaterType),
                ")",
            }));

            return stringBuilder.ToString();
        }
    }
}
