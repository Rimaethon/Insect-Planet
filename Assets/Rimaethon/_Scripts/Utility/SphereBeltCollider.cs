using System;
using System.Collections.Generic;
using System.Globalization;
using Insect_Planet._Scripts.ObjectManagers;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class SphereBeltCollider : MonoBehaviour
{
    [SerializeField] GameObject spawnPointPrefab;
    [SerializeField] private int numberOfPoints = 10;     
    [SerializeField] float rangePercentage = 20f;   
    [SerializeField] float paddingPercentage = 10f; 
    [Range(0.1f,5f)]
    [SerializeField] float spawnRandomnessYPercentage; 
    private SphereCollider _sphereCollider;
    [SerializeField] private int numberOfSlices=1;
    public Vector3 center;
    public bool generateDatFiles;
    public bool DestroyCollidersEachIteration;

    private float angle;
    [SerializeField]private bool calculatePositions;

    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();

        if (calculatePositions)
        {     

            GenerateHalfBelt();
            
        }
    }

    private void GenerateHalfBelt()
    {
        float sphereRadius = _sphereCollider.radius * gameObject.transform.localScale.x;
        float startOffset = sphereRadius + sphereRadius * paddingPercentage / 100f;

        center = _sphereCollider.transform.position + _sphereCollider.center;
        List<GameObject> instantiatedPrefabs = new List<GameObject>();
        List<Int16[][]> spawnPointsList = new List<Int16[][]>();
        
        for (int j = 0; j < numberOfSlices; j++)
        {
            Int16[][] spawnPoints= new Int16[numberOfPoints][];

            for (int i = 0; i < numberOfPoints; i++)
            {
                // Adjust the angle range to create a half belt
                float angle = (float)i / numberOfPoints * (2 * Mathf.PI / numberOfSlices ) + j * (2*MathF.PI/ numberOfSlices);
                spawnPoints[i]=new Int16[3];

                int maxRetries = 10; // You can adjust the maximum number of retries
                int retryCount = 0;
                while (retryCount < maxRetries)
                {
                    
                    // Calculate position based on polar coordinates
                    float distanceFromCenter = Random.Range(startOffset, startOffset + rangePercentage / 100f * startOffset);

                    float x = center.x + distanceFromCenter * Mathf.Cos(angle);
                    float y = center.y + Random.Range(-spawnRandomnessYPercentage / 100f * startOffset, spawnRandomnessYPercentage / 100f * startOffset);
                    float z = center.z + distanceFromCenter * Mathf.Sin(angle);
                    
                    Vector3 spawnPointPosition = new Vector3(x, y, z);
                    float yRotCos = Mathf.Cos(Mathf.Deg2Rad * 30);
                    float yRotSin = Mathf.Sin(Mathf.Deg2Rad * 30);

                    // Create the rotation matrix
                    Matrix4x4 rotationMatrix = new Matrix4x4(
                        new Vector4(1, 0, 0, 0),
                        new Vector4(0, yRotCos, -yRotSin, 0),
                        new Vector4(0, yRotSin, yRotCos, 0),
                        new Vector4(0, 0, 0, 1.0f)
                    );

                    // Apply the rotation around the specified center
                    Matrix4x4 translationToOrigin = Matrix4x4.Translate(-center);
                    Matrix4x4 translationBack = Matrix4x4.Translate(center);
                    Matrix4x4 combinedMatrix = translationBack * rotationMatrix * translationToOrigin;

                    // Apply the combined transformation to the spawn point
                    spawnPointPosition = combinedMatrix.MultiplyPoint(spawnPointPosition);

                    if (!CheckOverlap(spawnPointPosition))
                    {
                       
                        GameObject spawnedPrefab = Instantiate(spawnPointPrefab, spawnPointPosition, Quaternion.identity);
                        instantiatedPrefabs.Add(spawnedPrefab);
                        spawnPoints[i][0] = (Int16)spawnPointPosition.x;
                        spawnPoints[i][1] = (Int16)spawnPointPosition.y;
                        spawnPoints[i][2] = (Int16)spawnPointPosition.z;
                        break; 
                    }
                    retryCount++;
                }
              
                
                
            }
            if (DestroyCollidersEachIteration)
            {
                foreach (var prefab in instantiatedPrefabs)
                {
                    Destroy(prefab);
                }    
            }
     
            Vector3 spawnPointsCenter = CalculateCenter(spawnPoints);
            float yRotation= (j-1) * 2 * Mathf.PI / numberOfSlices * 57.2958f;
            string centerAsString = $"{spawnPointsCenter.x.ToString("F2", CultureInfo.InvariantCulture)}, {spawnPointsCenter.y.ToString("F2", CultureInfo.InvariantCulture)}, {spawnPointsCenter.z.ToString("F2", CultureInfo.InvariantCulture)}, {yRotation.ToString("F2", CultureInfo.InvariantCulture)}";

            if (generateDatFiles)
            {
                Vector3Serializer.SaveToFile(spawnPoints, centerAsString);
                
            }

           
        }
    }
    private Vector3 CalculateCenter(Int16[][] int16Arrays)
    {
        Int32 sumX = 0;
        Int32 sumY = 0;
        Int32 sumZ = 0;

        foreach (Int16[] int16Array in int16Arrays)
        {
            sumX += int16Array[0];
            sumY += int16Array[1];
            sumZ += int16Array[2];
        }

        int count = int16Arrays.Length; // Use Length for arrays

        float centerX = (sumX / count);
        float centerY =(sumY / count);
        float centerZ = (sumZ / count);

        return new Vector3(centerX, centerY, centerZ);
    }

    private bool CheckOverlap(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 1.59f); // Adjust the radius as needed

        foreach (Collider collider in colliders)
        {
            // Check if the collider is not the current sphere collider
            if (collider != _sphereCollider)
            {
                // If there is an overlap, return true
                return true;
            }
        }

        // No overlap found
        return false;
    }
}
