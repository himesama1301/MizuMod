using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Well : Building_WorkTable, IBuilding_DrinkWater
    {
        private UndergroundWaterPool pool = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            if (waterGrid == null)
            {
                Log.Error("waterGrid is null");
            }

            this.pool = waterGrid.GetPool(map.cellIndices.CellToIndex(this.Position));
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
            stringBuilder.Append(string.Format(MizuStrings.InspectStoredWaterPool + ": {0}%", (pool.CurrentWaterVolumePercent * 100).ToString("F0")));
            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Format(" ({0}/{1} L)", pool.CurrentWaterVolume.ToString("F2"), pool.MaxWaterVolume.ToString("F2")));
            }

            return stringBuilder.ToString();
        }

        public bool IsActivated
        {
            get
            {
                return true;
            }
        }

        public WaterType WaterType
        {
            get
            {
                if (this.pool == null) return WaterType.Undefined;

                return this.pool.WaterType;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (this.pool == null) return true;
                if (this.pool.CurrentWaterVolume <= 0f) return true;
                return false;
            }
        }

        public bool CanDrinkFor(Pawn p)
        {
            if (p.needs == null || p.needs.water() == null) return false;
            if (this.pool == null) return false;

            // 手が使用可能で、地下水の水量が十分にある
            return p.CanManipulate() && this.pool.CurrentWaterVolume >= p.needs.water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public void DrawWater(float amount)
        {
            if (this.pool == null) return;
            this.pool.CurrentWaterVolume = Mathf.Max(this.pool.CurrentWaterVolume - amount, 0);
        }
    }
}
