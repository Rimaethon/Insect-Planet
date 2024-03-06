using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Insect_Planet._Scripts.ObjectManagers
{
    public class RandomNonOverlapPositionGenerator : MonoBehaviour
    {
        [SerializeField] private int pointsToGenerate = 50;
        [SerializeField] private Mesh objectMesh;
        [Range(15, 100)] [SerializeField] private float colliderPaddingPercentage = 15f;
        private Collider _spawnArea;
        private Vector3 _spawnAreaMin;
        private Vector3 _spawnAreaMax;
        private float _sphereColliderRadius;
        [SerializeField] private List<Vector3> _spawnPoints;
        private Collider[] colliders;

        [SerializeField] private GameObject objectPrefab; // Assign your object prefab with sphere collider in the Inspector

        private void Awake()
        {
            _spawnArea = GetComponent<Collider>();
            if (objectMesh != null)
            {
                _sphereColliderRadius = Mathf.Max(objectMesh.bounds.extents.x, objectMesh.bounds.extents.y,objectMesh.bounds.extents.z);
                _sphereColliderRadius += _sphereColliderRadius * colliderPaddingPercentage / 100;
            }
            else
            {
                Debug.LogError("No mesh assigned to RandomNonOverlapPositionGenerator");
            }

            _spawnAreaMin = _spawnArea.bounds.min;
            _spawnAreaMax = _spawnArea.bounds.max;
            _spawnArea.enabled = false; // Disable the collider to prevent it from blocking raycasts
            CalculatePoints();
        }

        private void CalculatePoints()
        {

            for (int i = 0; i < pointsToGenerate; i++)
            {
                bool pointValid = false;
                Vector3 randomPoint = Vector3.zero;
                int attempts = 0;
                int maxAttempts = 100; // Set a reasonable maximum attempt count

                while (!pointValid && attempts < maxAttempts)
                {
                    randomPoint = GenerateRandomPoint();
                    pointValid = !CheckForOverlap(randomPoint);
                    attempts++;
                }

                if (pointValid)
                {
                    _spawnPoints.Add(randomPoint);

                    if (_spawnPoints.Count == pointsToGenerate)
                    {
                        //Vector3Serializer.SaveToFile(_spawnPoints);
                    }

                    // Instantiate the object prefab
                    InstantiateObjectPrefab(randomPoint);
                }
                else
                {
                    Debug.LogWarning("Max attempts reached for finding a non-overlapping point.");
                }
            }
        }

        private Vector3 GenerateRandomPoint()
        {
            float randomX = Random.Range(_spawnAreaMin.x, _spawnAreaMax.x);
            float randomY = Random.Range(_spawnAreaMin.y, _spawnAreaMax.y);
            float randomZ = Random.Range(_spawnAreaMin.z, _spawnAreaMax.z);

            return new Vector3(randomX, randomY, randomZ);
        }

        private bool CheckForOverlap(Vector3 point)
        {
            colliders = Physics.OverlapSphere(point, _sphereColliderRadius);

            // If there are overlapping colliders, return true
            return colliders.Length > 0;
        }

        private void InstantiateObjectPrefab(Vector3 position)
        {
            GameObject instantiatedObject = Instantiate(objectPrefab, position, Quaternion.identity);
            
            // Adjust the sphere collider radius of the instantiated object
            SphereCollider sphereCollider = instantiatedObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.radius = _sphereColliderRadius;
                sphereCollider.center = objectMesh.bounds.center;
            }
            else
            {
                Debug.LogError("SphereCollider component not found on the instantiated object.");
            }
        }
    }
}
