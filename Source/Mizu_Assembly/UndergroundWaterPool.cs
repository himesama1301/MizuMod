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
        }

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid, float maxWaterVolume, WaterType waterType) : this(waterGrid)
        {
            this.maxWaterVolume = maxWaterVolume;
            this.currentWaterVolume = maxWaterVolume;
            this.waterType = waterType;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.ID, "ID");
            Scribe_Values.Look<float>(ref this.maxWaterVolume, "maxWaterVolume");
            Scribe_Values.Look<float>(ref this.currentWaterVolume, "currenteWaterVolume");
            Scribe_Values.Look<WaterType>(ref this.waterType, "waterType");

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
    }
}
