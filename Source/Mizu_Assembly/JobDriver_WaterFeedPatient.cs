using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    internal class JobDriver_WaterFeedPatient : JobDriver
    {
        private const TargetIndex WaterIndex = TargetIndex.A;
        private const TargetIndex PatientIndex = TargetIndex.B;

        private bool getItemFromInventory;

        private Pawn patient
        {
            get
            {
                return this.TargetB.Thing as Pawn;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.getItemFromInventory, "getItemFromInventory", false, false);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.getItemFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.TargetA.Thing));
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // ターゲットがThing=水アイテムを摂取する場合

            // 水(食事)が使用不可能になったらFail
            ToilFailConditions.FailOnDestroyedNullOrForbidden<JobDriver_WaterFeedPatient>(this, WaterIndex);
            ToilFailConditions.FailOn<JobDriver_WaterFeedPatient>(this, () =>
            {
                if (this.patient == null)
                {
                    return true;
                }
                // 患者がベッドに入ってなかったらFail
                if (!this.patient.InBed())
                {
                    return true;
                }
                // 到達不能になっていたらFail
                if (!this.pawn.CanReach(this.patient, PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    return true;
                }
                return false;
            });

            // 水(食事)を予約
            if (ReservationUtility.CanReserveAndReach(this.pawn, this.TargetA, PathEndMode.Touch, Danger.Deadly, 1, this.job.count, null, false) == true)
            {
                yield return Toils_Reserve.Reserve(WaterIndex, 1, this.job.count, null);
            }
            else
            {
                yield break;
            }

            // 水を取得
            if (this.getItemFromInventory)
            {
                // 水(食事)を持ち物から取り出す
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
                // 水(食事)の場所まで行く
                yield return Toils_Goto.Goto(WaterIndex, PathEndMode.OnCell);
                // 水(食事)を拾う
                yield return Toils_Ingest.PickupIngestible(WaterIndex, this.pawn);
            }

            // 患者のもとへ移動
            yield return Toils_Goto.Goto(PatientIndex, PathEndMode.Touch);

            // 水を飲ませる＋エフェクト追加
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;
                    Thing thing = actor.CurJob.GetTarget(WaterIndex).Thing;
                    CompWater comp = thing.TryGetComp<CompWater>();
                    if (comp == null)
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        return;
                    }
                    actor.rotationTracker.FaceCell(actor.Position);
                    if (!thing.CanDrinkWaterNow())
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                        return;
                    }
                    actor.jobs.curDriver.ticksLeftThisToil = CompProperties_Water.BaseDrinkTicks;
                    if (thing.Spawned)
                    {
                        thing.Map.physicalInteractionReservationManager.Reserve(actor, this.job, thing);
                    }
                };
                toil.tickAction = delegate
                {
                    toil.actor.GainComfortFromCellIfPossible();
                };
                toil.WithProgressBar(WaterIndex, delegate
                {
                    Pawn actor = toil.actor;
                    Thing thing = actor.CurJob.GetTarget(WaterIndex).Thing;
                    if (thing == null)
                    {
                        return 1f;
                    }
                    return 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)CompProperties_Water.BaseDrinkTicks;
                }, false, -0.5f);
                toil.defaultCompleteMode = ToilCompleteMode.Delay;
                toil.FailOnDestroyedOrNull(WaterIndex);
                toil.AddFinishAction(delegate
                {
                    Pawn actor = toil.actor;
                    if (actor == null)
                    {
                        return;
                    }
                    if (actor.CurJob == null)
                    {
                        return;
                    }
                    Thing thing = actor.CurJob.GetTarget(WaterIndex).Thing;
                    if (thing == null)
                    {
                        return;
                    }
                    if (actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing))
                    {
                        actor.Map.physicalInteractionReservationManager.Release(actor, this.job, thing);
                    }
                });

                // エフェクト追加
                toil.WithEffect(delegate
                {
                    Pawn actor = toil.actor;
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(WaterIndex);
                    if (!target.HasThing)
                    {
                        return null;
                    }
                    EffecterDef effecter = null;
                    CompWater comp = target.Thing.TryGetComp<CompWater>();
                    if (comp != null)
                    {
                        effecter = comp.GetEffect;
                    }
                    return effecter;
                }, delegate
                {
                    if (!toil.actor.CurJob.GetTarget(WaterIndex).HasThing)
                    {
                        return null;
                    }
                    Thing thing = toil.actor.CurJob.GetTarget(WaterIndex).Thing;
                    if (this.patient != null)
                    {
                        return this.patient;
                    }
                    return null;
                });
                toil.PlaySustainerOrSound(delegate
                {
                    Pawn actor = toil.actor;
                    if (!actor.RaceProps.Humanlike)
                    {
                        return null;
                    }
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(WaterIndex);
                    if (!target.HasThing)
                    {
                        return null;
                    }
                    CompWater comp = target.Thing.TryGetComp<CompWater>();
                    if (comp == null)
                    {
                        return null;
                    }
                    return comp.PropsWater.getSound;
                });
                yield return toil;
            }
            // 水(食事)の摂取終了(心情、食事の処理)
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;
                    Job curJob = actor.jobs.curJob;
                    Thing thing = curJob.GetTarget(WaterIndex).Thing;

                    float num = this.patient.needs.water().WaterWanted;
                    float num2 = MizuUtility.GetWater(this.patient, thing, num);
                    if (!actor.Dead)
                    {
                        this.patient.needs.water().CurLevel += num2;
                    }
                    this.patient.records.AddTo(MizuDef.Record_WaterDrank, num2);
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return toil;
            }

            yield break;
        }
    }
}
