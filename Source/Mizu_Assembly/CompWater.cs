using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class CompWater : ThingComp
    {
        public CompProperties_Water Props { get { return (CompProperties_Water)this.props; } }
        public float WaterAmount { get { return this.Props.waterAmount; } }
        public WaterPreferability WaterPreferability { get { return this.Props.waterPreferability; } }
        public EffecterDef GetEffect { get { return this.Props.getEffect; } }
        public SoundDef GetSound { get { return this.Props.getSound; } }
        public int MaxNumToGetAtOnce { get { return this.Props.maxNumToGetAtOnce; } }
        public ThoughtDef DrinkThought { get { return this.Props.drinkThought; } }
        public HediffDef DrinkHediff { get { return this.Props.drinkHediff;  } }
        public float FoodPoisonChance { get { return this.Props.foodPoisonChance; } }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var floatMenuOption in base.CompFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }

            yield return new FloatMenuOption(string.Format(MizuStrings.FloatMenuGetWater, this.parent.Label), () =>
            {
                Job job = new Job(MizuDef.Job_DrinkWater, this.parent)
                {
                    count = MizuUtility.WillGetStackCountOf(selPawn, this.parent)
                };
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
            });

            //FloatMenuOption[] extraOptions = new FloatMenuOption[]
            //{
            //    new FloatMenuOption(string.Format(MizuStrings.FloatMenuGetWater, this.parent.Label), ()=>{
            //        Job job = new Job(MizuDef.Job_DrinkWater, this.parent)
            //        {
            //            count = MizuUtility.WillGetStackCountOf(selPawn, this.parent)
            //        };
            //        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

            //    }, MenuOptionPriority.Default, null, null, 0f, null, null),
            //};
            //return base.CompFloatMenuOptions(selPawn).Concat(extraOptions);
        }
    }
}
