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
    public class WorkGiver_Nurse : WorkGiver_TendOther
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var giver = t as Pawn;

            // 対象がポーンではない
            if (giver == null) return false;

            // 通常の看護条件を満たしていない
            //if (!base.HasJobOnThing(pawn, t, forced)) return false;

            // 人間用WorkGiverで相手が人間、または動物用WorkGiverで相手が動物、の組み合わせでない
            if (!((this.def.tendToHumanlikesOnly && giver.RaceProps.Humanlike) || (this.def.tendToAnimalsOnly && giver.RaceProps.Animal))) return false;

            // 治療可能な体勢になっていない
            if (!WorkGiver_Tend.GoodLayingStatusForTend(giver, pawn)) return false;

            // 免疫を得て直すタイプの健康状態を持っていない
            // (治療状態は問わない)
            if (!giver.health.hediffSet.hediffs.Any((hediff) => hediff.def.PossibleToDevelopImmunityNaturally())) return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("=== towel ===");
            return new Job(JobDefOf.WaitWander);
        }
    }
}
