using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insect_Planet._Scripts.ObjectManagers
{
    public sealed class ObjectPool : MonoBehaviour
    {
        #region Fields

        public enum StartupPoolMode
        {
            Awake,
            Start,
            CallManually
        }

        [Serializable]
        public class StartupPool
        {
            public int size;
            public GameObject prefab;
        }

        private static readonly List<GameObject> TempList = new();

        private readonly Dictionary<GameObject, List<GameObject>> _pooledObjects = new();
        private readonly Dictionary<GameObject, GameObject> _spawnedObjects = new();

        public StartupPoolMode startupPoolMode;
        public StartupPool[] startupPools;

        private bool _startupPoolsCreated;
        private Transform enemyTarget;

        #endregion


        #region Singleton

        private static ObjectPool _instance;

        public static ObjectPool Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<ObjectPool>();
                if (_instance != null)
                    return _instance;

                var obj = new GameObject("ObjectPool")
                {
                    transform =
                    {
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity,
                        localScale = Vector3.one
                    }
                };
                _instance = obj.AddComponent<ObjectPool>();
                return _instance;
            }
        }

        #endregion


        #region Unity Functions

        private void Awake()
        {
            _instance = this;
            if (startupPoolMode == StartupPoolMode.Awake)
                CreateStartupPools();
        }

        private void Start()
        {
            enemyTarget = GameObject.FindGameObjectWithTag("Player").transform;
            if (startupPoolMode == StartupPoolMode.Start)
                CreateStartupPools();
        }

        #endregion


        #region Pool Creation

        private static void CreateStartupPools()
        {
            if (Instance._startupPoolsCreated) return;
            Instance._startupPoolsCreated = true;
            var pools = Instance.startupPools;
            if (pools == null || pools.Length <= 0) return;
            foreach (var t in pools)
                CreatePool(t.prefab, t.size);
        }

        public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
        {
            CreatePool(prefab.gameObject, initialPoolSize);
        }

        public static void CreatePool(GameObject prefab, int initialPoolSize)
        {
            var active = prefab.activeSelf;
            var parent = Instance.transform;
            if (prefab == null || Instance._pooledObjects.ContainsKey(prefab)) return;
            var list = new List<GameObject>();
            Instance._pooledObjects.Add(prefab, list);

            if (initialPoolSize <= 0) return;
            prefab.SetActive(false);
            while (list.Count < initialPoolSize)
            {
                var obj = Instantiate(prefab, parent, true);
                list.Add(obj);
            }

            prefab.SetActive(active);
        }

        #endregion


        #region Pool Destruction

        public static void DestroyPooled(GameObject prefab)
        {
            List<GameObject> pooled;
            if (Instance._pooledObjects.TryGetValue(prefab, out pooled))
            {
                for (var i = 0; i < pooled.Count; ++i)
                    Destroy(pooled[i]);
                pooled.Clear();
            }
        }

        public static void DestroyPooled<T>(T prefab) where T : Component
        {
            DestroyPooled(prefab.gameObject);
        }

        public static void DestroyAll(GameObject prefab)
        {
            ReturnAllToPool(prefab);
            DestroyPooled(prefab);
        }

        public static void DestroyAll<T>(T prefab) where T : Component
        {
            DestroyAll(prefab.gameObject);
        }

        #endregion


        #region Spawning and Returning

        public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
        }

        public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            List<GameObject> list;
            Transform trans;
            GameObject obj;
            if (Instance._pooledObjects.TryGetValue(prefab, out list))
            {
                obj = null;
                if (list.Count > 0)
                {
                    while (obj == null && list.Count > 0)
                    {
                        obj = list[0];
                        list.RemoveAt(0);
                    }

                    if (obj != null)
                    {
                        trans = obj.transform;
                        trans.parent = parent;
                        trans.localPosition = position;
                        trans.localRotation = rotation;
                        obj.SetActive(true);
                        Instance._spawnedObjects.Add(obj, prefab);
                        obj.GetComponent<Enemy>().target = Instance.enemyTarget;
                        return obj;
                    }
                }

                obj = Instantiate(prefab);
                trans = obj.transform;
                trans.parent = parent;
                trans.localPosition = position;
                trans.localRotation = rotation;
                Instance._spawnedObjects.Add(obj, prefab);
                return obj;
            }

            obj = Instantiate(prefab);
            trans = obj.GetComponent<Transform>();
            trans.parent = parent;
            trans.localPosition = position;
            trans.localRotation = rotation;
            return obj;
        }

        public static void ReturnToPool<T>(T obj) where T : Component
        {
            ReturnToPool(obj.gameObject);
        }

        public static void ReturnToPool(GameObject obj)
        {
            GameObject prefab;
            if (Instance._spawnedObjects.TryGetValue(obj, out prefab))
                ReturnToPool(obj, prefab);
            else
                Destroy(obj);
        }

        private static void ReturnToPool(GameObject obj, GameObject prefab)
        {
            Instance._pooledObjects[prefab].Add(obj);
            Instance._spawnedObjects.Remove(obj);
            obj.transform.parent = Instance.transform;
            obj.SetActive(false);
        }

        public static void ReturnAllToPool<T>(T prefab) where T : Component
        {
            ReturnAllToPool(prefab.gameObject);
        }

        public static void ReturnAllToPool(GameObject prefab)
        {
            foreach (var item in Instance._spawnedObjects)
                if (item.Value == prefab)
                    TempList.Add(item.Key);
            for (var i = 0; i < TempList.Count; ++i)
                ReturnToPool(TempList[i]);
            TempList.Clear();
        }

        public static void ReturnAllToPool()
        {
            TempList.AddRange(Instance._spawnedObjects.Keys);
            for (var i = 0; i < TempList.Count; ++i)
                ReturnToPool(TempList[i]);
            TempList.Clear();
        }

        #endregion


        #region Utility Functions

        public static bool IsSpawned(GameObject obj)
        {
            return Instance._spawnedObjects.ContainsKey(obj);
        }

        public static int CountPooled<T>(T prefab) where T : Component
        {
            return CountPooled(prefab.gameObject);
        }

        public static int CountPooled(GameObject prefab)
        {
            List<GameObject> list;
            if (Instance._pooledObjects.TryGetValue(prefab, out list))
                return list.Count;
            return 0;
        }

        public static int CountSpawned<T>(T prefab) where T : Component
        {
            return CountSpawned(prefab.gameObject);
        }

        public static int CountSpawned(GameObject prefab)
        {
            var count = 0;
            foreach (var instancePrefab in Instance._spawnedObjects.Values)
                if (prefab == instancePrefab)
                    ++count;
            return count;
        }

        public static int CountAllPooled()
        {
            var count = 0;
            foreach (var list in Instance._pooledObjects.Values)
                count += list.Count;
            return count;
        }

        #endregion


        #region Get Objects

        public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            List<GameObject> pooled;
            if (Instance._pooledObjects.TryGetValue(prefab, out pooled))
                list.AddRange(pooled);
            return list;
        }

        public static List<T> GetPooled<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if (list == null)
                list = new List<T>();
            if (!appendList)
                list.Clear();
            List<GameObject> pooled;
            if (Instance._pooledObjects.TryGetValue(prefab.gameObject, out pooled))
                for (var i = 0; i < pooled.Count; ++i)
                    list.Add(pooled[i].GetComponent<T>());
            return list;
        }

        public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
        {
            if (list == null)
                list = new List<GameObject>();
            if (!appendList)
                list.Clear();
            foreach (var item in Instance._spawnedObjects)
                if (item.Value == prefab)
                    list.Add(item.Key);
            return list;
        }

        public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
        {
            if (list == null)
                list = new List<T>();
            if (!appendList)
                list.Clear();
            var prefabObj = prefab.gameObject;
            foreach (var item in Instance._spawnedObjects)
                if (item.Value == prefabObj)
                    list.Add(item.Key.GetComponent<T>());
            return list;
        }

        #endregion
    }
}