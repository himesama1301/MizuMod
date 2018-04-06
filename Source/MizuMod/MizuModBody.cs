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
        public static Settings Settings;

        public MizuModBody(ModContentPack content) : base(content)
        {
            MizuModBody.Settings = base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return MizuStrings.ModTitle;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            MizuModBody.Settings.DoSettingsWindowContents(inRect);
        }
    }
}
