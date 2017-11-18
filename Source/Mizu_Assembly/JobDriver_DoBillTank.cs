using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DoBillTank : JobDriver_DoBill
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toils = base.MakeNewToils().ToList();
            Toil lastToil = new Toil();
            lastToil.initAction = () =>
            {
                Building_WaterNetWorkTable workTable = this.TargetThingA as Building_WaterNetWorkTable;
                if (workTable == null)
                {
                    Log.Error("workTable is null");
                }
                FaucetRecipeDef recipe = this.job.bill.recipe as FaucetRecipeDef;
                CompWaterNetTank comp = workTable.GetComp<CompWaterNetTank>();
                if (comp == null)
                {
                    Log.Error("comp is null");
                }
                comp.DrawWaterVolume(recipe.needWaterVolume);
            };
            lastToil.defaultCompleteMode = ToilCompleteMode.Instant;

            toils.Insert(12, lastToil);

            return toils.AsEnumerable();
        }

        //public override string GetReport()
        //{
        //    return base.GetReport() + "!!!" + this.CurToilIndex.ToString();
        //}
    }
}
