// Designed by KINEMATION, 2023

using System.Collections.Generic;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Components
{
    [ExecuteInEditMode]
    public class BoneContainer : MonoBehaviour
    {
        [HideInInspector] public List<Transform> boneContainer = new List<Transform>();
        [SerializeField, HideInInspector] private bool isInitialized = false;

        public void Awake()
        {
            if (isInitialized) return;
            
            isInitialized = true;
            boneContainer.Clear();
            RefreshBoneContainer(transform);
            Debug.Log("Successfully added " + boneContainer.Count + " bones!");
        }

        public void RefreshBoneContainer(Transform parentTransform)
        {
            boneContainer.Add(parentTransform);
            foreach (Transform child in parentTransform)
            {
                RefreshBoneContainer(child);
            }
        }
    }
}
