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
        MapComponent_WaterNetManager WaterNetManager { get; }
        WaterNet InputWaterNet { get; set; }
        WaterNet OutputWaterNet { get; set; }
        Map Map { get; }
        WaterType OutputWaterType { get; }
        //List<IntVec3> Connectors { get; }
        List<IntVec3> InputConnectors { get; }
        List<IntVec3> OutputConnectors { get; }
        T GetComp<T>() where T : ThingComp;
        CellRect OccupiedRect();

        void CreateConnectors();
        void PrintForGrid(SectionLayer sectionLayer);
    }
}
