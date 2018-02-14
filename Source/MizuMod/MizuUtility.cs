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
        private static List<ThoughtDef> thoughtList = new List<ThoughtDef>();

        public const float SearchWaterRadiusForWildAnimal = 30f;

        public static Thing TryFindBestWaterSourceFor(Pawn getter, Pawn eater, bool priorQuality, bool allowBuilding, bool canUseInventory = true, bool allowForbidden = false, bool allowSociallyImproper = false)
        {
            // ドラッグ嫌いではない
            //    →ドラッグを許可
            bool allowDrug = !eater.IsTeetotaler();

            Thing inventoryThing = null;
            if (canUseInventory && getter.CanManipulate())
            {
                // 所持品から探すフラグON、取得者は操作が可能
                //   →所持品からベストな飲み物を探す
                inventoryThing = MizuUtility.BestWaterInInventory(getter, WaterPreferability.SeaWater, WaterPreferability.ClearWater, 0f, allowDrug);
            }

            if (inventoryThing != null)
            {
                // 所持品から見つかり、取得者はプレイヤーではない
                //   →そのまま飲む
                if (getter.Faction != Faction.OfPlayer) return inventoryThing;

                // プレイヤーだった場合
                //   →腐りかけならそのまま飲む
                if (inventoryThing.IsRotSoonForWater()) return inventoryThing;
            }

            // プレイヤー＆所持品の水は新鮮
            //   →マップからも探す
            Thing mapThing = MizuUtility.BestWaterSourceOnMap(getter, eater, priorQuality, allowBuilding, WaterPreferability.ClearWater, allowDrug, allowForbidden, allowSociallyImproper);

            if (eater.RaceProps.Animal && eater.Faction != Faction.OfPlayer)
            {
                // 野生の動物の場合、探したものが一定の距離以上であれば選択肢から除外
                // １個しかない水飲み場に全動物が集まるのを防ぐ
                if (mapThing != null && (eater.Position - mapThing.Position).LengthManhattan >= MizuUtility.SearchWaterRadiusForWildAnimal)
                {
                    mapThing = null;
                }
            }

            if (inventoryThing == null && mapThing == null)
            {
                // 所持品にまともな水なし、マップからいかなる水も見つけられない
                //   →ランクを落として所持品から探しなおす
                if (canUseInventory && getter.CanManipulate())
                {
                    // 見つかっても見つからなくてもその結果を返す
                    return MizuUtility.BestWaterInInventory(getter, WaterPreferability.SeaWater, WaterPreferability.ClearWater, 0f, allowDrug);
                }

                // 所持品から探せる状態ではない
                return null;
            }

            // 所持品にまともな水なし、マップから水が見つかった
            //   →マップの水を取得
            if (inventoryThing == null && mapThing != null) return mapThing;

            // 所持品からまともな水が見つかった、マップからはいかなる水も見つけられない
            //   →所持品の水を取得
            if (inventoryThing != null && mapThing == null) return inventoryThing;

            // 所持品からまともな水が、マップからは何らかの水が見つかった
            //   →どちらが良いか評価(スコアが高い方が良い)
            float scoreMapThing = MizuUtility.GetWaterItemScore(eater, mapThing, (float)(getter.Position - mapThing.Position).LengthManhattan, priorQuality);
            float scoreInventoryThing = MizuUtility.GetWaterItemScore(eater, inventoryThing, 0f, priorQuality);

            // 所持品のほうを優先しやすくする
            scoreInventoryThing += 30f;

            // マップの水のほうが高スコア
            if (scoreMapThing > scoreInventoryThing) return mapThing;

            // 所持品の水のほうが高スコア
            return inventoryThing;
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
                    && !thing.def.IsIngestible // 食べ物ではない
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

        public static Thing BestWaterSourceOnMap(Pawn getter, Pawn eater, bool priorQuality, bool allowBuilding, WaterPreferability maxPref = WaterPreferability.ClearWater, bool allowDrug = false, bool allowForbidden = false, bool allowSociallyImproper = false)
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

            Predicate<Thing> waterValidator = (t) =>
            {
                // 禁止されている＆禁止を無視して取得してはいけない
                if (!allowForbidden && t.IsForbidden(getter)) return false;

                // ドラッグ禁止＆対象はドラッグ
                if (!allowDrug && t.def.IsDrug) return false;

                // 取得者が予約できない
                if (!getter.CanReserve(t)) return false;

                var comp = t.TryGetComp<CompWaterSource>();

                // 水源として使用できない
                if (comp == null || !comp.IsWaterSource) return false;

                // 食べられるものは飲み物としては選ばれない
                if (t.def.IsIngestible) return false;

                // 操作が必要なのに操作できない
                if (comp.NeedManipulate && !getter.CanManipulate()) return false;

                var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

                if (comp.SourceType == CompProperties_WaterSource.SourceType.Item)
                {
                    // 水分がない
                    if (!t.CanGetWater()) return false;

                    // 水分を持っている=水アイテムである
                    WaterPreferability waterPreferability = t.GetWaterPreferability();

                    // 水の品質が範囲外
                    if (waterPreferability < WaterPreferability.SeaWater || waterPreferability > maxPref) return false;

                    // 現在飲める状態には無い
                    if (!t.CanDrinkWaterNow()) return false;

                    // 入植者は囚人部屋のアイテムを扱えないことがあるが、そのことに関するチェックでダメならfalse
                    if (!MizuUtility.IsWaterSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)) return false;

                    // 取得者がそれに気づいていない
                    if (!getter.AnimalAwareOf(t)) return false;

                    return true;
                }
                else if (comp.SourceType == CompProperties_WaterSource.SourceType.Building)
                {
                    // 取得者と摂取者が異なる(自分で飲みに行く必要がある)
                    if (getter != eater) return false;

                    var drinkWaterBuilding = t as IBuilding_DrinkWater;

                    // 水汲みに使えない
                    if (drinkWaterBuilding == null) return false;

                    // 水を飲む人が飲めない(能力が無い、水の量がない)
                    if (!drinkWaterBuilding.CanDrinkFor(eater)) return false;

                    // 最大水質を超えていたらダメ
                    if (waterTypeDef.waterPreferability > maxPref) return false;

                    // 野生人?(派閥所属なし?)はダメ
                    if (eater.IsWildMan()) return false;

                    // 自陣営or自陣営のホストの設備でなければダメ
                    // 動物でない場合は、という条件を追加
                    if (!eater.RaceProps.Animal && t.Faction != eater.Faction && t.Faction != eater.HostFaction) return false;

                    // 使えない状態はダメ
                    if (!drinkWaterBuilding.IsActivated) return false;

                    // 入植者は囚人部屋のアイテムを扱えないことがあるが、そのことに関するチェックでダメならfalse
                    if (!MizuUtility.IsWaterSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)) return false;

                    if (t.def.hasInteractionCell == true)
                    {
                        // 使用場所がある
                        if (!t.InteractionCell.Standable(t.Map) || !eater.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                        {
                            // 使用場所に立てない or 使用場所まで行けない
                            return false;
                        }
                    }
                    else
                    {
                        // 使用場所が無い
                        if (!getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.Position, t.Map, false), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false)))
                        {
                            // その設備にタッチできない
                            return false;
                        }
                    }

                    return true;
                }

                // それ以外
                return false;

            };

            if (getter.RaceProps.Humanlike)
            {
                // 取得者はHumanlikeである
                //   →条件を満たすものの中から最適な物を探す
                return MizuUtility.SpawnedWaterSearchInnerScan(
                    eater,
                    getter.Position,
                    getter.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything).FindAll((t) => {
                        if (t.CanDrinkWaterNow()) return true;

                        var building = t as IBuilding_DrinkWater;
                        if (building != null && building.CanDrinkFor(eater)) return true;

                        return false;
                    }),
                    PathEndMode.ClosestTouch,
                    TraverseParms.For(getter),
                    priorQuality,
                    9999f,
                    waterValidator);
            }

            // 取得者はHumanlikeではない

            // プレイヤー派閥に所属しているかどうかでリージョン?数を変える
            int searchRegionsMax = 30;
            if (getter.Faction == Faction.OfPlayer)
            {
                searchRegionsMax = 100;
            }

            HashSet<Thing> filtered = new HashSet<Thing>();
            foreach (Thing current in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, true))
            {
                // 自分を中心に半径2以内の物をチェック

                Pawn pawn = current as Pawn;
                if (pawn != null && pawn != getter && pawn.RaceProps.Animal && pawn.CurJob != null && pawn.CurJob.def == MizuDef.Job_DrinkWater && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
                {
                    // 自分ではない動物が現在水アイテムを摂取している
                    //   →今まさに摂取している物は探索対象から除外
                    filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
                }
            }

            bool ignoreEntirelyForbiddenRegions = !allowForbidden  // 禁止物のアクセスは許可されていない
                && ForbidUtility.CaresAboutForbidden(getter, true) // 禁止設定を守ろうとする
                && (getter.playerSettings != null && getter.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap != null); // 有効な制限エリアなし

            Predicate<Thing> predicate = (t) =>
            {
                // アイテムが条件を満たしていない
                if (!waterValidator(t)) return false;

                // すぐ近くで他の動物が飲んでいる水のリストに入っていない
                if (filtered.Contains(t)) return false;

                // 水の品質が最低値未満
                if (t.GetWaterPreferability() < WaterPreferability.SeaWater) return false;

                return true;
            };

            // 指定の条件下でアクセスできるものを探す
            Thing thing = null;

            // 水アイテムから
            thing = GenClosest.ClosestThingReachable(
                getter.Position,
                getter.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.ClosestTouch,
                TraverseParms.For(getter),
                9999f,
                predicate,
                null,
                0,
                searchRegionsMax,
                false,
                RegionType.Set_Passable,
                ignoreEntirelyForbiddenRegions);
            if (thing != null) return thing;

            // 水汲み設備
            thing = GenClosest.ClosestThingReachable(
                getter.Position,
                getter.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.ClosestTouch,
                TraverseParms.For(getter),
                9999f,
                predicate,
                null,
                0,
                searchRegionsMax,
                false,
                RegionType.Set_Passable,
                ignoreEntirelyForbiddenRegions);
            if (thing != null) return thing;

            // 条件を変えて再探索
            // 水アイテム
            thing = GenClosest.ClosestThingReachable(
                getter.Position,
                getter.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.ClosestTouch,
                TraverseParms.For(getter),
                9999f,
                waterValidator,  // ここが変わった
                null,
                0,
                searchRegionsMax,
                false,
                RegionType.Set_Passable,
                ignoreEntirelyForbiddenRegions);
            if (thing != null) return thing;

            // 水汲み設備
            thing = GenClosest.ClosestThingReachable(
                getter.Position,
                getter.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.ClosestTouch,
                TraverseParms.For(getter),
                9999f,
                waterValidator,  // ここが変わった
                null,
                0,
                searchRegionsMax,
                false,
                RegionType.Set_Passable,
                ignoreEntirelyForbiddenRegions);
            if (thing != null) return thing;

            return null;
        }

        private static bool IsWaterSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater, bool allowSociallyImproper)
        {
            // 囚人部屋にあっても強引に使用して良い
            if (allowSociallyImproper) return true;

            // 適切な場所にある
            if (t.IsSociallyProper(getter) || t.IsSociallyProper(eater, eater.IsPrisonerOfColony, !getter.RaceProps.Animal)) return true;

            return false;
        }

        private static Thing SpawnedWaterSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, bool priorQuality, float maxDistance = 9999f, Predicate<Thing> validator = null)
        {
            // 探索対象リストなし
            if (searchSet == null) return null;

            // 対象のポーンを決める(取得者優先、次点で摂取者)
            Pawn pawn = traverseParams.pawn ?? eater;

            Thing result = null;
            float maxScore = float.MinValue;

            foreach (var thing in searchSet)
            {
                // アイテムとの距離が限界以上離れていたらダメ
                float lengthManhattan = (float)(root - thing.Position).LengthManhattan;
                if (lengthManhattan > maxDistance) continue;

                // 現時点での候補アイテムのスコア(摂取者にとって)を超えていないならダメ
                float thingScore = MizuUtility.GetWaterItemScore(eater, thing, lengthManhattan, priorQuality);
                if (thingScore < maxScore) continue;

                // ポーンがそこまでたどり着けなければだめ
                if (!pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams)) continue;

                // まだ出現していない場合はダメ
                if (!thing.Spawned) continue;

                // アイテムが指定の条件を満たしていないならダメ
                if (validator != null && !validator(thing)) continue;

                // すべての条件を満足
                result = thing;
                maxScore = thingScore;
            }
            return result;
        }

        public static float GetWaterItemScore(Pawn eater, Thing t, float dist, bool priorQuality)
        {
            var comp = t.TryGetComp<CompWaterSource>();

            // 水源ではない or 水源として使えない
            if (comp == null || !comp.IsWaterSource) return float.MinValue;

            // 食べられるものは飲み物としては選ばない方針
            if (t.def.IsIngestible) return float.MinValue;

            // 水アイテムなのに水分量が少ない(食事におまけで付いてる水分など)
            //   1個あたりが少なくても、一度に摂取できる量が多い場合は水分摂取アイテムとして有効
            if (comp.SourceType == CompProperties_WaterSource.SourceType.Item && comp.WaterAmount * comp.MaxNumToGetAtOnce < Need_Water.MinWaterAmountPerOneDrink) return float.MinValue;

            var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

            // 基本点計算

            // 距離
            float distScore = -dist;

            // 心情変化量(水質)
            // メモ
            //   きれい= +10
            //   普通  =   0
            //   生水  =   0
            //   泥水  =  -6
            //   海水  =  -6
            float thoughtScore = 0f;

            // 禁欲の影響も含まれている
            foreach (var thought in MizuUtility.ThoughtsFromGettingWater(eater, t))
            {
                thoughtScore += thought.stages[0].baseMoodEffect;
            }

            // 食中毒
            // メモ
            //   きれい= 0    =>   0
            //   普通  = 0    =>   0
            //   生水  = 0.01 => -10
            //   泥水  = 0.03 => -30
            //   海水  = 0.03 => -30
            float foodPoisoningScore = -(waterTypeDef.foodPoisonChance * 1000f);

            // 健康悪化
            float hediffScore = 0f;
            if (waterTypeDef.hediffs != null)
            {
                foreach (var hediff in waterTypeDef.hediffs)
                {
                    hediffScore -= 100f;
                }
            }

            // 腐敗進行度
            float rotScore = 0f;
            if (t.IsRotSoonForWater()) rotScore += 10f;

            // 基本点合計メモ
            //          心情,食中毒,健康,合計(禁欲)
            //   きれい= +10,     0,   0, +10(   0)
            //   普通  =   0,     0,   0,   0(   0)
            //   生水  =   0,   -10,   0, -10( -10)
            //   泥水  =  -6,   -30,   0, -36( -30)
            //   海水  =  -6,   -30,-100,-136(-130)

            // 各種状態によるスコアの変化

            // 水質優先モードか否か
            if (priorQuality) distScore /= 10f;

            return (distScore + thoughtScore + foodPoisoningScore + rotScore);
        }

        public static float GetWaterTerrainScore(Pawn eater, IntVec3 c, float dist, bool priorQuality)
        {
            TerrainDef terrain = c.GetTerrain(eater.Map);

            // 水源ではない or 水源として使えない
            if (!terrain.IsWater()) return float.MinValue;

            var waterTypeDef = MizuDef.Dic_WaterTypeDef[terrain.ToWaterType()];

            // 基本点計算

            // 距離
            float distScore = -dist;

            // 心情変化量(水質)
            // メモ
            //   きれい= +10
            //   普通  =   0
            //   生水  =   0
            //   泥水  =  -6
            //   海水  =  -6
            float thoughtScore = 0f;

            // 禁欲の影響も含まれている
            List<ThoughtDef> thoughtList = new List<ThoughtDef>();
            MizuUtility.ThoughtsFromWaterTypeDef(eater, waterTypeDef, true, thoughtList);
            foreach (var thought in thoughtList)
            {
                thoughtScore += thought.stages[0].baseMoodEffect;
            }

            // 食中毒
            // メモ
            //   きれい= 0    =>   0
            //   普通  = 0    =>   0
            //   生水  = 0.01 => -10
            //   泥水  = 0.03 => -30
            //   海水  = 0.03 => -30
            float foodPoisoningScore = -(waterTypeDef.foodPoisonChance * 1000f);

            // 健康悪化
            float hediffScore = 0f;
            if (waterTypeDef.hediffs != null)
            {
                foreach (var hediff in waterTypeDef.hediffs)
                {
                    hediffScore -= 100f;
                }
            }

            // 基本点合計メモ
            //          心情,食中毒,健康,合計(禁欲)
            //   きれい= +10,     0,   0, +10(   0)
            //   普通  =   0,     0,   0,   0(   0)
            //   生水  =   0,   -10,   0, -10( -10)
            //   泥水  =  -6,   -30,   0, -36( -30)
            //   海水  =  -6,   -30,-100,-136(-130)

            // 各種状態によるスコアの変化

            // 水質優先モードか否か
            if (priorQuality) distScore /= 10f;

            return (distScore + thoughtScore + foodPoisoningScore);
        }

        public static float GetWater(Pawn getter, Thing thing, float waterWanted, bool withIngested)
        {
            // 摂取しようとしているものが既に消滅している(エラー)
            // 食事と同時に水分摂取する場合は既に消滅しているので無視する
            if (!withIngested && thing.Destroyed)
            {
                Log.Error(getter + " drank destroyed thing " + thing);
                return 0f;
            }

            // 現在飲めないはずのものを飲もうとしている(エラー)
            if (!thing.CanDrinkWaterNow())
            {
                Log.Error(getter + " drank CanDrinkWaterNow()=false thing " + thing);
                return 0f;
            }

            if (getter.needs.mood != null)
            {
                // 水分摂取による心情変化
                foreach (var thoughtDef in MizuUtility.ThoughtsFromGettingWater(getter, thing))
                {
                    getter.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
                }
            }

            // 健康状態の変化
            var comp = thing.TryGetComp<CompWaterSource>();
            if (comp == null)
            {
                Log.Error("comp is null");
                return 0.0f;
            }
            if (!comp.IsWaterSource)
            {
                Log.Error("not watersource");
                return 0.0f;
            }
            if (comp.SourceType != CompProperties_WaterSource.SourceType.Item)
            {
                Log.Error("source type is not item");
                return 0.0f;
            }

            var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

            // 指定された健康状態になる
            if (waterTypeDef.hediffs != null)
            {
                foreach (var hediff in waterTypeDef.hediffs)
                {
                    getter.health.AddHediff(HediffMaker.MakeHediff(hediff, getter));
                }
            }
            // 確率で食中毒
            float animalFactor = getter.RaceProps.Humanlike ? 1f : 0.1f; // 動物は1/10に抑える
            if (Rand.Value < waterTypeDef.foodPoisonChance * animalFactor)
            {
                FoodUtility.AddFoodPoisoningHediff(getter, thing);
            }

            int drankWaterItemCount;
            float gotWaterAmount;

            // 摂取個数と摂取水分量の計算
            thing.GetWaterCalculateAmounts(getter, waterWanted, withIngested, out drankWaterItemCount, out gotWaterAmount);

            if (withIngested)
            {
                // 食事の場合は後で個数を計算するのでここでは1個にする
                gotWaterAmount = comp.WaterAmount;
                drankWaterItemCount = 1;
            }

            // 食事と同時に水分摂取する場合は既に消滅しているので消滅処理をスキップする
            if (!withIngested && drankWaterItemCount > 0)
            {
                if (drankWaterItemCount == thing.stackCount)
                {
                    // アイテム消費数とスタック数が同じ
                    //   →完全消滅
                    thing.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    // スタック数と異なる
                    //   →消費した数だけ減らす
                    thing.SplitOff(drankWaterItemCount);
                }
            }
            return gotWaterAmount;
        }

        public static void PrePostIngested(Pawn ingester, Thing t, int num)
        {
            Need_Water need_water = ingester.needs.water();
            if (need_water == null) return;

            var comp = t.TryGetComp<CompWaterSource>();
            if (comp == null) return;

            // 食事のついでの水分摂取の場合、帰ってくる水分量は常に1個分
            float gotWaterAmount = MizuUtility.GetWater(ingester, t, need_water.WaterWanted, true);

            // 後で個数を掛け算する
            gotWaterAmount *= num;

            if (!ingester.Dead)
            {
                need_water.CurLevel += gotWaterAmount;
            }
            ingester.records.AddTo(MizuDef.Record_WaterDrank, gotWaterAmount);
        }

        public static List<ThoughtDef> ThoughtsFromGettingWater(Pawn getter, Thing t)
        {
            // 空のリスト
            thoughtList.Clear();

            // 心情ステータスの無いポーンには空のリストを返す
            if (getter.needs == null || getter.needs.mood == null) return thoughtList;

            var comp = t.TryGetComp<CompWaterSource>();
            if (comp == null) return thoughtList;
            if (!comp.IsWaterSource) return thoughtList;

            var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

            bool isDirect = comp.SourceType == CompProperties_WaterSource.SourceType.Building;
            ThoughtsFromWaterTypeDef(getter, waterTypeDef, isDirect, thoughtList);

            return thoughtList;
        }

        public static void ThoughtsFromWaterTypeDef(Pawn getter, WaterTypeDef waterTypeDef, bool isDirect, List<ThoughtDef> thoughtList)
        {
            // 禁欲主義は心情の変化を無視する
            if (getter.story != null && getter.story.traits != null && getter.story.traits.HasTrait(TraitDefOf.Ascetic)) return;

            // 水ごとの飲んだ時の心情が設定されていたら、それを与える
            if (waterTypeDef.thoughts != null) thoughtList.AddRange(waterTypeDef.thoughts);

            // 飲み方による心情の変化
            if (isDirect)
            {
                if (getter.CanManipulate())
                {
                    // 手ですくって飲む
                    thoughtList.Add(MizuDef.Thought_DrankScoopedWater);
                }
                else
                {
                    // 直接口をつけて飲む
                    thoughtList.Add(MizuDef.Thought_SippedWaterLikeBeast);
                }
            }
        }

        public static int WillGetStackCountOf(Pawn getter, Thing thing)
        {
            var comp = thing.TryGetComp<CompWaterSource>();

            // 水源ではない→摂取数0
            if (comp == null || !comp.IsWaterSource) return 0;

            // アイテムではない→摂取数0
            if (comp.SourceType != CompProperties_WaterSource.SourceType.Item) return 0;

            // それを一度に摂取できる数と、何個摂取すれば水分が100%になるのか、の小さい方
            int wantedWaterItemCount = Math.Min(thing.TryGetComp<CompWaterSource>().MaxNumToGetAtOnce, MizuUtility.StackCountForWater(thing, getter.needs.water().WaterWanted));

            // 1個未満なら1個にする
            if (wantedWaterItemCount < 1) return 1;

            return wantedWaterItemCount;
        }

        public static int StackCountForWater(Thing thing, float waterWanted)
        {
            var comp = thing.TryGetComp<CompWaterSource>();

            // 水源ではない
            if (comp == null || !comp.IsWaterSource) return 0;

            // 必要な水分がほぼゼロ
            if (waterWanted <= 0.0001f) return 0;

            // それを何個摂取すれば水分が十分になるのかを返す(最低値1)
            return Math.Max((int)Math.Round(waterWanted / comp.WaterAmount), 1);
        }

        public static ThingDef GetWaterThingDefFromTerrainType(WaterTerrainType waterTerrainType)
        {
            // 地形タイプ→水アイテム
            switch (waterTerrainType)
            {
                case WaterTerrainType.RawWater:
                    return MizuDef.Thing_RawWater;
                case WaterTerrainType.MudWater:
                    return MizuDef.Thing_MudWater;
                case WaterTerrainType.SeaWater:
                    return MizuDef.Thing_SeaWater;
                default:
                    return null;
            }
        }

        public static ThoughtDef GetThoughtDefFromTerrainType(WaterTerrainType waterTerrainType)
        {
            // 地形タイプ→心情
            switch (waterTerrainType)
            {
                case WaterTerrainType.MudWater:
                    return MizuDef.Thought_DrankMudWater;
                case WaterTerrainType.SeaWater:
                    return MizuDef.Thought_DrankSeaWater;
                default:
                    return null;
            }
        }

        public static ThingDef GetWaterThingDefFromWaterType(WaterType waterType)
        {
            // 水の種類→水アイテム
            switch (waterType)
            {
                case WaterType.ClearWater:
                    return MizuDef.Thing_ClearWater;
                case WaterType.NormalWater:
                    return MizuDef.Thing_NormalWater;
                case WaterType.RawWater:
                    return MizuDef.Thing_RawWater;
                case WaterType.MudWater:
                    return MizuDef.Thing_MudWater;
                case WaterType.SeaWater:
                    return MizuDef.Thing_SeaWater;
                default:
                    return null;
            }
        }

        public static ThingDef GetWaterThingDefFromWaterPreferability(WaterPreferability waterPreferability)
        {
            // 水の種類→水アイテム
            switch (waterPreferability)
            {
                case WaterPreferability.ClearWater:
                    return MizuDef.Thing_ClearWater;
                case WaterPreferability.NormalWater:
                    return MizuDef.Thing_NormalWater;
                case WaterPreferability.RawWater:
                    return MizuDef.Thing_RawWater;
                case WaterPreferability.MudWater:
                    return MizuDef.Thing_MudWater;
                case WaterPreferability.SeaWater:
                    return MizuDef.Thing_SeaWater;
                default:
                    return null;
            }
        }

        public static bool TryFindHiddenWaterSpot(Pawn pawn, out IntVec3 result)
        {
            var hiddenWaterSpot = pawn.Map.GetComponent<MapComponent_HiddenWaterSpot>();
            if (hiddenWaterSpot == null)
            {
                Log.Error("hiddenWaterSpot is null");
                result = IntVec3.Invalid;
                return false;
            }

            bool isFound = false;
            float maxScore = float.MinValue;
            result = IntVec3.Invalid;
            foreach (var c in hiddenWaterSpot.SpotCells)
            {
                float curDist = (pawn.Position - c).LengthManhattan;
                if (pawn.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    float curScore = MizuUtility.GetWaterTerrainScore(pawn, c, curDist, false);
                    if (maxScore < curScore)
                    {
                        isFound = true;
                        maxScore = curScore;
                        result = c;
                    }
                }
            }

            return isFound;
        }
    }
}
