using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public class Building_SprinklerExtinguishing : Building_WaterNet, IBuilding_WaterNet
    {
        private const int ExtinguishPower = 50;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void TickRare()
        {
            base.TickRare();
        }
    }
}
