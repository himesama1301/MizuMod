using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Alert_DehydrationAnimals : Alert
    {
        private IEnumerable<Pawn> DehydratingAnimals
        {
            get
            {
                return from p in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer)
                       where p.HostFaction == null && !p.RaceProps.Humanlike
                       where p.needs.water() != null && p.needs.water().Dehydrating
                       select p;
            }
        }

        public Alert_DehydrationAnimals()
        {
            this.defaultLabel = MizuStrings.AlertDehydrationAnimal.Translate();
        }

        public override string GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn current in from a in this.DehydratingAnimals
                                     orderby a.def.label
                                     select a)
            {
                stringBuilder.Append("    " + current.NameStringShort);
                if (current.Name.IsValid && !current.Name.Numerical)
                {
                    stringBuilder.Append(" (" + current.def.label + ")");
                }
                stringBuilder.AppendLine();
            }
            return string.Format(MizuStrings.AlertDehydrationAnimalDesc.Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(this.DehydratingAnimals.FirstOrDefault<Pawn>());
        }
    }
}
