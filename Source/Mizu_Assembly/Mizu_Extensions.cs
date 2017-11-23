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
                // 水分量0のアイテムの必要個数を計算させようとしている
                Log.Error("error in GetWaterCalculateAmounts : waterAmount == 0.0f");
                numTaken = 0;
                waterGot = 0.0f;
                return;
            }

            numTaken = (int)Math.Ceiling(waterWanted / waterAmount);  // そのアイテムで必要な水分を満たすのに何個必要か
            numTaken = Math.Min(Math.Min(numTaken, t.stackCount), t.TryGetComp<CompWater>().MaxNumToGetAtOnce);  // 必要数、スタック数、同時摂取可能数のうち最も低い数字
            numTaken = Math.Max(numTaken, 1);  // 最低値は1
            waterGot = (float)numTaken * waterAmount;  // 個数と1個当たりの水分の積→摂取水分量
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

        public static bool IsOutputTo(this IBuilding_WaterNet t1, IBuilding_WaterNet t2)
        {
            if (!t1.HasOutputConnector)
            {
                return false;
            }
            if (!t2.HasInputConnector)
            {
                return false;
            }

            bool t1_out_t2_body = false;
            foreach (var vec1 in t1.OutputConnectors)
            {
                foreach (var vec2 in t2.OccupiedRect())
                {
                    if (vec1 == vec2)
                    {
                        t1_out_t2_body = true;
                        break;
                    }
                }
                if (t1_out_t2_body) break;
            }

            bool t1_body_t2_in = false;
            foreach (var vec1 in t1.OccupiedRect())
            {
                foreach (var vec2 in t2.InputConnectors)
                {
                    if (vec1 == vec2)
                    {
                        t1_body_t2_in = true;
                        break;
                    }
                }
                if (t1_body_t2_in) break;
            }

            return t1_out_t2_body && t1_body_t2_in;
        }

        public static bool IsConnectedOr(this IBuilding_WaterNet t1, IBuilding_WaterNet t2)
        {
            return t1.IsOutputTo(t2) || t2.IsOutputTo(t1);
        }
        public static bool IsConnectedAnd(this IBuilding_WaterNet t1, IBuilding_WaterNet t2)
        {
            return t1.IsOutputTo(t2) && t2.IsOutputTo(t1);
        }

        public static WaterTerrainType GetWaterTerrainType(this TerrainDef def)
        {
            if (def.IsSea())
            {
                return WaterTerrainType.SeaWater;
            }
            else if (def.IsRiver())
            {
                return WaterTerrainType.FreshWater;
            }
            else if (def.IsLakeOrPond())
            {
                return WaterTerrainType.FreshWater;
            }
            else if (def.IsMarsh())
            {
                return WaterTerrainType.MudWater;
            }
            return WaterTerrainType.NoWater;
        }

        public static bool IsSea(this TerrainDef def)
        {
            return def.defName.Contains("WaterOcean");
        }

        public static bool IsRiver(this TerrainDef def)
        {
            return def.defName.Contains("WaterMoving");
        }

        public static bool IsLakeOrPond(this TerrainDef def)
        {
            return !def.IsSea() && !def.IsRiver() && def.defName.Contains("Water");
        }

        public static bool IsMarsh(this TerrainDef def)
        {
            return def.defName.Contains("Marsh");
        }

        public static bool IsMud(this TerrainDef def)
        {
            return def.defName.Contains("Mud");
        }

        public static WaterType ToWaterType(this TerrainDef def)
        {
            if (def.IsSea())
            {
                return WaterType.SeaWater;
            }
            else if (def.IsMarsh() || def.IsMud())
            {
                return WaterType.MudWater;
            }
            else if (def.IsRiver() || def.IsLakeOrPond())
            {
                return WaterType.NormalWater;
            }
            return WaterType.NoWater;
        }

        public static bool CanGetWater(this TerrainDef def)
        {
            return def.IsRiver() || def.IsLakeOrPond() || def.IsMarsh() || def.IsSea();
        }

        public static bool CanManipulate(this Pawn pawn)
        {
            return pawn.RaceProps.ToolUser && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        }
    }
}
