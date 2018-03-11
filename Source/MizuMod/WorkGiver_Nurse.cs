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

            // 自分自身の看病は出来ない
            if (pawn == t) return false;

            // 対象を予約できない
            if (!pawn.CanReserve(giver, 1, -1, null, forced)) return false;

            // 人間用WorkGiverで相手が人間、または動物用WorkGiverで相手が動物、の組み合わせでない
            if (!((this.def.tendToHumanlikesOnly && giver.RaceProps.Humanlike) || (this.def.tendToAnimalsOnly && giver.RaceProps.Animal))) return false;

            // 治療可能な体勢になっていない
            if (!WorkGiver_Tend.GoodLayingStatusForTend(giver, pawn)) return false;

            // 免疫を得て直すタイプの健康状態を持っていない
            // (治療状態は問わない)
            if (!giver.health.hediffSet.hediffs.Any((hediff) => hediff.def.PossibleToDevelopImmunityNaturally())) return false;

            // 看病された効果が残っている
            if (giver.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Nursed) != null) return false;

            // 看病アイテムのチェック
            var mopList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where((thing) =>
            {
                var comp = thing.TryGetComp<CompWaterTool>();
                if (comp == null) return false;
                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.Nurse)) return false;

                // 1回も使えないレベルの保有水量だったらダメ
                if (Mathf.Floor(comp.StoredWaterVolume / JobDriver_Nurse.ConsumeWaterVolume / 0.79f) <= 0) return false;

                return true;
            });
            if (mopList.Count() == 0) return false;
            if (mopList.Where((thing) => pawn.CanReserve(thing)).Count() == 0) return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // 看病ジョブ作成
            Job job = new Job(MizuDef.Job_Nurse);
            job.SetTarget(TargetIndex.A, t);

            // 一番近いツールを探す
            Thing candidateTool = null;
            int minDist = int.MaxValue;
            var toolList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where((thing) =>
            {
                var comp = thing.TryGetComp<CompWaterTool>();
                if (comp == null) return false;
                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.Nurse)) return false;

                // 1回も使えないレベルの保有水量だったらダメ
                // 80%未満で水を補充するので80%程度であれば使用可能とする
                if (Mathf.Floor(comp.StoredWaterVolume / JobDriver_Nurse.ConsumeWaterVolume / 0.79f) <= 0) return false;

                return true;
            });
            foreach (var tool in toolList)
            {
                // 予約できないツールはパス
                if (!pawn.CanReserve(tool)) continue;

                int toolDist = (tool.Position - pawn.Position).LengthHorizontalSquared;
                if (minDist > toolDist)
                {
                    minDist = toolDist;
                    candidateTool = tool;
                }
            }

            if (candidateTool == null)
            {
                Log.Error("candidateTool is null");
                return null;
            }

            // ツールをTargetBにセット
            job.SetTarget(TargetIndex.B, candidateTool);
            job.count = 1;

            return job;
        }
    }
}
