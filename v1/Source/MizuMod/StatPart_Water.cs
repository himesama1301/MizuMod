using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class StatPart_Water : StatPart
    {
        private float factorHealthy = 1f;
        private float factorSlightlyThirsty = 1f;
        private float factorThirsty = 1f;
        private float factorUrgentThirsty = 1f;
        private float factorDehydration = 1f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            // 水分要求を持ったポーンであるかどうかチェック
            if (!req.HasThing) return;
            var pawn = req.Thing as Pawn;
            if (pawn == null || pawn.needs.water() == null) return;

            val *= this.WaterMultiplier(pawn.needs.water().CurCategory);
        }

        public override string ExplanationPart(StatRequest req)
        {
            // 水分要求を持ったポーンであるかどうかチェック
            if (!req.HasThing) return null;
            var pawn = req.Thing as Pawn;
            if (pawn == null || pawn.needs.water() == null) return null;

            return GetLabel(pawn.needs.water().CurCategory) + ": x" + this.WaterMultiplier(pawn.needs.water().CurCategory).ToStringPercent();
        }

        private float WaterMultiplier(ThirstCategory thirst)
        {
            switch (thirst)
            {
                case ThirstCategory.Healthy:
                    return this.factorHealthy;
                case ThirstCategory.SlightlyThirsty:
                    return this.factorSlightlyThirsty;
                case ThirstCategory.Thirsty:
                    return this.factorThirsty;
                case ThirstCategory.UrgentlyThirsty:
                    return this.factorUrgentThirsty;
                case ThirstCategory.Dehydration:
                    return this.factorDehydration;
                default:
                    throw new InvalidOperationException();
            }
        }

        private string GetLabel(ThirstCategory thirst)
        {
            switch (thirst)
            {
                case ThirstCategory.Healthy:
                    return "MizuThirstLevel_Healthy".Translate();
                case ThirstCategory.SlightlyThirsty:
                    return "MizuThirstLevel_SlightlyThirsty".Translate();
                case ThirstCategory.Thirsty:
                    return "MizuThirstLevel_Thirsty".Translate();
                case ThirstCategory.UrgentlyThirsty:
                    return "MizuThirstLevel_UrgentlyThirsty".Translate();
                case ThirstCategory.Dehydration:
                    return "MizuThirstLevel_Dehydration".Translate();
                default:
                    throw new InvalidOperationException();
            }
        }

    }
}
