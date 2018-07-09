using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class JobDriver_Nurse : JobDriver
    {
        private const TargetIndex PatientInd = TargetIndex.A;
        private const TargetIndex ToolInd = TargetIndex.B;
        private const TargetIndex ToolPlaceInd = TargetIndex.C;
        private const int WorkTicks = 300;
        public const float ConsumeWaterVolume = 0.5f;

        private Pawn Patient
        {
            get
            {
                return (Pawn)this.job.GetTarget(PatientInd).Thing;
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
            this.pawn.Reserve(this.Patient, this.job);
            this.pawn.Reserve(this.Tool, this.job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 患者の状態による失敗条件

            // 死んだりいなくなったりしたら失敗
            this.FailOnDespawnedNullOrForbidden(PatientInd);
            this.FailOn(() =>
            {
                // 寝ていない状態になったら失敗
                if (!WorkGiver_Tend.GoodLayingStatusForTend(this.Patient, this.pawn)) return true;

                // 看護師と患者が同一人物だったら失敗
                return this.pawn == this.Patient;
            });
            // 精神崩壊状態次第で失敗とする
            this.FailOnAggroMentalState(PatientInd);
            base.AddEndCondition(delegate
            {
                // 看病が必要な状況なら続ける
                // 免疫を得る系の病気を持っている＆看病Hediffが無い
                if (Patient.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Nursed) == null) return JobCondition.Ongoing;

                // 既に看病されていたら終了
                return JobCondition.Succeeded;
            });

            // ツールまで移動
            yield return Toils_Goto.GotoThing(ToolInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(ToolInd);

            // ツールを手に取る
            yield return Toils_Haul.StartCarryThing(ToolInd);

            // 患者の元へ移動
            yield return Toils_Goto.GotoThing(PatientInd, PathEndMode.Touch);

            // 看病
            Toil workToil = new Toil();
            workToil.initAction = () =>
            {
                // 必要工数の計算
                this.ticksLeftThisToil = WorkTicks;
            };
            // 細々とした設定
            workToil.defaultCompleteMode = ToilCompleteMode.Delay;
            workToil.WithProgressBar(PatientInd, () => 1f - (float)this.ticksLeftThisToil / WorkTicks, true, -0.5f);
            workToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
            yield return workToil;

            // 看病完了時の処理
            var finishToil = new Toil();
            finishToil.initAction = () =>
            {
                // 看病状態追加
                if (Patient.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Nursed) == null)
                {
                    Patient.health.AddHediff(HediffMaker.MakeHediff(MizuDef.Hediff_Nursed, Patient));
                }

                // 水減少
                var comp = Tool.GetComp<CompWaterTool>();
                comp.StoredWaterVolume -= ConsumeWaterVolume;
            };
            finishToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finishToil;

            // ツールを片付ける場所を決める
            yield return Toils_Mizu.TryFindStoreCell(ToolInd, ToolPlaceInd);

            // 倉庫まで移動
            yield return Toils_Goto.GotoCell(ToolPlaceInd, PathEndMode.Touch);

            // 倉庫に置く
            yield return Toils_Haul.PlaceHauledThingInCell(ToolPlaceInd, null, true);
        }
    }
}
