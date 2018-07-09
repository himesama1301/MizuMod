using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Alert_DehydrationColonists : Alert
    {
        private IEnumerable<Pawn> DehydratingColonists
        {
            get
            {
                return from p in PawnsFinder.AllMaps_FreeColonistsSpawned
                       where p.needs.water() != null && p.needs.water().Dehydrating
                       select p;
            }
        }

        public Alert_DehydrationColonists()
        {
            this.defaultLabel = MizuStrings.AlertDehydration.Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override string GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in this.DehydratingColonists)
            {
                stringBuilder.AppendLine("    " + current.NameStringShort);
            }
            return string.Format(MizuStrings.AlertDehydrationDesc.Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(this.DehydratingColonists.FirstOrDefault<Pawn>());
        }
    }
}
