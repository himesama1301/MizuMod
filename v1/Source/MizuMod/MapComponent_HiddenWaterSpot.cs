using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class MapComponent_HiddenWaterSpot : MapComponent, ICellBoolGiver, IExposable
    {
        private const int RefreshInterval = 60000;

        private CellBoolDrawer drawer;
        private ushort[] spotGrid;
        private HashSet<IntVec3> spotCells;
        public HashSet<IntVec3> SpotCells
        {
            get
            {
                return this.spotCells;
            }
        }
        private int blockSizeX;
        private int blockSizeZ;
        private int allSpotNum;
        private int lastUpdateTick;

        public Color Color { get { return Color.white; } }

        public MapComponent_HiddenWaterSpot(Map map) : base(map)
        {
            this.spotGrid = new ushort[map.cellIndices.NumGridCells];
            this.spotCells = new HashSet<IntVec3>();
            this.drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 1f);
            this.lastUpdateTick = Find.TickManager.TicksGame;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            MapExposeUtility.ExposeUshort(this.map, (c) => this.spotGrid[this.map.cellIndices.CellToIndex(c)], (c, id) =>
            {
                this.spotGrid[this.map.cellIndices.CellToIndex(c)] = id;
            }, "spotGrid");
            Scribe_Collections.Look(ref this.spotCells, "spotCells", LookMode.Value);
            Scribe_Values.Look(ref this.blockSizeX, "blockSizeX");
            Scribe_Values.Look(ref this.blockSizeZ, "blockSizeZ");
            Scribe_Values.Look(ref this.allSpotNum, "allSpotNum");
            Scribe_Values.Look(ref this.lastUpdateTick, "lastUpdateTick");

            if (MizuDef.GlobalSettings.forDebug.enableResetHiddenWaterSpot)
            {
                this.spotCells = new HashSet<IntVec3>();
                this.CreateWaterSpot(
                    MizuDef.GlobalSettings.forDebug.resetHiddenWaterSpotBlockSizeX,
                    MizuDef.GlobalSettings.forDebug.resetHiddenWaterSpotBlockSizeZ,
                    MizuDef.GlobalSettings.forDebug.resetHiddenWaterSpotAllSpotNum);
            }
        }


        public bool GetCellBool(int index)
        {
            return (this.spotGrid[index] != 0);
        }

        public Color GetCellExtraColor(int index)
        {
            return (this.spotGrid[index] != 0) ? new Color(1f, 0.5f, 0.5f, 0.5f) : new Color(1f, 1f, 1f, 0f);
        }

        public void SetDirty()
        {
            if (this.map == Find.VisibleMap)
            {
                this.drawer.SetDirty();
            }

        }
        public void MarkForDraw()
        {
            if (this.map == Find.VisibleMap)
            {
                this.drawer.MarkForDraw();
            }
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();

            this.drawer.CellBoolDrawerUpdate();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (this.lastUpdateTick + RefreshInterval <= Find.TickManager.TicksGame)
            {
                this.lastUpdateTick = Find.TickManager.TicksGame;
                this.CreateWaterSpot(this.blockSizeX, this.blockSizeZ, this.allSpotNum);
                this.SetDirty();
            }
        }

        public void ClearWaterSpot()
        {
            for (int i = 0; i < this.spotGrid.Length; i++)
            {
                this.spotGrid[i] = 0;
            }
            this.spotCells.Clear();
        }

        public void CreateWaterSpot(int blockSizeX, int blockSizeZ, int allSpotNum)
        {
            this.ClearWaterSpot();

            this.blockSizeX = blockSizeX;
            this.blockSizeZ = blockSizeZ;
            this.allSpotNum = allSpotNum;

            int blockNumX = Mathf.CeilToInt((float)this.map.Size.x / 2 / blockSizeX);
            int blockNumZ = Mathf.CeilToInt((float)this.map.Size.z / 2 / blockSizeZ);
            var waterCellMap = new List<IntVec3>[blockNumX * 2, blockNumZ * 2];
            int allWaterNum = 0;

            for (int bx = -blockNumX; bx < blockNumX; bx++)
            {
                for (int bz = -blockNumZ; bz < blockNumZ; bz++)
                {
                    waterCellMap[bx + blockNumX, bz + blockNumZ] = new List<IntVec3>();
                    var waterCells = waterCellMap[bx + blockNumX, bz + blockNumZ];
                    foreach (var c in new CellRect(bx * blockSizeX + this.map.Size.x / 2, bz * blockSizeZ + this.map.Size.z / 2, blockSizeX, blockSizeZ))
                    {
                        if (c.InBounds(this.map) && c.GetTerrain(this.map).IsWaterStandable())
                        {
                            waterCells.Add(c);
                            allWaterNum++;
                        }
                    }
                }
            }

            for (int bx = -blockNumX; bx < blockNumX; bx++)
            {
                for (int bz = -blockNumZ; bz < blockNumZ; bz++)
                {
                    var waterCells = waterCellMap[bx + blockNumX, bz + blockNumZ];
                    int spotNum = Mathf.Min(Mathf.CeilToInt((float)waterCells.Count / allWaterNum * allSpotNum), waterCells.Count);
                    var randomCells = waterCells.InRandomOrder().ToList();
                    for (int i = 0; i < spotNum; i++)
                    {
                        this.spotGrid[this.map.cellIndices.CellToIndex(randomCells[i])] = 1;
                        this.spotCells.Add(randomCells[i]);
                    }
                }
            }
        }
    }
}
