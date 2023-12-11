// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingMeshSplitting
    {
        private const int SplitSize = 8000;

        class SplitMeshData
        {
            public List<int> indices = new List<int>();
            public List<Vector3> verts = new List<Vector3>();
            public List<Color> colors = new List<Color>();

            public List<TriangleData> tris = new List<TriangleData>();

            public Mesh CreateMesh()
            {
                Mesh newMesh = new Mesh();

                newMesh.indexFormat = IndexFormat.UInt32;
                newMesh.SetVertices(verts);
                newMesh.SetTriangles(indices, 0);
                newMesh.SetColors(colors);
                
#if UNITY_EDITOR
                UnityEditor.MeshUtility.Optimize(newMesh);
#endif
                return newMesh;
            }
        }

        class TriangleData
        {
            public Vector3 posA;
            public Vector3 posB;
            public Vector3 posC;

            public Color colA;
            //public Color colB;
            //public Color colC;

            public Vector3 center;
        }
        
        public static List<Mesh> Split(Mesh mesh, Matrix4x4 transformMatrix)
        {
            List<Mesh> splitMeshes = new List<Mesh>();
            
            SplitMeshData inputSM = GenerateFromMesh(mesh, transformMatrix);

            inputSM.tris.Sort((a, b) => a.center.x.CompareTo(b.center.x));

            foreach (SplitMeshData splitMeshesX in SplitTriangles(inputSM.tris, SplitSize * 8))
            {
                splitMeshesX.tris.Sort((a, b) => a.center.z.CompareTo(b.center.z));

                var splitMeshesZ = SplitTriangles(splitMeshesX.tris, SplitSize);

                foreach (var splitMeshData in splitMeshesZ)
                {
                    splitMeshes.Add(splitMeshData.CreateMesh());
                }
            }

            return splitMeshes;
        }
        
        class TupleComparer : IEqualityComparer<ValueTuple<Vector3, Color>>
        {
            public bool Equals(ValueTuple<Vector3, Color> x, ValueTuple<Vector3, Color> y)
            {
                return (x.Item1 == y.Item1) && (x.Item2 == y.Item2);
            }

            public int GetHashCode(ValueTuple<Vector3, Color> obj)
            {
                return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
            }
        }

        private static TupleComparer tupleComparer = new TupleComparer();
        
        private static List<SplitMeshData> SplitTriangles(List<TriangleData> triangles, int splitCount)
        {
            List<SplitMeshData> meshes = new List<SplitMeshData>();

            int cIndex = 0;
            Dictionary<ValueTuple<Vector3, Color>, int> spV = new Dictionary<ValueTuple<Vector3, Color>, int>(tupleComparer);

            SplitMeshData splitMeshData = new SplitMeshData();

            void SplitMesh()
            {
                int indicesCount = splitMeshData.indices.Count;
                
                for (int i = 0; i < indicesCount; i += 3)
                {
                    Vector3 pos1 = splitMeshData.verts[splitMeshData.indices[i + 0]];
                    Vector3 pos2 = splitMeshData.verts[splitMeshData.indices[i + 1]];
                    Vector3 pos3 = splitMeshData.verts[splitMeshData.indices[i + 2]];

                    splitMeshData.tris.Add(new TriangleData()
                    {
                        posA = pos1,
                        posB = pos2,
                        posC = pos3,

                        colA = splitMeshData.colors[splitMeshData.indices[i + 0]],
                        //colB = splitMeshData.colors[splitMeshData.indices[i + 1]],
                        //colC = splitMeshData.colors[splitMeshData.indices[i + 2]],

                        center = (pos1 + pos2 + pos3) / 3
                    });
                }

                meshes.Add(splitMeshData);

                splitMeshData = new SplitMeshData();
                spV.Clear();
                cIndex = 0;
            }

            void AddVerts(Vector3 pos, Color col)
            {
                if (spV.TryGetValue(new ValueTuple<Vector3, Color>(pos, col), out int vertPosAIndex))
                {
                    splitMeshData.indices.Add(vertPosAIndex);
                }
                else
                {
                    spV.Add(new ValueTuple<Vector3, Color>(pos, col), cIndex);
                    splitMeshData.verts.Add(pos);
                    splitMeshData.indices.Add(cIndex);
                    splitMeshData.colors.Add(col);
                    ++cIndex;
                }
            }
            
            foreach (TriangleData tri in triangles)
            {
                AddVerts(tri.posA, tri.colA);
                AddVerts(tri.posB, tri.colA);//tri.colB);
                AddVerts(tri.posC, tri.colA);//tri.colC);

                if (splitMeshData.verts.Count >= splitCount)
                {
                    SplitMesh();
                }
            }

            SplitMesh();

            return meshes;
        }

        private static SplitMeshData GenerateFromMesh(Mesh mesh, Matrix4x4 transformMat)
        {
            SplitMeshData splitMeshData = new SplitMeshData();

            Vector3[] verts = mesh.vertices;
            int[] indices = mesh.GetIndices(0);
            Color[] colors = mesh.colors;

            bool needsMul = transformMat != Matrix4x4.identity;

            int indicesLength = indices.Length;
            
            for (int i = 0; i < indicesLength; i += 3)
            {
                Vector3 worldVertPosA = needsMul ? transformMat.MultiplyPoint3x4(verts[indices[i + 0]]) : verts[indices[i + 0]];
                Vector3 worldVertPosB = needsMul ? transformMat.MultiplyPoint3x4(verts[indices[i + 1]]) : verts[indices[i + 1]];
                Vector3 worldVertPosC = needsMul ? transformMat.MultiplyPoint3x4(verts[indices[i + 2]]) : verts[indices[i + 2]];

                splitMeshData.tris.Add(new TriangleData()
                {
                    posA = worldVertPosA,
                    posB = worldVertPosB,
                    posC = worldVertPosC,

                    colA = colors[indices[i + 0]],
                    //colB = colors[indices[i + 1]],
                    //colC = colors[indices[i + 2]],

                    center =  (worldVertPosA + worldVertPosB + worldVertPosC) / 3
                });
            }

            splitMeshData.indices = indices.ToList();
            splitMeshData.verts = verts.ToList();
            splitMeshData.colors = colors.ToList();

            return splitMeshData;
        }
    }
}