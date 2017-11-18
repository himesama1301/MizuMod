using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MizuMod
{
    public class Building_WaterFilter : Building_WaterNet, IBuilding_WaterNet
    {
        public override bool IsSameConnector
        {
            get
            {
                return false;
            }
        }

        public override void CreateConnectors()
        {
            this.InputConnectors.Clear();
            this.OutputConnectors.Clear();

            this.InputConnectors.Add(this.Position + this.Rotation.FacingCell * (-1));
            this.OutputConnectors.Add(this.Position + this.Rotation.FacingCell);
        }
    }
}
