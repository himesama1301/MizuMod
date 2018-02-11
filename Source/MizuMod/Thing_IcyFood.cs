using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Thing_IcyFood : ThingWithComps
    {
        public const float BorderTemperature = 30f;

        protected override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);

            // 外気温が一定の温度以上→暑い季節
            if (ingester.Map.mapTemperature.OutdoorTemp > BorderTemperature)
            {
                if (ingester.needs.mood != null)
                {
                    // 暑い季節に氷菓を食べた
                    ingester.needs.mood.thoughts.memories.TryGainMemory(MizuDef.Thought_AteIcyFoodInHotSeason);
                }
            }
        }
    }
}
