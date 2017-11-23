using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace MizuMod
{
    public abstract class JobDriver_DrawWater : JobDriver_DoBill
    {
        protected GetWaterRecipeDef recipe;

        private Action finishAction = () => { };

        public override bool TryMakePreToilReservations()
        {
            if (!this.pawn.Reserve(this.job.GetTarget(BillGiverInd), this.job)) return false;

            this.recipe = this.job.bill.recipe as GetWaterRecipeDef;
            if (this.recipe == null) return false;

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 設備が使えなくなったら失敗
            ToilFailConditions.FailOnDespawnedNullOrForbidden(this, BillGiverInd);

            // その他の失敗条件設定
            this.SetFailCondition();

            // 設備まで行く
            yield return Toils_Goto.GotoCell(this.job.GetTarget(BillGiverInd).Thing.InteractionCell, PathEndMode.OnCell);

            // レシピ実行
            yield return Toils_Mizu.DoRecipeWorkDrawing(BillGiverInd);

            // レシピ終了処理
            yield return Toils_Mizu.FinishRecipeAndStartStoringProduct(this.FinishAction);
        }

        protected abstract void SetFailCondition();

        protected abstract Thing FinishAction();
    }
}
