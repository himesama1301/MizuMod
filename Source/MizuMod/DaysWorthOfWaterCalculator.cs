using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MizuMod
{
    public static class DaysWorthOfWaterCalculator
    {
        public static float ApproxDaysWorthOfWater(List<TransferableOneWay> transferables, IgnorePawnsInventoryMode ignoreInventory)
        {
            List<ThingCount> tmpThingCounts = new List<ThingCount>();
            List<Pawn> tmpPawns = new List<Pawn>();
            tmpThingCounts.Clear();
            tmpPawns.Clear();

            for (int i = 0; i < transferables.Count; i++)
            {
                TransferableOneWay transferableOneWay = transferables[i];
                if (transferableOneWay.HasAnyThing)
                {
                    if (transferableOneWay.AnyThing is Pawn)
                    {
                        for (int j = 0; j < transferableOneWay.CountToTransfer; j++)
                        {
                            tmpPawns.Add((Pawn)transferableOneWay.things[j]);
                        }
                    }
                    else
                    {
                        tmpThingCounts.Add(new ThingCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
                    }
                }
            }
            float result = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(tmpPawns, tmpThingCounts, ignoreInventory);
            tmpThingCounts.Clear();
            tmpPawns.Clear();
            return result;
        }

        private static bool AnyNonTerrainDrinkingPawn(List<Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].needs.mood != null && pawns[i].needs.water() != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static float ApproxDaysWorthOfWater(Caravan caravan)
        {
            return DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(caravan.PawnsListForReading, null, IgnorePawnsInventoryMode.DontIgnore);
        }

        private static float ApproxDaysWorthOfWater(List<Pawn> pawns, List<ThingCount> extraWater, IgnorePawnsInventoryMode ignoreInventory)
        {
            if (!DaysWorthOfWaterCalculator.AnyNonTerrainDrinkingPawn(pawns))
            {
                return 1000f;
            }
            List<ThingCount> tmpWater = new List<ThingCount>();
            tmpWater.Clear();
            if (extraWater != null)
            {
                for (int i = 0; i < extraWater.Count; i++)
                {
                    bool canGetWater = false;
                    for (int j = 0; j < extraWater[i].ThingDef.comps.Count; j++)
                    {
                        var compprop = extraWater[i].ThingDef.comps[j] as CompProperties_WaterSource;
                        if (compprop != null && compprop.sourceType == CompProperties_WaterSource.SourceType.Item && compprop.waterAmount > 0.0f)
                        {
                            canGetWater = true;
                            break;
                        }
                    }
                    if (canGetWater && extraWater[i].Count > 0)
                    {
                        tmpWater.Add(extraWater[i]);
                    }
                }
            }
            for (int j = 0; j < pawns.Count; j++)
            {
                if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawns[j], ignoreInventory))
                {
                    ThingOwner<Thing> innerContainer = pawns[j].inventory.innerContainer;
                    for (int k = 0; k < innerContainer.Count; k++)
                    {
                        if (innerContainer[k].CanGetWater())
                        {
                            tmpWater.Add(new ThingCount(innerContainer[k].def, innerContainer[k].stackCount));
                        }
                    }
                }
            }
            if (!tmpWater.Any<ThingCount>())
            {
                return 0f;
            }
            List<float> tmpDaysWorthOfFoodPerPawn = new List<float>();
            List<bool> tmpAnyFoodLeftIngestibleByPawn = new List<bool>();
            tmpDaysWorthOfFoodPerPawn.Clear();
            tmpAnyFoodLeftIngestibleByPawn.Clear();
            for (int l = 0; l < pawns.Count; l++)
            {
                tmpDaysWorthOfFoodPerPawn.Add(0f);
                tmpAnyFoodLeftIngestibleByPawn.Add(true);
            }
            float num = 0f;
            bool flag;
            do
            {
                flag = false;
                for (int m = 0; m < pawns.Count; m++)
                {
                    Pawn pawn = pawns[m];
                    if (tmpAnyFoodLeftIngestibleByPawn[m])
                    {
                        do
                        {
                            int num2 = DaysWorthOfWaterCalculator.BestEverGetWaterIndexFor(pawns[m], tmpWater);
                            if (num2 < 0)
                            {
                                tmpAnyFoodLeftIngestibleByPawn[m] = false;
                                break;
                            }
                            CompProperties_WaterSource compprop = null;
                            for (int x = 0; x < tmpWater[num2].ThingDef.comps.Count; x++)
                            {
                                compprop = tmpWater[num2].ThingDef.comps[x] as CompProperties_WaterSource;
                                if (compprop != null && compprop.sourceType == CompProperties_WaterSource.SourceType.Item)
                                {
                                    break;
                                }
                            }
                            if (compprop == null)
                            {
                                tmpAnyFoodLeftIngestibleByPawn[m] = false;
                                break;
                            }
                            Need_Water need_water = pawn.needs.water();
                            if (need_water == null)
                            {
                                tmpAnyFoodLeftIngestibleByPawn[m] = false;
                                break;
                            }
                            float num3 = Mathf.Min(compprop.waterAmount, need_water.WaterAmountBetweenThirstyAndHealthy);
                            float num4 = num3 / need_water.WaterAmountBetweenThirstyAndHealthy * (float)need_water.TicksUntilThirstyWhenHealthy / 60000f;
                            tmpDaysWorthOfFoodPerPawn[m] = tmpDaysWorthOfFoodPerPawn[m] + num4;
                            tmpWater[num2] = tmpWater[num2].WithCount(tmpWater[num2].Count - 1);
                            flag = true;
                        }
                        while (tmpDaysWorthOfFoodPerPawn[m] < num);
                        num = Mathf.Max(num, tmpDaysWorthOfFoodPerPawn[m]);
                    }
                }
            }
            while (flag);
            float num6 = 1000f;
            for (int n = 0; n < pawns.Count; n++)
            {
                num6 = Mathf.Min(num6, tmpDaysWorthOfFoodPerPawn[n]);
            }
            return num6;
        }

        private static int BestEverGetWaterIndexFor(Pawn pawn, List<ThingCount> water)
        {
            int num = -1;
            float num2 = 0f;
            for (int i = 0; i < water.Count; i++)
            {
                if (water[i].Count > 0)
                {
                    ThingDef thingDef = water[i].ThingDef;
                    if (MizuCaravanUtility.CanEverGetWater(thingDef, pawn))
                    {
                        float waterScore = MizuCaravanUtility.GetWaterScore(thingDef, pawn);
                        if (num < 0 || waterScore > num2)
                        {
                            num = i;
                            num2 = waterScore;
                        }
                    }
                }
            }
            return num;
        }
    }
}
