using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrawFromWaterPool : JobDriver_DoBill
    {
        private const TargetIndex BillGiverIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.GetTarget(BillGiverIndex), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var waterGrid = this.TargetThingA.Map.GetComponent<MapComponent_ShallowWaterGrid>();
            var pool = waterGrid.GetPool(this.TargetThingA.Map.cellIndices.CellToIndex(this.TargetThingA.Position));
            var recipe = this.job.bill.recipe as GetWaterRecipeDef;

            // 設備が使えなくなったら失敗
            ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_DrawFromWaterPool>(this, BillGiverIndex);

            // 地下水源の水量が必要量より少なくなったら失敗
            ToilFailConditions.FailOn<JobDriver_DrawFromWaterPool>(this, () =>
            {
                return pool.CurrentWaterVolume < recipe.needWaterVolume;
            });
            
            GetWaterRecipeDef getWaterRecipe = this.job.bill.recipe as GetWaterRecipeDef;
            if (getWaterRecipe == null)
            {
                this.GetActor().jobs.EndCurrentJob(JobCondition.Incompletable);
                yield break;
            }

            // 設備まで行く
            yield return Toils_Goto.GotoCell(this.job.GetTarget(BillGiverIndex).Thing.InteractionCell, PathEndMode.OnCell);

            // レシピ実行
            yield return Toils_Mizu.DoRecipeWorkDrawing(BillGiverIndex);

            // レシピ終了処理
            yield return Toils_Mizu.FinishRecipeAndStartStoringProduct(() =>
            {
                // 地下水脈から水を減らす
                pool.CurrentWaterVolume = Mathf.Max(0, pool.CurrentWaterVolume - recipe.needWaterVolume);

                // 水を生成
                return ThingMaker.MakeThing(MizuDef.Thing_NormalWater);
            });
        }

        //protected override IEnumerable<Toil> MakeNewToils()
        //{
        //    List<Toil> toils = base.MakeNewToils().ToList();
        //    Toil lastToil = new Toil();
        //    lastToil.initAction = () =>
        //    {
        //        var waterGrid = this.TargetThingA.Map.GetComponent<MapComponent_ShallowWaterGrid>();
        //        var pool = waterGrid.GetPool(this.TargetThingA.Map.cellIndices.CellToIndex(this.TargetThingA.Position));
        //        var recipe = this.job.bill.recipe as FaucetRecipeDef;

        //        pool.CurrentWaterVolume = Mathf.Max(0, pool.CurrentWaterVolume - recipe.needWaterVolume);
        //        waterGrid.SetDirty();
        //    };
        //    lastToil.defaultCompleteMode = ToilCompleteMode.Instant;

        //    toils.Insert(12, lastToil);

        //    return toils.AsEnumerable();
        //}

        //public override string GetReport()
        //{
        //    return base.GetReport() + "!!!" + this.CurToilIndex.ToString();
        //}
    }
}
