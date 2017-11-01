using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;
using Verse.AI;

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
        static void Postfix(Pawn pawn, Caravan caravan)
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

    [HarmonyPatch(typeof(ITab_Pawn_Gear))]
    [HarmonyPatch("DrawThingRow")]
    //[HarmonyPatch(new Type[] { typeof(float), typeof(float), typeof(Thing), typeof(bool) })]
    class ITab_Pawn_Gear_DrawThingRow
    {
        static void Postfix(ref float y, float width, Thing thing, bool showDropButtonIfPrisoner = false)
        //static bool Prefix(ref float y, ref float width, Thing thing, bool showDropButtonIfPrisoner)
        {
            float width2 = width - 72f;
            float y2 = y - 28f;

            Pawn selPawn = Find.Selector.SingleSelectedThing as Pawn;
            Corpse selCorpse = Find.Selector.SingleSelectedThing as Corpse;
            if (selPawn == null && selCorpse != null)
            {
                selPawn = selCorpse.InnerPawn;
            }
            if (selPawn == null)
            {
                return;
            }

            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed)
            {
                return;
            }
            if (thing.CanGetWater() && thing.CanDrinkWaterNow())
            {
                Rect rect3 = new Rect(width2 - 24f, y2, 24f, 24f);
                //TooltipHandler.TipRegion(rect3, "ConsumeThing".Translate(new object[]
                //{
                //        thing.LabelNoCount
                //}));
                TooltipHandler.TipRegion(rect3, string.Format(MizuStrings.FloatMenuGetWater, thing.LabelNoCount));
                if (Widgets.ButtonImage(rect3, ContentFinder<Texture2D>.Get("UI/Buttons/Ingest", true)))
                {
                    SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                    //this.InterfaceIngest(thing);
                    Job job = new Job(MizuDef.Job_DrinkWater, thing);
                    //job.count = Mathf.Min(thing.stackCount, thing.def.ingestible.maxNumToIngestAtOnce);
                    job.count = Mathf.Min(thing.stackCount, 1);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                }
            }
        //    return true;
        }
    }
}
