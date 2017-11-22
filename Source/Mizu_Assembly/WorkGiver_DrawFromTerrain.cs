using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_DrawFromTerrain : WorkGiver_DoBill
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

                if (bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != this.def.workType) continue;
                if (Find.TickManager.TicksGame < bill.lastIngredientSearchFailTicks + ReCheckFailedBillTicksRange.RandomInRange && FloatMenuMakerMap.makingFor != pawn) continue;

                bill.lastIngredientSearchFailTicks = 0;

                if (!bill.ShouldDoNow()) continue;
                if (!bill.PawnAllowedToStartAnew(pawn)) continue;
                if (!bill.recipe.PawnSatisfiesSkillRequirements(pawn))
                {
                    JobFailReason.Is(MissingSkillTranslated);
                    continue;
                }

                var getWaterRecipe = bill.recipe as GetWaterRecipeDef;
                if (getWaterRecipe == null || getWaterRecipe.needWaterTerrainTypes == null) continue;

                var thing = giver as Thing;
                if (getWaterRecipe.needWaterTerrainTypes.Contains(thing.Map.terrainGrid.TerrainAt(thing.Position).GetWaterTerrainType()))
                {
                    return new Job(MizuDef.Job_DrawFromTerrain, thing)
                    {
                        bill = bill,
                    };
                }

                if (FloatMenuMakerMap.makingFor != pawn)
                {
                    bill.lastIngredientSearchFailTicks = Find.TickManager.TicksGame;
                }
                else
                {
                    JobFailReason.Is(MissingMaterialsTranslated);
                }
            }
            return null;
        }
    }
}
