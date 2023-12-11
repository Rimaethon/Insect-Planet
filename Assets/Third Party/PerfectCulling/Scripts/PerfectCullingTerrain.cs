using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    [RequireComponent(typeof(UnityEngine.Terrain))]
    public class PerfectCullingTerrain : PerfectCullingMonoGroup
    {
        [HideInInspector] public Renderer terrainMeshRenderer;
        [HideInInspector] public MeshFilter terrainMeshFilter;
        
        public override List<Renderer> Renderers
        {
            get
            {
                return new List<Renderer>(1);
            }
        }

        public override List<UnityEngine.Behaviour> UnityBehaviours
        {
            get
            {
                return new List<UnityEngine.Behaviour>()
                {
                    GetComponent<UnityEngine.Terrain>() 
                };
            }
        }
        
        public override void PreBake(PerfectCullingBakingBehaviour bakingBehaviour)
        {
            UpdateRenderer();
            
            UnityEngine.Terrain terrain = GetComponent<UnityEngine.Terrain>();

            Renderer[] terrainRenderer = new Renderer[] {terrainMeshRenderer};
            
            foreach (var group in bakingBehaviour.bakeGroups)
            {
                if (group.unityBehaviours.Contains(terrain))
                {
                    group.renderers = terrainRenderer;
                }
            }
        }

        public override void PreSceneSave(PerfectCullingBakingBehaviour bakingBehaviour)
        {
        }

        public override void PostBake(PerfectCullingBakingBehaviour bakingBehaviour)
        {
            // Technically we don't need to do this because our mesh is never saved into the scene.
            // However the post bake hash calculation would include it and that's why we need to remove it.
            
            UnityEngine.Terrain terrain = GetComponent<UnityEngine.Terrain>();

            foreach (var group in bakingBehaviour.bakeGroups)
            {
                if (group.unityBehaviours.Contains(terrain))
                {
                    group.renderers = System.Array.Empty<Renderer>();
                }
            }
        }

        private void UpdateRenderer()
        {
            if (terrainMeshRenderer == null)
            {
                GameObject go = new GameObject("Terrain Bake Mesh [EditorOnly]");

                terrainMeshRenderer = go.AddComponent<MeshRenderer>();
            }

            if (terrainMeshFilter == null)
            {
                terrainMeshFilter = terrainMeshRenderer.gameObject.AddComponent<MeshFilter>();
            }

            UnityEngine.Terrain terrain = GetComponent<Terrain>();

            terrainMeshFilter.sharedMesh = TerrainToMeshUtility.CreateMesh(terrain);

            terrainMeshRenderer.transform.SetPositionAndRotation(terrain.transform.position, Quaternion.identity);

            // Don't disable this mesh. We need the Unity Renderer to see it!
            terrainMeshRenderer.transform.parent = transform;
            terrainMeshRenderer.gameObject.layer = PerfectCullingConstants.CamBakeLayer;
            terrainMeshRenderer.tag = "EditorOnly";
        }
    }
}