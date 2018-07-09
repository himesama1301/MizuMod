using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompProperties_DestroyMessage : CompProperties
    {
        public string messageKey;
        public List<DestroyMode> destroyModes;

        public CompProperties_DestroyMessage() : base(typeof(CompDestroyMessage)) { }
        public CompProperties_DestroyMessage(Type compClass) : base(compClass) { }
    }
}
