using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class GetWaterRecipeDef : RecipeDef
    {
        public List<WaterTerrainType> needWaterTerrainTypes = null;
        public List<WaterType> needWaterTypes = null;
        //public float needWaterVolume = 1.0f;
        public int getItemCount = 1;

        public override void PostLoad()
        {
            base.PostLoad();

            if (this.products == null)
            {
                var thingCountClass = new ThingCountClass();
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(thingCountClass, "thingDef", "Mizu_NormalWater");
                thingCountClass.count = 1;

                this.products = new List<ThingCountClass>()
                {
                    thingCountClass,
                };
            }
        }
    }
}
