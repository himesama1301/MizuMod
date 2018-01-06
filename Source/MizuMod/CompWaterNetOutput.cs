using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompWaterNetOutput : CompWaterNet
    {
        public new CompProperties_WaterNetOutput Props
        {
            get
            {
                return (CompProperties_WaterNetOutput)this.props;
            }
        }

        protected virtual float MaxOutputWaterFlow
        {
            get
            {
                return this.Props.maxOutputWaterFlow;
            }
        }
        protected virtual WaterType ForceOutputWaterType
        {
            get
            {
                return this.Props.forceOutputWaterType;
            }
        }
        protected virtual CompProperties_WaterNetOutput.OutputWaterFlowType OutputWaterFlowType
        {
            get
            {
                return this.Props.outputWaterFlowType;
            }
        }

        public float OutputWaterFlow { get; private set; }
        public WaterType OutputWaterType { get; private set; }

        private bool HasTank
        {
            get
            {
                return this.TankComp != null;
            }
        }
        private bool TankIsEmpty
        {
            get
            {
                return !this.HasTank || this.TankComp.StoredWaterVolume <= 0.0f;
            }
        }

        public override bool IsActivated
        {
            get
            {
                return base.IsActivated && (!this.HasTank || !this.TankIsEmpty);
            }
        }

        public CompWaterNetOutput() : base()
        {
            this.OutputWaterType = WaterType.NoWater;
        }

        public override void CompTick()
        {
            base.CompTick();

            this.UpdateOutputWaterStatus();
        }

        public void UpdateOutputWaterStatus()
        {
            if (!this.IsActivated)
            {
                // 機能していない
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            if (this.WaterNetBuilding.OutputWaterNet == null)
            {
                // 出力水道網なし
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            if (this.InputComp == null)
            {
                // 入力機能なしで出力だけあるものは存在しない
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            // 有効な出力先が1個でもあれば出力する
            bool foundEffectiveInputter = false;
            foreach (var t in this.WaterNetBuilding.OutputWaterNet.AllThings)
            {
                // 自分自身は除外
                if (t == this.WaterNetBuilding) continue;

                // 相手の入力水道網と自分の出力水道網が一致しない場合は除外
                if (this.WaterNetBuilding.OutputWaterNet != t.InputWaterNet) continue;

                // 入力機能が無効、または水道網から入力しないタイプは無効
                if (t.InputComp == null || !t.InputComp.IsActivated || !t.InputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet)) continue;

                // 貯水機能を持っているが満タンである場合は無効
                if (t.TankComp != null && t.TankComp.AmountCanAccept <= 0f) continue;

                // 有効な出力先が見つかった
                foundEffectiveInputter = true;
                break;
            }

            if (!foundEffectiveInputter)
            {
                // 有効な出力先なし
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            // 水源を決める
            if (this.HasTank)
            {
                if (!this.TankIsEmpty)
                {
                    // タンクがあり、タンクの中身がある
                    //   ⇒タンクが水源
                    this.OutputWaterType = this.TankComp.StoredWaterType;
                    this.OutputWaterFlow = this.MaxOutputWaterFlow;
                    return;
                }
            }
            else
            {
                // タンクがない
                //   ⇒水源は現在の入力

                // 基本は入力されている水質をそのまま出力とする
                // 出力の水質が強制されている場合はその水質にする
                WaterType outWaterType = this.InputComp.InputWaterType;
                if (this.ForceOutputWaterType != WaterType.Undefined) outWaterType = this.ForceOutputWaterType;
                    
                if (this.OutputWaterFlowType == CompProperties_WaterNetOutput.OutputWaterFlowType.Constant && this.InputComp.InputWaterFlow >= this.MaxOutputWaterFlow)
                {
                    // 定量出力タイプで、入力が出力量を超えている場合、機能する
                    this.OutputWaterType = outWaterType;
                    this.OutputWaterFlow = this.MaxOutputWaterFlow;
                    return;
                }
                else if (this.OutputWaterFlowType == CompProperties_WaterNetOutput.OutputWaterFlowType.Any)
                {
                    // 任意出力タイプの場合、入力と同じ量だけ出力する
                    this.OutputWaterType = outWaterType;
                    this.OutputWaterFlow = this.InputComp.InputWaterFlow;

                    // 結果的に出力量が0の場合、水の種類をクリアする
                    if (this.OutputWaterFlow == 0.0f) this.OutputWaterType = WaterType.NoWater;
                    return;
                }
            }

            // 有効な水源無し
            this.OutputWaterType = WaterType.NoWater;
            this.OutputWaterFlow = 0;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(MizuStrings.InspectWaterFlowOutput + ": " + this.OutputWaterFlow.ToString("F2") + " L/day");
            stringBuilder.Append(string.Concat(new string[]
            {
                "(",
                MizuStrings.GetInspectWaterTypeString(this.OutputWaterType),
                ")",
            }));

            return stringBuilder.ToString();
        }
    }
}
