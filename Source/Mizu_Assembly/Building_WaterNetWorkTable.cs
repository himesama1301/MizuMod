using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_WaterNetWorkTable : Building_WorkTable, IBuilding_WaterNet
    {
        public virtual bool HasConnector
        {
            get
            {
                return true;
            }
        }
        public virtual bool HasInputConnector
        {
            get
            {
                return false;
            }
        }
        public virtual bool HasOutputConnector
        {
            get
            {
                return false;
            }
        }
        public MapComponent_WaterNetManager WaterNetManager
        {
            get
            {
                return this.Map.GetComponent<MapComponent_WaterNetManager>();
            }
        }
        public WaterNet WaterNet { get; set; }
        public virtual WaterType OutputWaterType
        {
            get
            {
                return WaterType.NoWater;
            }
        }
        public List<IntVec3> Connectors { get; private set; }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.Connectors = new List<IntVec3>();
            this.CreateConnectors();

            this.WaterNetManager.AddThing(this);
        }

        public override void DeSpawn()
        {
            this.WaterNetManager.RemoveThing(this);

            base.DeSpawn();
        }

        public List<IntVec3> ConnectVecs
        {
            get
            {
                List<IntVec3> vecs = new List<IntVec3>();
                CellRect rect = this.OccupiedRect().ExpandedBy(1);

                foreach (var cell in rect.EdgeCells)
                {
                    if (cell.x == rect.minX && cell.z == rect.minZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.minX && cell.z == rect.maxZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.maxX && cell.z == rect.minZ)
                    {
                        continue;
                    }
                    if (cell.x == rect.maxX && cell.z == rect.maxZ)
                    {
                        continue;
                    }
                    vecs.Add(cell);
                }

                return vecs;
            }
        }

        public virtual void CreateConnectors()
        {
            this.Connectors.Clear();
            CellRect rect = this.OccupiedRect().ExpandedBy(1);

            foreach (var cell in rect.EdgeCells)
            {
                if (cell.x == rect.minX && cell.z == rect.minZ)
                {
                    continue;
                }
                if (cell.x == rect.minX && cell.z == rect.maxZ)
                {
                    continue;
                }
                if (cell.x == rect.maxX && cell.z == rect.minZ)
                {
                    continue;
                }
                if (cell.x == rect.maxX && cell.z == rect.maxZ)
                {
                    continue;
                }
                this.Connectors.Add(cell);
            }
        }

        public virtual void PrintForGrid(SectionLayer sectionLayer)
        {
            if (this.HasConnector)
            {
                MizuGraphics.LinkedWaterNetOverlay.Print(sectionLayer, this);
            }
        }

        public virtual CellRect OccupiedRect()
        {
            return GenAdj.OccupiedRect(this);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (DebugSettings.godMode)
            {
                if (stringBuilder.ToString() != string.Empty)
                {
                    stringBuilder.AppendLine();
                }
                if (this.WaterNet != null)
                {
                    stringBuilder.Append(string.Join(",", new string[] {
                        string.Format("NetID({0})", this.WaterNet.ID),
                        string.Format("In({0})", this.WaterNet.LastInputWaterFlow.ToString("F0")),
                        string.Format("Out({0})", this.WaterNet.LastOutputWaterFlow.ToString("F0")),
                        string.Format("Stored({0})", this.WaterNet.StoredWaterVolume.ToString("F0")),
                        string.Format("Type({0})", this.WaterNet.WaterType),
                    }));
                }
                else
                {
                    stringBuilder.Append("Net(null)");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
