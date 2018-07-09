using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Thing_Bucket : ThingWithComps
    {
        private List<float> graphicThreshold = new List<float>()
        {
            0.9f,
            100f,
        };

        private int graphicIndex = 0;
        private int prevGraphicIndex = 0;

        public override Graphic Graphic
        {
            get
            {
                return MizuGraphics.Buckets[this.graphicIndex].GetColoredVersion(MizuGraphics.Buckets[this.graphicIndex].Shader, this.DrawColor, this.DrawColorTwo);
            }
        }

        private CompWaterTool compWaterTool;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            this.compWaterTool = this.GetComp<CompWaterTool>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.graphicIndex, "graphicIndex");
            this.prevGraphicIndex = this.graphicIndex;
        }

        public override void Tick()
        {
            base.Tick();

            this.prevGraphicIndex = this.graphicIndex;
            if (this.compWaterTool == null)
            {
                this.graphicIndex = 0;
                return;
            }

            for (int i = 0; i < this.graphicThreshold.Count; i++)
            {
                if (this.compWaterTool.StoredWaterVolumePercent < this.graphicThreshold[i])
                {
                    this.graphicIndex = i;
                    break;
                }
            }

            if (this.graphicIndex != this.prevGraphicIndex)
            {
                this.DirtyMapMesh(this.Map);
            }
        }
    }
}
