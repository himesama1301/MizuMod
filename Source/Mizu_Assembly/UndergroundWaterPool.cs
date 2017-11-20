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
        public int ID;

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

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid, float maxWaterVolume) : this(waterGrid)
        {
            this.maxWaterVolume = maxWaterVolume;
            this.currentWaterVolume = maxWaterVolume;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.ID, "ID");
            Scribe_Values.Look<float>(ref this.maxWaterVolume, "maxWaterVolume");
            Scribe_Values.Look<float>(ref this.currentWaterVolume, "currenteWaterVolume");
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
