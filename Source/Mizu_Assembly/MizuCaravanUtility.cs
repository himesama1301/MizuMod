using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MizuMod
{
    public static class MizuCaravanUtility
    {
        public static bool TryGetBestWater(Caravan caravan, Pawn forPawn, out Thing water, out Pawn owner)
        {
            List<Thing> inv_thing_list = CaravanInventoryUtility.AllInventoryItems(caravan);
            Thing found_thing = null;
            float best_score = 0f;
            for (int i = 0; i < inv_thing_list.Count; i++)
            {
                Thing inv_thing = inv_thing_list[i];
                if (MizuCaravanUtility.CanNowGetWater(inv_thing, forPawn))
                {
                    float foodScore = MizuCaravanUtility.GetWaterScore(inv_thing, forPawn);
                    if (found_thing == null || foodScore > best_score)
                    {
                        found_thing = inv_thing;
                        best_score = foodScore;
                    }
                }
            }
            if (found_thing != null)
            {
                water = found_thing;
                owner = CaravanInventoryUtility.GetOwnerOf(caravan, found_thing);
                return true;
            }
            water = null;
            owner = null;
            return false;
        }

        public static bool CanNowGetWater(Thing water, Pawn pawn)
        {
            return water.CanDrinkWaterNow() && MizuCaravanUtility.CanEverGetWater(water, pawn) && (pawn.needs.water().CurCategory >= ThirstCategory.Dehydration || water.GetWaterPreferability() > WaterPreferability.NeverDrink);
        }

        public static bool CanEverGetWater(Thing water, Pawn pawn)
        {
            return water.CanGetWater() && (water.GetWaterPreferability() > WaterPreferability.NeverDrink);
        }

        public static float GetWaterScore(Thing water, Pawn pawn)
        {
            return (float)water.GetWaterPreferability();
        }
    }
}
