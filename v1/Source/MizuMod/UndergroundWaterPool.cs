using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class UndergroundWaterPool : IExposable
    {
        private bool debugFlag = true;

        public int ID;

        private WaterType waterType;
        public WaterType WaterType
        {
            get
            {
                return this.waterType;
            }
        }

        private float maxWaterVolume;
        public float MaxWaterVolume
        {
            get
            {
                return maxWaterVolume;
            }
        }
        private float currentWaterVolume;
        public float CurrentWaterVolume
        {
            get
            {
                return currentWaterVolume;
            }
            set
            {
                currentWaterVolume = Mathf.Max(0, Mathf.Min(this.maxWaterVolume, value));
                var curMaterialIndex = Mathf.RoundToInt(this.CurrentWaterVolumePercent * UndergroundWaterMaterials.MaterialCount);
                if (lastMaterialIndex != curMaterialIndex)
                {
                    lastMaterialIndex = curMaterialIndex;
                    this.waterGrid.SetDirty();
                }
            }
        }
        private float baseRegenRate;
        public float BaseRegenRate
        {
            get
            {
                return this.baseRegenRate;
            }
        }
        private float rainRegenRatePerCell;
        public float RainRegenRatePerCell
        {
            get
            {
                return this.rainRegenRatePerCell;
            }
        }
        private float outputWaterFlow;
        public float OutputWaterFlow
        {
            get
            {
                return this.outputWaterFlow;
            }
            set
            {
                this.outputWaterFlow = Mathf.Max(value, 0f);
            }
        }
        private int lastTick;

        private int lastMaterialIndex = UndergroundWaterMaterials.MaterialCount;

        public float CurrentWaterVolumePercent
        {
            get
            {
                return this.CurrentWaterVolume / this.maxWaterVolume;
            }
        }

        private MapComponent_WaterGrid waterGrid;

        private List<IntVec3> poolCells = null;

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid)
        {
            this.waterGrid = waterGrid;
            this.lastTick = Find.TickManager.TicksGame;
        }

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid, float maxWaterVolume, WaterType waterType, float baseRegenRate, float rainRegenRatePerCell) : this(waterGrid)
        {
            this.maxWaterVolume = maxWaterVolume;
            this.currentWaterVolume = maxWaterVolume;
            this.waterType = waterType;
            this.baseRegenRate = baseRegenRate;
            this.rainRegenRatePerCell = rainRegenRatePerCell;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.ID, "ID");
            Scribe_Values.Look<float>(ref this.maxWaterVolume, "maxWaterVolume");
            Scribe_Values.Look<float>(ref this.currentWaterVolume, "currenteWaterVolume");
            Scribe_Values.Look<WaterType>(ref this.waterType, "waterType");
            Scribe_Values.Look<float>(ref this.baseRegenRate, "baseRegenRate");
            Scribe_Values.Look<float>(ref this.rainRegenRatePerCell, "rainRegenRatePerCell");
            Scribe_Values.Look<int>(ref this.lastTick, "lastTick");
            Scribe_Values.Look<float>(ref this.outputWaterFlow, "outputWaterFlow");

            if (this.debugFlag)
            {
                this.debugFlag = false;
                if (MizuDef.GlobalSettings.forDebug.enableChangeWaterPoolType)
                {
                    this.waterType = MizuDef.GlobalSettings.forDebug.changeWaterPoolType;
                }
                if (MizuDef.GlobalSettings.forDebug.enableChangeWaterPoolVolume)
                {
                    this.maxWaterVolume *= MizuDef.GlobalSettings.forDebug.waterPoolVolumeRate;
                    this.currentWaterVolume *= MizuDef.GlobalSettings.forDebug.waterPoolVolumeRate;
                }
                if (MizuDef.GlobalSettings.forDebug.enableResetRegenRate)
                {
                    if (this.waterGrid is MapComponent_ShallowWaterGrid)
                    {
                        this.baseRegenRate = MizuDef.GlobalSettings.forDebug.resetBaseRegenRateRangeForShallow.RandomInRange;
                        this.rainRegenRatePerCell = MizuDef.GlobalSettings.forDebug.resetRainRegenRatePerCellForShallow;
                    }
                    if (this.waterGrid is MapComponent_DeepWaterGrid)
                    {
                        this.baseRegenRate = MizuDef.GlobalSettings.forDebug.resetBaseRegenRateRangeForDeep.RandomInRange;
                        this.rainRegenRatePerCell = MizuDef.GlobalSettings.forDebug.resetRainRegenRatePerCellForDeep;
                    }
                }
            }
        }

        public void MergeWaterVolume(UndergroundWaterPool p)
        {
            this.maxWaterVolume += p.maxWaterVolume;
            this.CurrentWaterVolume += p.CurrentWaterVolume;
        }

        public void MergePool(UndergroundWaterPool p, ushort[] idGrid)
        {
            this.MergeWaterVolume(p);
            for (int i = 0; i < idGrid.Length; i++)
            {
                if (idGrid[i] == p.ID)
                {
                    idGrid[i] = (ushort)this.ID;
                }
            }
        }

        public void Update()
        {
            if (this.poolCells == null)
            {
                this.GeneratePoolCells();
            }

            int curTick = Find.TickManager.TicksGame;

            // 基本回復量
            float addWaterVolumeBase = this.baseRegenRate / 60000.0f * (curTick - lastTick);

            // 屋根チェック
            int unroofedCells = 0;
            foreach (var c in this.poolCells)
            {
                if (!c.Roofed(this.waterGrid.map))
                {
                    unroofedCells++;
                }
            }

            // 雨による回復量
            float addWaterVolumeRain = this.rainRegenRatePerCell * unroofedCells / 60000.0f * this.waterGrid.map.weatherManager.RainRate * (curTick - lastTick);

            // 合計回復量
            float addWaterVolumeTotal = addWaterVolumeBase + addWaterVolumeRain;
            if (addWaterVolumeTotal < 0.0f) addWaterVolumeTotal = 0.0f;

            // 出力量(吸い上げられる量)との差分
            float deltaWaterVolume = addWaterVolumeTotal - this.outputWaterFlow / 60000.0f;

            // 差分を加算
            this.CurrentWaterVolume = Math.Min(this.CurrentWaterVolume + deltaWaterVolume, this.MaxWaterVolume);

            // 設定された量を減らしたのでクリア
            this.outputWaterFlow = 0f;

            lastTick = curTick;
        }

        private void GeneratePoolCells()
        {
            this.poolCells = new List<IntVec3>();

            foreach (var c in this.waterGrid.map.AllCells)
            {
                if (this.ID == this.waterGrid.GetID(c))
                {
                    this.poolCells.Add(c);
                }
            }
        }
    }
}
