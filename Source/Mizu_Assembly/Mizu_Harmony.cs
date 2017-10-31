using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.himesama.mizumod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(CaravanPawnsNeedsUtility))]
    [HarmonyPatch("TrySatisfyPawnNeeds")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Caravan) })]
    class CaravanPawnsNeedsUtility_TrySatisfyPawnNeeds_Patch
    {
        [HarmonyPostfix]
        static void CaravanPawnsNeedsUtility_AddSatisfyNeedWater(Pawn pawn, Caravan caravan)
        {
            Need_Water need_water = pawn.needs.water();
            if (need_water != null)
            {
                if (need_water.CurCategory < ThirstCategory.Thirsty)
                {
                    return;
                }
                Thing thing;
                Pawn pawn2;
                //if (VirtualPlantsUtility.CanEatVirtualPlantsNow(pawn))
                //{
                //    VirtualPlantsUtility.EatVirtualPlants(pawn);
                //}
                if (pawn.Tile >= 0 && !pawn.Dead && pawn.IsWorldPawn() && MizuUtility.CanDrinkTerrain(pawn))
                {
                    need_water.CurLevel += Rand.Range(0.2f, 0.4f);
                }
                else if (MizuCaravanUtility.TryGetBestWater(caravan, pawn, out thing, out pawn2))
                {
                    need_water.CurLevel += MizuUtility.GetWater(pawn, thing, need_water.WaterWanted);
                    if (thing.Destroyed)
                    {
                        if (pawn2 != null)
                        {
                            pawn2.inventory.innerContainer.Remove(thing);
                            caravan.RecacheImmobilizedNow();
                            caravan.RecacheDaysWorthOfFood();
                        }
                        if (!MizuCaravanUtility.TryGetBestWater(caravan, pawn, out thing, out pawn2))
                        {
                            Messages.Message(MizuStrings.MessageCaravanRunOutOfWater.Translate(new object[]
                            {
                                caravan.LabelCap,
                                pawn.Label
                            }), caravan, MessageSound.SeriousAlert);
                        }
                    }
                }
            }
        }
    }

}
