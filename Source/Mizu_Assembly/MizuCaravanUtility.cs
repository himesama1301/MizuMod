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
    public static class MizuCaravanUtility
    {
        public static bool TryGetBestWater(Caravan caravan, Pawn forPawn, out Thing water, out Pawn owner)
        {
            List<Thing> inv_thing_list = CaravanInventoryUtility.AllInventoryItems(caravan);
            Thing found_thing = null;
            float best_score = 0f;
            for (int i = 0; i < inv_thing_list.Count; i++)
            {
                Thing inv_thing = inv_thing_list[i];
                if (MizuCaravanUtility.CanNowGetWater(inv_thing, forPawn))
                {
                    float foodScore = MizuCaravanUtility.GetWaterScore(inv_thing, forPawn);
                    if (found_thing == null || foodScore > best_score)
                    {
                        found_thing = inv_thing;
                        best_score = foodScore;
                    }
                }
            }
            if (found_thing != null)
            {
                water = found_thing;
                owner = CaravanInventoryUtility.GetOwnerOf(caravan, found_thing);
                return true;
            }
            water = null;
            owner = null;
            return false;
        }

        public static bool CanNowGetWater(Thing water, Pawn pawn)
        {
            return water.CanDrinkWaterNow() && MizuCaravanUtility.CanEverGetWater(water, pawn) && (pawn.needs.water().CurCategory >= ThirstCategory.Dehydration || water.GetWaterPreferability() > WaterPreferability.NeverDrink);
        }

        public static bool CanEverGetWater(Thing water, Pawn pawn)
        {
            return water.CanGetWater() && (water.GetWaterPreferability() > WaterPreferability.NeverDrink);
        }

        public static bool CanEverGetWater(ThingDef water, Pawn pawn)
        {
            bool canGetWater = false;
            CompProperties_Water compprop_water = null;
            for (int i = 0; i < water.comps.Count; i++)
            {
                compprop_water = water.comps[i] as CompProperties_Water;
                if (compprop_water != null && compprop_water.waterAmount > 0.0f)
                {
                    canGetWater = true;
                }
            }
            return canGetWater && (compprop_water.waterPreferability > WaterPreferability.NeverDrink);
        }

        public static float GetWaterScore(Thing water, Pawn pawn)
        {
            return (float)water.GetWaterPreferability();
        }

        public static float GetWaterScore(ThingDef water, Pawn pawn)
        {
            CompProperties_Water compprop_water = null;
            for (int i = 0; i < water.comps.Count; i++)
            {
                compprop_water = water.comps[i] as CompProperties_Water;
                if (compprop_water != null)
                {
                    return (float)compprop_water.waterPreferability;
                }
            }
            return 0.0f;
        }

        public static bool daysWorthOfWaterDirty = true;
        private static float cachedDaysWorthOfWater;
        public static float DaysWorthOfWater(Dialog_FormCaravan dialog)
        {
            if (MizuCaravanUtility.daysWorthOfWaterDirty)
            {
                MizuCaravanUtility.daysWorthOfWaterDirty = false;
                MizuCaravanUtility.cachedDaysWorthOfWater = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(dialog.transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
            }
            return MizuCaravanUtility.cachedDaysWorthOfWater;
        }

        public static void DrawDaysWorthOfWaterInfo(Rect rect, float daysWorthOfWater, bool alignRight = false, float truncToWidth = float.MaxValue)
        {
            GUI.color = Color.gray;
            string text;
            if (daysWorthOfWater >= 1000f)
            {
                // 大量にある
                text = MizuStrings.LabelInfiniteDaysWorthOfWaterInfo;
            }
            else
            {
                // 大量には無い
                text = string.Format(MizuStrings.LabelDaysWorthOfWaterInfo, daysWorthOfWater.ToString("0.#"));
            }
            string text2 = text;
            if (truncToWidth != float.MaxValue)
            {
                // 表示幅指定がある場合、幅をオーバーしていたら「...」で省略する
                text2 = text.Truncate(truncToWidth, null);
            }
            Vector2 vector = Text.CalcSize(text2);
            Rect rect2;
            if (alignRight)
            {
                // 描画領域指定(右寄せ)
                rect2 = new Rect(rect.xMax - vector.x, rect.y, vector.x, vector.y);
            }
            else
            {
                // 描画領域指定(左寄せ)
                rect2 = new Rect(rect.x, rect.y, vector.x, vector.y);
            }
            // ラベル生成
            Widgets.Label(rect2, text2);
            string text3 = string.Empty;
            if (truncToWidth != float.MaxValue && Text.CalcSize(text).x > truncToWidth)
            {
                // 省略が発生している場合は、全文を追加
                text3 = text3 + text + "\n\n";
            }
            // ツールチップのテキストを追加
            text3 = text3 + MizuStrings.LabelDaysWorthOfWaterTooltip + "\n\n";
            // ラベルの領域にツールチップを設定
            TooltipHandler.TipRegion(rect2, text3);
            // GUIのカラーを戻す
            GUI.color = Color.white;
        }

        public static void AppendWaterWorthToCaravanInspectString(Caravan c, StringBuilder stringBuilder)
        {
            float daysWorthOfWater = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(c);
            string text;
            if (AnyPawnOutOfWater(c, out text))
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(MizuStrings.InspectCaravanOutOfWater);
                if (!text.NullOrEmpty())
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(text);
                    stringBuilder.Append(".");
                }
            }
            else if (daysWorthOfWater < 1000f)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(string.Format(MizuStrings.InspectCaravanDaysOfFood, daysWorthOfWater.ToString("0.#")));
            }
        }

        public static bool AnyPawnOutOfWater(Caravan c, out string dehydrationHediff)
        {
            List<Thing> tmpInvWater = new List<Thing>();
            List<Thing> list = CaravanInventoryUtility.AllInventoryItems(c);
            tmpInvWater.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].CanGetWater())
                {
                    tmpInvWater.Add(list[i]);
                }
            }
            List<Pawn> pawnsListForReading = c.PawnsListForReading;
            for (int j = 0; j < pawnsListForReading.Count; j++)
            {
                Pawn pawn = pawnsListForReading[j];
                if (pawn.needs.mood != null && pawn.needs.water() != null)
                {
                    bool flag = false;
                    for (int k = 0; k < tmpInvWater.Count; k++)
                    {
                        if (MizuCaravanUtility.CanEverGetWater(tmpInvWater[k].def, pawn))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        int num = -1;
                        string text = null;
                        for (int l = 0; l < pawnsListForReading.Count; l++)
                        {
                            Hediff firstHediffOfDef = pawnsListForReading[l].health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Dehydration, false);
                            if (firstHediffOfDef != null && (text == null || firstHediffOfDef.CurStageIndex > num))
                            {
                                num = firstHediffOfDef.CurStageIndex;
                                text = firstHediffOfDef.LabelCap;
                            }
                        }
                        dehydrationHediff = text;
                        return true;
                    }
                }
            }
            dehydrationHediff = null;
            return false;
        }
    }
}
