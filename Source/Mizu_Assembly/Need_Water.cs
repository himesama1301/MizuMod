using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Need_Water : Need
    {
        private const float HighSpeedFactorForDebug = 1.0f;

        public const float NeedBorder = 0.3f;

        private const float DehydrationSeverityPerDay = 0.10f;

        private const float DehydrationSeverityPerInterval = DehydrationSeverityPerDay / 150;

        private const float BaseFallPerTick = 1.33E-05f;

        public IntVec3 lastDrinkTerrainPos;

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

        public float WaterFallPerTick
        {
            get
            {
                return this.WaterFallPerTickAssumingCategory(this.CurCategory);
            }
        }

        public float WaterAmountBetweenThirstyAndHealthy
        {
            get
            {
                return (1f - this.PercentageThreshThirsty) * this.MaxLevel;
            }
        }

        public int TicksUntilThirstyWhenHealthy
        {
            get
            {
                return Mathf.CeilToInt(this.WaterAmountBetweenThirstyAndHealthy / this.WaterFallPerTick);
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

        private float WaterFallPerTickAssumingCategory(ThirstCategory cat)
        {
            switch (cat)
            {
                case ThirstCategory.Healthy:
                    return BaseFallPerTick;
                case ThirstCategory.Thirsty:
                    return BaseFallPerTick * 0.5f;
                case ThirstCategory.UrgentlyThirsty:
                    return BaseFallPerTick * 0.25f;
                case ThirstCategory.Dehydration:
                    return BaseFallPerTick * 0.15f;
                default:
                    return 999f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref this.lastDrinkTerrainPos, "lastDrinkTerrainPos", IntVec3.Invalid, false);
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
            if (base.IsFrozen)
            {
                return;
            }
            
            // 水分要求定価
            this.CurLevel -= WaterFallPerTick * 150f * HighSpeedFactorForDebug;

            if (this.Dehydrating)
            {
                // 脱水症状進行度アップ
                HealthUtility.AdjustSeverity(this.pawn, MizuDef.Hediff_Dehydration, DehydrationSeverityPerInterval * HighSpeedFactorForDebug);
            }
            else
            {
                // 脱水症状進行度ダウン
                HealthUtility.AdjustSeverity(this.pawn, MizuDef.Hediff_Dehydration, -DehydrationSeverityPerInterval * HighSpeedFactorForDebug);
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
