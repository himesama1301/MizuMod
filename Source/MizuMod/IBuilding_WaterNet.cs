using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public interface IBuilding_WaterNet
    {
        bool HasConnector { get; }
        bool HasInputConnector { get; }
        bool HasOutputConnector { get; }
        bool IsSameConnector { get; }
        bool IsActivated { get; }
        bool IsActivatedForWaterNet { get; }
        bool PowerOn { get; }
        bool SwitchIsOn { get; }
        CompWaterNetInput InputComp { get; }
        CompWaterNetOutput OutputComp { get; }
        CompWaterNetTank TankComp { get; }
        UndergroundWaterPool WaterPool { get; }

        MapComponent_WaterNetManager WaterNetManager { get; }
        WaterNet InputWaterNet { get; set; }
        WaterNet OutputWaterNet { get; set; }
        Map Map { get; }
        WaterType OutputWaterType { get; }
        List<IntVec3> InputConnectors { get; }
        List<IntVec3> OutputConnectors { get; }
        T GetComp<T>() where T : ThingComp;
        CellRect OccupiedRect();
        bool IsAdjacentToCardinalOrInside(IBuilding_WaterNet other);

        void CreateConnectors();
        void PrintForGrid(SectionLayer sectionLayer);
    }
}
