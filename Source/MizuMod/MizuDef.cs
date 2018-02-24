using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public static class MizuDef
    {
        public static GlobalSettingDef GlobalSettings = DefDatabase<MizuMod.GlobalSettingDef>.GetNamed("Mizu_GlobalSettings");

        public static NeedDef Need_Water = DefDatabase<NeedDef>.GetNamed("Mizu_Water");

        public static JobDef Job_DrinkWater = DefDatabase<JobDef>.GetNamed("Mizu_DrinkWater");
        public static JobDef Job_DrinkWaterFromBuilding = DefDatabase<JobDef>.GetNamed("Mizu_DrinkWaterFromBuilding");
        public static JobDef Job_FeedWaterPatient = DefDatabase<JobDef>.GetNamed("Mizu_FeedWaterPatient");
        public static JobDef Job_DeliverWater = DefDatabase<JobDef>.GetNamed("Mizu_DeliverWater");

        public static JobDef Job_DrawFromTerrain = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromTerrain");
        public static JobDef Job_DrawFromWaterPool = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromWaterPool");
        public static JobDef Job_DrawFromWaterNet = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromWaterNet");
        public static JobDef Job_PourWater = DefDatabase<JobDef>.GetNamed("Mizu_PourWater");
        public static JobDef Job_DrawWaterByPrisoner = DefDatabase<JobDef>.GetNamed("Mizu_DrawWaterByPrisoner");

        public static JobDef Job_GetSnow = DefDatabase<JobDef>.GetNamed("Mizu_GetSnow");
        public static JobDef Job_Mop = DefDatabase<JobDef>.GetNamed("Mizu_Mop");

        public static StatCategoryDef StatCategory_Water = DefDatabase<StatCategoryDef>.GetNamed("Mizu_WaterCategory");

        public static HediffDef Hediff_Dehydration = DefDatabase<HediffDef>.GetNamed("Mizu_Dehydration");
        public static HediffDef Hediff_DrankSeaWater = DefDatabase<HediffDef>.GetNamed("Mizu_DrankSeaWater");

        public static RecordDef Record_WaterDrank = DefDatabase<RecordDef>.GetNamed("Mizu_WaterDrank");
        public static RecordDef Record_WaterDrew = DefDatabase<RecordDef>.GetNamed("Mizu_WaterDrew");

        public static ThoughtDef Thought_DrankClearWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankClearWater");
        public static ThoughtDef Thought_DrankMudWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankMudWater");
        public static ThoughtDef Thought_DrankSeaWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankSeaWater");
        public static ThoughtDef Thought_DrankScoopedWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankScoopedWater");
        public static ThoughtDef Thought_SippedWaterLikeBeast = DefDatabase<ThoughtDef>.GetNamed("Mizu_SippedWaterLikeBeast");
        public static ThoughtDef Thought_AteIcyFoodInHotSeason = DefDatabase<ThoughtDef>.GetNamed("Mizu_AteIcyFoodInHotSeason");

        public static ThingDef Thing_ClearWater = DefDatabase<ThingDef>.GetNamed("Mizu_ClearWater");
        public static ThingDef Thing_NormalWater = DefDatabase<ThingDef>.GetNamed("Mizu_NormalWater");
        public static ThingDef Thing_RawWater = DefDatabase<ThingDef>.GetNamed("Mizu_RawWater");
        public static ThingDef Thing_MudWater = DefDatabase<ThingDef>.GetNamed("Mizu_MudWater");
        public static ThingDef Thing_SeaWater = DefDatabase<ThingDef>.GetNamed("Mizu_SeaWater");

        public static ThingDef Thing_Snowball = DefDatabase<ThingDef>.GetNamed("Mizu_Snowball");

        public static ThingDef Thing_WaterPipe = DefDatabase<ThingDef>.GetNamed("Mizu_WaterPipe");
        public static ThingDef Thing_WaterPipeInWater = DefDatabase<ThingDef>.GetNamed("Mizu_WaterPipeInWater");

        public static ThingDef Thing_MoppedThing = DefDatabase<ThingDef>.GetNamed("Mizu_MoppedThing");

        public static ThingCategoryDef ThingCategory_Waters = DefDatabase<ThingCategoryDef>.GetNamed("Mizu_Waters");

        public static WaterTypeDef WaterType_Clear = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeClear");
        public static WaterTypeDef WaterType_Normal = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeNormal");
        public static WaterTypeDef WaterType_Raw = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeRaw");
        public static WaterTypeDef WaterType_Mud = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeMud");
        public static WaterTypeDef WaterType_Sea = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeSea");

        public static List<ThingDef> List_WaterItem;
        public static Dictionary<WaterType, WaterTypeDef> Dic_WaterTypeDef;

        static MizuDef()
        {
            List_WaterItem = new List<ThingDef>()
            {
                Thing_ClearWater,
                Thing_NormalWater,
                Thing_RawWater,
                Thing_MudWater,
                Thing_SeaWater,
            };

            Dic_WaterTypeDef = new Dictionary<WaterType, WaterTypeDef>()
            {
                { WaterType.ClearWater, WaterType_Clear },
                { WaterType.NormalWater, WaterType_Normal },
                { WaterType.RawWater, WaterType_Raw },
                { WaterType.MudWater, WaterType_Mud },
                { WaterType.SeaWater, WaterType_Sea },
            };
        }
    }
}
