using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Area_Mop : Area
    {
        public override string Label
        {
            get
            {
                return MizuStrings.AreaMop.Translate();
            }
        }

        public override Color Color
        {
            get
            {
                return new Color(0.3f, 0.3f, 0.9f);
            }
        }

        public override int ListPriority
        {
            get
            {
                return 3999;
            }
        }

        public Area_Mop()
        {
        }

        public Area_Mop(AreaManager areaManager) : base(areaManager)
        {
        }

        public override string GetUniqueLoadID()
        {
            return "Area_" + this.ID + "_MizuMop";
        }
    }
}
