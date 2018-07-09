using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Alert_LowWater : Alert
    {
        private const float WaterAmountThresholdPerColonist = 4f;

        public Alert_LowWater()
        {
            this.defaultLabel = MizuStrings.AlertLowWater.Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override string GetExplanation()
        {
            Map map = this.MapWithLowWater();
            if (map == null)
            {
                return string.Empty;
            }
            float totalWater = map.resourceCounter.TotalWater();
            int num = map.mapPawns.FreeColonistsSpawnedCount + (from pr in map.mapPawns.PrisonersOfColony
                                                                where pr.guest.GetsFood
                                                                select pr).Count<Pawn>();
            int num2 = Mathf.FloorToInt(totalWater / (float)num);
            return string.Format(MizuStrings.AlertLowWaterDesc.Translate(), totalWater.ToString("F0"), num.ToStringCached(), num2.ToStringCached());
        }

        public override AlertReport GetReport()
        {
            if (Find.TickManager.TicksGame < 150000)
            {
                return false;
            }
            return this.MapWithLowWater() != null;
        }

        private Map MapWithLowWater()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map.IsPlayerHome)
                {
                    int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
                    if (map.resourceCounter.TotalWater() < WaterAmountThresholdPerColonist * (float)freeColonistsSpawnedCount)
                    {
                        return map;
                    }
                }
            }
            return null;
        }
    }
}
