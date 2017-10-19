using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace MizuMod
{
    public class CompWater : ThingComp
    {
        public CompProperties_Water PropsWater
        {
            get
            {
                return (CompProperties_Water)this.props;
            }
        }

        public float WaterAmount
        {
            get
            {
                return this.PropsWater.waterAmount;
            }
        }

        public WaterPreferability WaterPreferability
        {
            get
            {
                return this.PropsWater.waterPreferability;
            }
        }

        public EffecterDef GetEffect
        {
            get
            {
                return this.PropsWater.getEffect;
            }
        }

        public SoundDef GetSound
        {
            get
            {
                return this.PropsWater.getSound;
            }
        }

        public int MaxNumToGetAtOnce
        {
            get
            {
                return this.PropsWater.maxNumToGetAtOnce;
            }
        }

        public float ChairSearchRadius
        {
            get
            {
                return this.PropsWater.chairSearchRadius;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            FloatMenuOption[] extraOptions = new FloatMenuOption[]
            {
                new FloatMenuOption(string.Format(MizuStrings.FloatMenuGetWater, this.parent.Label), ()=>{
                    Job job = new Job(MizuDef.Job_DrinkWater, this.parent)
                    {
                        count = MizuUtility.WillGetStackCountOf(selPawn, this.parent)
                    };
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                }, MenuOptionPriority.Default, null, null, 0f, null, null),
            };
            return base.CompFloatMenuOptions(selPawn).Concat(extraOptions);
        }
    }
}
