using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrawFromTerrain : JobDriver_DrawWater
    {
        protected override void SetFailCondition()
        {
            // 水汲み中に地形が変化して水が汲めなくなったら失敗
            Toils_Mizu.FailOnChangingTerrain(this, BillGiverInd, recipe.needWaterTerrainTypes);
        }

        protected override Thing FinishAction()
        {
            // 現在の地形から水の種類を決定
            TerrainDef terrainDef = this.Map.terrainGrid.TerrainAt(this.job.GetTarget(BillGiverInd).Thing.Position);
            var waterTerrainType = terrainDef.GetWaterTerrainType();
            var waterThingDef = MizuUtility.GetWaterThingDefFromTerrainType(waterTerrainType);

            // 水を生成
            var createThing = ThingMaker.MakeThing(waterThingDef);
            if (createThing == null) return null;

            // 個数設定
            createThing.stackCount = recipe.getItemCount;
            return createThing;
        }
    }
}
