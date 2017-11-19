using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public float CurrentWaterVolume;

        public float CurrentWaterVolumePercent
        {
            get
            {
                return this.CurrentWaterVolume / this.maxWaterVolume;
            }
        }

        public UndergroundWaterPool()
        {

        }

        public UndergroundWaterPool(int maxWaterVolume)
        {
            this.maxWaterVolume = maxWaterVolume;
            this.CurrentWaterVolume = maxWaterVolume;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.ID, "ID");
            Scribe_Values.Look<float>(ref this.maxWaterVolume, "maxWaterVolume");
            Scribe_Values.Look<float>(ref this.CurrentWaterVolume, "currenteWaterVolume");
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
