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
            Pawn taker = pawn;
            Pawn giver = t as Pawn;

            // 与える相手が人でない、自分自身に与える→×
            if (giver == null || giver == taker) return false;

            // 人間のような食事の与え方をする仕事だが、与える相手が人間ではない→×
            if (this.def.feedHumanlikesOnly && !giver.RaceProps.Humanlike) return false;

            // 動物のような食事の与え方をする仕事だが、与える相手が動物ではない→×
            if (this.def.feedAnimalsOnly && !giver.RaceProps.Animal) return false;

            // 与える相手が水分要求を持っているが、喉が渇いていると感じていない→×
            if (giver.needs.water() == null || giver.needs.water().CurLevelPercentage > giver.needs.water().PercentageThreshThirsty + 0.02f) return false;

            // 与える相手の状態が、誰かに食事を与えてもらうべき状態ではない→×
            if (!FeedPatientUtility.ShouldBeFed(giver)) return false;

            // 給仕者が与える相手を「予約可能＆到達可能」ではない→×
            if (!taker.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced)) return false;

            if (MizuUtility.TryFindBestWaterSourceFor(taker, giver) == null)
            {
                // 与えられる水があるか探したが見つからなかった
                JobFailReason.Is(MizuStrings.JobFailReasonNoWater);
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn getter, Thing target, bool forced = false)
        {
            Pawn patient = target as Pawn;

            // 水を探す
            Thing waterThing = MizuUtility.TryFindBestWaterSourceFor(getter, patient);
            if (waterThing == null) return null;

            // 水を与えるジョブを発行
            return new Job(MizuDef.Job_FeedWaterPatient)
            {
                targetA = waterThing,
                targetB = patient,
                count = MizuUtility.WillGetStackCountOf(patient, waterThing)
            };
        }
    }
}
