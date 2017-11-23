using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public static class Toils_Mizu
    {
        public static T FailOnChangingTerrain<T>(this T f, TargetIndex index, List<WaterTerrainType> waterTerrainTypeList) where T : IJobEndable
        {
            f.AddEndCondition(() =>
            {
                Thing thing = f.GetActor().jobs.curJob.GetTarget(index).Thing;
                TerrainDef terrainDef = thing.Map.terrainGrid.TerrainAt(thing.Position);
                if (!waterTerrainTypeList.Contains(terrainDef.GetWaterTerrainType()))
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            return f;
        }

        public static Toil DoRecipeWorkDrawing(TargetIndex billGiverIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;

                jobDriver_DoBill.workLeft = curJob.bill.recipe.WorkAmountTotal(null);
                jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
                jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;

                curJob.bill.Notify_DoBillStarted(actor);
            };
            toil.tickAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;

                jobDriver_DoBill.ticksSpentDoingRecipeWork++;
                curJob.bill.Notify_PawnDidWork(actor);

                IBillGiverWithTickAction billGiverWithTickAction = actor.CurJob.GetTarget(billGiverIndex).Thing as IBillGiverWithTickAction;
                if (billGiverWithTickAction != null)
                {
                    // 設備の時間経過処理
                    billGiverWithTickAction.UsedThisTick();
                }

                // 工数を進める処理
                float num = (curJob.RecipeDef.workSpeedStat != null) ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat, true) : 1f;
                Building_WorkTable building_WorkTable = jobDriver_DoBill.BillGiver as Building_WorkTable;
                if (building_WorkTable != null)
                {
                    num *= building_WorkTable.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor, true);
                }
                if (DebugSettings.fastCrafting)
                {
                    num *= 30f;
                }
                jobDriver_DoBill.workLeft -= num;

                // 椅子から快適さを得る
                actor.GainComfortFromCellIfPossible();

                // 完了チェック
                if (jobDriver_DoBill.workLeft <= 0f)
                {
                    jobDriver_DoBill.ReadyForNextToil();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, billGiverIndex);
            toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.WithProgressBar(billGiverIndex, delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.CurJob;
                return 1f - ((JobDriver_DoBill)actor.jobs.curDriver).workLeft / curJob.bill.recipe.WorkAmountTotal(null);
            }, false, -0.5f);
            toil.FailOn(() => toil.actor.CurJob.bill.suspended);
            return toil;
        }

        public static Toil FinishRecipeAndStartStoringProduct(Func<Thing> makeRecipeProduct)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;

                // 経験値取得
                if (curJob.RecipeDef.workSkill != null)
                {
                    float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.11f * curJob.RecipeDef.workSkillLearnFactor;
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp, false);
                }

                // 生産物の生成
                Thing thing = makeRecipeProduct();
                if (thing == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    return;
                }
                
                curJob.bill.Notify_IterationCompleted(actor, null);
                RecordsUtility.Notify_BillDone(actor, new List<Thing>() { thing });

                // 床置き指定
                if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
                {
                    if (!GenPlace.TryPlaceThing(thing, actor.Position, actor.Map, ThingPlaceMode.Near, null))
                    {
                        Log.Error(string.Concat(new object[]
                        {
                            actor,
                            " could not drop recipe product ",
                            thing,
                            " near ",
                            actor.Position
                        }));
                    }
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    return;
                }

                // 最適な倉庫まで持っていく
                thing.SetPositionDirect(actor.Position);
                IntVec3 c;
                if (StoreUtility.TryFindBestBetterStoreCellFor(thing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out c, true))
                {
                    actor.carryTracker.TryStartCarry(thing);
                    curJob.targetA = thing;
                    curJob.targetB = c;
                    curJob.count = 99999;
                    return;
                }
                if (!GenPlace.TryPlaceThing(thing, actor.Position, actor.Map, ThingPlaceMode.Near, null))
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Bill doer could not drop product ",
                        thing,
                        " near ",
                        actor.Position
                    }));
                }
                actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil StartCarryFromInventory(TargetIndex thingIndex)
        {
            // 水(食事)を持ち物から取り出す
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(thingIndex).Thing;
                if (actor.inventory != null && thing != null)
                {
                    actor.inventory.innerContainer.Take(thing);
                    actor.carryTracker.TryStartCarry(thing);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.FailOnDestroyedOrNull(thingIndex);
            return toil;
        }

        public static Toil StartPathToDrinkSpot(TargetIndex thingIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                IntVec3 intVec = IntVec3.Invalid;

                intVec = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.GetTarget(thingIndex).Thing);
                actor.Map.pawnDestinationReservationManager.Reserve(actor, actor.CurJob, intVec);
                actor.pather.StartPath(intVec, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        public static Toil Drink(TargetIndex thingIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(thingIndex).Thing;
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
                    thing.Map.physicalInteractionReservationManager.Reserve(actor, actor.CurJob, thing);
                }
            };
            toil.tickAction = delegate
            {
                toil.actor.GainComfortFromCellIfPossible();
            };
            toil.WithProgressBar(thingIndex, delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(thingIndex).Thing;
                if (thing == null)
                {
                    return 1f;
                }
                return 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)CompProperties_Water.BaseDrinkTicks;
            }, false, -0.5f);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(thingIndex);
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
                Thing thing = actor.CurJob.GetTarget(thingIndex).Thing;
                if (thing == null)
                {
                    return;
                }
                if (actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing))
                {
                    actor.Map.physicalInteractionReservationManager.Release(actor, actor.CurJob, thing);
                }
            });

            // エフェクト追加
            toil.WithEffect(delegate
            {
                Pawn actor = toil.actor;
                LocalTargetInfo target = toil.actor.CurJob.GetTarget(thingIndex);
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
                if (!toil.actor.CurJob.GetTarget(thingIndex).HasThing)
                {
                    return null;
                }
                return toil.actor.CurJob.GetTarget(thingIndex).Thing;
            });
            toil.PlaySustainerOrSound(delegate
            {
                Pawn actor = toil.actor;
                if (!actor.RaceProps.Humanlike)
                {
                    return null;
                }
                LocalTargetInfo target = toil.actor.CurJob.GetTarget(thingIndex);
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
            return toil;
        }

        public static Toil FinishDrink(TargetIndex thingIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(thingIndex).Thing;
                float num = actor.needs.water().WaterWanted;
                float num2 = MizuUtility.GetWater(actor, thing, num);
                if (!actor.Dead)
                {
                    actor.needs.water().CurLevel += num2;
                }
                actor.records.AddTo(MizuDef.Record_WaterDrank, num2);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil DrinkTerrain(TargetIndex thingIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                actor.rotationTracker.FaceCell(actor.Position);
                actor.jobs.curDriver.ticksLeftThisToil = CompProperties_Water.BaseDrinkTicks;
            };
            toil.tickAction = delegate
            {
                toil.actor.GainComfortFromCellIfPossible();
            };
            toil.WithProgressBar(thingIndex, delegate
            {
                return 1f - (float)toil.actor.jobs.curDriver.ticksLeftThisToil / (float)CompProperties_Water.BaseDrinkTicks;
            }, false, -0.5f);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOn((t) =>
            {
                Pawn actor = toil.actor;
                return actor.CurJob.targetA.Cell.IsForbidden(actor) || !actor.CanReach(actor.CurJob.targetA.Cell, PathEndMode.OnCell, Danger.Deadly);
            });

            // エフェクト追加
            toil.PlaySustainerOrSound(delegate
            {
                return DefDatabase<SoundDef>.GetNamed("Ingest_Beer");
            });
            return toil;
        }

        public static Toil FinishDrinkTerrain()
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
            return toil;
        }
    }
}
