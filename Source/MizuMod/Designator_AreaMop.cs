﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public abstract class Designator_AreaMop : Designator
    {
        private DesignateMode mode;

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public Designator_AreaMop(DesignateMode mode)
        {
            this.mode = mode;
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.useMouseIcon = true;
            //this.hotKey = KeyBindingDefOf.Misc7;
            //this.tutorTag = "AreaSnowClear";
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            bool flag = base.Map.areaManager.Mop()[c];
            if (this.mode == DesignateMode.Add)
            {
                return !flag;
            }
            return flag;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (this.mode == DesignateMode.Add)
            {
                base.Map.areaManager.Mop()[c] = true;
            }
            else
            {
                base.Map.areaManager.Mop()[c] = false;
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            base.Map.areaManager.Mop().MarkForDraw();
        }
    }
}
