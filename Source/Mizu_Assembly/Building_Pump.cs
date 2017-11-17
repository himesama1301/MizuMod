using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Building_Pump : Building_WaterNetBase
    {
        protected WaterType waterType = WaterType.NoWater;
        public virtual WaterType WaterType
        {
            get
            {
                return waterType;
            }
            protected set
            {
                if (waterType != value)
                {
                    waterType = value;
                    CompWaterNetPump comp = this.GetComp<CompWaterNetPump>(); 
                    if (comp == null)
                    {
                        Log.Error("CompWaterNetPump is null");
                        return;
                    }
                    comp.Manager.RefreshWaterNets();
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            CompWaterNetPump comp = this.GetComp<CompWaterNetPump>();
            if (comp == null)
            {
                this.WaterType = WaterType.NoWater;
                return;
            }

            this.TickWaterType(comp);
        }

        public virtual void TickWaterType(CompWaterNetPump comp)
        {
            if (comp.WaterFlow > 0.0f)
            {
                this.WaterType = WaterType.NormalWater;
            }
            else
            {
                this.WaterType = WaterType.NoWater;
            }
        }
    }
}
