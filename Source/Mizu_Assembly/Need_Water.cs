using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Need_Water : Need
    {
        private const float HighSpeedFactorForDebug = 1.0f;

        public const float NeedBorder = 0.3f;

        private const float DehydrationSeverityPerDay = 0.17f;

        private const float DehydrationSeverityPerInterval = DehydrationSeverityPerDay / 150;

        private const float BaseFallPerTick = 2.0E-05f;

        public ThirstCategory CurCategory
        {
            get
            {
                if (this.CurLevel <= 0f)
                {
                    return ThirstCategory.Dehydration;
                }
                if (this.CurLevel < NeedBorder * 0.4f)
                {
                    return ThirstCategory.UrgentlyThirsty;
                }
                if (this.CurLevel < NeedBorder * 0.8f)
                {
                    return ThirstCategory.Thirsty;
                }
                return ThirstCategory.Healthy;
            }
        }

        public bool Dehydrating
        {
            get
            {
                return this.CurCategory == ThirstCategory.Dehydration;
            }
        }

        public float PercentageThreshUrgentlyThirsty
        {
            get
            {
                return NeedBorder * 0.4f;
            }
        }

        public float PercentageThreshThirsty
        {
            get
            {
                return NeedBorder * 0.8f;
            }
        }

        public float WaterWanted
        {
            get
            {
                return this.MaxLevel - this.CurLevel;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                return -1;
            }
        }

        public Need_Water(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(this.PercentageThreshUrgentlyThirsty);
            this.threshPercents.Add(this.PercentageThreshThirsty);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SetInitialLevel()
        {
            this.CurLevel = Rand.Range(0.5f, 0.8f);
        }

        public override void NeedInterval()
        {
            if (this.pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if (!base.IsFrozen)
            {
                this.CurLevel -= BaseFallPerTick * 150f * HighSpeedFactorForDebug;
            }
            if (!base.IsFrozen)
            {
                if (this.Dehydrating)
                {
                    HealthUtility.AdjustSeverity(this.pawn, MizuDef.Hediff_Dehydration, DehydrationSeverityPerInterval * HighSpeedFactorForDebug);
                }
                else
                {
                    HealthUtility.AdjustSeverity(this.pawn, MizuDef.Hediff_Dehydration, -DehydrationSeverityPerInterval * HighSpeedFactorForDebug);
                }
            }
        }

        public override string GetTipString()
        {
            return string.Concat(new string[]
            {
                base.LabelCap,
                ": ",
                base.CurLevelPercentage.ToStringPercent(),
                " (",
                this.CurLevel.ToString("0.##"),
                " / ",
                this.MaxLevel.ToString("0.##"),
                ")\n",
                this.def.description
            });
        }
    }
}
