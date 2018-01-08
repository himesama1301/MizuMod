using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public abstract class JobDriver_DrawWater : JobDriver_DoBill
    {
        protected DefExtension_WaterRecipe ext;

        private Action finishAction = () => { };

        public override bool TryMakePreToilReservations()
        {
            if (!this.pawn.Reserve(this.job.GetTarget(BillGiverInd), this.job)) return false;

            this.ext = this.job.bill.recipe.GetModExtension<DefExtension_WaterRecipe>();
            if (this.ext == null) return false;

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // その他の失敗条件設定
            this.SetFailCondition();

            PathEndMode peMode;
            if (this.job.GetTarget(BillGiverInd).Thing.def.hasInteractionCell)
            {
                peMode = PathEndMode.InteractionCell;
            }
            else
            {
                peMode = PathEndMode.ClosestTouch;
            }

            // 設備まで行く
            yield return Toils_Goto.GotoThing(BillGiverInd, peMode);

            // レシピ実行
            yield return Toils_Mizu.DoRecipeWorkDrawing(BillGiverInd);

            // レシピ終了処理
            yield return Toils_Mizu.FinishRecipeAndStartStoringProduct(this.FinishAction);

            // 最適な倉庫まで運ぶ場合はさらに処理をする

            // 持っていく
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.B);

            // 置く
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.A, null, false);
        }

        protected abstract void SetFailCondition();

        protected abstract Thing FinishAction();
    }
}
