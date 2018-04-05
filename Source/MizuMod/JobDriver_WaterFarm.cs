using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class JobDriver_WaterFarm : JobDriver
    {
        private const TargetIndex WateringInd = TargetIndex.A;
        private const TargetIndex ToolInd = TargetIndex.B;
        private const TargetIndex ToolPlaceInd = TargetIndex.C;
        private const int WorkingTicks = 60;
        public const float ConsumeWaterVolume = 1f;

        private IntVec3 WateringPos
        {
            get
            {
                return this.job.GetTarget(WateringInd).Cell;
            }
        }
        private ThingWithComps Tool
        {
            get
            {
                return (ThingWithComps)this.job.GetTarget(ToolInd).Thing;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(WateringInd), this.job);
            this.pawn.Reserve(this.Tool, this.job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // ツールまで移動
            yield return Toils_Goto.GotoThing(ToolInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(ToolInd);

            // ツールを手に取る
            yield return Toils_Haul.StartCarryThing(ToolInd);

            // ターゲットが水やり対象として不適になっていたらリストから外す
            Toil initExtractTargetFromQueue = Toils_Mizu.ClearConditionSatisfiedTargets(WateringInd, (lti) =>
            {
                var mapComp = this.Map.GetComponent<MapComponent_Watering>();
                return mapComp.Get(this.Map.cellIndices.CellToIndex(lti.Cell)) > 0;
            });
            yield return initExtractTargetFromQueue;

            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(WateringInd);

            // ターゲットキューから次のターゲットを取り出す
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(WateringInd, true);

            // ターゲットの元へ移動
            yield return Toils_Goto.GotoCell(WateringInd, PathEndMode.Touch);

            // 作業中
            Toil workToil = new Toil();
            workToil.initAction = delegate
            {
                // 必要工数の計算
                this.ticksLeftThisToil = WorkingTicks;
            };
            // 細々とした設定
            workToil.defaultCompleteMode = ToilCompleteMode.Delay;
            workToil.WithProgressBar(WateringInd, () => 1f - (float)this.ticksLeftThisToil / WorkingTicks, true, -0.5f);
            workToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
            yield return workToil;

            // 作業終了
            var finishToil = new Toil();
            finishToil.initAction = () =>
            {
                // 水やり更新
                var mapComp = this.Map.GetComponent<MapComponent_Watering>();
                mapComp.Set(this.Map.cellIndices.CellToIndex(WateringPos), MapComponent_Watering.MaxWateringValue);
                this.Map.mapDrawer.SectionAt(WateringPos).dirtyFlags = MapMeshFlag.Terrain;

                // ツールから水を減らす
                var compTool = Tool.GetComp<CompWaterTool>();
                compTool.StoredWaterVolume -= ConsumeWaterVolume;
            };
            finishToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finishToil;

            // 最初に戻る
            yield return Toils_Jump.JumpIf(initExtractTargetFromQueue, () =>
            {
                return this.pawn.jobs.curJob.GetTargetQueue(WateringInd).Count > 0;
            });

            // ツールを片付ける場所を決める
            yield return Toils_Mizu.TryFindStoreCell(ToolInd, ToolPlaceInd);

            // 倉庫まで移動
            yield return Toils_Goto.GotoCell(ToolPlaceInd, PathEndMode.Touch);

            // 倉庫に置く
            yield return Toils_Haul.PlaceHauledThingInCell(ToolPlaceInd, null, true);
        }
    }
}
