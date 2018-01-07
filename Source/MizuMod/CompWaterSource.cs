using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class CompWaterSource : ThingComp
    {
        public CompProperties_WaterSource Props { get { return (CompProperties_WaterSource)this.props; } }

        public CompProperties_WaterSource.SourceType SourceType {  get { return this.Props.sourceType; } }
        public EffecterDef GetEffect { get { return this.Props.getEffect; } }
        public SoundDef GetSound { get { return this.Props.getSound; } }
        public int BaseDrinkTicks { get { return this.Props.baseDrinkTicks; } }
        public bool NeedManipulate { get { return this.Props.needManipulate; } }
        public float WaterAmount { get { return this.Props.waterAmount; } }
        public int MaxNumToGetAtOnce { get { return this.Props.maxNumToGetAtOnce; } }
        public float WaterVolume { get { return this.Props.waterVolume; } }

        public WaterType WaterType
        {
            get
            {
                switch (this.SourceType)
                {
                    case CompProperties_WaterSource.SourceType.Item:
                        return this.Props.waterType;
                    case CompProperties_WaterSource.SourceType.Building:
                        var building = this.parent as IBuilding_DrinkWater;
                        if (building == null) return WaterType.Undefined;
                        return building.WaterType;
                    default:
                        return WaterType.Undefined;
                }
            }
        }

        public bool IsWaterSource
        {
            get
            {
                return (this.WaterType != WaterType.Undefined && this.WaterType != WaterType.NoWater);
            }
        }

        //public override void PostIngested(Pawn ingester)
        //{
        //    base.PostIngested(ingester);

        //    Need_Water need_water = ingester.needs.water();
        //    if (need_water == null) return;

        //    float gotWaterAmount = MizuUtility.GetWater(ingester, this.parent, need_water.WaterWanted, true);
        //    if (!ingester.Dead)
        //    {
        //        need_water.CurLevel += gotWaterAmount;
        //    }
        //    ingester.records.AddTo(MizuDef.Record_WaterDrank, gotWaterAmount);
        //}

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var floatMenuOption in base.CompFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }

            if (this.SourceType == CompProperties_WaterSource.SourceType.Item && !this.parent.def.IsIngestible)
            {
                // 水アイテムで、食べることが出来ないものは飲める

                if (selPawn.IsColonistPlayerControlled)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(string.Format(MizuStrings.FloatMenuGetWater, this.parent.LabelNoCount));

                    if (!this.parent.IsSociallyProper(selPawn))
                    {
                        // 囚人部屋のものは表示を追加
                        stringBuilder.Append(string.Concat(
                            " (",
                            "ReservedForPrisoners".Translate(),
                            ")"
                        ));
                    }
                    foreach (var p in this.parent.Map.mapPawns.AllPawns)
                    {
                        if (this.parent.Map.reservationManager.ReservedBy(this.parent, p))
                        {
                            // 予約されている物は表示を追加
                            stringBuilder.AppendLine();
                            stringBuilder.Append(string.Format(string.Concat(
                                " (",
                                "ReservedBy".Translate(),
                                ")"),
                            p.NameStringShort));

                            break;
                        }
                    }

                    yield return new FloatMenuOption(stringBuilder.ToString(), () =>
                    {
                        Job job = new Job(MizuDef.Job_DrinkWater, this.parent)
                        {
                            count = MizuUtility.WillGetStackCountOf(selPawn, this.parent)
                        };
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                    });
                }
            }
        }
    }
}
