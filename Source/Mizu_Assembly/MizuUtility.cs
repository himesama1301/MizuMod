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
            // ドラッグ嫌いではない
            //    →ドラッグを許可
            bool allowDrug = !eater.IsTeetotaler();

            Thing inventoryThing = null;
            if (canUseInventory && getter.CanManipulate())
            {
                // 所持品から探すフラグON、取得者は操作が可能
                //   →所持品からベストな飲み物を探す(まともな水の範囲で)
                inventoryThing = MizuUtility.BestWaterInInventory(getter, WaterPreferability.NormalWater, WaterPreferability.ClearWater, 0f, allowDrug);
            }

            if (inventoryThing != null && getter.Faction != Faction.OfPlayer)
            {
                // 所持品から見つかり、取得者はプレイヤーではない
                //   →そのまま飲む
                // プレイヤーだった場合はマップ中の飲み物も探して、より適切なものを選ぶため保留
                waterSource = inventoryThing;
                waterDef = inventoryThing.def;
                return true;
            }

            // マップからベストな飲み物を探す
            Thing mapThing = MizuUtility.BestWaterSourceOnMap(getter, eater, WaterPreferability.ClearWater, allowForbidden, allowSociallyImproper);
            if (inventoryThing == null && mapThing == null)
            {
                // 所持品にまともな水なし、マップからいかなる水も見つけられない
                //   →ランクを落として所持品から探しなおす
                if (canUseInventory && getter.CanManipulate())
                {
                    inventoryThing = MizuUtility.BestWaterInInventory(getter, WaterPreferability.SeaWater, WaterPreferability.ClearWater, 0f, allowDrug);
                    if (inventoryThing != null)
                    {
                        // 良くない水が所持品から見つかった
                        waterSource = inventoryThing;
                        waterDef = inventoryThing.def;
                        return true;
                    }
                }

                // 所持品から探せる状態ではないor所持品にいかなる水もなし
                //   →見つからなかった
                waterSource = null;
                waterDef = null;
                return false;
            }

            if (inventoryThing == null && mapThing != null)
            {
                // 所持品にまともな水なし、マップから水が見つかった
                //   →マップの水を取得
                waterSource = mapThing;
                waterDef = mapThing.def;
                return true;
            }
            if (inventoryThing != null && mapThing == null)
            {
                // 所持品からまともな水が見つかった、マップからはいかなる水も見つけられない
                //   →所持品の水を取得
                waterSource = inventoryThing;
                waterDef = inventoryThing.def;
                return true;
            }

            // 所持品からまともな水が、マップからは何らかの水が見つかった
            //   →どちらが良いか評価(スコアが高い方が良い)
            float scoreMapThing = MizuUtility.GetWaterItemScore(eater, mapThing, (float)(getter.Position - mapThing.Position).LengthManhattan, false);
            float scoreInventoryThing = MizuUtility.GetWaterItemScore(eater, inventoryThing, 0f, false);
            scoreInventoryThing -= 32f;

            if (scoreMapThing > scoreInventoryThing)
            {
                // マップの水のほうが高スコア
                waterSource = mapThing;
                waterDef = mapThing.def;
                return true;
            }

            // 所持品の水のほうが高スコア
            waterSource = inventoryThing;
            waterDef = inventoryThing.def;
            return true;
        }

        public static Thing BestWaterInInventory(Pawn holder, WaterPreferability minWaterPref = WaterPreferability.SeaWater, WaterPreferability maxWaterPref = WaterPreferability.ClearWater, float minStackWaterAmount = 0.0f, bool allowDrug = false)
        {
            // 所持品から探すのに所持品オブジェクトなし
            if (holder == null || holder.inventory == null) return null;

            foreach (var thing in holder.inventory.innerContainer)
            {
                // 所持品をひとつずつチェック
                float waterAmount = thing.GetWaterAmount();
                WaterPreferability waterPreferability = thing.GetWaterPreferability();

                if (thing.CanGetWater() // 飲み物として飲めるもの
                    && thing.CanDrinkWaterNow() // 現在飲める状態にある
                    && waterPreferability >= minWaterPref && waterPreferability <= maxWaterPref // 品質が指定範囲内
                    && (allowDrug || !thing.def.IsDrug) // ドラッグ許可か、そもそもドラッグでない
                    && (waterAmount * thing.stackCount >= minStackWaterAmount)) // 水の量の最低値指定を満たしている
                {
                    return thing;
                }
            }

            // 条件に合うものが1個も見つからなかった
            return null;
        }

        public static Thing BestWaterSourceOnMap(Pawn getter, Pawn eater, WaterPreferability maxPref = WaterPreferability.ClearWater, bool allowForbidden = false, bool allowSociallyImproper = false)
        {
            if (!getter.CanManipulate() && getter != eater)
            {
                // 取得者は操作不可、取得者と摂取者が違う
                //   →マップから取得して持ち運ぶことができない
                //   →エラー
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

            Predicate<Thing> waterValidator = delegate (Thing t)
            {
                // 禁止されている＆禁止を無視して取得してはいけない
                if (!allowForbidden && t.IsForbidden(getter)) return false;

                // 水分を持っていない(摂取しても水分を得られない)
                if (!t.CanGetWater()) return false;

                float waterAmount = t.GetWaterAmount();
                WaterPreferability waterPreferability = t.GetWaterPreferability();

                // 水の品質が範囲外
                if (waterPreferability < WaterPreferability.SeaWater || waterPreferability > maxPref) return false;

                // 現在飲める状態には無い
                if (!t.CanDrinkWaterNow()) return false;

                // ？
                if (!MizuUtility.IsWaterSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)) return false;

                // ？
                if (!getter.AnimalAwareOf(t)) return false;

                // 取得者が予約できない
                if (!getter.CanReserve(t)) return false;

                return true;
            };

            Thing thing;
            if (getter.RaceProps.Humanlike)
            {
                thing = MizuUtility.SpawnedWaterSearchInnerScan(
                    eater,
                    getter.Position,
                    getter.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything).FindAll((t) => t.CanDrinkWaterNow()),
                    PathEndMode.ClosestTouch,
                    TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false),
                    9999f, waterValidator);
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
                    float num5 = MizuUtility.GetWaterItemScore(eater, thing, num4, false);
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

        public static float GetWaterItemScore(Pawn eater, Thing t, float dist, bool takingToInventory = false)
        {
            float score = 300f;  // 基本点

            // 距離が遠いと減点
            score -= dist;

            if (t.GetWaterPreferability() == WaterPreferability.Undefined)
            {
                // 水ではない、もしくは水だけど水の種類データが未設定
                //   →最低スコア
                return float.MinValue;
            }
            return score;
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

        public static ThingDef GetWaterThingDefFromTerrainType(WaterTerrainType waterTerrainType)
        {
            switch (waterTerrainType)
            {
                case WaterTerrainType.FreshWater:
                    return MizuDef.Thing_NormalWater;
                case WaterTerrainType.MudWater:
                    return MizuDef.Thing_MudWater;
                case WaterTerrainType.SeaWater:
                    return MizuDef.Thing_SeaWater;
                default:
                    return null;
            }
        }

        public static ThingDef GetWaterThingDefFromWaterType(WaterType waterType)
        {
            switch (waterType)
            {
                case WaterType.ClearWater:
                    return MizuDef.Thing_ClearWater;
                case WaterType.NormalWater:
                    return MizuDef.Thing_NormalWater;
                case WaterType.RainWater:
                    return MizuDef.Thing_RainWater;
                case WaterType.MudWater:
                    return MizuDef.Thing_MudWater;
                case WaterType.SeaWater:
                    return MizuDef.Thing_SeaWater;
                default:
                    return null;
            }
        }
    }
}
