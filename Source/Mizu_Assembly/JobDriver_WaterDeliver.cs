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
        private const TargetIndex ChewSpotIndex = TargetIndex.C;

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

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 水が使用不可能になったらFail
            ToilFailConditions.FailOnDestroyedNullOrForbidden<JobDriver_WaterDeliver>(this, WaterIndex);

            // 水(食事)を予約
            if (ReservationUtility.CanReserveAndReach(this.pawn, this.TargetA, PathEndMode.Touch, Danger.Deadly, 1, this.CurJob.count, null, false) == true)
            {
                yield return Toils_Reserve.Reserve(WaterIndex, 1, this.CurJob.count, null);
            }
            else
            {
                yield break;
            }

            // 水を取得
            if (this.drinkingFromInventory)
            {
                // 水を持ち物から取り出す
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;
                    Job curJob = actor.jobs.curJob;
                    Thing thing = curJob.GetTarget(TargetIndex.A).Thing;
                    if (actor.inventory != null && thing != null)
                    {
                        actor.inventory.innerContainer.Take(thing);
                        actor.carryTracker.TryStartCarry(thing);
                    }
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                toil.FailOnDestroyedOrNull(WaterIndex);
                yield return toil;
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

            // 置く
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;
                    if (actor.carryTracker == null || actor.carryTracker.CarriedThing == null)
                    {
                        return;
                    }
                    Thing thing = null;
                    bool isDropSuccess = actor.carryTracker.TryDropCarriedThing(this.TargetC.Cell, ThingPlaceMode.Direct, out thing);
                    if (!isDropSuccess)
                    {
                        isDropSuccess = actor.carryTracker.TryDropCarriedThing(this.TargetC.Cell, ThingPlaceMode.Near, out thing);
                    }

                    if (isDropSuccess)
                    {
                        if (pawn.Map.reservationManager.ReservedBy(thing, pawn))
                        {
                            pawn.Map.reservationManager.Release(thing, pawn);
                        }
                        ReservationUtility.Reserve((Pawn)this.TargetB.Thing, thing);
                    }
                    
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                toil.atomicWithPrevious = true;
                yield return toil;
            }
            yield break;
        }
    }
}
