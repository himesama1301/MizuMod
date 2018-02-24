using System;
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
        private int MinTicksSinceThickened = 600;

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Filth);
            }
        }

        public override int LocalRegionsToScanFirst
        {
            get
            {
                return 4;
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Filth);
        }

        //public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        //{
        //    if (base.HasJobOnCell(pawn, c) == false) return false;

        //    // プレイヤー派閥でないなら絶対モップ掛けしない
        //    if (pawn.Faction != Faction.OfPlayer) return false;

        //    // モップエリア外はやらない
        //    if (pawn.Map.areaManager.Mop()[c] == false) return false;

        //    // 人工フロアかつモップ掛けが完了していなければOK
        //    // ThingでやるかMapComponentでやるか……

        //    return true;
        //}

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Faction != Faction.OfPlayer) return false;

            Filth filth = t as Filth;
            if (filth == null) return false;

            // モップエリア外はやらない
            if (pawn.Map.areaManager.Mop()[filth.Position] == false) return false;

            // 予約可不可＆汚れが貯まってから少しの間は無視チェック
            LocalTargetInfo target = t;
            return pawn.CanReserve(target, 1, -1, null, forced) && filth.TicksSinceThickened >= this.MinTicksSinceThickened;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // 掃除ジョブ作成
            Job job = new Job(MizuDef.Job_Mop);
            job.AddQueuedTarget(TargetIndex.A, t);

            int num = 15;
            Map map = t.Map;
            Room room = t.GetRoom(RegionType.Set_Passable);
            for (int i = 0; i < 100; i++)
            {
                // 対象の汚れの周囲100マスをサーチ
                IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
                {
                    // そこが同じ部屋の中
                    List<Thing> thingList = intVec.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        // その場所にあるThingをチェック
                        Thing thing = thingList[j];
                        if (this.HasJobOnThing(pawn, thing, forced) && thing != t)
                        {
                            // 同じジョブが作成可能(汚れがある等)あるならこののジョブの処理対象に追加
                            job.AddQueuedTarget(TargetIndex.A, thing);
                        }
                    }

                    // 掃除最大個数チェック(15個)
                    if (job.GetTargetQueue(TargetIndex.A).Count >= num)
                    {
                        break;
                    }
                }
            }
            if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
            {
                // 掃除対象が5個以上あるならポーンからの距離が近い順に掃除させる
                job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
            }

            return job;
        }

    }
}
