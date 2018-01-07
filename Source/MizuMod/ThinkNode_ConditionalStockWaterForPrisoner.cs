using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class ThinkNode_ConditionalStockWaterForPrisoner : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            // そもそもポーンがいる場所は囚人部屋ではない
            if (!pawn.Position.IsInPrisonCell(pawn.Map)) return true;

            // 現在の部屋を取得
            var myRoom = pawn.Position.GetRoom(pawn.Map);
            if (myRoom == null) return true;

            // 部屋にいる人数を取得
            int prisonerNum = myRoom.Owners.Count();

            int waterItemNum = 0;
            foreach (var t in pawn.Map.listerThings.AllThings)
            {
                // 食べられるものは対象外
                if (t.def.IsIngestible) continue;

                // 十分な水分を持った水アイテムでなければ対象外
                var comp = t.TryGetComp<CompWaterSource>();
                if (comp == null || comp.SourceType != CompProperties_WaterSource.SourceType.Item || comp.WaterAmount < Need_Water.MinWaterAmountPerOneDrink) continue;

                // 同じ部屋にあればカウント
                if (t.Position.GetRoom(t.Map) == myRoom) waterItemNum++;
            }

            // 水アイテムの個数が囚人よりも少なければ、不足していると考える
            return waterItemNum >= prisonerNum;
        }
    }
}
