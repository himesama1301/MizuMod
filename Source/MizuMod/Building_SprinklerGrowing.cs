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

        private const float UseWaterVolumePerOne = 0.1f;
        private const int ExtinguishPower = 50;

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

            // デバッグオプションがONなら時間設定や電力状態を無視
            if (this.compPowerTrader.PowerOn || MizuDef.GlobalSettings.forDebug.enableAlwaysActivateSprinklerGrowing)
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
                    float useWaterVolume = UseWaterVolumePerOne * sameRoomCells.Count();

                    // デバッグオプションがONなら消費貯水量を0.1Lにする
                    if (MizuDef.GlobalSettings.forDebug.enableAlwaysActivateSprinklerGrowing)
                    {
                        useWaterVolume = 0.1f;
                    }

                    if (this.InputWaterNet.StoredWaterVolumeForFaucet >= useWaterVolume)
                    {
                        // 水を減らしてからセルに水やり効果
                        this.InputWaterNet.DrawWaterVolumeForFaucet(useWaterVolume);
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

                            // 消火効果(仮)
                            // 複製しないとダメージを受けて消えた時点で元のリストから除外されてエラーになる
                            var fireList = new List<Fire>(this.Map.thingGrid.ThingsListAt(c).Where((t) => t is Fire).Select((t) => t as Fire));
                            foreach (var fire in fireList)
                            {
                                fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, ExtinguishPower));
                            }
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
