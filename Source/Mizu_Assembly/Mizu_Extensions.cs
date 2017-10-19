using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public static class Mizu_Extensions
    {
        public static bool CanDrinkWater(this Thing t)
        {
            return (t.TryGetComp<CompWater>() != null);
        }

        public static bool CanDrinkWaterNow(this Thing t)
        {
            return !t.IsBurning() && t.CanDrinkWater();
        }

        public static bool CanGetWater(this Thing t)
        {
            if (!t.CanDrinkWater())
            {
                return false;
            }
            CompWater comp = t.TryGetComp<CompWater>();
            return (comp.WaterAmount > 0.0f);
        }

        public static float GetWaterAmount(this Thing t)
        {
            CompWater comp = t.TryGetComp<CompWater>();
            return (comp != null) ? Math.Max(comp.WaterAmount, 0.0f) : 0.0f;
        }

        public static WaterPreferability GetWaterPreferability(this Thing t)
        {
            CompWater comp = t.TryGetComp<CompWater>();
            return (comp != null) ? comp.WaterPreferability : WaterPreferability.Undefined;
        }

        public static Need_Water water(this Pawn_NeedsTracker needs)
        {
            return needs.TryGetNeed<Need_Water>();
        }

        public static void GetWaterCalculateAmounts(this Thing t, Pawn getter, float waterWanted, out int numTaken, out float waterGot)
        {
            float waterAmount = t.GetWaterAmount();
            if (waterAmount == 0.0f)
            {
                Log.Error("error in GetWaterCalculateAmounts : waterAmount == 0.0f");
                numTaken = 0;
                waterGot = 0.0f;
                return;
            }
            numTaken = (int)Math.Ceiling(waterWanted / waterAmount);
            numTaken = Math.Min(Math.Min(numTaken, t.stackCount), t.TryGetComp<CompWater>().MaxNumToGetAtOnce);
            numTaken = Math.Max(numTaken, 1);
            waterGot = (float)numTaken * waterAmount;
        }

        public static float TotalWater(this ResourceCounter rc)
        {
            float num = 0.0f;
            foreach (KeyValuePair<ThingDef, int> current in rc.AllCountedAmounts)
            {
                if (current.Key.comps == null)
                {
                    continue;
                }
                CompProperties_Water comp = (CompProperties_Water)current.Key.comps.Find((c) => c.compClass == typeof(CompWater));
                if (comp == null)
                {
                    continue;
                }
                if (comp.waterAmount > 0.0f)
                {
                    num += comp.waterAmount * (float)current.Value;
                }
            }
            return num;
        }
    }
}
