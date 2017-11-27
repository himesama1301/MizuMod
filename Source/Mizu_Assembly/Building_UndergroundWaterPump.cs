using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public abstract class Building_UndergroundWaterPump : Building_WaterNet, IBuilding_WaterNet
    {
        public abstract MapComponent_WaterGrid WaterGrid { get; }

        public override WaterType OutputWaterType
        {
            get
            {
                return WaterType.NormalWater;
            }
        }

        private UndergroundWaterPool pool = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.pool = this.WaterGrid.GetPool(map.cellIndices.CellToIndex(this.Position));
            if (this.pool == null)
            {
                Log.Error("pool is null");
            }
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(string.Format("Stored Water={0}%", (pool.CurrentWaterVolumePercent * 100).ToString("F0")));
            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Format(" ({0}/{1} L)", pool.CurrentWaterVolume, pool.MaxWaterVolume));
            }

            return stringBuilder.ToString();
        }
    }
}
