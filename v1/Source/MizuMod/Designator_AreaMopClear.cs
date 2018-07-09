using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Designator_AreaMopClear : Designator_AreaMop
    {
        public Designator_AreaMopClear() : base(DesignateMode.Remove)
        {
            this.defaultLabel = MizuStrings.DesignatorAreaMopClear.Translate();
            this.defaultDesc = MizuStrings.DesignatorAreaMopClearDescription.Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOff", true);
            this.soundDragSustain = SoundDefOf.DesignateDragAreaDelete;
            this.soundDragChanged = SoundDefOf.DesignateDragAreaDeleteChanged;
            this.soundSucceeded = SoundDefOf.DesignateAreaDelete;
        }
    }
}
