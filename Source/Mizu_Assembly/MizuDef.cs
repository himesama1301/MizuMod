using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public static class MizuDef
    {
        public static JobDef Job_DrinkWater = DefDatabase<JobDef>.GetNamed("Mizu_DrinkWater");
        public static JobDef Job_FeedWaterPatient = DefDatabase<JobDef>.GetNamed("Mizu_FeedWaterPatient");
        public static JobDef Job_DeliverWater = DefDatabase<JobDef>.GetNamed("Mizu_DeliverWater");
        public static JobDef Job_DoBillFaucet = DefDatabase<JobDef>.GetNamed("Mizu_DoBillFaucet");
        public static JobDef Job_DoBillTank = DefDatabase<JobDef>.GetNamed("Mizu_DoBillTank");

        public static JobDef Job_DrawFromTerrain = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromTerrain");
        public static JobDef Job_DrawFromWaterPool = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromWaterPool");

        public static StatCategoryDef StatCategory_Water = DefDatabase<StatCategoryDef>.GetNamed("Mizu_WaterCategory");

        public static HediffDef Hediff_Dehydration = DefDatabase<HediffDef>.GetNamed("Mizu_Dehydration");

        public static RecordDef Record_WaterDrank = DefDatabase<RecordDef>.GetNamed("Mizu_WaterDrank");

        public static ThoughtDef Thought_DrankWaterDirectly = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankWaterDirectly");

        public static ThingDef Thing_ClearWater = DefDatabase<ThingDef>.GetNamed("Mizu_ClearWater");
        public static ThingDef Thing_NormalWater = DefDatabase<ThingDef>.GetNamed("Mizu_NormalWater");
        public static ThingDef Thing_RainWater = DefDatabase<ThingDef>.GetNamed("Mizu_RainWater");
        public static ThingDef Thing_MudWater = DefDatabase<ThingDef>.GetNamed("Mizu_MudWater");
        public static ThingDef Thing_SeaWater = DefDatabase<ThingDef>.GetNamed("Mizu_SeaWater");

        public static ThingCategoryDef ThingCategory_Waters = DefDatabase<ThingCategoryDef>.GetNamed("Mizu_Waters");
    }
}
