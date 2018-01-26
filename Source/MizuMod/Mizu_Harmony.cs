using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;
using Verse.AI;

using Harmony;

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

    // キャラバン移動中に水分要求を補充する行動を追加
    [HarmonyPatch(typeof(CaravanPawnsNeedsUtility))]
    [HarmonyPatch("TrySatisfyPawnNeeds")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Caravan) })]
    class CaravanPawnsNeedsUtility_TrySatisfyPawnNeeds_Patch
    {
        static void Postfix(Pawn pawn, Caravan caravan)
        {
            // キャラバンのポーンの水分要求の処理
            if (pawn.needs == null) return;

            Need_Water need_water = pawn.needs.water();
            if (need_water == null) return;

            // 喉が渇いてない場合は飲まない
            if (need_water.CurCategory <= ThirstCategory.Healthy) return;

            // タイルが0以上(?)、死んでない、ローカルではなく惑星マップ上にいる(キャラバンしてる)、そのポーンが地形から水を飲める(心情がある/ない、脱水症状まで進んでいる/いない、など)
            if (pawn.Tile >= 0 && !pawn.Dead && pawn.IsWorldPawn() && pawn.CanDrinkFromTerrain())
            {
                WaterTerrainType drankTerrainType = caravan.GetWaterTerrainType();

                // 水を飲めない場所
                if (drankTerrainType == WaterTerrainType.NoWater) return;

                // 地形から水を飲む
                need_water.CurLevel = 1.0f;

                if (drankTerrainType == WaterTerrainType.SeaWater)
                {
                    // 海水の場合の健康状態悪化
                    pawn.health.AddHediff(HediffMaker.MakeHediff(MizuDef.Hediff_DrankSeaWater, pawn));
                }

                // 心情要求がなければここで終了
                if (pawn.needs.mood == null) return;

                // 直接水を飲んだ心情付加
                if (pawn.CanManipulate())
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(MizuDef.Thought_DrankScoopedWater);
                }
                else
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(MizuDef.Thought_SippedWaterLikeBeast);
                }

                // キャラバンのいる地形に応じた心情を付加
                ThoughtDef thoughtDef = MizuUtility.GetThoughtDefFromTerrainType(drankTerrainType);
                if (thoughtDef != null)
                {
                    // 水の種類による心情
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
                }

                return;
            }

            // 水アイテムを探す
            Thing waterThing;
            Pawn inventoryPawn;

            // アイテムが見つからない
            if (!MizuCaravanUtility.TryGetBestWater(caravan, pawn, out waterThing, out inventoryPawn)) return;

            // アイテムに応じた水分を摂取＆心情変化＆健康変化
            float numWater = MizuUtility.GetWater(pawn, waterThing, need_water.WaterWanted, false);
            need_water.CurLevel += numWater;
            pawn.records.AddTo(MizuDef.Record_WaterDrank, numWater);

            // 水アイテムが消滅していない場合(スタックの一部だけ消費した場合等)はここで終了
            if (!waterThing.Destroyed) return;

            if (inventoryPawn != null)
            {
                // 誰かの所持品にあった水スタックを飲みきったのであれば、所持品欄から除去
                inventoryPawn.inventory.innerContainer.Remove(waterThing);

                // 移動不可状態を一旦リセット(して再計算させる？)
                caravan.RecacheImmobilizedNow();

                // 水の残量再計算フラグON
                MizuCaravanUtility.daysWorthOfWaterDirty = true;
            }

            if (!MizuCaravanUtility.TryGetBestWater(caravan, pawn, out waterThing, out inventoryPawn))
            {
                // 飲んだことにより水がなくなったら警告を出す
                Messages.Message(string.Format(MizuStrings.MessageCaravanRunOutOfWater, caravan.LabelCap, pawn.Label), caravan, MessageTypeDefOf.ThreatBig);
            }
        }
    }

    // 所持品欄の水アイテムに「水を飲む」ボタンを追加
    [HarmonyPatch(typeof(ITab_Pawn_Gear))]
    [HarmonyPatch("DrawThingRow")]
    class ITab_Pawn_Gear_DrawThingRow
    {
        static void Postfix(ref float y, float width, Thing thing, bool inventory = false)
        {
            // 所持品に含まれる水アイテムに、所持品を直接摂取するボタンを増やす
            // yは次の行のtop上端座標、widthは右端座標

            // db=DrinkButton
            const float dbWidth = 24f;
            const float dbHeight = 24f;

            // 1アイコンにつき幅24f
            // 情報アイコン、ドロップアイコン、食べるアイコンを考慮すれば3個
            // 食べると飲むを分離できたはずなので2で良い
            float dbRight = width - 24f * 2;
            float dbTop = y - 28f;

            Pawn selPawn = Find.Selector.SingleSelectedThing as Pawn;
            Corpse selCorpse = Find.Selector.SingleSelectedThing as Corpse;
            if (selPawn == null && selCorpse != null)
            {
                selPawn = selCorpse.InnerPawn;
            }

            // ポーンデータがないなら終了
            if (selPawn == null) return;

            // プレイヤーが操作するポーンではない、またはそのポーンは倒れている→終了
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed) return;

            // 水として飲めないアイテムなら終了
            if (!thing.CanGetWater() || !thing.CanDrinkWaterNow()) return;

            // 水アイテムでなかったり、食べられるものは能動的に飲むことはできない
            var comp = thing.TryGetComp<CompWaterSource>();
            if (comp == null || comp.SourceType != CompProperties_WaterSource.SourceType.Item || thing.IsIngestibleFor(selPawn)) return;

            // ツールチップとボタンを追加
            Rect dbRect = new Rect(dbRight - dbWidth, dbTop, dbWidth, dbHeight);
            TooltipHandler.TipRegion(dbRect, string.Format(MizuStrings.FloatMenuGetWater, thing.LabelNoCount));
            if (Widgets.ButtonImage(dbRect, MizuGraphics.Texture_ButtonIngest))
            {
                SoundDefOf.TickHigh.PlayOneShotOnCamera(null);
                Job job = new Job(MizuDef.Job_DrinkWater, thing)
                {
                    count = MizuUtility.WillGetStackCountOf(selPawn, thing)
                };
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
            }
        }
    }

    // キャラバンの荷物選択時、右上に現在の水分総量を表示させる処理を追加
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

                // 食料表示処理のコード位置を頼りに挿入すべき場所を探す
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("DrawDaysWorthOfFoodInfo"))
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

    // 水アイテム不足で出発しようとしたとき、警告ダイアログを出す
    // 実際の処理は、食料不足の警告ダイアログに便乗する形
    [HarmonyPatch(typeof(Dialog_FormCaravan))]
    [HarmonyPatch("DoBottomButtons")]
    class Dialog_FormCaravan_DoBottomButtons
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int key_index = -1;
            int insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);

            // 処理を挿入すべき場所の近くにあった文字列のnullチェック処理を手掛かりにする(かなり強引なやり方)
            for (int i = 0; i < codes.Count; i++)
            {
                if (key_index < 0 && codes[i].opcode == OpCodes.Ldstr && codes[i].operand.ToString() == "CaravanFoodWillRotSoonWarningDialog")
                {
                    key_index = i;
                }
                if (key_index >= 0 && insert_index < 0 && codes[i].opcode == OpCodes.Ldarg_0)
                {
                    insert_index = i + 1;
                }
                //if (found_ldnull < 2 && codes[i].opcode == OpCodes.Ldnull)
                //{
                //    replace_index = i;
                //    found_ldnull++;
                //}
                //if (found_ldnull >= 2 && codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("NullOrEmpty"))
                //{
                //    insert_index = i;
                //}
            }

            if (key_index > -1 && insert_index > -1)
            {
                //List<CodeInstruction> replace_codes = new List<CodeInstruction>();
                //replace_codes.Add(new CodeInstruction(OpCodes.Ldstr, string.Empty));
                //codes[replace_index].opcode = OpCodes.Nop;
                //codes.InsertRange(replace_index + 1, replace_codes);
                //insert_index += replace_codes.Count;

                List<CodeInstruction> insert_codes = new List<CodeInstruction>();
                //codes[insert_index - 1].opcode = OpCodes.Nop;
                //insert_codes.Add(new CodeInstruction(OpCodes.Ldstr, "{0}{1}"));

                //insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_2));
                //insert_codes.Add(new CodeInstruction(OpCodes.Newarr, typeof(object)));

                //insert_codes.Add(new CodeInstruction(OpCodes.Dup));
                //insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                //insert_codes.Add(new CodeInstruction(OpCodes.Ldloc_1));
                //insert_codes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

                //insert_codes.Add(new CodeInstruction(OpCodes.Dup));
                //insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                //insert_codes.Add(new CodeInstruction(OpCodes.Ldstr, "\n\n uhawww okwww"));
                //insert_codes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

                //insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Format), new Type[] { typeof(string), typeof(object[]) })));
                //insert_codes.Add(new CodeInstruction(OpCodes.Stloc_1));

                insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                insert_codes.Add(new CodeInstruction(OpCodes.Ldloc_S, 4));
                insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.AddWaterWarningString), new Type[] { typeof(Dialog_FormCaravan), typeof(List<string>) })));
                //insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));

                codes.InsertRange(insert_index, insert_codes);
            }
            return codes.AsEnumerable();
        }
    }

    // キャラバン編成画面で積荷の内容が変化したときに、水の総量再計算フラグを立てる処理を追加
    [HarmonyPatch(typeof(Dialog_FormCaravan))]
    [HarmonyPatch("CountToTransferChanged")]
    class Dialog_FormCaravan_CountToTransferChanged
    {
        static void Postfix()
        {
            MizuCaravanUtility.daysWorthOfWaterDirty = true;
        }
    }

    // 惑星画面でキャラバンを選んだ時に左下に出てくる情報ウィンドウに積荷の水分量表示を追加
    [HarmonyPatch(typeof(Caravan))]
    [HarmonyPatch("GetInspectString")]
    class Caravan_GetInspectString
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool found_caravanDaysOfFood = false;
            int foundNum_Pop = 0;
            int insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (found_caravanDaysOfFood == false)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && !codes[i].operand.ToString().Contains("CaravanDaysOfFoodRot") && codes[i].operand.ToString().Contains("CaravanDaysOfFood"))
                    {
                        found_caravanDaysOfFood = true;
                        foundNum_Pop = 0;
                    }
                }
                else if (foundNum_Pop < 1)
                {
                    if (codes[i].opcode == OpCodes.Pop)
                    {
                        foundNum_Pop++;
                    }
                }
                else
                {
                    insert_index = i;
                    break;
                }
            }

            if (insert_index > -1)
            {
                List<CodeInstruction> new_codes = new List<CodeInstruction>();
                new_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                new_codes.Add(new CodeInstruction(OpCodes.Ldloc_0));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.AppendWaterWorthToCaravanInspectString))));

                codes.InsertRange(insert_index + 1, new_codes);
            }
            return codes.AsEnumerable();
        }
    }

    // ポーンの所持品生成処理(トレーダーがやってくるとき等)に水アイテムを持たせる処理を追加
    [HarmonyPatch(typeof(PawnInventoryGenerator))]
    [HarmonyPatch("GiveRandomFood")]
    class PawnInventoryGenerator_GiveRandomFood
    {
        static void Postfix(Pawn p)
        {
            if (p.kindDef.invNutrition <= 0.001f) return;

            ThingDef thingDef = null;
            if (p.kindDef.itemQuality > QualityCategory.Normal)
            {
                thingDef = MizuDef.Thing_ClearWater;
            }
            else if (p.kindDef.itemQuality == QualityCategory.Normal)
            {
                thingDef = MizuDef.Thing_NormalWater;
            }
            else
            {
                float value = Rand.Value;
                if (value < 0.7f)
                {
                    // 70%
                    thingDef = MizuDef.Thing_RawWater;
                }
                else if ((double)value < 0.9)
                {
                    // 20%
                    thingDef = MizuDef.Thing_MudWater;
                }
                else
                {
                    // 10%
                    thingDef = MizuDef.Thing_SeaWater;
                }
            }

            var compprop = thingDef.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null) return;

            Thing thing = ThingMaker.MakeThing(thingDef, null);
            thing.stackCount = GenMath.RoundRandom(p.kindDef.invNutrition / compprop.waterAmount);

            p.inventory.TryAddItemNotForSale(thing);
        }
    }

    // 水アイテムが囚人部屋に置いてあるとき、囚人用として扱う処理を追加
    [HarmonyPatch(typeof(HaulAIUtility))]
    [HarmonyPatch("PawnCanAutomaticallyHaulBasicChecks")]
    class HaulAIUtility_PawnCanAutomaticallyHaulBasicChecks
    {
        static void Postfix(ref bool __result, Pawn p, Thing t, bool forced)
        {
            if (t.CanGetWater() && !t.IsSociallyProper(p, false, true))
            {
                JobFailReason.Is("ReservedForPrisoners".Translate());
                __result = false;
                return;
            }
        }
    }

    // 食べ物に水分設定がある場合、食べた後に水分も回復する処理を追加
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Ingested")]
    class Thing_Ingested
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString().Contains("PostIngested"))
                {
                    insert_index = i - 1;
                    break;
                }
            }

            if (insert_index > -1)
            {
                List<CodeInstruction> insert_codes = new List<CodeInstruction>();
                codes[insert_index - 1].opcode = OpCodes.Nop;

                insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
                insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                insert_codes.Add(new CodeInstruction(OpCodes.Ldloc_S, 3));
                insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuUtility), nameof(MizuUtility.PrePostIngested), new Type[] { typeof(Pawn), typeof(Thing), typeof(int) })));

                insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));

                codes.InsertRange(insert_index, insert_codes);
            }
            return codes.AsEnumerable();
        }
    }

    // 包囲攻撃の仕送りで食料を送るときに一緒に水も送るようにする
    [HarmonyPatch(typeof(LordToil_Siege))]
    [HarmonyPatch("LordToilTick")]
    class LordToil_Siege_LordToilTick
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("DropSupplies"))
                {
                    insert_index = i + 1;
                }
            }

            if (insert_index > -1)
            {
                List<CodeInstruction> insert_codes = new List<CodeInstruction>();

                insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
                insert_codes.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(MizuDef), nameof(MizuDef.Thing_ClearWater))));
                insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_S, 20));
                insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LordToil_Siege), "DropSupplies", new Type[] { typeof(ThingDef), typeof(int) })));

                codes.InsertRange(insert_index, insert_codes);
            }
            return codes.AsEnumerable();
        }
    }

    // 輸送ポッド積荷選択時、右上に現在の水分総量を表示させる処理を追加
    [HarmonyPatch(typeof(Dialog_LoadTransporters))]
    [HarmonyPatch("DoWindowContents")]
    class Dialog_LoadTransporters_DoWindowContents
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

                // 食料表示処理のコード位置を頼りに挿入すべき場所を探す
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("DrawDaysWorthOfFoodInfo"))
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
                new_codes.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Dialog_LoadTransporters), "transferables")));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.DaysWorthOfWater2))));
                new_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                new_codes.Add(new CodeInstruction(OpCodes.Ldc_R4, float.MaxValue));
                new_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.DrawDaysWorthOfWaterInfo))));

                codes.InsertRange(insert_index + 1, new_codes);

                //List<CodeInstruction> new_codes2 = new List<CodeInstruction>();
                //new_codes2.Add(new CodeInstruction(OpCodes.Ldarg_0));
                //new_codes2.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Dialog_LoadTransporters), "daysWorthOfFoodDirty")));
                //new_codes2.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.LogBool))));
                //codes.InsertRange(0, new_codes2);

            }
            return codes.AsEnumerable();
        }
    }

    // よくわからないけどなぜか追加できないのでコメントアウト

    //// 輸送ポッド積荷選択画面で積荷の内容が変化したときに、水の総量再計算フラグを立てる処理を追加
    //[HarmonyPatch(typeof(Dialog_LoadTransporters))]
    //[HarmonyPatch("CountToTransferChanged")]
    //class Dialog_LoadTransporters_CountToTransferChanged
    //{
    //    static void Postfix()
    //    {
    //        MizuCaravanUtility.daysWorthOfWaterDirty = true;
    //    }
    //}

    // 食事要求がないポーンの水分要求を非表示にする
    [HarmonyPatch(typeof(Pawn_NeedsTracker))]
    [HarmonyPatch("ShouldHaveNeed")]
    class Pawn_NeedsTracker_ShouldHaveNeed
    {
        static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
        {
            if (nd == MizuDef.Need_Water)
            {
                if (__instance.food == null)
                {
                    __result = false;
                }
            }
        }
    }
}
