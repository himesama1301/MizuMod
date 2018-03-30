using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class MizuModBody : Mod
    {
        public MizuModBody(ModContentPack content) : base(content)
        {

        }

        public override string SettingsCategory()
        {
            return "No Water, No Life.";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
        }
    }
}
