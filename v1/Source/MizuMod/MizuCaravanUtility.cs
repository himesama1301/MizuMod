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
        // 水が大量にあるかどうかの閾値(単位：日数)
        private const float InfiniteDaysWorthOfWaterThreshold = 1000.0f;

        // 水が少ない警告を出すかどうかの閾値(単位：日数)
        private const float DaysWorthOfWaterWarningBeforeLeavingThreshold = 5.0f;

        // 水が無いかどうかの判断閾値(単位：日数)
        private const float DaysWorthOfNoWaterThreshold = 0.1f;

        public static bool TryGetBestWater(Caravan caravan, Pawn forPawn, out Thing water, out Pawn owner)
        {
            List<Thing> inventoryThings = CaravanInventoryUtility.AllInventoryItems(caravan);
            Thing foundThing = null;
            float bestScore = float.MinValue;

            // キャラバンの全所持品をチェック
            foreach (var thing in inventoryThings)
            {
                // それが飲めるものかどうか
                if (!MizuCaravanUtility.CanNowGetWater(thing, forPawn)) continue;

                float waterScore = MizuCaravanUtility.GetWaterScore(thing, forPawn);

                // 今まで見つけたベストスコアを超えたか
                if (bestScore >= waterScore) continue;

                foundThing = thing;
                bestScore = waterScore;
            }

            if (foundThing != null)
            {
                // 何かしらの水が見つかった
                water = foundThing;
                // 水が個人の所持品に含まれている場合は持ち主が誰かを調べておく
                owner = CaravanInventoryUtility.GetOwnerOf(caravan, foundThing);
                return true;
            }

            water = null;
            owner = null;
            return false;
        }

        public static bool CanNowGetWater(Thing water, Pawn pawn)
        {
            return !water.IngestibleNow && water.CanDrinkWaterNow() && MizuCaravanUtility.CanEverGetWater(water, pawn) && (pawn.needs.water().CurCategory >= ThirstCategory.Dehydration || water.GetWaterPreferability() > WaterPreferability.NeverDrink);
        }

        public static bool CanEverGetWater(Thing water, Pawn pawn)
        {
            return water.CanGetWater() && (water.GetWaterPreferability() > WaterPreferability.NeverDrink);
        }

        public static bool CanEverGetWater(ThingDef water, Pawn pawn)
        {
            var compprop = water.GetCompProperties<CompProperties_WaterSource>();

            return (compprop != null && compprop.waterAmount > 0.0f && (compprop.waterType >= WaterType.SeaWater && compprop.waterType <= WaterType.ClearWater));
        }

        public static float GetWaterScore(Thing water, Pawn pawn)
        {
            return (float)water.GetWaterPreferability();
        }

        public static float GetWaterScore(ThingDef water, Pawn pawn)
        {
            var compprop = water.GetCompProperties<CompProperties_WaterSource>();
            if (compprop != null)
            {
                return (float)compprop.waterType;
            }
            return 0.0f;
        }

        public static bool daysWorthOfWaterDirty = true;
        private static float cachedDaysWorthOfWater;
        public static float DaysWorthOfWater_FormCaravan(Dialog_FormCaravan dialog)
        {
            if (MizuCaravanUtility.daysWorthOfWaterDirty)
            {
                MizuCaravanUtility.daysWorthOfWaterDirty = false;
                MizuCaravanUtility.cachedDaysWorthOfWater = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(dialog.transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
            }
            return MizuCaravanUtility.cachedDaysWorthOfWater;
        }
        public static float DaysWorthOfWater_LoadTransporters(List<TransferableOneWay> transferables)
        {
            //if (MizuCaravanUtility.daysWorthOfWaterDirty)
            //{
                MizuCaravanUtility.daysWorthOfWaterDirty = false;
                MizuCaravanUtility.cachedDaysWorthOfWater = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
            //}
            return MizuCaravanUtility.cachedDaysWorthOfWater;
        }
        public static float DaysWorthOfWater_Trade(List<Thing> playerCaravanAllPawnsAndItems, List<Tradeable> tradeables)
        {
            if (MizuCaravanUtility.daysWorthOfWaterDirty)
            {
                MizuCaravanUtility.daysWorthOfWaterDirty = false;
                MizuCaravanUtility.cachedDaysWorthOfWater = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWaterLeftAfterTradeableTransfer(playerCaravanAllPawnsAndItems, tradeables, IgnorePawnsInventoryMode.Ignore);
            }
            return MizuCaravanUtility.cachedDaysWorthOfWater;
        }

        public static void DrawDaysWorthOfWaterInfo(Rect rect, float daysWorthOfWater, bool alignRight = false, float truncToWidth = float.MaxValue)
        {
            GUI.color = Color.gray;
            string originalText;

            if (daysWorthOfWater >= InfiniteDaysWorthOfWaterThreshold)
            {
                // 大量にある
                originalText = MizuStrings.LabelInfiniteDaysWorthOfWaterInfo.Translate();
            }
            else
            {
                // 大量には無い
                originalText = string.Format(MizuStrings.LabelDaysWorthOfWaterInfo.Translate(), daysWorthOfWater.ToString("0.#"));
            }

            string truncText = originalText;
            if (truncToWidth != float.MaxValue)
            {
                // 表示幅指定がある場合、幅をオーバーしていたら「...」で省略する
                // オーバーしていなければオリジナルテキストが返ってくる？
                truncText = originalText.Truncate(truncToWidth, null);
            }

            // 省略テキストの描画サイズ
            Vector2 truncTextSize = Text.CalcSize(truncText);
            Rect truncTextRect;
            if (alignRight)
            {
                // 描画領域指定(右寄せ)
                truncTextRect = new Rect(rect.xMax - truncTextSize.x, rect.y, truncTextSize.x, truncTextSize.y);
            }
            else
            {
                // 描画領域指定(左寄せ)
                truncTextRect = new Rect(rect.x, rect.y, truncTextSize.x, truncTextSize.y);
            }

            // ラベル生成
            Widgets.Label(truncTextRect, truncText);

            string toolTipText = string.Empty;
            if (truncToWidth != float.MaxValue && Text.CalcSize(originalText).x > truncToWidth)
            {
                // 省略が発生している場合は、全文を追加
                toolTipText = toolTipText + originalText + "\n\n";
            }

            // ツールチップのテキストを追加
            toolTipText = toolTipText + MizuStrings.LabelDaysWorthOfWaterTooltip.Translate() + "\n\n";

            // ラベルの領域にツールチップを設定
            TooltipHandler.TipRegion(truncTextRect, toolTipText);

            // GUIのカラーを戻す
            GUI.color = Color.white;
        }

        public static void AppendWaterWorthToCaravanInspectString(Caravan c, StringBuilder stringBuilder)
        {
            string worstDehydrationText;
            if (AnyPawnOutOfWater(c, out worstDehydrationText))
            {
                // 水不足のポーンがいる
                stringBuilder.AppendLine();
                stringBuilder.Append(MizuStrings.InspectCaravanOutOfWater.Translate());

                if (!worstDehydrationText.NullOrEmpty())
                {
                    // 脱水症状のテキストがあるならそれも追加
                    stringBuilder.Append(" ");
                    stringBuilder.Append(worstDehydrationText);
                    stringBuilder.Append(".");
                }
            }
            else 
            {
                // 水不足のポーンがいないなら、総量をチェック
                float daysWorthOfWater = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(c);
                if (daysWorthOfWater < InfiniteDaysWorthOfWaterThreshold)
                {
                    // 水は大量というわけでないなら、水残量を表示
                    stringBuilder.AppendLine();
                    stringBuilder.Append(string.Format(MizuStrings.InspectCaravanDaysOfWater.Translate(), daysWorthOfWater.ToString("0.#")));
                }
            }
        }

        public static bool AnyPawnOutOfWater(Caravan c, out string worstDehydrationText)
        {
            // キャラバンの全所持品の水アイテムリスト作成
            List<Thing> tmpInvWater = CaravanInventoryUtility.AllInventoryItems(c).FindAll((t) => t.CanGetWater());

            bool allFoundWaterItem = true;

            // キャラバン内の全ポーンをチェック
            foreach (var pawn in c.PawnsListForReading)
            {
                // 水分要求なし→水不要
                if (pawn.needs.water() == null) continue;

                // 心情ステータス無し、キャラバンの地形に水がある→アイテムがなくても水は飲める
                if (pawn.needs.mood == null && c.GetWaterTerrainType() != WaterTerrainType.NoWater) continue;

                // そのポーンが飲める水があるなら良し
                if (tmpInvWater.Exists((t) => MizuCaravanUtility.CanEverGetWater(t.def, pawn))) continue;

                // 適切な水アイテムを見つけられなかったポーンがいる
                allFoundWaterItem = false;
                break;
            }

            if (allFoundWaterItem)
            {
                // 全ポーンが水アイテムを見つけた
                //   →脱水症状のテキストは不要
                worstDehydrationText = null;
                return false;
            }

            // 適切なアイテムを見つけられなかったポーンがいる
            //   →全ポーンの脱水症状をチェックして最悪の状態を調査、そのテキストを取得
            int maxHediffStageIndex = -1;
            string maxHediffText = null;
            foreach (var pawn in c.PawnsListForReading)
            {
                // 脱水症状の健康状態を持っているか
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Dehydration, false);
                if (hediff == null) continue;

                if (maxHediffText == null || maxHediffStageIndex < hediff.CurStageIndex)
                {
                    // 最悪状態なら更新
                    maxHediffStageIndex = hediff.CurStageIndex;
                    maxHediffText = hediff.LabelCap;
                }
            }

            // 最悪の脱水症状テキストを返す
            worstDehydrationText = maxHediffText;
            return true;
        }

        public static void AddWaterWarningString(Dialog_FormCaravan dialog, List<string> strList)
        {
            float daysWorthOfWater = DaysWorthOfWater_FormCaravan(dialog);

            if (daysWorthOfWater < DaysWorthOfWaterWarningBeforeLeavingThreshold)
            {
                // キャラバン出発前の警告ダイアログに表示が必要な状態である

                if (daysWorthOfWater >= DaysWorthOfNoWaterThreshold)
                {
                    // 少しはある
                    strList.Add(string.Format(MizuStrings.LabelDaysWorthOfWaterWarningDialog.Translate(), daysWorthOfWater.ToString("0.#")));
                }
                else
                {
                    // 全く水を持っていない
                    strList.Add(MizuStrings.LabelDaysWorthOfWaterWarningDialog_NoWater.Translate());
                }
            }
        }

        //public static void LogBool(bool b)
        //{
        //    Log.Message(string.Format("{0}, {1}", b.ToString(), MizuCaravanUtility.daysWorthOfWaterDirty.ToString()));
        //}
    }
}
