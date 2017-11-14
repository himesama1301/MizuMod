using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using UnityEngine;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public class CompWaterNetTank : CompWaterNet
    {
        private static readonly float BarThick = 0.25f;
        private static readonly Material WaterNetTankBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.8f, 0.8f), false);
        private static readonly Material WaterNetTankBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);

        private float storedWaterVolume = 0;

        public float AmountCanAccept
        {
            get
            {
                if (this.IsBrokenDown)
                {
                    return 0f;
                }
                return (this.Props.storedWaterVolumeMax - this.storedWaterVolume);
            }
        }

        public float StoredWaterVolume
        {
            get
            {
                return this.storedWaterVolume;
            }
        }

        public float StoredWaterVolumePercent
        {
            get
            {
                return this.StoredWaterVolume / this.Props.storedWaterVolumeMax;
            }
        }

        public new CompProperties_WaterTank Props
        {
            get
            {
                return (CompProperties_WaterTank)this.props;
            }
        }

        public bool NeedSupply
        {
            get
            {
                return this.AmountCanAccept > 0.0f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.storedWaterVolume, "storedWaterVolume", 0f, false);
            if (this.storedWaterVolume > this.Props.storedWaterVolumeMax)
            {
                this.storedWaterVolume = this.Props.storedWaterVolumeMax;
            }
        }

        public void AddWaterVolume(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative water volume " + amount);
                return;
            }
            if (amount > this.AmountCanAccept)
            {
                amount = this.AmountCanAccept;
            }
            this.storedWaterVolume += amount;
        }

        public float DrawWaterVolume(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot draw negative water volume " + amount);
                return 0.0f;
            }
            if (amount > this.StoredWaterVolume)
            {
                amount = this.StoredWaterVolume;
            }
            this.storedWaterVolume -= amount;
            return amount;
        }

        public override string CompInspectStringExtra()
        {
            string text = string.Concat(new string[]
            {
                MizuStrings.InspectWaterTankStored,
                ": ",
                this.storedWaterVolume.ToString("F0"),
                " / ",
                this.Props.storedWaterVolumeMax.ToString("F0"),
                " WaterVolume"
            });

            string baseStr = base.CompInspectStringExtra();
            if (!string.IsNullOrEmpty(baseStr))
            {
                text += "\n" + baseStr;
            }
            return text;
        }

        public override void PostDraw()
        {
            base.PostDraw();

            GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest();
            r.center = this.parent.DrawPos + Vector3.up * 0.1f;
            r.size = new Vector2(this.parent.RotatedSize.x, BarThick);
            r.fillPercent = this.StoredWaterVolumePercent;
            r.filledMat = CompWaterNetTank.WaterNetTankBarFilledMat;
            r.unfilledMat = CompWaterNetTank.WaterNetTankBarUnfilledMat;
            r.margin = 0.15f;
            GenDraw.DrawFillableBar(r);
        }
    }
}
