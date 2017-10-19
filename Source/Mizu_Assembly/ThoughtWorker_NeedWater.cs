using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class ThoughtWorker_NeedWater : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.needs.water() == null)
            {
                return ThoughtState.Inactive;
            }
            switch (p.needs.water().CurCategory)
            {
                case ThirstCategory.Healthy:
                    return ThoughtState.Inactive;
                case ThirstCategory.Thirsty:
                    return ThoughtState.ActiveAtStage(0);
                case ThirstCategory.UrgentlyThirsty:
                    return ThoughtState.ActiveAtStage(1);
                case ThirstCategory.Dehydration:
                    {
                        Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Dehydration, false);
                        int num = (firstHediffOfDef != null) ? firstHediffOfDef.CurStageIndex : 0;
                        return ThoughtState.ActiveAtStage(2 + num);
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
