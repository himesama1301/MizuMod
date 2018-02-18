using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Designator_AreaSnowGetExpand : Designator_AreaSnowGet
    {
        public Designator_AreaSnowGetExpand() : base(DesignateMode.Add)
        {
            this.defaultLabel = MizuStrings.DesignatorAreaSnowGetExpand.Translate();
            this.defaultDesc = MizuStrings.DesignatorAreaSnowGetExpandDescription.Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOn", true);
            this.soundDragSustain = SoundDefOf.DesignateDragAreaAdd;
            this.soundDragChanged = SoundDefOf.DesignateDragAreaAddChanged;
            this.soundSucceeded = SoundDefOf.DesignateAreaAdd;
        }
    }
}
