using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public static class MizuStrings
    {
        public static readonly string ModTitle = "No Water, No Life.";

        // 水アイテムやポーンのジョブ関連
        public static readonly string CompWaterAmount = "MizuWaterAmount";
        public static readonly string FloatMenuGetWater = "MizuConsumeWater";
        public static readonly string JobFailReasonNoWater = "MizuNoWater";

        // コロニー警告(右下あたりのアレ)
        public static readonly string AlertLowWater = "MizuLowWater";
        public static readonly string AlertLowWaterDesc = "MizuLowWaterDesc";
        public static readonly string AlertDehydration = "MizuDehydration";
        public static readonly string AlertDehydrationDesc = "MizuDehydrationDesc";
        public static readonly string AlertDehydrationAnimal = "MizuDehydrationAnimals";
        public static readonly string AlertDehydrationAnimalDesc = "MizuDehydrationAnimalsDesc";

        // キャラバン関連
        public static readonly string MessageCaravanRunOutOfWater = "MizuMessageCaravanRunOutOfWater";
        public static readonly string LabelInfiniteDaysWorthOfWaterInfo = "MizuInfiniteDaysWorthOfWaterInfo";
        public static readonly string LabelDaysWorthOfWaterInfo = "MizuDaysWorthOfWaterInfo";
        public static readonly string LabelDaysWorthOfWaterTooltip = "MizuDaysWorthOfWaterTooltip";
        public static readonly string InspectCaravanOutOfWater = "MizuCaravanOutOfWater";
        public static readonly string InspectCaravanDaysOfWater = "MizuCaravanDaysOfWater";
        public static readonly string LabelDaysWorthOfWaterWarningDialog = "MizuDaysWorthOfWaterWarningDialog";
        public static readonly string LabelDaysWorthOfWaterWarningDialog_NoWater = "MizuDaysWorthOfWaterWarningDialog_NoWater";

        // 水道網関連
        public static readonly string InspectWaterFlowOutput = "MizuWaterFlowOutput";
        public static readonly string InspectWaterFlowInput = "MizuWaterFlowInput";
        public static readonly string InspectWaterTankStored = "MizuWaterTankStored";
        public static readonly string InspectValveClosed = "MizuValveClosed";
        public static readonly string InspectStoredWaterPool = "MizuStoredWaterPool";
        public static readonly string InspectWaterTankDraining = "MizuDraining";

        public static readonly string InspectWaterTypeNo = "MizuWaterTypeNo";
        public static readonly string InspectWaterTypeClear = "MizuWaterTypeClear";
        public static readonly string InspectWaterTypeNormal = "MizuWaterTypeNormal";
        public static readonly string InspectWaterTypeRaw = "MizuWaterTypeRaw";
        public static readonly string InspectWaterTypeMud = "MizuWaterTypeMud";
        public static readonly string InspectWaterTypeSea = "MizuWaterTypeSea";

        public static readonly string AcceptanceReportCannotBuildMulti = "MizuCannotBuildMultiWaterworksBuilding";
        public static readonly string AcceptanceReportCantBuildExceptOverWater = "MizuCantBuildExceptOverWater";

        // 水道管解体
        public static readonly string DesignatorDeconstructPipe = "MizuDesignatorDeconstructPipe";
        public static readonly string DesignatorDeconstructPipeDescription = "MizuDesignatorDeconstructPipeDesc";

        // 雪
        public static readonly string AreaSnowGet = "MizuSnowGet";
        public static readonly string DesignatorAreaSnowGetExpand = "MizuDesignatorAreaSnowGetExpand";
        public static readonly string DesignatorAreaSnowGetExpandDescription = "MizuDesignatorAreaSnowGetExpandDesc";
        public static readonly string DesignatorAreaSnowGetClear = "MizuDesignatorAreaSnowGetClear";
        public static readonly string DesignatorAreaSnowGetClearDescription = "MizuDesignatorAreaSnowGetClearDesc";

        // モップ
        public static readonly string AreaMop = "MizuMop";
        public static readonly string DesignatorAreaMopExpand = "MizuDesignatorAreaMopExpand";
        public static readonly string DesignatorAreaMopExpandDescription = "MizuDesignatorAreaMopExpandDesc";
        public static readonly string DesignatorAreaMopClear = "MizuDesignatorAreaMopClear";
        public static readonly string DesignatorAreaMopClearDescription = "MizuDesignatorAreaMopClearDesc";

        // 水ツール
        public static readonly string InspectWaterToolStored = "MizuWaterToolStored";

        // オプション
        public static readonly string OptionSetDefault = "MizuSetDefault";
        public static readonly string OptionGrowthRateFactorInNotWatering = "MizuGrowthRateFactorInNotWatering";
        public static readonly string OptionGrowthRateFactorInWatering = "MizuGrowthRateFactorInWatering";

        public static string GetInspectWaterTypeString(WaterType waterType)
        {
            switch (waterType)
            {
                case WaterType.NoWater:
                    return MizuStrings.InspectWaterTypeNo.Translate();
                case WaterType.ClearWater:
                    return MizuStrings.InspectWaterTypeClear.Translate();
                case WaterType.NormalWater:
                    return MizuStrings.InspectWaterTypeNormal.Translate();
                case WaterType.RawWater:
                    return MizuStrings.InspectWaterTypeRaw.Translate();
                case WaterType.MudWater:
                    return MizuStrings.InspectWaterTypeMud.Translate();
                case WaterType.SeaWater:
                    return MizuStrings.InspectWaterTypeSea.Translate();
                default:
                    Log.Error("unknown water type");
                    return "unknown";
            }
        }
    }
}
