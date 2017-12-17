using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public class CompWaterNetTank : CompWaterNet
    {
        private static readonly float BarThick = 0.15f;
        private static readonly Material BarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.8f, 0.8f), false);
        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);

        public new CompProperties_WaterNetTank Props
        {
            get
            {
                return (CompProperties_WaterNetTank)this.props;
            }
        }

        public float AmountCanAccept
        {
            get
            {
                if (!this.IsActivated)
                {
                    return 0f;
                }
                return (this.MaxWaterVolume - this.StoredWaterVolume);
            }
        }

        public float MaxWaterVolume
        {
            get
            {
                return this.Props.maxWaterVolume;
            }
        }
        public bool ShowBar
        {
            get
            {
                return this.Props.showBar;
            }
        }
        public float DrainWaterFlow
        {
            get
            {
                return this.Props.drainWaterFlow;
            }
        }
        public int FlatID
        {
            get
            {
                return this.Props.flatID;
            }
        }

        private float storedWaterVolume = 0;
        public float StoredWaterVolume
        {
            get
            {
                return this.storedWaterVolume;
            }
            set
            {
                if (value > this.MaxWaterVolume)
                {
                    this.storedWaterVolume = this.MaxWaterVolume;
                }
                else if (value < 0.0f)
                {
                    this.storedWaterVolume = 0.0f;
                }
                else
                {
                    this.storedWaterVolume = value;
                }
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

        public bool IsDraining
        {
            get
            {
                return (this.compFlickable != null && !this.compFlickable.SwitchIsOn);
            }
        }

        private CompFlickable compFlickable = null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.storedWaterVolume, "storedWaterVolume");
            Scribe_Values.Look<WaterType>(ref this.storedWaterType, "storedWaterType", WaterType.NoWater);

            if (this.storedWaterVolume > this.MaxWaterVolume)
            {
                this.storedWaterVolume = this.MaxWaterVolume;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compFlickable = this.parent.GetComp<CompFlickable>();

            this.storedWaterVolume = 0.0f;
            this.storedWaterType = WaterType.NoWater;
        }

        public float AddWaterVolume(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative water volume " + amount);
                return 0.0f;
            }

            float prevWaterVolume = this.StoredWaterVolume;
            this.StoredWaterVolume += amount;
            return this.StoredWaterVolume - prevWaterVolume;
        }

        public float DrawWaterVolume(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot draw negative water volume " + amount);
                return 0.0f;
            }
            float prevWaterVolume = this.StoredWaterVolume;
            this.StoredWaterVolume -= amount;
            return prevWaterVolume - this.StoredWaterVolume;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }

            stringBuilder.Append(string.Concat(new string[]
            {
                MizuStrings.InspectWaterTankStored,
                ": ",
                this.StoredWaterVolume.ToString("F2"),
                " / ",
                this.MaxWaterVolume.ToString("F2"),
                " L"
            }));
            if (this.IsDraining)
            {
                stringBuilder.Append(string.Concat(new string[]
                {
                    "(",
                    MizuStrings.InspectWaterTankDraining,
                    ")",
                }));
            }

            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Format("({0})", this.StoredWaterType));
            }

            return stringBuilder.ToString();
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (this.ShowBar)
            {
                GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest();
                r.center = this.parent.DrawPos + Vector3.up * 0.1f + Vector3.back * this.parent.def.size.z / 4.0f;
                r.size = new Vector2(this.parent.RotatedSize.x, BarThick);
                //r.center = new Vector3(this.parent.DrawPos.x, 0.1f, 1.0f - r.size.y / 2.0f);
                //Log.Message(this.parent.DrawPos.ToString());
                r.fillPercent = this.StoredWaterVolume / this.MaxWaterVolume;
                r.filledMat = BarFilledMat;
                r.unfilledMat = BarUnfilledMat;
                r.margin = 0.15f;
                GenDraw.DrawFillableBar(r);
            }
        }
    }
}
