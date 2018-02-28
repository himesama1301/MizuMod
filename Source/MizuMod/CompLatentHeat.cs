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
                var map = this.parent.Map;
                var owner = this.parent.holdingOwner;

                if (this.ChangedThingDef == null)
                {
                    // 変化後アイテムの設定が無い場合は消滅
                    this.DestroyParent(map, owner);
                    return;
                }

                // 変化後のアイテムを生成
                var changedThing = ThingMaker.MakeThing(this.ChangedThingDef);
                changedThing.stackCount = this.parent.stackCount;

                // 腐敗度の処理
                this.SetRotProgress(changedThing, this.GetRotProgress());

                // 消滅と生成
                this.DestroyParent(map, owner);
                this.CreateNewThing(changedThing, map, owner);
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

        private void CreateNewThing(Thing thing, Map map, ThingOwner owner)
        {
            if (map != null)
            {
                // マップに落ちている場合
                GenSpawn.Spawn(thing, this.parent.Position, map);
                return;
            }

            if (owner != null)
            {
                // 何らかの物の中に入っている場合
                if (owner.TryAdd(thing) == false)
                {
                    Log.Error("failed TryAdd");
                }
                return;
            }
        }

        private void DestroyParent(Map map, ThingOwner owner)
        {
            if (map != null)
            {
                // マップに落ちている場合
                this.parent.Destroy(DestroyMode.Vanish);
                return;
            }

            if (owner != null)
            {
                // 何らかの物の中に入っている場合
                owner.Remove(this.parent);
                this.parent.Destroy(DestroyMode.Vanish);
                return;
            }
        }

        private float GetRotProgress()
        {
            var compRotThis = this.parent.TryGetComp<CompRottable>();

            if (compRotThis == null)
            {
                // 腐敗度がないなら隠し腐敗度を返す
                return this.hiddenRotProgress;
            }
            else
            {
                // 腐敗度があるならその値を返す
                return compRotThis.RotProgress;
            }
        }

        private void SetRotProgress(Thing thing, float rotProgress)
        {
            var compRotChanged = thing.TryGetComp<CompRottable>();
            if (compRotChanged != null)
            {
                // 腐敗度を持つ
                //   →腐敗度をそこに設定(氷→水の変化時はここに入るはず)
                compRotChanged.RotProgress = rotProgress;
            }
            else
            {
                // 腐敗度を持たない
                //   →潜熱を持つかチェック
                var compLatentHeatChanged = thing.TryGetComp<CompLatentHeat>();
                if (compLatentHeatChanged != null)
                {
                    // 潜熱を持っている
                    //   →腐敗度を隠し腐敗度として設定(水→氷の変化時はここに入るはず)
                    compLatentHeatChanged.HiddenRotProgress = rotProgress;
                }
            }

            // 腐敗度も潜熱も持っていないなら、設定したかった腐敗進行値は捨てる
        }
    }
}
