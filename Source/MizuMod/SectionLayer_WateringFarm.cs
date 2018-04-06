using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class SectionLayer_WateringFarm : SectionLayer
    {
        private Material material = new Material(MatBases.Snow);
        private Texture2D texture = ContentFinder<Texture2D>.Get("Things/Mizu_Watering", true);

        public SectionLayer_WateringFarm(Section section) : base(section)
        {
            this.relevantChangeTypes = MapMeshFlag.Terrain;
            this.material.mainTexture = this.texture;
            this.material.color = new Color(1f, 1f, 1f, 0.1f);
        }

        public override void Regenerate()
        {
            var wateringComp = base.Map.GetComponent<MapComponent_Watering>();

            base.ClearSubMeshes(MeshParts.All);
            foreach (IntVec3 current in this.section.CellRect)
            {
                if (wateringComp.Get(base.Map.cellIndices.CellToIndex(current)) > 0)
                {
                    Printer_Plane.PrintPlane(this, current.ToVector3Shifted(), Vector2.one, this.material);
                }
            }
            base.FinalizeMesh(MeshParts.All);

            //LayerSubMesh subMesh = base.GetSubMesh(this.material);
            //if (subMesh.mesh.vertexCount == 0)
            //{
            //    MakeBaseGeometry(this.section, subMesh, AltitudeLayer.Terrain);
            //}
            //subMesh.Clear(MeshParts.Colors);
            //CellRect cellRect = this.section.CellRect;
            //int num = base.Map.Size.z - 1;
            //int num2 = base.Map.Size.x - 1;
            //bool flag = false;
            //CellIndices cellIndices = base.Map.cellIndices;

            //for (int x = cellRect.minX; x <= cellRect.maxX; x++)
            //{
            //    for (int z = cellRect.minZ; z <= cellRect.maxZ; z++)
            //    {
            //        if (grid[cellIndices.CellToIndex(x, z)] > 0)
            //        {
            //            subMesh.colors.Add(new Color32(255, 255, 255, 128));
            //            flag = true;
            //        }
            //    }
            //}
            //if (flag)
            //{
            //    subMesh.disabled = false;
            //    subMesh.FinalizeMesh(MeshParts.Colors);
            //}
            //else
            //{
            //    subMesh.disabled = true;
            //}
        }

        // 仮
        private static void MakeBaseGeometry(Section section, LayerSubMesh sm, AltitudeLayer altitudeLayer)
        {
            sm.Clear(MeshParts.Verts | MeshParts.Tris);
            CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
            cellRect.ClipInsideMap(section.map);
            float y = Altitudes.AltitudeFor(altitudeLayer);
            sm.verts.Capacity = cellRect.Area * 9;
            for (int i = cellRect.minX; i <= cellRect.maxX; i++)
            {
                for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                {
                    sm.verts.Add(new Vector3((float)i, y, (float)j));
                    sm.verts.Add(new Vector3((float)i, y, (float)j + 0.5f));
                    sm.verts.Add(new Vector3((float)i, y, (float)(j + 1)));
                    sm.verts.Add(new Vector3((float)i + 0.5f, y, (float)(j + 1)));
                    sm.verts.Add(new Vector3((float)(i + 1), y, (float)(j + 1)));
                    sm.verts.Add(new Vector3((float)(i + 1), y, (float)j + 0.5f));
                    sm.verts.Add(new Vector3((float)(i + 1), y, (float)j));
                    sm.verts.Add(new Vector3((float)i + 0.5f, y, (float)j));
                    sm.verts.Add(new Vector3((float)i + 0.5f, y, (float)j + 0.5f));
                }
            }
            int num = cellRect.Area * 8 * 3;
            sm.tris.Capacity = num;
            int num2 = 0;
            while (sm.tris.Count < num)
            {
                sm.tris.Add(num2 + 7);
                sm.tris.Add(num2);
                sm.tris.Add(num2 + 1);
                sm.tris.Add(num2 + 1);
                sm.tris.Add(num2 + 2);
                sm.tris.Add(num2 + 3);
                sm.tris.Add(num2 + 3);
                sm.tris.Add(num2 + 4);
                sm.tris.Add(num2 + 5);
                sm.tris.Add(num2 + 5);
                sm.tris.Add(num2 + 6);
                sm.tris.Add(num2 + 7);
                sm.tris.Add(num2 + 7);
                sm.tris.Add(num2 + 1);
                sm.tris.Add(num2 + 8);
                sm.tris.Add(num2 + 1);
                sm.tris.Add(num2 + 3);
                sm.tris.Add(num2 + 8);
                sm.tris.Add(num2 + 3);
                sm.tris.Add(num2 + 5);
                sm.tris.Add(num2 + 8);
                sm.tris.Add(num2 + 5);
                sm.tris.Add(num2 + 7);
                sm.tris.Add(num2 + 8);
                num2 += 9;
            }
            sm.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
        }
    }
}
