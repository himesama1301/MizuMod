using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public static class UndergroundWaterMaterials
    {
        public const int MaterialCount = 10;

        private static readonly Material[] materials;

        static UndergroundWaterMaterials()
        {
            materials = new Material[MaterialCount];
            Color[] colorArr = new Color[]
            {
                new Color(1.0f, 0.0f, 0.0f, 0.25f),
                new Color(1.0f, 1.0f, 0.0f, 0.25f),
                new Color(0.0f, 1.0f, 0.8f, 0.25f),
                new Color(0.0f, 1.0f, 1.0f, 0.25f),
            };

            for (int i = 0; i < MaterialCount; i++)
            {
                materials[i] = MatsFromSpectrum.Get(colorArr, (float)i / (float)MaterialCount);
            }
        }

        public static Material Mat(int index)
        {
            if (index >= MaterialCount)
            {
                index = MaterialCount - 1;
            }
            if (index < 0)
            {
                index = 0;
            }
            return materials[index];
        }
    }
}
