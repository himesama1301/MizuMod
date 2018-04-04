using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class WorkGiver_WaterFarm : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        //public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        //{
        //    return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where((t) => t is IPlantToGrowSettable);
        //}

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            IEnumerable<IntVec3> potentialCells = null;

            // 農地チェック
            var growingZoneList = pawn.Map.zoneManager.AllZones.Where((zone) => zone is Zone_Growing);
            foreach (var zone in growingZoneList)
            {
                if (potentialCells == null)
                {
                    potentialCells = zone.Cells.AsEnumerable();
                }
                else
                {
                    potentialCells = potentialCells.Concat(zone.Cells);
                }
            }

            // 植木鉢チェック
            foreach (var building in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where((t) => t is Building_PlantGrower))
            {
                if (building.def.building == null || building.def.building.sowTag == null || building.def.building.sowTag == "Hydroponic") continue;

                if (potentialCells == null)
                {
                    potentialCells = building.OccupiedRect().Cells;
                }
                else
                {
                    potentialCells = potentialCells.Concat(building.OccupiedRect().Cells);
                }
            }

            if (potentialCells == null)
            {
                potentialCells = new List<IntVec3>();
            }
            return potentialCells;
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            // プレイヤー派閥でないなら何もしない
            if (pawn.Faction != Faction.OfPlayer) return false;

            // 水やりが必要かどうかチェック
            // 種植え可能＆成長しきっていない植物があれば水やりが必要
            bool needWatering = false;
            var thingList = pawn.Map.thingGrid.ThingsAt(c);
            foreach (var thing in thingList)
            {
                var plant = thing as Plant;
                var building = thing as Building_PlantGrower;
                if (plant != null)
                {
                    // 植物

                    // 種植え情報を持っていない(植えられない)
                    if (plant.def.plant.sowTags == null || plant.def.plant.sowTags.Count <= 0) continue;

                    // 既に育ち切っている
                    if (plant.Growth >= 1.0f) continue;

                    // そのセルには水やりが必要
                    needWatering = true;
                    break;
                }
                else if (building != null)
                {
                    // 植物を植えられる建造物

                    // 建造物上の植物チェック
                    foreach (var p in building.PlantsOnMe)
                    {
                        // 現在注目しているセルではない
                        if (p.Position != c) continue;

                        // 既に育ち切っている
                        if (p.Growth >= 1.0f) continue;

                        // そのセルには水やりが必要
                        needWatering = true;
                        break;
                    }
                }

            }
            if (!needWatering) return false;

            // 既に水やりされている場所にはやらない
            var mapComp = pawn.Map.GetComponent<MapComponent_Watering>();
            if (mapComp == null) return false;
            if (mapComp.Get(pawn.Map.cellIndices.CellToIndex(c)) > 0) return false;

            // その場所を予約できないならやらない
            if (!pawn.CanReserve(c)) return false;

            // ツールチェック
            var toolList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where((t) =>
            {
                // 使用禁止チェック
                if (t.IsForbidden(pawn)) return false;

                var comp = t.TryGetComp<CompWaterTool>();
                if (comp == null) return false;
                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.WaterFarm)) return false;

                int maxQueueLength = (int)Mathf.Floor(comp.StoredWaterVolume / JobDriver_Mop.ConsumeWaterVolume);
                if (maxQueueLength <= 0) return false;

                return true;
            });
            if (toolList.Count() == 0) return false;
            if (toolList.Where((t) => pawn.CanReserve(t)).Count() == 0) return false;

            return true;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            // ジョブ作成
            Job job = new Job(MizuDef.Job_WaterFarm);
            job.AddQueuedTarget(TargetIndex.A, cell);

            // 一番近いツールを探す
            Thing candidateTool = null;
            int minDist = int.MaxValue;
            var toolList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where((t) =>
            {
                // 使用禁止チェック
                if (t.IsForbidden(pawn)) return false;

                var comp = t.TryGetComp<CompWaterTool>();
                if (comp == null) return false;
                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.WaterFarm)) return false;

                int maxQueueLengthForCheck = (int)Mathf.Floor(comp.StoredWaterVolume / JobDriver_WaterFarm.ConsumeWaterVolume);
                if (maxQueueLengthForCheck <= 0) return false;

                return true;
            });

            foreach (var tool in toolList)
            {
                // 予約できないツールはパス
                if (!pawn.CanReserve(tool)) continue;

                int toolDist = (tool.Position - pawn.Position).LengthHorizontalSquared;
                if (minDist > toolDist)
                {
                    minDist = toolDist;
                    candidateTool = tool;
                }
            }

            if (candidateTool == null)
            {
                Log.Error("candidateTool is null");
                return null;
            }

            // ツールをTargetBにセット
            job.targetB = candidateTool;
            job.count = 1;

            var compTool = candidateTool.TryGetComp<CompWaterTool>();
            int maxQueueLength = Mathf.RoundToInt(compTool.StoredWaterVolume / JobDriver_WaterFarm.ConsumeWaterVolume);
            Map map = pawn.Map;
            Room room = cell.GetRoom(map);
            for (int i = 0; i < 100; i++)
            {
                // 対象のセルの周囲100マスをサーチ
                IntVec3 intVec = cell + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
                {
                    // そこが同じ部屋の中
                    if (this.HasJobOnCell(pawn, intVec) && intVec != cell)
                    {
                        // 同じジョブが作成可能であるならこのジョブの処理対象に追加
                        job.AddQueuedTarget(TargetIndex.A, intVec);
                    }

                    // 最大個数チェック
                    if (job.GetTargetQueue(TargetIndex.A).Count >= maxQueueLength) break;
                }
            }

            if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
            {
                // 対象が5個以上あるならポーンからの距離が近い順に仕事をさせる
                job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
            }

            return job;
        }
    }
}
