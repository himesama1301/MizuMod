using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class CompDestroyMessage : ThingComp
    {
        public CompProperties_DestroyMessage Props
        {
            get
            {
                return (CompProperties_DestroyMessage)this.props;
            }
        }

        public string MessageKey
        {
            get
            {
                return this.Props.messageKey;
            }
        }

        public List<DestroyMode> DestroyModes
        {
            get
            {
                return this.Props.destroyModes;
            }
        }

        public CompDestroyMessage() { }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (string.IsNullOrEmpty(this.MessageKey)) return;

            if (this.DestroyModes == null || !this.DestroyModes.Contains(mode)) return;

            MoteMaker.ThrowText(this.parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), previousMap, this.MessageKey.Translate(), Color.white);
        }
    }
}
