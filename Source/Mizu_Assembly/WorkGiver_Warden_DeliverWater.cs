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
            if (!base.ShouldTakeCareOfPrisoner(pawn, t))
            {
                return null;
            }
            Pawn pawn2 = (Pawn)t;
            if (!pawn2.guest.CanBeBroughtFood)
            {
                return null;
            }
            if (!pawn2.Position.IsInPrisonCell(pawn2.Map))
            {
                return null;
            }
            Need_Water need_water = pawn2.needs.water();
            if (need_water == null)
            {
                return null;
            }
            if (need_water.CurLevelPercentage >= need_water.PercentageThreshThirsty + 0.02f)
            {
                return null;
            }
            if (WardenFeedUtility.ShouldBeFed(pawn2))
            {
                return null;
            }

            Thing thing = MizuUtility.TryFindBestWaterSourceFor(pawn, pawn2, true, false, false);
            if (thing == null)
            {
                return null;
            }
            if (thing.GetRoom(RegionType.Set_Passable) == pawn2.GetRoom(RegionType.Set_Passable))
            {
                return null;
            }
            if (WorkGiver_Warden_DeliverWater.WaterAvailableInRoomTo(pawn2))
            {
                return null;
            }
            return new Job(MizuDef.Job_DeliverWater, thing, pawn2)
            {
                count = MizuUtility.WillGetStackCountOf(pawn2, thing),
                targetC = RCellFinder.SpotToChewStandingNear(pawn2, thing)
            };
        }

        private static bool WaterAvailableInRoomTo(Pawn prisoner)
        {
            if (prisoner.carryTracker.CarriedThing != null && WorkGiver_Warden_DeliverWater.WaterAmountAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
            {
                return true;
            }
            float num = 0f;
            float num2 = 0f;
            Room room = prisoner.GetRoom(RegionType.Set_Passable);
            if (room == null)
            {
                return false;
            }
            for (int i = 0; i < room.RegionCount; i++)
            {
                Region region = room.Regions[i];
                List<Thing> list = region.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
                for (int j = 0; j < list.Count; j++)
                {
                    Thing thing = list[j];
                    if (!thing.CanDrinkWater() || thing.GetWaterPreferability() > WaterPreferability.NeverDrink)
                    {
                        num2 += WorkGiver_Warden_DeliverWater.WaterAmountAvailableForFrom(prisoner, thing);
                    }
                }
                List<Thing> list2 = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                for (int k = 0; k < list2.Count; k++)
                {
                    Pawn pawn = list2[k] as Pawn;
                    Need_Water need_water = pawn.needs.water();
                    if (need_water != null && pawn.IsPrisonerOfColony && need_water.CurLevelPercentage < need_water.PercentageThreshThirsty + 0.02f && pawn.carryTracker.CarriedThing == null)
                    {
                        num += need_water.WaterWanted;
                    }
                }
            }
            return num2 + 0.5f >= num;
        }

        private static float WaterAmountAvailableForFrom(Pawn p, Thing waterSource)
        {
            if (waterSource.CanGetWater())
            {
                return waterSource.GetWaterAmount() * (float)waterSource.stackCount;
            }
            return 0f;
        }
    }
}
