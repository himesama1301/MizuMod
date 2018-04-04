using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class MapComponent_Watering : MapComponent, IExposable
    {
        public const ushort MaxWateringValue = 10;
        public const int IntervalTicks = 250;

        // 水やり効果がなくなるまでの残りTick
        private ushort[] wateringGrid;

        private int elapsedTicks;
        private int randomIndex;
        public MapComponent_Watering(Map map) : base(map)
        {
            this.elapsedTicks = 0;
            this.randomIndex = 0;
            this.wateringGrid = new ushort[map.cellIndices.NumGridCells];
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref this.elapsedTicks, "elapsedTicks");
            Scribe_Values.Look(ref this.randomIndex, "randomIndex");
            MapExposeUtility.ExposeUshort(this.map, (c) => this.wateringGrid[this.map.cellIndices.CellToIndex(c)], (c, val) =>
            {
                this.wateringGrid[this.map.cellIndices.CellToIndex(c)] = val;
            }, "wateringGrid");
        }

        public ushort GetWateringValue(int index)
        {
            return this.wateringGrid[index];
        }

        public void SetWateringValue(int index, ushort val)
        {
            this.wateringGrid[index] = (val < MaxWateringValue) ? val : MaxWateringValue;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            
            this.elapsedTicks++;
            if (this.elapsedTicks >= IntervalTicks)
            {
                this.elapsedTicks = 0;

                int numCells = this.map.Area * IntervalTicks / 60000 * 10;

                for (int i = 0; i < numCells; i++)
                {
                    var c = this.map.cellsInRandomOrder.Get(this.randomIndex);
                    var terrain = this.map.terrainGrid.TerrainAt(c);
                    if (this.map.weatherManager.RainRate > 0.5f && !this.map.roofGrid.Roofed(c) && terrain.fertility >= 0.01f)
                    {
                        // 雨が降れば水やり効果
                        this.wateringGrid[this.map.cellIndices.CellToIndex(c)] = 10;
                        this.map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlag.Terrain;
                    }
                    else if (this.wateringGrid[this.map.cellIndices.CellToIndex(c)] > 0)
                    {
                        // 水が渇く
                        this.wateringGrid[this.map.cellIndices.CellToIndex(c)]--;
                        if (this.wateringGrid[this.map.cellIndices.CellToIndex(c)] == 0)
                        {
                            this.map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlag.Terrain;
                        }
                    }

                    this.randomIndex++;
                    if (this.randomIndex >= this.map.cellIndices.NumGridCells)
                    {
                        this.randomIndex = 0;
                    }
                }
            }
        }
    }
}
