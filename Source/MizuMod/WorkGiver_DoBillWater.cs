using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class WorkGiver_DoBillWater : WorkGiver_DoBill
    {
        private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);
        private static string MissingSkillTranslated = "MissingSkill".Translate();
        private static string MissingMaterialsTranslated = "MissingMaterials".Translate();
        private static string MissingWaterTranslated = "MizuMissingWater".Translate();
        private static string FullWaterTranslated = "MizuFullWater".Translate();

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            IBillGiver billGiver = thing as IBillGiver;
            if (billGiver == null) return null;
            if (this.def.fixedBillGiverDefs == null || !this.def.fixedBillGiverDefs.Contains(thing.def)) return null;

            if (!billGiver.CurrentlyUsableForBills()) return null;

            LocalTargetInfo target = thing;
            if (!pawn.CanReserve(target, 1, -1, null, forced)) return null;
            if (thing.IsBurning()) return null;
            if (thing.IsForbidden(pawn)) return null;

            billGiver.BillStack.RemoveIncompletableBills();
            return this.StartBillJob(pawn, billGiver);
        }

        private Job StartBillJob(Pawn pawn, IBillGiver giver)
        {
            foreach (var bill in giver.BillStack)
            {
                // レシピが要求する仕事の種類と、WorkGiver側の仕事の種類があっているかチェック
                if (bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != this.def.workType) continue;

                // 再チェック時間を過ぎていないかチェック(右クリックメニューからの場合は例外)
                if (Find.TickManager.TicksGame < bill.lastIngredientSearchFailTicks + ReCheckFailedBillTicksRange.RandomInRange && FloatMenuMakerMap.makingFor != pawn) continue;

                // チェック時間更新
                bill.lastIngredientSearchFailTicks = 0;

                // 今それをする必要があるか
                if (!bill.ShouldDoNow()) continue;

                // そのポーンが新規に仕事をできるか
                if (!bill.PawnAllowedToStartAnew(pawn)) continue;

                // レシピに必要なスキルを持っているか
                if (!bill.recipe.PawnSatisfiesSkillRequirements(pawn))
                {
                    JobFailReason.Is(MissingSkillTranslated);
                    continue;
                }

                // 材料はあるか
                var isFoundIngredients = TryFindBestBillIngredients(bill, pawn, (Thing)giver, this.chosenIngThings);

                // 消費する水はあるか
                var isFoundWater = IsFoundWater(giver, bill.recipe.GetModExtension<DefExtension_WaterRecipe>(), this.chosenIngThings);

                // 水を入れる余地が残っているか
                var isNotFullWater = IsNotFullWater(giver, bill.recipe.GetModExtension<DefExtension_WaterRecipe>(), this.chosenIngThings);

                if (isFoundIngredients && isFoundWater && isNotFullWater)
                {
                    return this.TryStartNewDoBillJob(pawn, bill, giver);
                }

                if (FloatMenuMakerMap.makingFor != pawn)
                {
                    // 右クリックメニューからでなく、ジョブを開始できなかったらチェック時間を更新
                    bill.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                }
                else
                {
                    // 右クリックメニューからの場合、できなかった理由を表示（素材不足）
                    if (!isFoundIngredients)
                    {
                        JobFailReason.Is(MissingMaterialsTranslated);
                    }
                    if (!isFoundWater)
                    {
                        JobFailReason.Is(MissingWaterTranslated);
                    }
                    if (!isNotFullWater)
                    {
                        JobFailReason.Is(FullWaterTranslated);
                    }
                }
            }
            return null;
        }

        private class DefCountList
        {
            private List<ThingDef> defs = new List<ThingDef>();

            private List<float> counts = new List<float>();

            public int Count
            {
                get
                {
                    return this.defs.Count;
                }
            }

            public float this[ThingDef def]
            {
                get
                {
                    int num = this.defs.IndexOf(def);
                    if (num < 0)
                    {
                        return 0f;
                    }
                    return this.counts[num];
                }
                set
                {
                    int num = this.defs.IndexOf(def);
                    if (num < 0)
                    {
                        this.defs.Add(def);
                        this.counts.Add(value);
                        num = this.defs.Count - 1;
                    }
                    else
                    {
                        this.counts[num] = value;
                    }
                    this.CheckRemove(num);
                }
            }

            public float GetCount(int index)
            {
                return this.counts[index];
            }

            public void SetCount(int index, float val)
            {
                this.counts[index] = val;
                this.CheckRemove(index);
            }

            public ThingDef GetDef(int index)
            {
                return this.defs[index];
            }

            private void CheckRemove(int index)
            {
                if (this.counts[index] == 0f)
                {
                    this.counts.RemoveAt(index);
                    this.defs.RemoveAt(index);
                }
            }

            public void Clear()
            {
                this.defs.Clear();
                this.counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                this.Clear();
                foreach (var t in things)
                {
                    this[t.def] = this[t.def] + (float)t.stackCount;
                }
            }
        }

        private List<ThingAmount> chosenIngThings = new List<ThingAmount>();
        private static List<Thing> newRelevantThings = new List<Thing>();
        private static List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();
        private static List<Thing> relevantThings = new List<Thing>();
        private static HashSet<Thing> processedThings = new HashSet<Thing>();
        private static DefCountList availableCounts = new DefCountList();

        private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingAmount> chosen)
        {
            // 最終結果クリア
            chosen.Clear();

            // 中間結果クリア
            newRelevantThings.Clear();

            // そもそも材料不要
            if (bill.recipe.ingredients.Count == 0) return true;

            // 作業位置
            var rootCell = GetBillGiverRootCell(billGiver, pawn);

            // リージョン取得
            Region rootReg = rootCell.GetRegion(pawn.Map, RegionType.Set_Passable);
            if (rootReg == null) return false;

            // 素材を順番に並べて取得(任意材料、固定材料、その他)
            MakeIngredientsListInProcessingOrder(ingredientsOrdered, bill);

            // 中間結果クリア
            relevantThings.Clear();
            processedThings.Clear();
            bool foundAll = false;

            // 材料の基本探索条件
            Predicate<Thing> baseValidator = (t) =>
            {
                return t.Spawned  // スポーン済み
                && !t.IsForbidden(pawn) // 禁止されていない
                && (float)(t.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius  // billごとの材料探索範囲以内
                && bill.IsFixedOrAllowedIngredient(t)  // billとして許可された材料である
                && bill.recipe.ingredients.Any((ingNeed) => ingNeed.filter.Allows(t))  // レシピとして許可された材料である
                && pawn.CanReserve(t);  // 予約可能
            };

            TraverseParms traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = (from, r) => r.Allows(traverseParams, false);
            int adjacentRegionsAvailable = rootReg.Neighbors.Count((region) => entryCondition(rootReg, region));
            int regionsProcessed = 0;

            // ???
            processedThings.AddRange(relevantThings);

            RegionProcessor regionProcessor = (r) =>
            {
                // 運搬可能な物ループ
                foreach (var thing in r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver)))
                {
                    // 既に含まれている物は無視
                    if (processedThings.Contains(thing)) continue;

                    // そのリージョンからその物にタッチしに行けるか
                    if (!ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn)) continue;

                    // 基本条件を満足しない場合はダメ
                    if (!baseValidator(thing)) continue;

                    // リストに加える
                    newRelevantThings.Add(thing);
                    processedThings.Add(thing);
                }
                regionsProcessed++;

                if (newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
                {
                    // 二つの物の距離を比べる
                    Comparison<Thing> comparison = (t1, t2) =>
                    {
                        float t1dist = (float)(t1.Position - rootCell).LengthHorizontalSquared;
                        float t2dist = (float)(t2.Position - rootCell).LengthHorizontalSquared;
                        return t1dist.CompareTo(t2dist);
                    };

                    // 距離の昇順?にソート
                    newRelevantThings.Sort(comparison);

                    // 探索した素材を追加
                    relevantThings.AddRange(newRelevantThings);

                    // 新しく探索した素材リストをクリア
                    newRelevantThings.Clear();

                    // 素材リストの中から最適な素材を見つける
                    if (TryFindBestBillIngredientsInSet(relevantThings, bill, chosen))
                    {
                        // 全部見つかった
                        foundAll = true;
                        return true;
                    }
                }

                // 全部は見つからなかった
                return false;
            };
            RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor);

            relevantThings.Clear();
            newRelevantThings.Clear();

            return foundAll;
        }

        private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
        {
            Building building = billGiver as Building;

            // 建造物でない
            if (building == null) return billGiver.Position;

            // 建造物で作業場所を持っている場合
            if (building.def.hasInteractionCell) return building.InteractionCell;

            // 建造物だけど作業場所が無い場合

            // 周囲8方向で立つことが出来るセルを探す
            var standableAdjacentCells = building.OccupiedRect().ExpandedBy(1).EdgeCells.Where((c) =>
            {
                foreach (var t in building.Map.thingGrid.ThingsListAt(c))
                {
                    // そのセルに、「立つことが出来る」以外の物がある⇒立てないセルはfalse
                    if (t.def.passability != Traversability.Standable) return false;
                }
                // 立てるセルの場合true
                return true;
            });

            // 1マスをランダムで選ぶ
            return standableAdjacentCells.RandomElement();
        }

        private static void MakeIngredientsListInProcessingOrder(List<IngredientCount> ingredientsOrdered, Bill bill)
        {
            // 並べ替え済み材料リストクリア
            ingredientsOrdered.Clear();

            // 任意材料があるなら最初に追加
            if (bill.recipe.productHasIngredientStuff)
            {
                ingredientsOrdered.Add(bill.recipe.ingredients[0]);
            }

            // 任意材料以外のもので固定材料であれば次に追加
            // (多分コンポーネント等の他で代用できないもの)
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                if (!bill.recipe.productHasIngredientStuff || i != 0)
                {
                    IngredientCount ingredientCount = bill.recipe.ingredients[i];
                    if (ingredientCount.IsFixedIngredient)
                    {
                        ingredientsOrdered.Add(ingredientCount);
                    }
                }
            }

            // その他の材料をすべて追加
            // (肉なら何でもいい、みたいな部分)
            for (int j = 0; j < bill.recipe.ingredients.Count; j++)
            {
                IngredientCount item = bill.recipe.ingredients[j];
                if (!ingredientsOrdered.Contains(item))
                {
                    ingredientsOrdered.Add(item);
                }
            }
        }

        private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            return TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen);
        }

        private static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingAmount> chosen)
        {
            RecipeDef recipe = bill.recipe;
            chosen.Clear();
            availableCounts.Clear();

            // 利用可能な材料リストとその個数
            // Things側はゲーム中に見えているThingのリスト
            // Counts側は、違うThingでも個数をまとめている
            availableCounts.GenerateFrom(availableThings);

            // レシピが必要としている材料の種類だけループ
            foreach (var ingredientCount in recipe.ingredients)
            { 
                bool isIngredientFound = false;
                for (int j = 0; j < availableCounts.Count; j++)
                {
                    // 利用可能な物
                    var curDef = availableCounts.GetDef(j);
                    var curCount = availableCounts.GetCount(j);

                    // レシピ完遂のためにそれが何個必要なのか
                    float requiredCount = (float)ingredientCount.CountRequiredOfFor(curDef, bill.recipe);
                    float remainRequiredCount = requiredCount;

                    // 利用可能な個数は必要数より少ない
                    if (requiredCount > curCount) continue;

                    // レシピとして許可されていない
                    if (!ingredientCount.filter.Allows(curDef)) continue;

                    // レシピの固定材料でもなければ billとして許可されているわけでもない
                    if (!ingredientCount.IsFixedIngredient && !bill.ingredientFilter.Allows(curDef)) continue;

                    // Thingの検索
                    foreach (var availableThing in availableThings)
                    {
                        // 探している物ではない
                        if (availableThing.def != curDef) continue;

                        // 未使用の材料数(全個数から、既に追加されている個数を引いた数)
                        int unusedCount = availableThing.stackCount - ThingAmount.CountUsed(chosen, availableThing);

                        // 未使用数0以下
                        if (unusedCount <= 0) continue;

                        // そのスタックからいくつ使うか(必要数と未使用数のうち小さい方)
                        int actualUseCount = Mathf.Min(Mathf.FloorToInt(remainRequiredCount), unusedCount);

                        // リストに加える
                        ThingAmount.AddToList(chosen, availableThing, actualUseCount);

                        // 残りの必要個数を減らす
                        remainRequiredCount -= (float)actualUseCount;

                        // まだ必要であれば続けて探す
                        if (remainRequiredCount >= 0.001f) continue;

                        // その材料は全部見つかった
                        //   ⇒残りの利用可能数を減らす
                        isIngredientFound = true;
                        availableCounts.SetCount(j, curCount - requiredCount);
                        break;
                    }

                    // その材料は全部見つかったので残りは探さなくていい
                    if (isIngredientFound) break;
                }

                // 1個でも見つからない材料があればダメ
                if (!isIngredientFound) return false;
            }

            // 全部見つかったのでOK
            return true;
        }

        private Job TryStartNewDoBillJob(Pawn pawn, Bill bill, IBillGiver giver)
        {
            // 材料の運搬先に物があった場合(邪魔なものがある場合)、それをどかす処理
            var job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
            if (job != null) return job;

            // レシピ実行(本命)
            var job2 = CreateNewJob(bill.recipe.GetModExtension<DefExtension_WaterRecipe>());
            if (job2 == null) return null;

            job2.targetA = giver as Thing;
            job2.targetQueueB = new List<LocalTargetInfo>(this.chosenIngThings.Count);
            job2.countQueue = new List<int>(this.chosenIngThings.Count);
            for (int i = 0; i < this.chosenIngThings.Count; i++)
            {
                job2.targetQueueB.Add(this.chosenIngThings[i].thing);
                job2.countQueue.Add(this.chosenIngThings[i].count);
            }
            job2.targetC = GetBillGiverRootCell(giver as Thing, pawn);
            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.bill = bill;
            return job2;
        }

        protected bool IsFoundWater(IBillGiver giver, DefExtension_WaterRecipe ext, List<ThingAmount> chosen)
        {
            if (ext == null) return true;

            if (giver == null) return false;

            var thing = giver as Thing;
            if (thing == null) return false;

            switch (ext.recipeType)
            {
                case DefExtension_WaterRecipe.RecipeType.DrawFromTerrain:
                    {
                        // 水質チェック
                        return ext.needWaterTerrainTypes != null && ext.needWaterTerrainTypes.Contains(thing.Map.terrainGrid.TerrainAt(thing.Position).GetWaterTerrainType());
                    }
                case DefExtension_WaterRecipe.RecipeType.DrawFromWaterPool:
                    {
                        var waterGrid = thing.Map.GetComponent<MapComponent_ShallowWaterGrid>();
                        var pool = waterGrid.GetPool(thing.Map.cellIndices.CellToIndex(thing.Position));

                        // 水質条件チェック
                        if (!ext.needWaterTypes.Contains(pool.WaterType)) return false;

                        // 入力水道網の水の種類から水アイテムの種類を決定
                        var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(pool.WaterType);
                        if (waterThingDef == null) return false;

                        // 水アイテムの水源情報を得る
                        var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
                        if (compprop == null) return false;

                        // 水量チェック
                        if (pool.CurrentWaterVolume < compprop.waterVolume * ext.getItemCount) return false;

                        return true;
                    }
                case DefExtension_WaterRecipe.RecipeType.DrawFromWaterNet:
                    {
                        var workTable = giver as Building_WaterNetWorkTable;
                        if (workTable == null || workTable.InputWaterNet == null) return false;

                        var targetWaterType = WaterType.NoWater;
                        var targetWaterVolume = 0.0f;

                        if (ext.canDrawFromFaucet)
                        {
                            // 蛇口から汲むレシピ
                            targetWaterType = workTable.InputWaterNet.StoredWaterTypeForFaucet;
                            targetWaterVolume = workTable.InputWaterNet.StoredWaterVolumeForFaucet;
                        }
                        else
                        {
                            // 自身から汲むレシピ(水箱など)
                            targetWaterType = workTable.TankComp.StoredWaterType;
                            targetWaterVolume = workTable.TankComp.StoredWaterVolume;
                        }

                        // 水質チェック
                        if (!ext.needWaterTypes.Contains(targetWaterType)) return false;

                        // 入力水道網の水の種類から水アイテムの種類を決定
                        var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(targetWaterType);
                        if (waterThingDef == null) return false;

                        // 水アイテムの水源情報を得る
                        var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
                        if (compprop == null) return false;

                        // 水量チェック
                        if (targetWaterVolume < compprop.waterVolume * ext.getItemCount) return false;

                        return true;
                    }
                case DefExtension_WaterRecipe.RecipeType.PourWater:
                    return true;
                default:
                    Log.Error("recipeType is Undefined");
                    return false;
            }
        }

        protected bool IsNotFullWater(IBillGiver giver, DefExtension_WaterRecipe ext, List<ThingAmount> chosen)
        {
            if (ext == null) return true;

            if (giver == null) return false;

            var thing = giver as Thing;
            if (thing == null) return false;

            switch (ext.recipeType)
            {
                case DefExtension_WaterRecipe.RecipeType.DrawFromTerrain:
                    return true;
                case DefExtension_WaterRecipe.RecipeType.DrawFromWaterPool:
                    return true;
                case DefExtension_WaterRecipe.RecipeType.DrawFromWaterNet:
                    return true;
                case DefExtension_WaterRecipe.RecipeType.PourWater:
                    {
                        var building = thing as Building_WaterNetWorkTable;
                        if (building == null) return false;

                        var totalWaterVolume = 0f;
                        foreach (var ta in chosen)
                        {
                            var sourceComp = ta.thing.TryGetComp<CompWaterSource>();
                            if (sourceComp != null)
                            {
                                totalWaterVolume += sourceComp.WaterVolume * ta.count;
                            }
                        }
                        if (GetTotalAmountCanAccept(building) < totalWaterVolume) return false;

                        return true;
                    }
                default:
                    Log.Error("recipeType is Undefined");
                    return false;
            }
        }

        protected static float GetTotalAmountCanAccept(Building_WaterNetWorkTable workTable)
        {
            if (workTable.TankComp == null) return 0f;

            float totalAmountCanAccept = workTable.TankComp.AmountCanAccept;
            if (workTable.InputWaterNet != null && workTable.InputWaterNet.FlatTankList != null && workTable.InputWaterNet.FlatTankList.Count() > 0)
            {
                var flatTanks = workTable.InputWaterNet.FlatTankList.First((flatTankElement) => flatTankElement.Contains(workTable));
                if (flatTanks != null)
                {
                    totalAmountCanAccept = 0f;
                    foreach (var tank in flatTanks)
                    {
                        totalAmountCanAccept += tank.TankComp.AmountCanAccept;
                    }
                }
            }
            return totalAmountCanAccept;
        }

        protected static Job CreateNewJob(DefExtension_WaterRecipe ext)
        {
            if (ext == null) return new Job(JobDefOf.DoBill);

            switch (ext.recipeType)
            {
                case DefExtension_WaterRecipe.RecipeType.DrawFromTerrain:
                    return new Job(MizuDef.Job_DrawFromTerrain);
                case DefExtension_WaterRecipe.RecipeType.DrawFromWaterPool:
                    return new Job(MizuDef.Job_DrawFromWaterPool);
                case DefExtension_WaterRecipe.RecipeType.DrawFromWaterNet:
                    return new Job(MizuDef.Job_DrawFromWaterNet);
                case DefExtension_WaterRecipe.RecipeType.PourWater:
                    return new Job(MizuDef.Job_PourWater);
                default:
                    Log.Error("recipeType is Undefined");
                    return null;
            }
        }
    }
}
