using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrinkWater : JobDriver
    {
        private const TargetIndex WaterIndex = TargetIndex.A;

        private bool drinkingFromInventory;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.drinkingFromInventory, "drinkingFromInventory", false, false);
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.drinkingFromInventory = (this.pawn.inventory != null && this.pawn.inventory.Contains(this.TargetA.Thing));
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (this.job.targetA.HasThing)
            {
                // ターゲットがThing=水アイテムを摂取する場合

                // 水が使用不可能になったらFail
                ToilFailConditions.FailOnDestroyedNullOrForbidden<JobDriver_DrinkWater>(this, WaterIndex);

                // 水を予約
                if (!this.pawn.Map.reservationManager.ReservedBy(this.TargetA.Thing, pawn))
                {
                    yield return Toils_Reserve.Reserve(WaterIndex, 1, this.job.count);
                }

                // 水を取得
                if (this.drinkingFromInventory)
                {
                    // 所持品から取り出す
                    yield return Toils_Mizu.StartCarryFromInventory(WaterIndex);
                }
                else
                {
                    // 水の場所まで行く
                    yield return Toils_Goto.Goto(WaterIndex, PathEndMode.OnCell);

                    // 水を拾う
                    yield return Toils_Ingest.PickupIngestible(WaterIndex, this.pawn);
                }

                // 飲む場所を決めてそこへ移動
                yield return Toils_Mizu.StartPathToDrinkSpot(WaterIndex);

                // 水を摂取
                yield return Toils_Mizu.Drink(WaterIndex);

                // 水の摂取終了(心情、水分、アイテム個数の処理)
                yield return Toils_Mizu.FinishDrink(WaterIndex);

                if (this.drinkingFromInventory && !this.TargetA.ThingDestroyed)
                {
                    // 所持品から取り出した＆まだ残っている場合は所持品に戻す
                    yield return Toils_Mizu.AddCarriedThingToInventory();
                }
            }
            else
            {
                // ターゲットがThingではない=水アイテムを摂取しない場合=水地形を利用する場合

                // 選んだ水地形が使用不可能or到達不可能になったらFail
                ToilFailConditions.FailOn<JobDriver_DrinkWater>(this, () =>
                {
                    return this.job.targetA.Cell.IsForbidden(pawn) || !pawn.CanReach(this.job.targetA.Cell, PathEndMode.OnCell, Danger.Deadly);
                });

                // 水地形まで移動
                yield return Toils_Goto.GotoCell(WaterIndex, PathEndMode.OnCell);

                // 水地形から水分を摂取
                yield return Toils_Mizu.DrinkTerrain(WaterIndex);

                // 終了
                yield return Toils_Mizu.FinishDrinkTerrain(WaterIndex);
            }
        }
    }
}
