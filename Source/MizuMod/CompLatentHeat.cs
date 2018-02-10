using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
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

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<float>(ref this.latentHeatAmount, "latentHeatAmount");
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

                Log.Warning("予期しない到達");
            }
        }

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
