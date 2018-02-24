﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class WorkGiver_Mop : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override int LocalRegionsToScanFirst
        {
            get
            {
                return 4;
            }
        }

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            return pawn.Map.areaManager.Mop().ActiveCells;
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            // プレイヤー派閥でないなら絶対モップ掛けしない
            if (pawn.Faction != Faction.OfPlayer) return false;

            // モップエリア外はやらない
            if (pawn.Map.areaManager.Mop()[c] == false) return false;

            // 人工フロアかつカーペット以外の場所ならOK
            //   自然地形の汚れが付く＝人工フロア
            //   カーペットの研究が必要なもの＝カーペット
            var terrain = c.GetTerrain(pawn.Map);
            if (terrain.acceptTerrainSourceFilth == false || (terrain.researchPrerequisites != null && terrain.researchPrerequisites.Contains(ResearchProjectDefOf.CarpetMaking))) return false;

            // その場所に汚れがあったらやらない
            var filthList = c.GetThingList(pawn.Map).Where((t) => { return t is Filth; });
            if (filthList != null && filthList.Count() > 0) return false;

            // その場所を予約できないならやらない
            if (!pawn.CanReserve(c)) return false;

            return true;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            // モップジョブ作成
            Job job = new Job(MizuDef.Job_Mop);
            job.AddQueuedTarget(TargetIndex.A, cell);

            int maxQueueLength = 15;
            Map map = pawn.Map;
            Room room = cell.GetRoom(map);
            for (int i = 0; i < 100; i++)
            {
                // 対象の汚れの周囲100マスをサーチ
                IntVec3 intVec = cell + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
                {
                    // そこが同じ部屋の中
                    if (this.HasJobOnCell(pawn, intVec) && intVec != cell)
                    {
                        // 同じジョブが作成可能(汚れがある等)あるならこのジョブの処理対象に追加
                        job.AddQueuedTarget(TargetIndex.A, intVec);
                    }

                    // 掃除最大個数チェック(15個)
                    if (job.GetTargetQueue(TargetIndex.A).Count >= maxQueueLength) break;
                }
            }

            if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
            {
                // 掃除対象が5個以上あるならポーンからの距離が近い順に掃除させる
                job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
            }

            return job;
        }

        //public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        //{
        //    // 掃除ジョブ作成
        //    Job job = new Job(MizuDef.Job_Mop);
        //    job.AddQueuedTarget(TargetIndex.A, t);

        //    int num = 15;
        //    Map map = t.Map;
        //    Room room = t.GetRoom(RegionType.Set_Passable);
        //    for (int i = 0; i < 100; i++)
        //    {
        //        // 対象の汚れの周囲100マスをサーチ
        //        IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
        //        if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
        //        {
        //            // そこが同じ部屋の中
        //            List<Thing> thingList = intVec.GetThingList(map);
        //            for (int j = 0; j < thingList.Count; j++)
        //            {
        //                // その場所にあるThingをチェック
        //                Thing thing = thingList[j];
        //                if (this.HasJobOnThing(pawn, thing, forced) && thing != t)
        //                {
        //                    // 同じジョブが作成可能(汚れがある等)あるならこののジョブの処理対象に追加
        //                    job.AddQueuedTarget(TargetIndex.A, thing);
        //                }
        //            }

        //            // 掃除最大個数チェック(15個)
        //            if (job.GetTargetQueue(TargetIndex.A).Count >= num)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
        //    {
        //        // 掃除対象が5個以上あるならポーンからの距離が近い順に掃除させる
        //        job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
        //    }

        //    return job;
        //}

    }
}