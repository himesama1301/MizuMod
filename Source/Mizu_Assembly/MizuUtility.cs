using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public static class MizuUtility
    {
        private static HashSet<Thing> filtered = new HashSet<Thing>();
        private static List<ThoughtDef> getWaterThoughts = new List<ThoughtDef>();

        public static bool TryFindBestWaterSourceFor(Pawn getter, Pawn eater, out Thing waterSource, out ThingDef waterDef, bool canUseInventory = true, bool allowForbidden = false, bool allowSociallyImproper = false)
        {
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);

            Thing thing = null;
            if (canUseInventory)
            {
                if (getterCanManipulate)
                {
                    thing = MizuUtility.BestWaterInInventory(getter, null, WaterPreferability.SeaWater, WaterPreferability.ClearWater, 0f);
                }
                if (thing != null)
                {
                    if (getter.Faction != Faction.OfPlayer)
                    {
                        waterSource = thing;
                        waterDef = thing.def;
                        return true;
                    }
                    waterSource = thing;
                    waterDef = waterSource.def;
                    return true;
                }
            }

            bool allowPlant = getter == eater;
            Thing thing2 = MizuUtility.BestWaterSourceOnMap(getter, eater, WaterPreferability.SeaWater, allowForbidden, allowSociallyImproper);
            if (thing == null && thing2 == null)
            {
                if (canUseInventory && getterCanManipulate)
                {
                    thing = MizuUtility.BestWaterInInventory(getter, null, WaterPreferability.SeaWater, WaterPreferability.ClearWater, 0f);
                    if (thing != null)
                    {
                        waterSource = thing;
                        waterDef = thing.def;
                        return true;
                    }
                }
                waterSource = null;
                waterDef = null;
                return false;
            }
            if (thing == null && thing2 != null)
            {
                waterSource = thing2;
                waterDef = waterSource.def;
                return true;
            }
            if (thing2 == null && thing != null)
            {
                waterSource = thing;
                waterDef = waterSource.def;
                return true;
            }
            float num = MizuUtility.WaterSourceOptimality(eater, thing2, (float)(getter.Position - thing2.Position).LengthManhattan, false);
            float num2 = MizuUtility.WaterSourceOptimality(eater, thing, 0f, false);
            num2 -= 32f;
            if (num > num2)
            {
                waterSource = thing2;
                waterDef = waterSource.def;
                return true;
            }
            waterSource = thing;
            waterDef = waterSource.def;
            return true;
        }

        public static Thing BestWaterInInventory(Pawn holder, Pawn eater = null, WaterPreferability minWaterPref = WaterPreferability.SeaWater, WaterPreferability maxWaterPref = WaterPreferability.ClearWater, float minStackWaterAmount = 0.0f)
        {
            if (holder.inventory == null)
            {
                return null;
            }
            if (eater == null)
            {
                eater = holder;
            }
            ThingOwner<Thing> innerContainer = holder.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Thing thing = innerContainer[i];
                float waterAmount = thing.GetWaterAmount();
                WaterPreferability waterPreferability = thing.GetWaterPreferability();

                if (thing.CanGetWater() && thing.CanDrinkWaterNow() && waterPreferability >= minWaterPref && waterPreferability <= maxWaterPref)
                {
                    float num = waterAmount * (float)thing.stackCount;
                    if (num >= minStackWaterAmount)
                    {
                        return thing;
                    }
                }
            }
            return null;
        }

        public static Thing BestWaterSourceOnMap(Pawn getter, Pawn eater, WaterPreferability maxPref = WaterPreferability.ClearWater, bool allowForbidden = false, bool allowSociallyImproper = false)
        {
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                Log.Error(string.Concat(new object[]
                {
                    getter,
                    " tried to find food to bring to ",
                    eater,
                    " but ",
                    getter,
                    " is incapable of Manipulation."
                }));
                return null;
            }
            WaterPreferability minPref = WaterPreferability.SeaWater;

            Predicate<Thing> waterValidator = delegate (Thing t)
            {
                if (!allowForbidden && t.IsForbidden(getter))
                {
                    return false;
                }

                if (!t.CanGetWater())
                {
                    return false;
                }

                float waterAmount = t.GetWaterAmount();
                WaterPreferability waterPreferability = t.GetWaterPreferability();
                if (waterPreferability < minPref)
                {
                    return false;
                }
                if (waterPreferability > maxPref)
                {
                    return false;
                }
                if (!t.CanDrinkWaterNow() || !MizuUtility.IsWaterSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) || !getter.AnimalAwareOf(t) || !getter.CanReserve(t, 1, -1, null, false))
                {
                    return false;
                }
                return true;
            };

            Thing thing;
            if (getter.RaceProps.Humanlike)
            {
                Predicate<Thing> validator = waterValidator;
                thing = MizuUtility.SpawnedWaterSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything).FindAll((t) => t.CanDrinkWaterNow()), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
            }
            else
            {
                int searchRegionsMax = 30;
                if (getter.Faction == Faction.OfPlayer)
                {
                    searchRegionsMax = 100;
                }
                MizuUtility.filtered.Clear();
                foreach (Thing current in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, true))
                {
                    Pawn pawn = current as Pawn;
                    if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == MizuDef.Job_DrinkWater && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
                    {
                        MizuUtility.filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
                    }
                }
                bool flag = !allowForbidden && ForbidUtility.CaresAboutForbidden(getter, true) && getter.playerSettings != null && getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null;
                Predicate<Thing> predicate = (Thing t) => waterValidator(t) && !MizuUtility.filtered.Contains(t) && t.GetWaterPreferability() > WaterPreferability.SeaWater;
                Predicate<Thing> validator = predicate;
                bool ignoreEntirelyForbiddenRegions = flag;
                thing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, searchRegionsMax, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                MizuUtility.filtered.Clear();
                if (thing == null)
                {
                    validator = waterValidator;
                    ignoreEntirelyForbiddenRegions = flag;
                    thing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, searchRegionsMax, false, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                }
            }
            return thing;
        }

        private static bool IsWaterSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
        {
            if (!allowSociallyImproper)
            {
                bool animalsCare = !getter.RaceProps.Animal;
                if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
                {
                    return false;
                }
            }
            return true;
        }

        private static Thing SpawnedWaterSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null)
        {
            if (searchSet == null)
            {
                return null;
            }
            Pawn pawn = traverseParams.pawn ?? eater;
            int num = 0;
            int num2 = 0;
            Thing result = null;
            float num3 = float.MinValue;
            for (int i = 0; i < searchSet.Count; i++)
            {
                Thing thing = searchSet[i];
                num2++;
                float num4 = (float)(root - thing.Position).LengthManhattan;
                if (num4 <= maxDistance)
                {
                    float num5 = MizuUtility.WaterSourceOptimality(eater, thing, num4, false);
                    if (num5 >= num3)
                    {
                        if (pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams))
                        {
                            if (thing.Spawned)
                            {
                                if (validator == null || validator(thing))
                                {
                                    result = thing;
                                    num3 = num5;
                                    num++;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static float WaterSourceOptimality(Pawn eater, Thing t, float dist, bool takingToInventory = false)
        {
            float num = 300f;
            num -= dist;
            WaterPreferability preferability = t.GetWaterPreferability();
            if (preferability != WaterPreferability.Undefined)
            {
                return num;
            }
            return -9999999f;
        }

        public static float GetWater(Pawn getter, Thing thing, float waterWanted)
        {
            if (thing.Destroyed)
            {
                Log.Error(getter + " drank destroyed thing " + thing);
                return 0f;
            }
            if (!thing.CanDrinkWaterNow())
            {
                Log.Error(getter + " drank CanDrinkWaterNow()=false thing " + thing);
                return 0f;
            }
            // 食事による心情変化
            if (getter.needs.mood != null)
            {
                // 現在は何も変化しない
                List<ThoughtDef> list = MizuUtility.ThoughtsFromGettingWater(getter, thing);
                for (int j = 0; j < list.Count; j++)
                {
                    getter.needs.mood.thoughts.memories.TryGainMemory(list[j], null);
                }
            }
            int num;
            float result;
            thing.GetWaterCalculateAmounts(getter, waterWanted, out num, out result);
            if (num > 0)
            {
                if (num == thing.stackCount)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    thing.SplitOff(num);
                }
            }
            return result;
        }

        public static List<ThoughtDef> ThoughtsFromGettingWater(Pawn getter, Thing t)
        {
            MizuUtility.getWaterThoughts.Clear();
            return MizuUtility.getWaterThoughts;
        }

        public static int WillGetStackCountOf(Pawn getter, Thing thing)
        {
            CompWater comp = thing.TryGetComp<CompWater>();
            if (comp == null)
            {
                return 0;
            }

            int num = Math.Min(comp.MaxNumToGetAtOnce, MizuUtility.StackCountForWater(thing, getter.needs.water().WaterWanted));
            if (num < 1)
            {
                num = 1;
            }
            return num;
        }

        public static int StackCountForWater(Thing thing, float water)
        {
            CompWater comp = thing.TryGetComp<CompWater>();
            if (comp == null)
            {
                return 0;
            }
            if (water <= 0.0001f)
            {
                return 0;
            }
            return Math.Max((int)Math.Round(water / comp.WaterAmount), 1);
        }

        public static bool CanDrinkTerrain(Pawn pawn)
        {
            Need_Water need_water = pawn.needs.water();

            if (need_water == null)
            {
                // 水分要求なし = そもそも水を必要としていない
                return false;
            }
            else if (pawn.needs.mood == null)
            {
                // 心情無し = 地面から水をすすることに抵抗なし
                return true;
            }
            else if (need_water.CurCategory == ThirstCategory.Dehydration)
            {
                // 心情有り、水分要求あり、状態が脱水症状 = (心情悪化するけど)地形から水を摂取する
                return true;
            }

            // 心情あり、水分要求あり、状態はまだ大丈夫 = 地形から水を摂取しない
            return false;
        }
    }
}
