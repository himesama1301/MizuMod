using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobGiver_GetWater : ThinkNode_JobGiver
    {
        //private const int MaxDistanceOfSearchWaterTerrain = 300;
        private const int SearchWaterIntervalTick = 180;

        public override float GetPriority(Pawn pawn)
        {
            Need_Water need_water = pawn.needs.water();

            if (need_water == null) return 0.0f;
            if (need_water.CurCategory <= ThirstCategory.Healthy) return 0.0f;

            return 9.4f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Water need_water = pawn.needs.water();
            if (need_water == null) return null;

            // 最後に水を探してから少し経つまで次の探索はしない
            if (need_water.lastSearchWaterTick + SearchWaterIntervalTick > Find.TickManager.TicksGame) return null;
            need_water.lastSearchWaterTick = Find.TickManager.TicksGame;

            // 水アイテムを探す
            Thing thing = MizuUtility.TryFindBestWaterSourceFor(pawn, pawn, false);
            if (thing != null)
            {
                // 水アイテムが見つかった
                return new Job(MizuDef.Job_DrinkWater, thing)
                {
                    count = MizuUtility.WillGetStackCountOf(pawn, thing)
                };
            }

            //if (!pawn.CanDrinkFromTerrain())
            //{
            //    // 地形から直接水を摂取しない
            //    //   →少しうろうろさせる
            //    return null;
            //}

            // 前回使ったことがある水地形を探す
            //int maxDistance = MaxDistanceOfSearchWaterTerrain;
            //IntVec3 lastDirtyWaterVec = IntVec3.Invalid;
            //if (need_water.lastDrinkTerrainPos != IntVec3.Invalid)
            //{
            //    // 地形変化で前回の場所が水地形でなくなっていないか確認
            //    TerrainDef terrain = pawn.Map.terrainGrid.TerrainAt(need_water.lastDrinkTerrainPos);
            //    if (terrain.CanGetWater())
            //    {
            //        // 前回の水地形までの距離を最大探索距離にする
            //        maxDistance = (int)(need_water.lastDrinkTerrainPos - pawn.Position).LengthHorizontal;
            //        lastDirtyWaterVec = need_water.lastDrinkTerrainPos;
            //    }
            //    else
            //    {
            //        // 前回の水地形が使えなくなっていたので情報をリセット
            //        need_water.lastDrinkTerrainPos = IntVec3.Invalid;
            //    }
            //}

            // 地形利用処理が重いので削除、動物はどんなマップでも水が欲しい時には地形から摂取可能(家畜はダメ)
            //   ただしアイテムを見つけた時は先にアイテムから飲む
            if (pawn.RaceProps.Animal && pawn.Faction != Faction.OfPlayer)
            {
                need_water.CurLevel = 1.0f;
            }

            // どの水地形を利用するか決める
            //Predicate<IntVec3> validator = (vec) =>
            //{
            //    TerrainDef terrain = pawn.Map.terrainGrid.TerrainAt(vec);
            //    return !vec.IsForbidden(pawn) && terrain.passability == Traversability.Standable && terrain.CanGetWater();
            //};

            //IntVec3 dirtyWaterVec = IntVec3.Invalid;
            //bool isTerrainFound = CellFinder.TryRandomClosewalkCellNear(pawn.Position, pawn.Map, 300, out dirtyWaterVec, validator);

            //if (isTerrainFound)
            //{
            //    // 水地形を発見
            //    need_water.lastDrinkTerrainPos = dirtyWaterVec;
            //    return new Job(MizuDef.Job_DrinkWater, dirtyWaterVec);
            //}

            //for (int i = 10; i < MaxDistanceOfSearchWaterTerrain;)
            //{
            //    // 近場は試行回数を増やす
            //    int maxTrial = 1;
            //    if (i < 50) maxTrial = 3;

            //    for (int j = 0; j < maxTrial; j++)
            //    {
            //        IntVec3 dirtyWaterVec = IntVec3.Invalid;
            //        bool isTerrainFound = CellFinder.TryRandomClosewalkCellNear(pawn.Position, pawn.Map, i, out dirtyWaterVec, validator);

            //        if (isTerrainFound)
            //        {
            //            // 水地形を発見
            //            need_water.lastDrinkTerrainPos = dirtyWaterVec;
            //            return new Job(MizuDef.Job_DrinkWater, dirtyWaterVec);
            //        }
            //    }

            //    if (i < 50)
            //    {
            //        i += 10;
            //    }
            //    else
            //    {
            //        i += 50;
            //    }
            //}

            // 見つからなかったので前回のタイルを使用
            //if (lastDirtyWaterVec != IntVec3.Invalid)
            //{
            //    return new Job(MizuDef.Job_DrinkWater, lastDirtyWaterVec);
            //}

            // 水を発見できず、前回の水地形情報もなし
            return null;
            //return new Job(JobDefOf.GotoWander);
        }
    }
}
