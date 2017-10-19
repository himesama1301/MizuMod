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
        public static JobDef Job_DrinkWater = DefDatabase<JobDef>.GetNamed("Mizu_DrinkWater", true);
        public static JobDef Job_FeedWaterPatient = DefDatabase<JobDef>.GetNamed("Mizu_FeedWaterPatient", true);
        public static JobDef Job_DeliverWater = DefDatabase<JobDef>.GetNamed("Mizu_DeliverWater", true);

        public static StatCategoryDef StatCategory_Water = DefDatabase<StatCategoryDef>.GetNamed("Mizu_WaterCategory", true);

        public static HediffDef Hediff_Dehydration = DefDatabase<HediffDef>.GetNamed("Mizu_Dehydration", true);

        public static RecordDef Record_WaterDrank = DefDatabase<RecordDef>.GetNamed("Mizu_WaterDrank", true);

        public static ThoughtDef Thought_DrankWaterDirectly = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankWaterDirectly", true);
    }
}
