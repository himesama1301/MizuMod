using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public interface IBuilding_DrinkWater
    {
        WaterPreferability WaterPreferability { get; }
        bool IsActivated { get; }
        int DrinkWorkAmount { get; }
        bool IsEmpty { get; }

        bool CanDrinkFor(Pawn p);
        void DrawWater(float amount);
    }
}
