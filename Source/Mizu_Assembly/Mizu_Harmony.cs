using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;
using Verse.AI;
using System.Reflection.Emit;

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
                            Messages.Message(string.Format(MizuStrings.MessageCaravanRunOutOfWater, caravan.LabelCap, pawn.Label), caravan, MessageSound.SeriousAlert);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Gear))]
    [HarmonyPatch("DrawThingRow")]
    class ITab_Pawn_Gear_DrawThingRow
    {
        static void Postfix(ref float y, float width, Thing thing, bool showDropButtonIfPrisoner = false)
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

    [HarmonyPatch(typeof(Dialog_FormCaravan))]
    [HarmonyPatch("DoWindowContents")]
    class Dialog_FormCaravan_DoWindowContents
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                //if (codes[i].operand != null)
                //{
                //    Log.Message(string.Format("{0}, {1}, {2}", codes[i].opcode.ToString(), codes[i].operand.GetType().ToString(), codes[i].operand.ToString()));
                //}
                //else
                //{
                //    Log.Message(string.Format("{0}", codes[i].opcode.ToString()));
                //}
                if (codes[i].opcode == OpCodes.Call)
                {
                    if (codes[i].operand.ToString().Contains("DrawDaysWorthOfFoodInfo"))
                    {
                        insert_index = i;
                        //Log.Message("type  = " + codes[i].operand.GetType().ToString());
                        //Log.Message("val   = " + codes[i].operand.ToString());
                        //Log.Message("count = " + codes[i].labels.Count.ToString());
                        //for (int j = 0; j < codes[i].labels.Count; j++)
                        //{
                        //    Log.Message(string.Format("label[{0}] = {1}", j, codes[i].labels[j].ToString()));
                        //}
                    }
                }
            }

            if (insert_index > -1)
            {
                List<CodeInstruction> new_codes = new List<CodeInstruction>();
                new_codes.Add(new CodeInstruction(OpCodes.Ldloca_S, 2));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rect), "get_x")));
                new_codes.Add(new CodeInstruction(OpCodes.Ldloca_S, 2));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rect), "get_y")));
                new_codes.Add(new CodeInstruction(OpCodes.Ldc_R4, 38.0f));
                new_codes.Add(new CodeInstruction(OpCodes.Add));
                new_codes.Add(new CodeInstruction(OpCodes.Ldloca_S, 2));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rect), "get_width")));
                new_codes.Add(new CodeInstruction(OpCodes.Ldloca_S, 2));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rect), "get_height")));
                new_codes.Add(new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Rect), new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) })));
                new_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.DaysWorthOfWater))));
                new_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                new_codes.Add(new CodeInstruction(OpCodes.Ldc_R4, float.MaxValue));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.DrawDaysWorthOfWaterInfo))));

                codes.InsertRange(insert_index + 1, new_codes);
            }
            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan))]
    [HarmonyPatch("CountToTransferChanged")]
    class Dialog_FormCaravan_CountToTransferChanged
    {
        static void Postfix()
        {
            MizuCaravanUtility.daysWorthOfWaterDirty = true;
        }
    }

}
