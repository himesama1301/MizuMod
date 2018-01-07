using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public interface IBuilding_DrinkWater
    {
        WaterType WaterType { get; }
        bool IsActivated { get; }
        bool IsEmpty { get; }

        bool CanDrinkFor(Pawn p);
        bool CanDrawFor(Pawn p);
        void DrawWater(float amount);
    }
}
