using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class CompLatentHeat : ThingComp
    {
        public CompProperties_LatentHeat Props
        {
            get
            {
                return (CompProperties_LatentHeat)this.props;
            }
        }

        public ThingDef ChangedThingDef
        {
            get
            {
                return this.Props.changedThingDef;
            }
        }

        public float TemperatureThreshold
        {
            get
            {
                return this.Props.temperatureThreshold;
            }
        }

        public CompProperties_LatentHeat.AddCondition AddLatentHeatCondition
        {
            get
            {
                return this.Props.addLatentHeatCondition;
            }
        }

        public float LatentHeatThreshold
        {
            get
            {
                return this.Props.latentHeatThreshold;
            }
        }

        // 潜熱値
        private float latentHeatAmount;
        public float LatentHeatAmount
        {
            get
            {
                return this.latentHeatAmount;
            }
            set
            {
                this.latentHeatAmount = Mathf.Max(0f, value);
            }
        }

        // 隠し腐敗度
        private float hiddenRotProgress;
        public float HiddenRotProgress
        {
            get
            {
                return this.hiddenRotProgress;
            }
            set
            {
                this.hiddenRotProgress = value;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref this.latentHeatAmount, "latentHeatAmount");
            Scribe_Values.Look(ref this.hiddenRotProgress, "hiddenRotProgress");
        }

        public override void CompTickRare()
        {
            base.CompTick();

            // 閾値より温度が高い場合はプラス
            var deltaTemperature = this.parent.AmbientTemperature - this.TemperatureThreshold;

            // 溶ける・凍る判定
            int direction = 0;
            switch (this.AddLatentHeatCondition)
            {
                case CompProperties_LatentHeat.AddCondition.Above:
                    // 温度が高いと潜熱プラス→溶ける
                    direction = 1;
                    break;
                case CompProperties_LatentHeat.AddCondition.Below:
                    // 温度が低いと潜熱プラス→凍る
                    direction = -1;
                    break;
                default:
                    Log.Error("AddLatentHeatCondition is invalid");
                    break;
            }

            // 潜熱値変更
            // (最後はデバッグ用の係数を掛けている)
            this.LatentHeatAmount += deltaTemperature * direction * MizuDef.GlobalSettings.forDebug.latentHeatRate;

            if (this.latentHeatAmount >= this.LatentHeatThreshold)
            {
                // 潜熱値が閾値を超えた時の処理

                // 変化後のアイテムを生成
                var changedThing = ThingMaker.MakeThing(this.ChangedThingDef);
                changedThing.stackCount = this.parent.stackCount;

                // 腐敗度の処理
                // 腐敗しないアイテムなら隠し腐敗度、腐敗するアイテムなら現在の腐敗度を取得

                // 変化前のアイテムの腐敗度
                var compRotThis = this.parent.TryGetComp<CompRottable>();
                float rotProgressThis = (compRotThis == null) ? this.hiddenRotProgress : compRotThis.RotProgress;

                // 変化後のアイテムの腐敗度
                var compRotChanged = changedThing.TryGetComp<CompRottable>();
                if (compRotChanged != null)
                {
                    // 腐敗度を持つ
                    //   →腐敗度をそこに設定(氷→水の変化時はここに入るはず
                    compRotChanged.RotProgress = rotProgressThis;
                }
                else
                {
                    // 腐敗度を持たない
                    //   →潜熱を持つかチェック
                    var compLatentHeatChanged = changedThing.TryGetComp<CompLatentHeat>();
                    if (compLatentHeatChanged != null)
                    {
                        // 潜熱を持っている
                        //   →腐敗度を隠し腐敗度として設定(水→氷の変化時はここに入るはず)
                        compLatentHeatChanged.HiddenRotProgress = rotProgressThis;
                    }
                }

                var map = this.parent.Map;
                if (map != null)
                {
                    // マップに置いてある場合
                    GenSpawn.Spawn(changedThing, parent.Position, map);
                    this.parent.Destroy(DestroyMode.Vanish);
                    return;
                }

                var owner = this.parent.holdingOwner;
                if (owner != null)
                {
                    // 何らかの物の中に入っている場合
                    owner.Remove(this.parent);
                    this.parent.Destroy(DestroyMode.Vanish);
                    if (owner.TryAdd(changedThing) == false)
                    {
                        Log.Error("failed TryAdd");
                    }
                    return;
                }

                Log.Error("予期しない到達(デバッグ用)");
            }
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);

            // 全体に対するother側の割合
            float otherRatio = (float)count / (float)(this.parent.stackCount + count);

            var otherComp = otherStack.TryGetComp<CompLatentHeat>();
            if (otherComp == null) return;

            // 潜熱値の計算(加重平均)
            this.LatentHeatAmount = Mathf.Lerp(this.LatentHeatAmount, otherComp.LatentHeatAmount, otherRatio);

            // 隠し腐敗度の計算(加重平均)
            this.HiddenRotProgress = Mathf.Lerp(this.HiddenRotProgress, otherComp.HiddenRotProgress, otherRatio);
        }

        // 分離した時、潜熱値は特に変更しない
        //public override void PostSplitOff(Thing piece)
        //{
        //    base.PostSplitOff(piece);
        //}

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (DebugSettings.godMode)
            {
                if (stringBuilder.ToString() != string.Empty)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append("LatentHeatAmount:" + this.latentHeatAmount.ToString("F2"));
                stringBuilder.AppendLine();
                stringBuilder.Append("HiddenRotProgress:" + this.hiddenRotProgress.ToString());
            }
            //if (stringBuilder.ToString() != string.Empty)
            //{
            //    stringBuilder.AppendLine();
            //}
            //stringBuilder.Append(MizuStrings.InspectWaterFlowInput + ": " + this.InputWaterFlow.ToString("F2") + " L/day");
            //stringBuilder.Append(string.Concat(new string[]
            //{
            //    "(",
            //    MizuStrings.GetInspectWaterTypeString(this.InputWaterType),
            //    ")",
            //}));

            return stringBuilder.ToString();
        }
    }
}
