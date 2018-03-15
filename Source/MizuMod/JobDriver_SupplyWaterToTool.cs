using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;
using RimWorld;

namespace MizuMod
{
    public class JobDriver_SupplyWaterToTool : JobDriver
    {
        private const TargetIndex SourceInd = TargetIndex.A;
        private const TargetIndex ToolInd = TargetIndex.B;
        private const TargetIndex StoreToolPosInd = TargetIndex.C;

        private ThingWithComps SourceThing
        {
            get
            {
                return (ThingWithComps)this.job.GetTarget(SourceInd).Thing;
            }
        }
        private ThingWithComps Tool
        {
            get
            {
                return (ThingWithComps)this.job.GetTarget(ToolInd).Thing;
            }
        }

        public override bool TryMakePreToilReservations()
        {
            this.pawn.Reserve(SourceThing, this.job);
            this.pawn.Reserve(Tool, this.job);
            return true;
        }

        private float maxTick;
        private bool needManipulate;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 水ツールを手に取る
            yield return Toils_Goto.GotoThing(ToolInd, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(ToolInd);

            // 水汲み設備へ移動
            PathEndMode peMode = PathEndMode.ClosestTouch;
            if (SourceThing.def.hasInteractionCell)
            {
                peMode = PathEndMode.InteractionCell;
            }
            yield return Toils_Goto.GotoThing(SourceInd, peMode);

            // 水汲み
            Toil supplyToil = new Toil();
            supplyToil.initAction = () =>
            {
                var compSource = SourceThing.GetComp<CompWaterSource>();
                this.needManipulate = compSource.NeedManipulate;

                // 水汲み速度関連はリファクタリングしたい
                var ticksForFull = compSource.BaseDrinkTicks;

                var compTool = Tool.GetComp<CompWaterTool>();
                var totalTicks = (int)(ticksForFull * (1f - compTool.StoredWaterVolumePercent));
                if (!needManipulate)
                {
                    // 手が必要ない→水にドボンですぐに補給できる
                    totalTicks /= 10;
                }
                // 小数の誤差を考慮して1Tick余分に多く実行する
                totalTicks += 1;

                this.maxTick = totalTicks;
                this.ticksLeftThisToil = totalTicks;
            };
            supplyToil.tickAction = () =>
            {
                var compSource = SourceThing.GetComp<CompWaterSource>();
                var compTool = Tool.GetComp<CompWaterTool>();
                var building = SourceThing as IBuilding_DrinkWater;

                var supplyWaterVolume = compTool.MaxWaterVolume / compSource.BaseDrinkTicks;
                if (!needManipulate)
                {
                    supplyWaterVolume *= 10;
                }
                compTool.StoredWaterVolume += supplyWaterVolume;
                compTool.StoredWaterType = building.WaterType;

                building.DrawWater(supplyWaterVolume);

                if (building.IsEmpty)
                {
                    this.ReadyForNextToil();
                }
            };
            supplyToil.defaultCompleteMode = ToilCompleteMode.Delay;
            supplyToil.WithProgressBar(SourceInd, () => 1f - (float)this.ticksLeftThisToil / this.maxTick, true, -0.5f);
            supplyToil.EndOnDespawnedOrNull(SourceInd);
            yield return supplyToil;

            // 水ツールを戻す
            yield return Toils_Mizu.TryFindStoreCell(ToolInd, StoreToolPosInd);
            yield return Toils_Goto.GotoCell(StoreToolPosInd, PathEndMode.OnCell);
            yield return Toils_Haul.PlaceHauledThingInCell(StoreToolPosInd, null, true);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref this.maxTick, "maxTick");
            Scribe_Values.Look(ref this.needManipulate, "needManipulate");
        }
    }
}
