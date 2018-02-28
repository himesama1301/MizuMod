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
        public static readonly string AlertDehydrationAnimal = "MizuDehydrationAnimals".Translate();
        public static readonly string AlertDehydrationAnimalDesc = "MizuDehydrationAnimalsDesc".Translate();

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

        public static readonly string InspectWaterTypeNo = "MizuWaterTypeNo".Translate();
        public static readonly string InspectWaterTypeClear = "MizuWaterTypeClear".Translate();
        public static readonly string InspectWaterTypeNormal = "MizuWaterTypeNormal".Translate();
        public static readonly string InspectWaterTypeRaw = "MizuWaterTypeRaw".Translate();
        public static readonly string InspectWaterTypeMud = "MizuWaterTypeMud".Translate();
        public static readonly string InspectWaterTypeSea = "MizuWaterTypeSea".Translate();

        public static readonly string AcceptanceReportCannotBuildMulti = "MizuCannotBuildMultiWaterworksBuilding".Translate();

        // 水道管解体
        public static readonly string DesignatorDeconstructPipe = "MizuDesignatorDeconstructPipe".Translate();
        public static readonly string DesignatorDeconstructPipeDescription = "MizuDesignatorDeconstructPipeDesc".Translate();


        public static string GetInspectWaterTypeString(WaterType waterType)
        {
            switch (waterType)
            {
                case WaterType.NoWater:
                    return MizuStrings.InspectWaterTypeNo;
                case WaterType.ClearWater:
                    return MizuStrings.InspectWaterTypeClear;
                case WaterType.NormalWater:
                    return MizuStrings.InspectWaterTypeNormal;
                case WaterType.RawWater:
                    return MizuStrings.InspectWaterTypeRaw;
                case WaterType.MudWater:
                    return MizuStrings.InspectWaterTypeMud;
                case WaterType.SeaWater:
                    return MizuStrings.InspectWaterTypeSea;
                default:
                    Log.Error("unknown water type");
                    return "unknown";
            }
        }
    }
}
