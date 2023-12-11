// Perfect Culling (C) 2021 Patrick König
//

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    [RequireComponent(typeof(UnityEngine.Terrain))]
    public class TerrainToMeshUtility : MonoBehaviour
    {
        private const int DefaultMeshResolutionX = 196;
        private const int DefaultMeshResolutionZ = 196;
        
        private readonly string EditorOnlyTag = "EditorOnly";

        [Range(1, 512)]
        public int MeshResolutionX = DefaultMeshResolutionX;
        
        [Range(1, 512)]
        public int MeshResolutionZ = DefaultMeshResolutionZ;

        public MeshRenderer meshRendererReference;
       
        public void CreateorUpdateMesh()
        {
            UnityEngine.Terrain terrain = GetComponent<UnityEngine.Terrain>();

            Mesh mesh = CreateMesh(terrain, MeshResolutionX, MeshResolutionZ);

            string terrainName = $"Mesh for {terrain.name} [EditorOnly]";

            if (meshRendererReference == null)
            {
                GameObject newGo = new GameObject(terrainName);
                newGo.tag = EditorOnlyTag;

                newGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            
                meshRendererReference = newGo.AddComponent<MeshRenderer>();
                meshRendererReference.enabled = false;
            }
            else
            {
                MeshFilter mf = meshRendererReference.GetComponent<MeshFilter>();

                if (mf.sharedMesh != null)
                {
                    GameObject.DestroyImmediate(mf.sharedMesh);
                }
                
                mf.sharedMesh = mesh;
            }
            
            meshRendererReference.transform.SetPositionAndRotation(terrain.transform.position, Quaternion.identity);
        }

        public static Mesh CreateMesh(UnityEngine.Terrain terrain, int meshResolutionX = DefaultMeshResolutionX, int meshResolutionZ = DefaultMeshResolutionZ)
        {
            string terrainName = $"Mesh for {terrain.name} [EditorOnly]";

            TerrainData terrainData = terrain.terrainData;

            if (terrainData == null)
            {
                Debug.LogError("Terrain data is null.");
                
                return null;
            }

            float xSpacing = terrainData.size.x / meshResolutionX;
            float ySpacing = terrainData.size.z / meshResolutionZ;

            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = terrainName;

            Vector3[] vertices = new Vector3[(meshResolutionX + 1) * (meshResolutionZ + 1)];
            for (int i = 0, y = 0; y <= meshResolutionZ; y++)
            {
                for (int x = 0; x <= meshResolutionX; x++, i++)
                {
                    float h = terrain.SampleHeight(new Vector3(x * xSpacing, 0f, y * ySpacing) + terrain.transform.position);

                    vertices[i] = new Vector3(x * xSpacing, h, y * ySpacing);
                }
            }

            mesh.vertices = vertices;

            int[] triangles = new int[meshResolutionX * meshResolutionZ * 6];
            for (int ti = 0, vi = 0, y = 0; y < meshResolutionZ; y++, vi++)
            {
                for (int x = 0; x < meshResolutionX; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + meshResolutionX + 1;
                    triangles[ti + 5] = vi + meshResolutionX + 2;
                }
            }

            mesh.triangles = triangles;

            return mesh;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(TerrainToMeshUtility))]
    public class TerrainConvertHelperV1Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TerrainToMeshUtility helper = target as TerrainToMeshUtility;
            ;
            
            DrawDefaultInspector();

            if (GUILayout.Button(helper.meshRendererReference != null ? "Update mesh" : "Create mesh"))
            {
                helper.CreateorUpdateMesh();
            }

            if (helper.meshRendererReference != null)
            {
                if (GUILayout.Button("Select MeshRenderer"))
                {
                    UnityEditor.Selection.activeGameObject = helper.meshRendererReference.gameObject;
                }
            }
        }
    }
#endif
}