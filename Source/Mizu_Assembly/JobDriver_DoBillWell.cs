using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DoBillWell : JobDriver_DoBill
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> toils = base.MakeNewToils().ToList();
            Toil lastToil = new Toil();
            lastToil.initAction = () =>
            {
                var waterGrid = this.TargetThingA.Map.GetComponent<MapComponent_ShallowWaterGrid>();
                var pool = waterGrid.GetPool(this.TargetThingA.Map.cellIndices.CellToIndex(this.TargetThingA.Position));
                var recipe = this.job.bill.recipe as FaucetRecipeDef;

                pool.CurrentWaterVolume = Mathf.Max(0, pool.CurrentWaterVolume - recipe.needWaterVolume);
                waterGrid.SetDirty();
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
