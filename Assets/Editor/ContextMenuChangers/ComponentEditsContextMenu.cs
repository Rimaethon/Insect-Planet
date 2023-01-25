using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

/// <summary>
/// Adds tp the context menu options to  edit the components of a gameobject
/// </summary>
public class ComponentEditsContextMenu
{

    /// <summary>
    /// Adds Mesh colliders if any child or parent has a mesh renderer but no collider
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/Add Mesh Colliders", false, 0)]
    public static void AddMeshColliders()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<Transform> allTransforms = gameObject.GetComponentsInChildren<Transform>().ToList();
        foreach (Transform tForm in allTransforms)
        {
            if (tForm.GetComponent<MeshRenderer>() && !tForm.GetComponent<MeshCollider>())
            {
                tForm.gameObject.AddComponent(typeof(MeshCollider));
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Make mesh colliders convex
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/Make Mesh Colliders Convex", false, 0)]
    public static void MakeMeshCollidersConvex()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<MeshCollider> allMeshColliders = gameObject.GetComponentsInChildren<MeshCollider>().ToList();
        foreach (MeshCollider meshCollider in allMeshColliders)
        {
            meshCollider.convex = true;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Add Capsule Colliders to any object with a mesh renderer
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/Add Capsule Colliders", false, 0)]
    public static void AddCapsuleColliders()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<MeshRenderer> allMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
        foreach (MeshRenderer meshRenderer in allMeshRenderers)
        {
            if (!meshRenderer.GetComponent<CapsuleCollider>())
            {
                meshRenderer.gameObject.AddComponent(typeof(CapsuleCollider));
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Add Box Colliders to any object with a mesh renderer
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/Add Box Colliders", false, 0)]
    public static void AddBoxColliders()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<MeshRenderer> allMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
        foreach (MeshRenderer meshRenderer in allMeshRenderers)
        {
            if (!meshRenderer.GetComponent<BoxCollider>())
            {
                meshRenderer.gameObject.AddComponent(typeof(BoxCollider));
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Removes all colliders from child and parent
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/Remove Colliders", false, 0)]
    public static void RemoveColliders()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<Collider> allColliders = gameObject.GetComponentsInChildren<Collider>().ToList();
        for (int i = 0; i < allColliders.Count; i++)
        {
            Object.DestroyImmediate(allColliders[i]);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Changes all mesh renderers to not cast shadows
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/MeshRenderers/StopCastingShadows", false, 0)]
    public static void StopCastingShadows()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<MeshRenderer> allMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
        for (int i = 0; i < allMeshRenderers.Count; i++)
        {
            allMeshRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Changes all transform local positions to be at zero zero zero
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/ZeroLocalPositions", false, 0)]
    public static void ZeroLocalPositions()
    {
        GameObject gameObject = Selection.activeGameObject;
        List<Transform> allTransforms = gameObject.GetComponentsInChildren<Transform>().ToList();
        for (int i = 0; i < allTransforms.Count; i++)
        {
            allTransforms[i].localPosition = Vector3.zero;
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    /// <summary>
    /// Changes all transform local positions to be at zero zero zero
    /// </summary>
    [MenuItem("GameObject/ComponentEdits/ProjectSpecific/AddEnvironmentPieceComponents", false, 0)]
    public static void AddEnvironmentPieceComponents()
    {

        foreach(GameObject gameObject in Selection.gameObjects)
        {
            // Remove all Rigidbodies
            List<Rigidbody> allRigidbodies = gameObject.GetComponentsInChildren<Rigidbody>().ToList();
            for (int i = 0; i < allRigidbodies.Count; i++)
            {
                Object.DestroyImmediate(allRigidbodies[i]);
            }

            // Set up Health Script
            Health gameObjectHealth = gameObject.GetComponent<Health>();
            if (gameObjectHealth == null)
            {
                gameObjectHealth = gameObject.AddComponent<Health>();
            }

            // -1 team id for environment
            gameObjectHealth.teamId = -1;
            gameObjectHealth.isAlwaysInvincible = true;

            // Add mesch colliders as well
            List<Transform> allTransforms = gameObject.GetComponentsInChildren<Transform>().ToList();
            foreach (Transform tForm in allTransforms)
            {
                if (tForm.GetComponent<MeshRenderer>() && !tForm.GetComponent<MeshCollider>())
                {
                    tForm.gameObject.AddComponent(typeof(MeshCollider));
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
}
