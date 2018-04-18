using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Verb_LaunchBucketWater : Verb_LaunchProjectile
    {
        public override void WarmupComplete()
        {
            Log.Message("WarmupComplete");
            base.WarmupComplete();
        }

        protected override bool TryCastShot()
        {
            Log.Message("TryCastShot");
            return base.TryCastShot();
        }
    }
}
