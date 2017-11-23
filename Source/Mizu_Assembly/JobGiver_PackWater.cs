using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobGiver_PackWater : ThinkNode_JobGiver
    {
        private const float MinWaterAmount = 0.3f;

        private const float MinWaterPerColonistToDo = 1.5f;

        public const WaterPreferability MinWaterPreferability = WaterPreferability.SeaWater;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.inventory == null)
            {
                return null;
            }
            ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Thing thing = innerContainer[i];
                CompWater comp = thing.TryGetComp<CompWater>();

                if (comp != null && comp.WaterAmount > MinWaterAmount && comp.WaterPreferability >= MinWaterPreferability)
                {
                    return null;
                }
            }

            if (pawn.Map.resourceCounter.TotalWater() < (float)pawn.Map.mapPawns.ColonistsSpawnedCount * MinWaterPerColonistToDo)
            {
                return null;
            }
            Thing thing2 = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 20f, delegate (Thing t)
            {
                CompWater comp2 = t.TryGetComp<CompWater>();
                if (comp2 == null || comp2.WaterAmount < MinWaterAmount || t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, false) || !t.IsSociallyProper(pawn))
                {
                    return false;
                }
                List<ThoughtDef> list = MizuUtility.ThoughtsFromGettingWater(pawn, t);
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].stages[0].baseMoodEffect < 0f)
                    {
                        return false;
                    }
                }
                return true;
            }, (Thing x) => MizuUtility.GetWaterItemScore(pawn, x, 0f, false), 24, 30);
            if (thing2 == null)
            {
                return null;
            }
            return new Job(JobDefOf.TakeInventory, thing2)
            {
                count = 1
            };
        }
    }
}
