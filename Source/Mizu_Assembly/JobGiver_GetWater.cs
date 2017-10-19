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
                if (pawn.needs.mood == null || (need_water.CurCategory == ThirstCategory.Dehydration))
                {
                    // どの水地形を利用するか決める
                    Predicate<IntVec3> validator = (vec) =>
                    {
                        TerrainDef terrain = pawn.Map.terrainGrid.TerrainAt(vec);
                        return !vec.IsForbidden(pawn) && terrain.passability == Traversability.Standable && (terrain.defName.Contains("Water") || terrain.defName.Contains("Marsh"));
                    };
                    for (int i = 10; i < 500; i += 10)
                    {
                        IntVec3 dirtyWaterVec = IntVec3.Invalid;
                        bool isTerrainFound = CellFinder.TryRandomClosewalkCellNear(pawn.Position, pawn.Map, i, out dirtyWaterVec, validator);

                        if (isTerrainFound)
                        {
                            return new Job(MizuDef.Job_DrinkWater, dirtyWaterVec);
                        }
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
