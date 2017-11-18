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
        MapComponent_WaterNetManager WaterNetManager { get; }
        WaterNet WaterNet { get; set; }
        Map Map { get; }
        WaterType OutputWaterType { get; }
        List<IntVec3> Connectors { get; }
        T GetComp<T>() where T : ThingComp;
        CellRect OccupiedRect();

        void CreateConnectors();
        void PrintForGrid(SectionLayer sectionLayer);
    }
}
