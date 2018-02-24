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
        private const TargetIndex MopInd = TargetIndex.A;
        private const int MoppingTicks = 200;

        private IntVec3 MoppingPos
        {
            get
            {
                return this.job.GetTarget(MopInd).Cell;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(MopInd), this.job, 1, -1, null);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // ターゲットが掃除対象として不適になっていたらリストから外す
            //Thing系にしか使えない
            Toil initExtractTargetFromQueue = Toils_Mizu.ClearCondifionSatisfiedTargets(MopInd, (lti) =>
            {
                return lti.Cell.GetFirstThing(this.pawn.Map, MizuDef.Thing_MoppedThing) != null;
            });
            yield return initExtractTargetFromQueue;

            // ターゲットが空になっていたら成功扱いで終了
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(MopInd);

            // ターゲットキューから次のターゲットを取り出す
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(MopInd, true);

            // ターゲットの元へ移動
            yield return Toils_Goto.GotoCell(MopInd, PathEndMode.Touch)
                .JumpIf(() =>
                {
                    var target = this.pawn.jobs.curJob.GetTarget(MopInd);
                    if (target.HasThing) return true;

                    return target.Cell.GetFirstThing(this.pawn.Map, MizuDef.Thing_MoppedThing) != null;
                }, initExtractTargetFromQueue)
                .JumpIfOutsideMopArea(MopInd, initExtractTargetFromQueue);

            // ピカピカ追加
            Toil mopToil = new Toil();
            mopToil.initAction = delegate
            {
                // 必要工数の計算
                this.ticksLeftThisToil = MoppingTicks;
            };
            mopToil.AddFinishAction(() =>
            {
                // モップオブジェクト生成
                var moppedThing = ThingMaker.MakeThing(MizuDef.Thing_MoppedThing);
                GenSpawn.Spawn(moppedThing, this.MoppingPos, mopToil.actor.Map);
            });
            // 細々とした設定
            mopToil.defaultCompleteMode = ToilCompleteMode.Delay;
            mopToil.WithProgressBar(MopInd, () => 1f - (float)this.ticksLeftThisToil / MoppingTicks, true, -0.5f);
            mopToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
            // 掃除中に条件が変更されたら最初に戻る
            mopToil.JumpIf(() =>
            {
                var target = this.pawn.jobs.curJob.GetTarget(MopInd);
                if (target.HasThing) return true;

                return target.Cell.GetFirstThing(this.pawn.Map, MizuDef.Thing_MoppedThing) != null;
            }, initExtractTargetFromQueue);
            mopToil.JumpIfOutsideMopArea(MopInd, initExtractTargetFromQueue);
            yield return mopToil;

            // 最初に戻る
            yield return Toils_Jump.Jump(initExtractTargetFromQueue);
        }
    }
}
