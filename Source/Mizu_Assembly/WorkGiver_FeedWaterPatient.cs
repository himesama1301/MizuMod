using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_FeedWaterPatient : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || pawn2 == pawn)
            {
                return false;
            }
            if (this.def.feedHumanlikesOnly && !pawn2.RaceProps.Humanlike)
            {
                return false;
            }
            if (this.def.feedAnimalsOnly && !pawn2.RaceProps.Animal)
            {
                return false;
            }
            if (pawn2.needs.water() == null || pawn2.needs.water().CurLevelPercentage > pawn2.needs.water().PercentageThreshThirsty + 0.02f)
            {
                return false;
            }
            if (!FeedPatientUtility.ShouldBeFed(pawn2))
            {
                return false;
            }
            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
            {
                return false;
            }

            Thing thing = MizuUtility.TryFindBestWaterSourceFor(pawn, pawn2, true, false, false);
            if (thing == null)
            {
                JobFailReason.Is(MizuStrings.JobFailReasonNoWater);
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn getter, Thing target, bool forced = false)
        {
            Pawn patient = target as Pawn;

            Thing waterThing = MizuUtility.TryFindBestWaterSourceFor(getter, patient, true, false, false);
            if (waterThing == null) return null;

            return new Job(MizuDef.Job_FeedWaterPatient)
            {
                targetA = waterThing,
                targetB = patient,
                count = MizuUtility.WillGetStackCountOf(patient, waterThing)
            };
        }
    }
}
