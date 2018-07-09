using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public abstract class MapComponent_WaterGrid : MapComponent, ICellBoolGiver, IExposable
    {
        private CellBoolDrawer drawer;
        private ushort[] poolIDGrid;
        private List<UndergroundWaterPool> pools = new List<UndergroundWaterPool>();

        public Color Color { get { return Color.white; } }

        public MapComponent_WaterGrid(Map map) : base(map)
        {
            this.poolIDGrid = new ushort[map.cellIndices.NumGridCells];
            this.drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 1f);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            MapExposeUtility.ExposeUshort(this.map, (c) => this.poolIDGrid[this.map.cellIndices.CellToIndex(c)], (c, id) =>
            {
                this.poolIDGrid[this.map.cellIndices.CellToIndex(c)] = id;
            }, "poolIDGrid");

            Scribe_Collections.Look<UndergroundWaterPool>(ref this.pools, "pools", LookMode.Deep, new object[]
            {
                this
            });
        }

        public void AddWaterPool(UndergroundWaterPool pool, IEnumerable<IntVec3> cells)
        {
            List<UndergroundWaterPool> mergePools = new List<UndergroundWaterPool>();
            mergePools.Add(pool);

            // 既存の水源と被るセルを調べる
            foreach (var c in cells)
            {
                if (this.GetID(c) != 0)
                {
                    var existPool = this.pools.Find((p) => p.ID == this.GetID(c));
                    if (existPool == null)
                    {
                        Log.Error("existPool is null");
                    }
                    if (!mergePools.Contains(existPool))
                    {
                        mergePools.Add(existPool);
                    }
                }
            }

            // 上書き覚悟でとりあえず追加
            this.pools.Add(pool);
            foreach (var c in cells)
            {
                this.SetID(c, pool.ID);
            }

            if (mergePools.Count >= 2)
            {
                // 最小の水源IDのものに統合する

                // 最小の水源IDを調べる
                UndergroundWaterPool minPool = null;
                foreach (var p in mergePools)
                {
                    if (minPool == null || minPool.ID > p.ID)
                    {
                        minPool = p;
                    }
                }

                // 統合される水源から最小IDのものを除き、消滅予定水源リストに変換
                mergePools.Remove(minPool);

                // 水源リストから消滅予定水源を除去しつつ水量を統合
                foreach (var p in mergePools)
                {
                    this.pools.Remove(p);
                    minPool.MergeWaterVolume(p);
                }

                // 全セルを調べ、消滅予定水源IDの場所を最小IDに変更
                for (int i = 0; i < this.poolIDGrid.Length; i++)
                {
                    //Log.Message("i=" + i.ToString());
                    if (mergePools.Find((p) => p.ID == this.GetID(i)) != null)
                    {
                        this.SetID(i, minPool.ID);
                    }
                }
            }
        }

        public void ModifyPoolGrid()
        {
            List<IntVec3> nearVecs = new List<IntVec3>();
            for (int x = 0; x < this.map.Size.x; x++)
            {
                for (int z = 0; z < this.map.Size.z; z++)
                {
                    int curIndex = this.map.cellIndices.CellToIndex(new IntVec3(x, 0, z));
                    if (this.GetID(curIndex) == 0)
                    {
                        continue;
                    }

                    nearVecs.Clear();
                    if ((x - 1) >= 0) nearVecs.Add(new IntVec3(x - 1, 0, z));
                    if ((x + 1) < this.map.Size.x) nearVecs.Add(new IntVec3(x + 1, 0, z));
                    if ((z - 1) >= 0) nearVecs.Add(new IntVec3(x, 0, z - 1));
                    if ((z + 1) < this.map.Size.z) nearVecs.Add(new IntVec3(x, 0, z + 1));

                    foreach (var nearVec in nearVecs)
                    {
                        int nearIndex = this.map.cellIndices.CellToIndex(nearVec);
                        if (this.GetID(nearIndex) == 0)
                        {
                            continue;
                        }

                        if (this.GetID(curIndex) != this.GetID(nearIndex))
                        {
                            var curPool = this.pools.Find((p) => p.ID == this.GetID(curIndex));
                            if (curPool == null)
                            {
                                Log.Error("curPool is null");
                            }
                            var nearPool = this.pools.Find((p) => p.ID == this.GetID(nearIndex));
                            if (nearPool == null)
                            {
                                Log.Error("nearPool is null");
                            }
                            curPool.MergePool(nearPool, this.poolIDGrid);
                            this.pools.Remove(nearPool);

                            break;
                        }
                    }
                }
            }
        }

        public int GetID(int index)
        {
            return (int)this.poolIDGrid[index];
        }
        public int GetID(IntVec3 c)
        {
            return this.GetID(this.map.cellIndices.CellToIndex(c));
        }
        public void SetID(int index, int id)
        {
            this.poolIDGrid[index] = (ushort)id;
        }
        public void SetID(IntVec3 c, int id)
        {
            this.SetID(this.map.cellIndices.CellToIndex(c), id);
        }
        public UndergroundWaterPool GetPool(int index)
        {
            return pools.Find((p) => p.ID == this.poolIDGrid[index]);
        }
        public bool GetCellBool(int index)
        {
            return (this.poolIDGrid[index] != 0);
        }

        public Color GetCellExtraColor(int index)
        {
            var pool = pools.Find((p) => p.ID == this.poolIDGrid[index]);
            return UndergroundWaterMaterials.Mat(Mathf.RoundToInt(pool.CurrentWaterVolumePercent * UndergroundWaterMaterials.MaterialCount)).color;
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

            foreach (var pool in this.pools)
            {
                pool.Update();
            }
        }
    }
}
