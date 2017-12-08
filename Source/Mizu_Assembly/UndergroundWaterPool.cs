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
                currentWaterVolume = value;
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

        public void RegenPool()
        {
            int curTick = Find.TickManager.TicksGame;

            // 基本回復量
            float addWaterVolumeBase = this.baseRegenRate / 60000.0f * (curTick - lastTick);

            // 雨による回復量
            //float addWaterVolumeRain = baseRegenRate / 60000.0f * this.waterGrid.map.weatherManager.RainRate * (curTick - lastTick);

            float addWaterVolumeTotal = addWaterVolumeBase;
            if (addWaterVolumeTotal < 0.0f)
            {
                addWaterVolumeTotal = 0.0f;
            }
            if (addWaterVolumeTotal > 0.0f)
            {
                this.CurrentWaterVolume = Math.Min(this.CurrentWaterVolume + addWaterVolumeTotal, this.MaxWaterVolume);
            }

            lastTick = curTick;
        }
    }
}
