using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_Warden_DeliverWater : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn warden = pawn;
            Pawn prisoner = t as Pawn;

            // 世話が必要でない
            if (!base.ShouldTakeCareOfPrisoner(warden, prisoner)) return null;

            // 囚人が食事を持って来てもらえる扱いではない
            if (!prisoner.guest.CanBeBroughtFood) return null;

            // 囚人は牢屋にいない
            if (!prisoner.Position.IsInPrisonCell(prisoner.Map)) return null;

            Need_Water need_water = prisoner.needs.water();

            // 水分要求がない
            if (need_water == null) return null;

            // 喉が渇いていない
            if (need_water.CurLevelPercentage >= need_water.PercentageThreshThirsty + 0.02f) return null;

            // (囚人が病人だから)食事を与えられるべき状態である(部屋に運ばれたものを自分で食べることができない)
            if (WardenFeedUtility.ShouldBeFed(prisoner)) return null;

            // 水が見つからない
            Thing thing = MizuUtility.TryFindBestWaterSourceFor(warden, prisoner, false);
            if (thing == null) return null;

            // 見つかった水アイテムは既に囚人がいる部屋の中にある
            if (thing.GetRoom(RegionType.Set_Passable) == prisoner.GetRoom(RegionType.Set_Passable)) return null;

            // 部屋の中に十分な量の水がある
            if (WorkGiver_Warden_DeliverWater.WaterAvailableInRoomTo(prisoner)) return null;

            // 水を運んでくるジョブを発行
            return new Job(MizuDef.Job_DeliverWater, thing, prisoner)
            {
                count = MizuUtility.WillGetStackCountOf(prisoner, thing),
                targetC = RCellFinder.SpotToChewStandingNear(prisoner, thing)
            };
        }

        private static bool WaterAvailableInRoomTo(Pawn prisoner)
        {
            // 囚人が何か物を運んでいる＆その物から得られる水分量は正の値
            if (prisoner.carryTracker.CarriedThing != null && WorkGiver_Warden_DeliverWater.WaterAmountAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
            {
                return true;
            }

            float allPawnWantedWater = 0.0f;
            float allThingWaterAmount = 0f;

            Room room = prisoner.GetRoom(RegionType.Set_Passable);
            if (room == null) return false;

            foreach (var region in room.Regions)
            {
                // 囚人の部屋の中の全水アイテムの水分量を計算
                foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
                {
                    if (!thing.CanDrinkWater() || thing.GetWaterPreferability() > WaterPreferability.NeverDrink)
                    {
                        allThingWaterAmount += WorkGiver_Warden_DeliverWater.WaterAmountAvailableForFrom(prisoner, thing);
                    }
                }

                // 囚人の部屋のポーンの要求水分量の合計を計算
                foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
                {
                    Pawn pawn = thing as Pawn;
                    Need_Water need_water = pawn.needs.water();

                    // 水要求なし
                    if (need_water == null) continue;

                    // コロニーの囚人ではない
                    if (!pawn.IsPrisonerOfColony) continue;

                    // 喉が渇いていない
                    if (need_water.CurLevelPercentage >= need_water.PercentageThreshThirsty + 0.02f) continue;

                    // 物を運んでいる
                    if (pawn.carryTracker.CarriedThing != null) continue;

                    allPawnWantedWater += need_water.WaterWanted;
                }
            }

            // その部屋に十分な水の量があればtrue
            return allThingWaterAmount + 0.5f >= allPawnWantedWater;
        }

        private static float WaterAmountAvailableForFrom(Pawn p, Thing waterSource)
        {
            // その物は水分を得られるものではない
            if (!waterSource.CanGetWater()) return 0.0f;

            return waterSource.GetWaterAmount() * (float)waterSource.stackCount;
        }
    }
}
