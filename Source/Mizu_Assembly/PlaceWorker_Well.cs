using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_Well : PlaceWorker
    {
        private const float MinFertility = 0.5f;
        private const float MinDistance = 20.0f;
        private const float MinDistanceSquared = MinDistance * MinDistance;

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
            ThingDef def = checkingDef as ThingDef;
            if (def == null)
            {
                return false;
            }

            IntVec3 vec_z = rot.FacingCell;
            Rot4 rot_x = new Rot4(rot.AsInt);
            rot_x.Rotate(RotationDirection.Clockwise);
            IntVec3 vec_x = rot_x.FacingCell;

            bool cond_ok = true;

            // 肥沃度チェック
            for (int x = 0; x < def.Size.x; x++)
            {
                for (int z = 0; z < def.Size.z; z++)
                {
                    IntVec3 cur_vec = loc + vec_x * x + vec_z * z;
                    TerrainDef terrainLoc = this.Map.terrainGrid.TerrainAt(cur_vec);
                    if (terrainLoc.fertility < MinFertility)
                    {
                        cond_ok = false;
                        break;
                    }
                }
            }

            // 井戸同士の距離チェック
            List<Thing> other_wells = this.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).FindAll((t) => t.def == def);
            List<Thing> other_wells_blueprint = this.Map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).FindAll((t) => t.def.defName.Contains(def.defName));
            other_wells.AddRange(other_wells_blueprint);
            for (int i = 0; i < other_wells.Count; i++)
            {
                if ((loc - other_wells[i].Position).LengthHorizontalSquared < MinDistanceSquared)
                {
                    cond_ok = false;
                    break;
                }
            }
            return cond_ok;
        }
    }
}
