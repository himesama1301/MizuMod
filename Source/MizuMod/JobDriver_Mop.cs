using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class JobDriver_Mop : JobDriver
    {
        private float cleaningWorkDone;

        private float totalCleaningWorkDone;

        private float totalCleaningWorkRequired;

        private const TargetIndex FilthInd = TargetIndex.A;

        private Filth Filth
        {
            get
            {
                return (Filth)this.job.GetTarget(FilthInd).Thing;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(FilthInd), this.job, 1, -1, null);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // ターゲットが掃除対象として不適になっていたらリストから外す
            Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(FilthInd);
            yield return initExtractTargetFromQueue;

            // ターゲットが空になっていたら成功扱いで終了
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(FilthInd);

            // ターゲットキューから次のターゲットを取り出す
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(FilthInd, true);

            // ターゲットの元へ移動
            yield return Toils_Goto.GotoThing(FilthInd, PathEndMode.Touch)
                .JumpIfDespawnedOrNullOrForbidden(FilthInd, initExtractTargetFromQueue)
                .JumpIfOutsideMopArea(FilthInd, initExtractTargetFromQueue);

            // 掃除行動
            Toil clean = new Toil();
            clean.initAction = delegate
            {
                // 必要工数の計算
                this.cleaningWorkDone = 0f;
                this.totalCleaningWorkDone = 0f;
                this.totalCleaningWorkRequired = this.Filth.def.filth.cleaningWorkToReduceThickness * (float)this.Filth.thickness;
            };
            clean.tickAction = delegate
            {
                Filth filth = this.Filth;

                // 進捗更新
                this.cleaningWorkDone += 1f;
                this.totalCleaningWorkDone += 1f;

                if (this.cleaningWorkDone > filth.def.filth.cleaningWorkToReduceThickness)
				{
                    // 汚れ1枚分の掃除完了

                    // 汚れを1枚減らす
                    filth.ThinFilth();
                    this.cleaningWorkDone = 0f;

                    if (filth.Destroyed)
                    {
                        // ターゲットの汚れが完全になくなった
                        clean.actor.records.Increment(RecordDefOf.MessesCleaned);
                        this.ReadyForNextToil();
                        return;
                    }
                }
            };
            // 細々とした設定
            clean.defaultCompleteMode = ToilCompleteMode.Never;
            clean.WithEffect(EffecterDefOf.Clean, FilthInd);
            clean.WithProgressBar(FilthInd, () => this.totalCleaningWorkDone / this.totalCleaningWorkRequired, true, -0.5f);
            clean.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
            // 掃除中に条件が変更されたら最初に戻る
            clean.JumpIfDespawnedOrNullOrForbidden(FilthInd, initExtractTargetFromQueue);
            clean.JumpIfOutsideMopArea(FilthInd, initExtractTargetFromQueue);
            yield return clean;

            // 最初に戻る
            yield return Toils_Jump.Jump(initExtractTargetFromQueue);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.cleaningWorkDone, "cleaningWorkDone", 0f, false);
            Scribe_Values.Look<float>(ref this.totalCleaningWorkDone, "totalCleaningWorkDone", 0f, false);
            Scribe_Values.Look<float>(ref this.totalCleaningWorkRequired, "totalCleaningWorkRequired", 0f, false);
        }
    }
}
