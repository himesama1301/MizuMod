using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public abstract class WorkGiver_DrawWater : WorkGiver_DoBill
    {
        private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);
        private static string MissingSkillTranslated = "MissingSkill".Translate();
        private static string MissingMaterialsTranslated = "MissingMaterials".Translate();

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            IBillGiver billGiver = thing as IBillGiver;
            if (billGiver == null) return null;
            if (this.def.fixedBillGiverDefs == null || !this.def.fixedBillGiverDefs.Contains(thing.def)) return null;

            if (!billGiver.CurrentlyUsableForBills()) return null;

            LocalTargetInfo target = thing;
            if (!pawn.CanReserve(target, 1, -1, null, forced)) return null;
            if (thing.IsBurning()) return null;
            if (thing.IsForbidden(pawn)) return null;

            billGiver.BillStack.RemoveIncompletableBills();
            return this.StartBillJob(pawn, billGiver);
        }

        private Job StartBillJob(Pawn pawn, IBillGiver giver)
        {
            for (int i = 0; i < giver.BillStack.Count; i++)
            {
                Bill bill = giver.BillStack[i];

                // レシピが要求する仕事の種類と、WorkGiver側の仕事の種類があっているかチェック
                if (bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != this.def.workType) continue;

                // 再チェック時間を過ぎていないかチェック(右クリックメニューからの場合は例外)
                if (Find.TickManager.TicksGame < bill.lastIngredientSearchFailTicks + ReCheckFailedBillTicksRange.RandomInRange && FloatMenuMakerMap.makingFor != pawn) continue;

                // チェック時間更新
                bill.lastIngredientSearchFailTicks = 0;

                // 今それをする必要があるか
                if (!bill.ShouldDoNow()) continue;

                // そのポーンが新規に仕事をできるか
                if (!bill.PawnAllowedToStartAnew(pawn)) continue;

                // レシピに必要なスキルを持っているか
                if (!bill.recipe.PawnSatisfiesSkillRequirements(pawn))
                {
                    JobFailReason.Is(MissingSkillTranslated);
                    continue;
                }

                // 水チェック
                var getWaterRecipe = bill.recipe as GetWaterRecipeDef;
                if (getWaterRecipe == null) return null;

                // 必要な水の条件を満たしているか
                var job = this.CreateJobIfSatisfiedWaterCondition(giver, getWaterRecipe, bill);
                if (job != null)　return job;

                if (FloatMenuMakerMap.makingFor != pawn)
                {
                    // 右クリックメニューからでなく、ジョブを開始できなかったらチェック時間を更新
                    bill.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                }
                else
                {
                    // 右クリックメニューからの場合、できなかった理由を表示（素材不足）
                    JobFailReason.Is(MissingMaterialsTranslated);
                }
            }
            return null;
        }

        protected abstract Job CreateJobIfSatisfiedWaterCondition(IBillGiver giver, GetWaterRecipeDef recipe, Bill bill);
    }
}
