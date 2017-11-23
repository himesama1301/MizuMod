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

        private UndergroundWaterPool waterPool = null;
        private CompWaterNetTank tankComp = null;

        private bool HasTank
        {
            get
            {
                return tankComp != null;
            }
        }
        private bool TankIsEmpty
        {
            get
            {
                return !this.HasTank || tankComp.StoredWaterVolume <= 0.0f;
            }
        }

        public override bool IsActivated
        {
            get
            {
                return base.IsActivated && (!this.HasTank || !this.TankIsEmpty);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.OutputWaterType = WaterType.NoWater;
            this.tankComp = this.parent.GetComp<CompWaterNetTank>();

            var pump = this.parent as Building_UndergroundWaterPump;
            if (pump != null)
            {
                this.waterPool = pump.WaterGrid.GetPool(this.WaterNetBuilding.Map.cellIndices.CellToIndex((this.WaterNetBuilding as Building).Position));
            }
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

            // 出力水道網に、自分自身ではなく水道網から入力を受け付けていて、(タンクではない)or(タンクだが満タンではない)があるか探す
            bool foundNotFullTank = false;
            if (this.WaterNetBuilding.OutputWaterNet != null)
            {
                foreach (var t in this.WaterNetBuilding.OutputWaterNet.Things)
                {
                    CompWaterNetTank tankComp = t.GetComp<CompWaterNetTank>();
                    CompWaterNetInput inputComp = t.GetComp<CompWaterNetInput>();

                    if (t == this.WaterNetBuilding)
                    {
                        continue;
                    }
                    if (inputComp == null || !inputComp.IsActivated || inputComp.InputType != CompProperties_WaterNetInput.InputType.WaterNet)
                    {
                        continue;
                    }
                    if (tankComp != null && tankComp.AmountCanAccept == 0.0f)
                    {
                        continue;
                    }

                    foundNotFullTank = true;
                    break;
                }
            }

            if (!this.HasTank)
            {
                CompWaterNetInput inputComp = this.WaterNetBuilding.GetComp<CompWaterNetInput>();
                if (inputComp == null)
                {
                    if (foundNotFullTank)
                    {
                        // 貯蔵機能なし、入力なし、水道網の中に満タンでないタンクあり
                        this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                        if (this.waterPool == null)
                        {
                            // 対応する地下水脈なし=地上ポンプ
                            this.OutputWaterFlow = this.MaxOutputWaterFlow;
                        }
                        else if (this.waterPool.CurrentWaterVolume <= 0.0f)
                        {
                            // 地下水脈が空
                            this.OutputWaterFlow = 0.0f;
                        }
                        else
                        {
                            // 地下水脈に水がある
                            this.OutputWaterFlow = this.MaxOutputWaterFlow;
                            this.waterPool.CurrentWaterVolume = Math.Max(this.waterPool.CurrentWaterVolume - this.MaxOutputWaterFlow / 60000.0f, 0.0f);
                        }
                    }
                    else
                    {
                        // 貯蔵機能なし、入力なし、水道網の中に満タンでないタンクあり
                        this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                        this.OutputWaterFlow = 0.0f;
                    }
                }
                else
                {
                    // 貯蔵機能なし、入力あり=フィルター
                    if (this.OutputWaterFlowType == CompProperties_WaterNetOutput.OutputWaterFlowType.Constant)
                    {
                        this.OutputWaterFlow = this.MaxOutputWaterFlow;
                    }
                    else
                    {
                        this.OutputWaterFlow = inputComp.InputWaterFlow;
                    }
                    if (this.OutputWaterFlow == 0.0f)
                    {
                        this.OutputWaterType = WaterType.NoWater;
                    }
                    else if (this.ForceOutputWaterType != WaterType.Undefined)
                    {
                        this.OutputWaterType = this.ForceOutputWaterType;
                    }
                    else
                    {
                        this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                    }
                }
                return;
            }

            if (this.TankIsEmpty)
            {
                // 貯蔵機能あり、中身なし
                this.OutputWaterType = WaterType.NoWater;
                this.OutputWaterFlow = 0f;
                return;
            }

            if (foundNotFullTank)
            {
                CompWaterNetInput inputComp = this.WaterNetBuilding.GetComp<CompWaterNetInput>();
                if (inputComp != null && inputComp.InputType == CompProperties_WaterNetInput.InputType.WaterNet && inputComp.InputWaterFlow > 0.0f)
                {
                    // 貯蔵機能あり、中身あり、水道網の中に満タンでないタンクあり、水道網からの供給を受けている
                    this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                    this.OutputWaterFlow = 0;
                }
                else
                {
                    // 貯蔵機能あり、中身あり、水道網の中に満タンでないタンクあり、水道網からの供給を受けていない
                    this.OutputWaterType = tankComp.StoredWaterType;
                    this.OutputWaterFlow = this.MaxOutputWaterFlow;
                }
            }
            else
            {
                // 貯蔵機能あり、中身あり、水道網の中に満タンでないタンクなし
                this.OutputWaterType = this.WaterNetBuilding.OutputWaterType;
                this.OutputWaterFlow = 0;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(MizuStrings.InspectWaterFlowOutput + ": " + this.OutputWaterFlow.ToString("#####0") + " WaterVolume/day");
            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Format("({0})", this.OutputWaterType));
            }

            return stringBuilder.ToString();
        }
    }
}
