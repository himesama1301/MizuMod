using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Faucet : Building_WaterNetWorkTable, IBuilding_WaterNet
    {
        public override void CreateConnectors()
        {
            this.InputConnectors.Clear();
            this.OutputConnectors.Clear();

            this.InputConnectors.Add(this.Position + this.Rotation.FacingCell);
            this.OutputConnectors.Add(this.Position + this.Rotation.FacingCell);
        }
    }
}
