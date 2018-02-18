using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Designator_DeconstructPipe : Designator_Deconstruct
    {
        public Designator_DeconstructPipe()
        {
            this.defaultLabel = MizuStrings.DesignatorDeconstructPipe.Translate();
            this.defaultDesc = MizuStrings.DesignatorDeconstructPipeDescription.Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct", true);
            this.soundDragSustain = SoundDefOf.DesignateDragStandard;
            this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.DesignateDeconstruct;
            //this.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
        }

        public override void DesignateThing(Thing t)
        {
            if (t.def.Claimable && t.Faction != Faction.OfPlayer)
            {
                t.SetFaction(Faction.OfPlayer, null);
            }
            Thing innerIfMinified = t.GetInnerIfMinified();
            if (DebugSettings.godMode || innerIfMinified.GetStatValue(StatDefOf.WorkToBuild, true) == 0f || t.def.IsFrame || t.def.IsBlueprint)
            {
                t.Destroy(DestroyMode.Deconstruct);
            }
            else
            {
                base.Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Deconstruct));
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            // 建設済みのパイプなら〇
            if (base.CanDesignateThing(t).Accepted && (t is Building_Pipe)) return true;

            // パイプの設計or施行なら〇
            if ((t.def.IsBlueprint || t.def.IsFrame) && (t.def.entityDefToBuild == MizuDef.Thing_WaterPipe || t.def.entityDefToBuild == MizuDef.Thing_WaterPipeInWater)) return true;

            // それ以外は×
            return false;
        }
    }
}
