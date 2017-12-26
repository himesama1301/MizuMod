using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrinkWaterFromBuilding : JobDriver
    {
        private TargetIndex BuildingIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.GetTarget(BuildingIndex), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var t = this.job.GetTarget(BuildingIndex).Thing;

            // 設備まで移動
            if (t.def.hasInteractionCell)
            {
                // 使用場所がある
                yield return Toils_Goto.GotoThing(BuildingIndex, PathEndMode.InteractionCell);
            }
            else
            {
                // 使用場所がない
                yield return Toils_Goto.GotoThing(BuildingIndex, PathEndMode.ClosestTouch);
            }

            // 水を飲む
            yield return Toils_Mizu.DrinkFromBuilding(BuildingIndex);
        }
    }
}
