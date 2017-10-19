using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    internal class JobDriver_DrinkWater : JobDriver
    {
        private const TargetIndex WaterIndex = TargetIndex.A;
        private const TargetIndex TableIndex = TargetIndex.B;

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
            if (this.CurJob.targetA.HasThing)
            {
                // ターゲットがThing=水アイテムを摂取する場合

                // 水(食事)が使用不可能になったらFail
                ToilFailConditions.FailOnDestroyedNullOrForbidden<JobDriver_DrinkWater>(this, WaterIndex);

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

                // イスを探して移動する or なければ立ち食い場所を決めてそこへ移動
                {
                    Toil toil = new Toil();
                    toil.initAction = delegate
                    {
                        Pawn actor = toil.actor;
                        IntVec3 intVec = IntVec3.Invalid;
                        Thing thing = null;
                        Thing thing2 = actor.CurJob.GetTarget(WaterIndex).Thing;
                        Predicate<Thing> baseChairValidator = delegate (Thing t)
                        {
                            if (t.def.building == null || !t.def.building.isSittable)
                            {
                                return false;
                            }
                            if (t.IsForbidden(pawn))
                            {
                                return false;
                            }
                            if (!actor.CanReserve(t, 1, -1, null, false))
                            {
                                return false;
                            }
                            if (!t.IsSociallyProper(actor))
                            {
                                return false;
                            }
                            if (t.IsBurning())
                            {
                                return false;
                            }
                            if (t.HostileTo(pawn))
                            {
                                return false;
                            }
                            bool result = false;
                            for (int i = 0; i < 4; i++)
                            {
                                IntVec3 c = t.Position + GenAdj.CardinalDirections[i];
                                Building edifice = c.GetEdifice(t.Map);
                                if (edifice != null && edifice.def.surfaceType == SurfaceType.Eat)
                                {
                                    result = true;
                                    break;
                                }
                            }
                            return result;
                        };
                        CompWater comp = thing2.TryGetComp<CompWater>();
                        if (comp != null && comp.ChairSearchRadius > 0f)
                        {
                            thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, false), comp.ChairSearchRadius, (Thing t) => baseChairValidator(t) && t.Position.GetDangerFor(pawn, t.Map) == Danger.None, null, 0, -1, false, RegionType.Set_Passable, false);
                        }
                        if (thing == null)
                        {
                            intVec = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.GetTarget(WaterIndex).Thing);
                            Danger chewSpotDanger = intVec.GetDangerFor(pawn, actor.Map);
                            if (chewSpotDanger != Danger.None)
                            {
                                thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(actor, Danger.Deadly, TraverseMode.ByPawn, false), comp.ChairSearchRadius, (Thing t) => baseChairValidator(t) && t.Position.GetDangerFor(pawn, t.Map) <= chewSpotDanger, null, 0, -1, false, RegionType.Set_Passable, false);
                            }
                        }
                        if (thing != null)
                        {
                            intVec = thing.Position;
                            actor.Reserve(thing, 1, -1, null);
                        }
                        actor.Map.pawnDestinationManager.ReserveDestinationFor(actor, intVec);
                        actor.pather.StartPath(intVec, PathEndMode.OnCell);
                    };
                    toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                    yield return toil;
                }
                // テーブルの方に向く
                yield return Toils_Ingest.FindAdjacentEatSurface(TableIndex, WaterIndex);

                // 水(食事)を摂取＋エフェクト追加
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
                        actor.Drawer.rotator.FaceCell(actor.Position);
                        if (!thing.CanDrinkWaterNow())
                        {
                            actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                            return;
                        }
                        actor.jobs.curDriver.ticksLeftThisToil = CompProperties_Water.BaseDrinkTicks;
                        if (thing.Spawned)
                        {
                            thing.Map.physicalInteractionReservationManager.Reserve(actor, thing);
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
                            actor.Map.physicalInteractionReservationManager.Release(actor, thing);
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
                    //if (chewer != toil.actor)
                    //{
                    //    return chewer;
                    //}
                    if (TableIndex != TargetIndex.None && toil.actor.CurJob.GetTarget(TableIndex).IsValid)
                        {
                            return toil.actor.CurJob.GetTarget(TableIndex);
                        }
                        return thing;
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
                        float num = actor.needs.water().WaterWanted;
                        float num2 = MizuUtility.GetWater(actor, thing, num);
                        if (!actor.Dead)
                        {
                            actor.needs.water().CurLevel += num2;
                        }
                        actor.records.AddTo(MizuDef.Record_WaterDrank, num2);
                    };
                    toil.defaultCompleteMode = ToilCompleteMode.Instant;
                    yield return toil;
                }
            }
            else
            {
                // ターゲットがThingではない=水アイテムを摂取しない場合=水地形を利用する場合

                // 選んだ水地形が使用不可能or到達不可能になったらFail
                ToilFailConditions.FailOn<JobDriver_DrinkWater>(this, () =>
                {
                    return this.CurJob.targetA.Cell.IsForbidden(pawn) || !pawn.CanReach(this.CurJob.targetA.Cell, PathEndMode.OnCell, Danger.Deadly);
                });

                // 水地形まで移動
                yield return Toils_Goto.GotoCell(WaterIndex, PathEndMode.OnCell);

                // 水地形から水分を摂取＋エフェクト追加
                {
                    Toil toil = new Toil();
                    toil.initAction = delegate
                    {
                        Pawn actor = toil.actor;
                        actor.Drawer.rotator.FaceCell(actor.Position);
                        actor.jobs.curDriver.ticksLeftThisToil = CompProperties_Water.BaseDrinkTicks;
                    };
                    toil.tickAction = delegate
                    {
                        toil.actor.GainComfortFromCellIfPossible();
                    };
                    toil.WithProgressBar(WaterIndex, delegate
                    {
                        return 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)CompProperties_Water.BaseDrinkTicks;
                    }, false, -0.5f);
                    toil.defaultCompleteMode = ToilCompleteMode.Delay;
                    toil.FailOn((t) =>
                    {
                        return this.CurJob.targetA.Cell.IsForbidden(pawn) || !pawn.CanReach(this.CurJob.targetA.Cell, PathEndMode.OnCell, Danger.Deadly);
                    });

                    // エフェクト追加
                    toil.PlaySustainerOrSound(delegate
                    {
                        return DefDatabase<SoundDef>.GetNamed("Ingest_Beer");
                    });
                    yield return toil;
                }

                // 終了
                {
                    Toil toil = new Toil();
                    toil.initAction = delegate
                    {
                        Pawn actor = toil.actor;
                        Need_Water need_water = actor.needs.water();
                        float numWater = need_water.MaxLevel - need_water.CurLevel;
                        if (actor.needs.mood != null)
                        {
                            actor.needs.mood.thoughts.memories.TryGainMemory(MizuDef.Thought_DrankWaterDirectly);
                        }
                        if (!actor.Dead)
                        {
                            actor.needs.water().CurLevel += numWater;
                        }
                        actor.records.AddTo(MizuDef.Record_WaterDrank, numWater);
                    };
                    toil.defaultCompleteMode = ToilCompleteMode.Instant;
                    yield return toil;
                }
            }
            yield break;
        }
    }
}
