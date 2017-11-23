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
    }
}
