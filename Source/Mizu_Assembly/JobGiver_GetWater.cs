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
        public override float GetPriority(Pawn pawn)
        {
            Need_Water need_water = pawn.needs.water();
            if (need_water == null)
            {
                return 0f;
            }
            if (need_water.CurLevelPercentage < Need_Water.NeedBorder)
            {
                return 9.4f;
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Water need_water = pawn.needs.water();
            if (need_water == null)
            {
                return null;
            }

            Thing thing;
            ThingDef def;
            if (!MizuUtility.TryFindBestWaterSourceFor(pawn, pawn, out thing, out def, true, false, false))
            {
                // 心情ステータスなし or 心情ステータスあり＋脱水症状まで進んでいる → 地面から直接水をすする
                if (MizuUtility.CanDrinkTerrain(pawn) == true)
                {
                    // 前回使ったことがある水タイルを探す
                    // 前回使ったタイルより近い範囲を新たに探索して、見つからなかったら前回と同じタイルを使う
                    int maxDistance = 500;
                    IntVec3 lastDirtyWaterVec = IntVec3.Invalid;
                    if (need_water.lastDrinkTerrainPos != IntVec3.Invalid)
                    {
                        // 地形変化で前回のタイルが水タイルでなくなっていないか確認
                        TerrainDef terrain = pawn.Map.terrainGrid.TerrainAt(need_water.lastDrinkTerrainPos);
                        if (terrain.defName.Contains("Water") || terrain.defName.Contains("Marsh"))
                        {
                            maxDistance = (int)(need_water.lastDrinkTerrainPos - pawn.Position).LengthHorizontal;
                            lastDirtyWaterVec = need_water.lastDrinkTerrainPos;
                        }
                        else
                        {
                            need_water.lastDrinkTerrainPos = IntVec3.Invalid;
                        }
                    }

                    // どの水地形を利用するか決める
                    Predicate<IntVec3> validator = (vec) =>
                    {
                        TerrainDef terrain = pawn.Map.terrainGrid.TerrainAt(vec);
                        return !vec.IsForbidden(pawn) && terrain.passability == Traversability.Standable && (terrain.defName.Contains("Water") || terrain.defName.Contains("Marsh"));
                    };
                    for (int i = 10; i < maxDistance; i += 10)
                    {
                        // 近場は試行回数を増やす
                        int maxTrial = 1;
                        if (i < 50)
                        {
                            maxTrial = 5;
                        }
                        for (int j = 0; j < maxTrial; j++)
                        {
                            IntVec3 dirtyWaterVec = IntVec3.Invalid;
                            bool isTerrainFound = CellFinder.TryRandomClosewalkCellNear(pawn.Position, pawn.Map, i, out dirtyWaterVec, validator);

                            if (isTerrainFound)
                            {
                                need_water.lastDrinkTerrainPos = dirtyWaterVec;
                                return new Job(MizuDef.Job_DrinkWater, dirtyWaterVec);
                            }
                        }
                    }

                    // 見つからなかったので前回のタイルを使用
                    if (lastDirtyWaterVec != IntVec3.Invalid)
                    {
                        return new Job(MizuDef.Job_DrinkWater, lastDirtyWaterVec);
                    }
                }
                return null;
            }
            
            return new Job(MizuDef.Job_DrinkWater, thing)
            {
                count = MizuUtility.WillGetStackCountOf(pawn, thing)
            };
        }
    }
}
