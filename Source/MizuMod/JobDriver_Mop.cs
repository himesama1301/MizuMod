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
        private const TargetIndex MoppingInd = TargetIndex.A;
        private const TargetIndex MopInd = TargetIndex.B;
        private const TargetIndex MopPlaceInd = TargetIndex.C;
        private const int MoppingTicks = 60;
        public const float ConsumeWaterVolume = 0.05f;

        private IntVec3 MoppingPos
        {
            get
            {
                return this.job.GetTarget(MoppingInd).Cell;
            }
        }
        private ThingWithComps Mop
        {
            get
            {
                return (ThingWithComps)this.job.GetTarget(MopInd).Thing;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(MoppingInd), this.job);
            this.pawn.Reserve(this.Mop, this.job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // モップまで移動
            yield return Toils_Goto.GotoThing(MopInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(MopInd);

            // モップを手に取る
            yield return Toils_Haul.StartCarryThing(MopInd);

            // ターゲットが掃除対象として不適になっていたらリストから外す
            //Thing系にしか使えない
            Toil initExtractTargetFromQueue = Toils_Mizu.ClearCondifionSatisfiedTargets(MoppingInd, (lti) =>
            {
                return lti.Cell.GetFirstThing(this.pawn.Map, MizuDef.Thing_MoppedThing) != null;
            });
            yield return initExtractTargetFromQueue;

            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(MoppingInd);

            // ターゲットキューから次のターゲットを取り出す
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(MoppingInd, true);

            // ターゲットの元へ移動
            yield return Toils_Goto.GotoCell(MoppingInd, PathEndMode.Touch)
                .JumpIf(() =>
                {
                    var target = this.pawn.jobs.curJob.GetTarget(MoppingInd);
                    if (target.HasThing) return true;

                    return target.Cell.GetFirstThing(this.pawn.Map, MizuDef.Thing_MoppedThing) != null;
                }, initExtractTargetFromQueue)
                .JumpIfOutsideMopArea(MoppingInd, initExtractTargetFromQueue);

            // モップ掛け作業中
            Toil mopToil = new Toil();
            mopToil.initAction = delegate
            {
                // 必要工数の計算
                this.ticksLeftThisToil = MoppingTicks;
            };
            // 細々とした設定
            mopToil.defaultCompleteMode = ToilCompleteMode.Delay;
            mopToil.WithProgressBar(MoppingInd, () => 1f - (float)this.ticksLeftThisToil / MoppingTicks, true, -0.5f);
            mopToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
            // 掃除中に条件が変更されたら最初に戻る
            mopToil.JumpIf(() =>
            {
                var target = this.pawn.jobs.curJob.GetTarget(MoppingInd);
                if (target.HasThing) return true;

                return target.Cell.GetFirstThing(this.pawn.Map, MizuDef.Thing_MoppedThing) != null;
            }, initExtractTargetFromQueue);
            mopToil.JumpIfOutsideMopArea(MoppingInd, initExtractTargetFromQueue);
            yield return mopToil;

            // モップ掛け終了
            var finishToil = new Toil();
            finishToil.initAction = () =>
            {
                // モップオブジェクト生成
                var moppedThing = ThingMaker.MakeThing(MizuDef.Thing_MoppedThing);
                GenSpawn.Spawn(moppedThing, this.MoppingPos, mopToil.actor.Map);

                // モップから水を減らす
                var compTool = Mop.GetComp<CompWaterTool>();
                compTool.StoredWaterVolume -= ConsumeWaterVolume;
            };
            finishToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finishToil;

            // 最初に戻る
            yield return Toils_Jump.JumpIf(initExtractTargetFromQueue, () =>
            {
                return this.pawn.jobs.curJob.GetTargetQueue(MoppingInd).Count > 0;
            });

            // モップを片付ける場所を決める
            yield return Toils_Mizu.TryFindStoreCell(MopInd, MopPlaceInd);
            //Toil startCarryToil = new Toil();
            //startCarryToil.initAction = () =>
            //{
            //    var actor = startCarryToil.actor;
            //    var curJob = actor.jobs.curJob;
            //    IntVec3 c;
            //    if (StoreUtility.TryFindBestBetterStoreCellFor(Mop, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out c))
            //    {
            //        curJob.targetC = c;
            //        curJob.count = 99999;
            //        return;
            //    }
            //};
            //startCarryToil.defaultCompleteMode = ToilCompleteMode.Instant;
            //yield return startCarryToil;

            // 倉庫まで移動
            yield return Toils_Goto.GotoCell(MopPlaceInd, PathEndMode.Touch);

            // 倉庫に置く
            yield return Toils_Haul.PlaceHauledThingInCell(MopPlaceInd, null, true);
        }
    }
}
