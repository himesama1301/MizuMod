﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_WaterNetWorkTable : Building_WorkTable, IBuilding_WaterNet
    {
        // コネクタがあるか
        public virtual bool HasConnector
        {
            get
            {
                return this.HasInputConnector || this.HasOutputConnector;
            }
        }

        // 入力コネクタがあるか
        public virtual bool HasInputConnector
        {
            get
            {
                return this.InputConnectors.Count > 0;
            }
        }

        // 出力コネクタがあるか
        public virtual bool HasOutputConnector
        {
            get
            {
                return this.OutputConnectors.Count > 0;
            }
        }

        // 入力コネクタと出力コネクタは同じか
        public virtual bool IsSameConnector
        {
            get
            {
                return true;
            }
        }

        // 電力供給が賄えているか
        public bool PowerOn
        {
            get
            {
                return this.powerTraderComp == null || this.powerTraderComp.PowerOn;
            }
        }

        // スイッチはONか
        public bool SwitchIsOn
        {
            get
            {
                return FlickUtility.WantsToBeOn(this);
            }
        }

        // 機能しているか
        public virtual bool IsActivated
        {
            get
            {
                // 壊れていない、電力供給ありor不要、(電力不要でも切り替えがある場合)ONになっている
                return !this.IsBrokenDown() && this.PowerOn && this.SwitchIsOn;
            }
        }

        // 水道網として機能しているか(水を通すのか)
        // 基本的に電気が通ってなくても、壊れていても水は通す
        public virtual bool IsActivatedForWaterNet
        {
            get
            {
                return true;
            }
        }

        // 水道網管理オブジェクト
        public MapComponent_WaterNetManager WaterNetManager
        {
            get
            {
                return this.Map.GetComponent<MapComponent_WaterNetManager>();
            }
        }

        // 出力する水の種類
        public virtual WaterType OutputWaterType
        {
            get
            {
                return WaterType.NoWater;
            }
        }

        public virtual UndergroundWaterPool WaterPool
        {
            get
            {
                return null;
            }
        }

        // 水抜き機能があるか
        public bool HasDrainCapability
        {
            get
            {
                return this.flickableComp != null && this.SourceComp != null && this.SourceComp.SourceType == CompProperties_WaterSource.SourceType.Building;
            }
        }

        // 水抜き中か
        public bool IsDraining
        {
            get
            {
                return (this.flickableComp != null && !this.flickableComp.SwitchIsOn);
            }
        }

        public WaterNet InputWaterNet { get; set; }
        public WaterNet OutputWaterNet { get; set; }

        public List<IntVec3> InputConnectors { get; private set; }
        public List<IntVec3> OutputConnectors { get; private set; }

        private CompPowerTrader powerTraderComp;
        protected CompFlickable flickableComp;

        private CompWaterSource sourceComp;
        public CompWaterSource SourceComp
        {
            get
            {
                if (this.sourceComp == null) this.sourceComp = this.GetComp<CompWaterSource>();
                return this.sourceComp;
            }
        }
        private CompWaterNetInput inputComp;
        public CompWaterNetInput InputComp
        {
            get
            {
                if (this.inputComp == null) this.inputComp = this.GetComp<CompWaterNetInput>();
                return this.inputComp;
            }
        }
        private CompWaterNetOutput outputComp;
        public CompWaterNetOutput OutputComp
        {
            get
            {
                if (this.outputComp == null) this.outputComp = this.GetComp<CompWaterNetOutput>();
                return this.outputComp;
            }
        }
        private CompWaterNetTank tankComp;
        public CompWaterNetTank TankComp
        {
            get
            {
                if (this.tankComp == null) this.tankComp = this.GetComp<CompWaterNetTank>();
                return this.tankComp;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.powerTraderComp = this.GetComp<CompPowerTrader>();
            this.flickableComp = this.GetComp<CompFlickable>();

            this.InputConnectors = new List<IntVec3>();
            this.OutputConnectors = new List<IntVec3>();
            this.CreateConnectors();

            this.WaterNetManager.AddThing(this);
        }

        public override void DeSpawn()
        {
            this.WaterNetManager.RemoveThing(this);

            base.DeSpawn();
        }

        public List<IntVec3> ConnectVecs
        {
            get
            {
                List<IntVec3> vecs = new List<IntVec3>();
                CellRect rect = this.OccupiedRect().ExpandedBy(1);

                foreach (var cell in rect.EdgeCells)
                {
                    if (cell.x == rect.minX && cell.z == rect.minZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.minX && cell.z == rect.maxZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.maxX && cell.z == rect.minZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.maxX && cell.z == rect.maxZ)
                    {
                        continue;
                    }
                    vecs.Add(cell);
                }

                return vecs;
            }
        }

        public virtual void CreateConnectors()
        {
            this.InputConnectors.Clear();
            this.OutputConnectors.Clear();
            CellRect rect = this.OccupiedRect().ExpandedBy(1);

            foreach (var cell in rect.EdgeCells)
            {
                if (cell.x == rect.minX && cell.z == rect.minZ)
                {
                    continue;
                }
                if (cell.x == rect.minX && cell.z == rect.maxZ)
                {
                    continue;
                }
                if (cell.x == rect.maxX && cell.z == rect.minZ)
                {
                    continue;
                }
                if (cell.x == rect.maxX && cell.z == rect.maxZ)
                {
                    continue;
                }
                this.InputConnectors.Add(cell);
                this.OutputConnectors.Add(cell);
            }
        }

        public virtual void PrintForGrid(SectionLayer sectionLayer)
        {
            if (this.IsActivatedForWaterNet)
            {
                MizuGraphics.LinkedWaterNetOverlay.Print(sectionLayer, this);
            }
        }

        public virtual CellRect OccupiedRect()
        {
            return GenAdj.OccupiedRect(this);
        }

        public virtual bool IsAdjacentToCardinalOrInside(IBuilding_WaterNet other)
        {
            return GenAdj.IsAdjacentToCardinalOrInside(this.OccupiedRect(), other.OccupiedRect());
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (this.HasDrainCapability && this.IsDraining)
            {
                stringBuilder.Append(string.Concat(new string[]
                {
                    MizuStrings.InspectWaterTankDraining.Translate(),
                }));
            }

            if (DebugSettings.godMode)
            {
                if (stringBuilder.ToString() != string.Empty)
                {
                    stringBuilder.AppendLine();
                }
                if (this.InputWaterNet != null)
                {
                    stringBuilder.Append(string.Join(",", new string[] {
                        string.Format("InNetID({0})", this.InputWaterNet.ID),
                        string.Format("Stored({0},{1})", this.InputWaterNet.StoredWaterVolume.ToString("F2"), this.InputWaterNet.StoredWaterType.ToString()),
                        string.Format("Flow({0})", this.InputWaterNet.WaterType),
                    }));
                }
                else
                {
                    stringBuilder.Append("InNet(null)");
                }
                stringBuilder.AppendLine();
                if (this.OutputWaterNet != null)
                {
                    stringBuilder.Append(string.Join(",", new string[] {
                        string.Format("OutNetID({0})", this.OutputWaterNet.ID),
                        string.Format("Stored({0},{1})", this.OutputWaterNet.StoredWaterVolume.ToString("F2"), this.OutputWaterNet.StoredWaterType.ToString()),
                        string.Format("Flow({0})", this.OutputWaterNet.WaterType),
                    }));
                }
                else
                {
                    stringBuilder.Append("OutNet(null)");
                }
            }

            return stringBuilder.ToString();
        }

        public float StoredWaterVolume
        {
            get
            {
                if (this.inputComp != null && this.inputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet))
                {
                    return this.InputWaterNet.StoredWaterVolume;
                }
                else if (this.tankComp != null)
                {
                    return this.tankComp.StoredWaterVolume;
                }

                return 0.0f;
            }
        }

        public WaterType StoredWaterType
        {
            get
            {
                if (this.inputComp != null && this.inputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet))
                {
                    return this.InputWaterNet.StoredWaterType;
                }
                else if (this.tankComp != null)
                {
                    return this.tankComp.StoredWaterType;
                }

                return WaterType.NoWater;
            }
        }

        public void AddWaterVolume(float amount)
        {
            if (this.tankComp != null)
            {
                this.tankComp.AddWaterVolume(amount);
            }
        }

        public void DrawWaterVolume(float amount)
        {
            if (this.inputComp != null && this.inputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet))
            {
                this.InputWaterNet.DrawWaterVolume(amount);
            }
            else if (this.tankComp != null)
            {
                this.tankComp.DrawWaterVolume(amount);                
            }
        }
    }
}
