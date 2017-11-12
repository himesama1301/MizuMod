using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DoBillFaucet : JobDriver_DoBill
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toils = base.MakeNewToils().ToList();
            Toil lastToil = new Toil();
            lastToil.initAction = () =>
            {
                CompWaterNet comp = this.TargetThingA.TryGetComp<CompWaterNet>();
                if (comp == null)
                {
                    Log.Error("comp is null");
                }
                FaucetRecipeDef recipe = this.CurJob.bill.recipe as FaucetRecipeDef;
                comp.WaterNet.DrawWaterVolume(recipe.needWaterVolume);
            };
            lastToil.defaultCompleteMode = ToilCompleteMode.Instant;

            toils.Insert(15, lastToil);

            return toils.AsEnumerable();
        }
    }
}
