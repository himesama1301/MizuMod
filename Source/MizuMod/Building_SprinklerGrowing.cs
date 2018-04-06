using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public class Building_SprinklerGrowing : Building_WaterNet, IBuilding_WaterNet
    {
        private CompPowerTrader compPowerTrader;
        private CompSchedule compSchedule;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPowerTrader = this.GetComp<CompPowerTrader>();
            this.compSchedule = this.GetComp<CompSchedule>();

            this.ResetPowerOutput();
        }

        public override void TickRare()
        {
            base.TickRare();

            if (this.compPowerTrader.PowerOn)
            {
                // 電源ON、故障無し、稼働時間範囲内の時
                if (this.InputWaterNet != null)
                {
                    // 水やり範囲
                    var cells = GenRadial.RadialCellsAround(base.Position, this.def.specialDisplayRadius, true);

                    // 設備の置かれた部屋
                    var room = this.Position.GetRoom(this.Map);

                    // 設備と同じ部屋に属するセル(肥沃度あり)
                    // 暫定で植木鉢は無効とする
                    var sameRoomCells = cells.Where((c) => c.GetRoom(this.Map) == room && this.Map.terrainGrid.TerrainAt(c).fertility >= 0.01f);

                    var wateringComp = this.Map.GetComponent<MapComponent_Watering>();

                    // 10の水やり効果で1L→1の水やり効果で0.1L
                    // 水が足りているかチェック
                    if (this.InputWaterNet.StoredWaterVolumeForFaucet >= 0.1f * sameRoomCells.Count())
                    {
                        // 水を減らしてからセルに水やり効果
                        this.InputWaterNet.DrawWaterVolumeForFaucet(0.1f * sameRoomCells.Count());
                        foreach (var c in sameRoomCells)
                        {
                            wateringComp.Add(this.Map.cellIndices.CellToIndex(c), 1);
                            this.Map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlag.Terrain;

                            // 水やりエフェクト(仮)
                            var mote = (MoteThrown)ThingMaker.MakeThing(MizuDef.Mote_SprinklerWater);
                            //mote.Scale = 1f;
                            //mote.rotationRate = (float)(Rand.Chance(0.5f) ? -30 : 30);
                            mote.exactPosition = c.ToVector3Shifted();
                            GenSpawn.Spawn(mote, c, base.Map);
                        }
                    }
                }
            }

            this.ResetPowerOutput();

        }

        private void ResetPowerOutput()
        {
            if (this.compSchedule.Allowed)
            {
                // 稼働中の消費電力
                this.compPowerTrader.PowerOutput = -this.compPowerTrader.Props.basePowerConsumption;
            }
            else
            {
                // 非稼働時の消費電力
                this.compPowerTrader.PowerOutput = -this.compPowerTrader.Props.basePowerConsumption * 0.1f;
            }
        }
    }
}
