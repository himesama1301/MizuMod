using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_WaterDeliver : JobDriver
    {
        private const TargetIndex WaterIndex = TargetIndex.A;
        private const TargetIndex PrisonerIndex = TargetIndex.B;
        private const TargetIndex DropSpotIndex = TargetIndex.C;

        private bool drinkingFromInventory;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.drinkingFromInventory, "drinkingFromInventory", false, false);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.drinkingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.TargetA.Thing));
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 水が使用不可能になったらFail
            ToilFailConditions.FailOnDestroyedNullOrForbidden<JobDriver_WaterDeliver>(this, WaterIndex);

            if (!this.pawn.CanReserveAndReach(this.TargetA, PathEndMode.Touch, Danger.Deadly, 1, this.job.count))
            {
                // 水を予約できなかったら終了
                this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                yield break;
            }

            // 予約する
            if (!this.pawn.Map.reservationManager.ReservedBy(this.TargetA.Thing, pawn))
            {
                yield return Toils_Reserve.Reserve(WaterIndex, 1, this.job.count, null);
            }

            if (this.drinkingFromInventory)
            {
                // 水を持ち物から取り出す
                yield return Toils_Mizu.StartCarryFromInventory(WaterIndex);
            }
            else
            {
                // 水の場所まで行く
                yield return Toils_Goto.Goto(WaterIndex, PathEndMode.OnCell);

                // 水を拾う
                yield return Toils_Ingest.PickupIngestible(WaterIndex, this.pawn);
            }

            // スポットまで移動する
            yield return Toils_Goto.GotoCell(this.TargetC.Cell, PathEndMode.OnCell);

            // 置いて囚人に予約させる
            yield return Toils_Mizu.DropCarriedThing(PrisonerIndex, DropSpotIndex);
        }
    }
}
