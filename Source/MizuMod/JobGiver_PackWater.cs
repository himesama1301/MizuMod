using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobGiver_PackWater : ThinkNode_JobGiver
    {
        private const float NeedTotalWaterAmount = 1.0f;

        private const float MinWaterPerColonistToDo = 1.5f;

        private const int ContinuousPackIntervalTick = 150;

        public const WaterPreferability MinWaterPreferability = WaterPreferability.SeaWater;

        protected override Job TryGiveJob(Pawn pawn)
        {
            // 所持品インスタンスがない
            if (pawn.inventory == null) return null;

            // 水分要求がない
            Need_Water need_water = pawn.needs.water();
            if (need_water == null) return null;

            // 既に条件を満たしたアイテムを持っているか？
            foreach (var thing in pawn.inventory.innerContainer)
            {
                var comp = thing.TryGetComp<CompWaterSource>();
                if (comp == null) continue;
                if (!comp.IsWaterSource) continue;
                if (comp.SourceType != CompProperties_WaterSource.SourceType.Item) continue;
                if (comp.WaterAmount < Need_Water.MinWaterAmountPerOneItem) continue;
                if (MizuDef.Dic_WaterTypeDef[comp.WaterType].waterPreferability < MinWaterPreferability) continue;

                return null;
            }

            // マップ中の水アイテムの合計水分量が、最低限必要とされる水の量(×入植者の人数)以下しかなければ、
            // 個人の所持品には入れない
            if (pawn.Map.resourceCounter.TotalWater() < (float)pawn.Map.mapPawns.ColonistsSpawnedCount * MinWaterPerColonistToDo) return null;

            var waterThing = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn),
                20f,
                (t) => {
                    var comp = t.TryGetComp<CompWaterSource>();
                    if (comp == null) return false; // 水源でないもの×
                    if (comp.SourceType != CompProperties_WaterSource.SourceType.Item) return false; // 水アイテムではないもの×
                    if (comp.WaterAmount < Need_Water.MinWaterAmountPerOneItem) return false;  // 最低水分量を満たしていないもの×
                    if (t.IsForbidden(pawn)) return false;  // 禁止されている×
                    if (!pawn.CanReserve(t)) return false;  // 予約不可能×
                    if (!t.IsSociallyProper(pawn)) return false;  // 囚人部屋の物×

                    return true;
                }, (x) => MizuUtility.GetWaterItemScore(pawn, x, 0f, true)  // スコアの高いものが優先？
            );

            if (waterThing == null) return null;

            return new Job(JobDefOf.TakeInventory, waterThing)
            {
                count = Mathf.Min(MizuUtility.StackCountForWater(waterThing, NeedTotalWaterAmount), waterThing.stackCount)
            };
        }
    }
}
