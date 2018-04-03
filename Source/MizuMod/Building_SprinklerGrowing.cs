using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public class Building_SprinklerGrowing : Building_WaterNet, IBuilding_WaterNet
    {
        private CompPowerTrader compPowerTrader;
        private CompSchedule compSchedule;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = this.GetComp<CompPowerTrader>();
            this.compSchedule = this.GetComp<CompSchedule>();
        }

        public override void TickRare()
        {
            base.TickRare();

            if (this.compPowerTrader.PowerOn)
            {
                // 電源ON、故障無し、稼働時間範囲内の時
            }

            if (this.compSchedule.Allowed)
            {
                // 稼働中の消費電力
                this.compPowerTrader.PowerOutput = -this.compPowerTrader.Props.basePowerConsumption;
            }
            else
            {
                // 非稼働時の消費電力
                this.compPowerTrader.PowerOutput = -this.compPowerTrader.Props.basePowerConsumption * 0.1f;
            }
        }
    }
}
