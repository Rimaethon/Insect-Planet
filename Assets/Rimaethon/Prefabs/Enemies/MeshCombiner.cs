using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombiner : MonoBehaviour
{
    [SerializeField] private MeshFilter[] meshFilters;
    void Start()
    {
        CombineMeshesAndSave();
    }

    void CombineMeshesAndSave()
    {
        meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine, true, true);
        transform.GetComponent<MeshFilter>().sharedMesh = combinedMesh;

        // Activate the GameObject after combining meshes
        transform.gameObject.SetActive(true);

        // Save the combined mesh as an asset
        SaveCombinedMesh(combinedMesh, "CombinedMesh");
    }

    void SaveCombinedMesh(Mesh mesh, string fileName)
    {
        string filePath = $"Assets/{fileName}.asset";
      //  AssetDatabase.CreateAsset(mesh, filePath);
        //AssetDatabase.SaveAssets();
    }
}