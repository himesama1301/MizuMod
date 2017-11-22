using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DoBillDrawing : JobDriver_DoBill
    {
        private const TargetIndex BillGiverIndex = TargetIndex.A;

        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.GetTarget(BillGiverIndex), this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 設備が使えなくなったら失敗
            ToilFailConditions.FailOnDespawnedNullOrForbidden<JobDriver_DoBillDrawing>(this, BillGiverIndex);

            // 水汲み中に地形が変化して水が汲めなくなったら失敗
            Toils_Mizu.FailOnChangingTerrain<JobDriver_DoBillDrawing>(this, BillGiverIndex);

            // 設備まで行く
            yield return Toils_Goto.GotoCell(this.job.GetTarget(BillGiverIndex).Thing.InteractionCell, PathEndMode.OnCell);

            // レシピ実行
            yield return Toils_Mizu.DoRecipeWorkDrawing(BillGiverIndex);

            // レシピ終了処理
            yield return Toils_Mizu.FinishRecipeAndStartStoringProduct(() =>
            {
                // 現在の地形から水の種類を決定
                TerrainDef terrainDef = this.Map.terrainGrid.TerrainAt(this.job.GetTarget(BillGiverIndex).Thing.Position);
                var waterTerrainType = terrainDef.GetWaterTerrainType();
                var waterThingDef = MizuUtility.GetWaterThingDefFromTerrainType(waterTerrainType);

                // 水を生成
                return ThingMaker.MakeThing(waterThingDef);
            });
        }
    }
}
