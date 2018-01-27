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
        public const float NeedWaterVolumePerDay = 1.5f;
        public const float DrinkFromBuildingMargin = 1.5f;
        public const float MinWaterAmountPerOneDrink = 0.3f;

        private DefExtension_NeedWater extInt;
        private DefExtension_NeedWater ext
        {
            get
            {
                if (this.extInt == null)
                {
                    this.extInt = this.def.GetModExtension<DefExtension_NeedWater>();
                }
                return this.extInt;
            }
        }

        public int lastSearchWaterTick;

        public ThirstCategory CurCategory
        {
            get
            {
                if (this.CurLevelPercentage <= 0f)
                {
                    return ThirstCategory.Dehydration;
                }
                if (this.CurLevelPercentage < this.PercentageThreshUrgentlyThirsty)
                {
                    return ThirstCategory.UrgentlyThirsty;
                }
                if (this.CurLevelPercentage < this.PercentageThreshThirsty)
                {
                    return ThirstCategory.Thirsty;
                }
                if (this.CurLevelPercentage < this.PercentageThreshSlightlyThirsty)
                {
                    return ThirstCategory.SlightlyThirsty;
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
                return this.ext.urgentlyThirstyBorder;
            }
        }

        public float PercentageThreshThirsty
        {
            get
            {
                return this.ext.thirstyBorder;
            }
        }

        public float PercentageThreshSlightlyThirsty
        {
            get
            {
                return this.ext.slightlyThirstyBorder;
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

        public override float MaxLevel
        {
            get
            {
                return this.pawn.BodySize * this.pawn.ageTracker.CurLifeStage.foodMaxFactor;
            }
        }

        public Need_Water(Pawn pawn) : base(pawn)
        {
            this.lastSearchWaterTick = Find.TickManager.TicksGame;
        }

        private float WaterFallPerTickAssumingCategory(ThirstCategory cat)
        {
            // 基本低下量(基本値＋温度補正)
            float fallPerTickBase = this.ext.fallPerTickBase + this.ext.fallPerTickFromTempCurve.Evaluate(this.pawn.AmbientTemperature - this.pawn.ComfortableTemperatureRange().max);

            // 食事と同じ値を利用
            fallPerTickBase *= this.pawn.ageTracker.CurLifeStage.hungerRateFactor * this.pawn.RaceProps.baseHungerRate * this.pawn.health.hediffSet.GetThirstRateFactor();

            switch (cat)
            {
                case ThirstCategory.Healthy:
                    return fallPerTickBase;
                case ThirstCategory.SlightlyThirsty:
                    return fallPerTickBase;
                case ThirstCategory.Thirsty:
                    return fallPerTickBase * 0.5f;
                case ThirstCategory.UrgentlyThirsty:
                    return fallPerTickBase * 0.25f;
                case ThirstCategory.Dehydration:
                    return fallPerTickBase * 0.15f;
                default:
                    return 999f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.lastSearchWaterTick, "lastSearchWaterTick");
        }

        public override void SetInitialLevel()
        {
            if (this.pawn.RaceProps.Humanlike)
            {
                this.CurLevelPercentage = 0.8f;
            }
            else
            {
                this.CurLevelPercentage = Rand.Range(0.5f, 0.8f);
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                this.lastSearchWaterTick = Find.TickManager.TicksGame;
            }
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1F, bool drawArrows = true, bool doTooltip = true)
        {
            if (this.threshPercents == null)
            {
                this.threshPercents = new List<float>()
                {
                    this.PercentageThreshUrgentlyThirsty,
                    this.PercentageThreshThirsty,
                    this.PercentageThreshSlightlyThirsty,
                };
            }

            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }

        public override void NeedInterval()
        {
            if (this.pawn.RaceProps.IsMechanoid) return;
            if (base.IsFrozen) return;
            
            // 水分要求低下
            this.CurLevel -= WaterFallPerTick * 150f * MizuDef.GlobalSettings.forDebug.needWaterReduceRate;

            int directionFactor = -1;
            if (this.Dehydrating)
            {
                // 脱水症状深刻化方向に変更
                directionFactor = 1;
            }

            // 脱水症状進行度更新
            HealthUtility.AdjustSeverity(
                this.pawn,
                MizuDef.Hediff_Dehydration,
                directionFactor * this.ext.dehydrationSeverityPerDay / 150 * MizuDef.GlobalSettings.forDebug.needWaterReduceRate);
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
