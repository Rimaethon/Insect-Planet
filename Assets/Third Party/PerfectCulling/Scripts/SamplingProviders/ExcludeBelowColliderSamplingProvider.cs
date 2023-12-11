// Perfect Culling (C) 2021 Patrick König
//

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Koenigz.PerfectCulling.SamplingProviders
{ 
    [RequireComponent(typeof(PerfectCullingBakingBehaviour))]
    [ExecuteAlways]
    public class ExcludeBelowColliderSamplingProvider : SamplingProviderBase
    {
        [Header("Exclude cells below this collider")]
        [SerializeField] private Collider excludeBelow;

        [Header("Offset")] [SerializeField] [Range(-10f, 10f)] private float offsetY = 1.0f;
        public override string Name => nameof(ExcludeBelowColliderSamplingProvider) + ": " + (excludeBelow != null ? excludeBelow.name : "(null)");
      
        public override void InitializeSamplingProvider()
        {
        }

        public override bool IsSamplingPositionActive(PerfectCullingBakingBehaviour bakingBehaviour, Vector3 pos)
        {
            if (excludeBelow == null)
            {
                return true;
            }
            
            bool cachedQueryHitBackfaces = Physics.queriesHitBackfaces;

            try
            {
                // MeshCollider might not have back faces but we need them to detect a hit.
                // That's why we temporarily override this.
                Physics.queriesHitBackfaces = true;

                RaycastHit raycastHit;

                // Exclude everything below the Custom_Exclude object.
                // To determine that we are below the object we simply raycast upwards.
                // We offset the ray starting position by 1 meter to also catch sampling positions that are inside the collision volume.

                Vector3 offset = new Vector3(0f, offsetY, 0f);
                
                if (excludeBelow.Raycast(new Ray(pos - offset, Vector3.up), out raycastHit, float.MaxValue))
                {
                    return false;
                }
            }
            finally
            {
                // Restore cached settings
                Physics.queriesHitBackfaces = cachedQueryHitBackfaces;
            }

            return true;
        }

        public float GetOffsetY() => offsetY;
        public Collider GetCollider() => excludeBelow;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExcludeBelowColliderSamplingProvider))]
    class ExcludeBelowColliderSamplingProviderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.HelpBox($"Please use {nameof(ExcludeBelowColliderArraySamplingProvider)} instead. This component is being deprecated.\nFeel free to use the \"Migrate it for me!\" button but save your scene in case it goes wrong.", MessageType.Warning);

            if (GUILayout.Button("Migrate it for me!"))
            {
                ExcludeBelowColliderSamplingProvider provider = target as ExcludeBelowColliderSamplingProvider;
                GameObject providerGo = provider.gameObject;

                ExcludeBelowColliderSamplingProvider[] allExcludeBelowColliderSamplingProviders = providerGo.GetComponents<ExcludeBelowColliderSamplingProvider>();

                Dictionary<float, HashSet<Collider>> dictionary = new Dictionary<float, HashSet<Collider>>();

                foreach (ExcludeBelowColliderSamplingProvider x in allExcludeBelowColliderSamplingProviders)
                {
                    if (dictionary.TryGetValue(x.GetOffsetY(), out var list))
                    {
                        list.Add(x.GetCollider());
                    }
                    else
                    {
                        dictionary.Add(x.GetOffsetY(), new HashSet<Collider>() { x.GetCollider() });
                    }
                }

                foreach (var kv in dictionary)
                {
                    ExcludeBelowColliderArraySamplingProvider arraySamplingProvider = providerGo.AddComponent<ExcludeBelowColliderArraySamplingProvider>();

                    arraySamplingProvider.Migrate(kv.Key, kv.Value);
                }

                foreach (var p in allExcludeBelowColliderSamplingProviders)
                {
                    GameObject.DestroyImmediate(p);
                }
                
                UnityEditor.EditorUtility.SetDirty(providerGo);
            }
        }
    }
#endif
}