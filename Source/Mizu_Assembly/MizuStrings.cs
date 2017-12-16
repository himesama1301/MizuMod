using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public static class MizuStrings
    {
        // 水アイテムやポーンのジョブ関連
        public static readonly string CompWaterAmount = "MizuWaterAmount".Translate();
        public static readonly string FloatMenuGetWater = "MizuConsumeWater".Translate();
        public static readonly string JobFailReasonNoWater = "MizuNoWater".Translate();

        // コロニー警告(右下あたりのアレ)
        public static readonly string AlertLowWater = "MizuLowWater".Translate();
        public static readonly string AlertLowWaterDesc = "MizuLowWaterDesc".Translate();
        public static readonly string AlertDehydration = "MizuDehydration".Translate();
        public static readonly string AlertDehydrationDesc = "MizuDehydrationDesc".Translate();

        // キャラバン関連
        public static readonly string MessageCaravanRunOutOfWater = "MizuMessageCaravanRunOutOfWater".Translate();
        public static readonly string LabelInfiniteDaysWorthOfWaterInfo = "MizuInfiniteDaysWorthOfWaterInfo".Translate();
        public static readonly string LabelDaysWorthOfWaterInfo = "MizuDaysWorthOfWaterInfo".Translate();
        public static readonly string LabelDaysWorthOfWaterTooltip = "MizuDaysWorthOfWaterTooltip".Translate();
        public static readonly string InspectCaravanOutOfWater = "MizuCaravanOutOfWater".Translate();
        public static readonly string InspectCaravanDaysOfWater = "MizuCaravanDaysOfWater".Translate();
        public static readonly string LabelDaysWorthOfWaterWarningDialog = "MizuDaysWorthOfWaterWarningDialog".Translate();
        public static readonly string LabelDaysWorthOfWaterWarningDialog_NoWater = "MizuDaysWorthOfWaterWarningDialog_NoWater".Translate();

        // 水道網関連
        public static readonly string InspectWaterFlowOutput = "MizuWaterFlowOutput".Translate();
        public static readonly string InspectWaterFlowInput = "MizuWaterFlowInput".Translate();
        public static readonly string InspectWaterTankStored = "MizuWaterTankStored".Translate();
        public static readonly string InspectValveClosed = "MizuValveClosed".Translate();
        public static readonly string InspectStoredWaterPool = "MizuStoredWaterPool".Translate();
        public static readonly string InspectWaterTankDraining = "MizuDraining".Translate();
    }
}
