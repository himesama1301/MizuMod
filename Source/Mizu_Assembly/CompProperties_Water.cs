using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class CompProperties_Water : CompProperties
    {
        public const int BaseDrinkTicks = 100;

        public float waterAmount = 0.0f;
        public WaterPreferability waterPreferability = WaterPreferability.Undefined;
        public EffecterDef getEffect = null;
        public SoundDef getSound = null;
        public int maxNumToGetAtOnce = 1;
        public ThoughtDef drinkThought = null;
        public HediffDef drinkHediff = null;

        public CompProperties_Water()
        {
            this.compClass = typeof(CompWater);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (var statDrawEntry in base.SpecialDisplayStats())
            {
                yield return statDrawEntry;
            }
            yield return new StatDrawEntry(MizuDef.StatCategory_Water, MizuStrings.CompWaterAmount, waterAmount.ToString("0.##"), 11);
            //StatDrawEntry[] extraStats = new StatDrawEntry[] {
            //    new StatDrawEntry(MizuDef.StatCategory_Water, MizuStrings.CompWaterAmount , waterAmount.ToString("0.##"), 11),
            //};
            //return base.SpecialDisplayStats().Concat(extraStats);
        }

    }
}
