using System.Collections.Generic;
using UnityEngine;

namespace Insect_Planet._Scripts.ObjectManagers
{
	public sealed class ObjectPool : MonoBehaviour
	{
		public enum StartupPoolMode { Awake, Start, CallManually };

		[System.Serializable]
		public class StartupPool
		{
			public int size;
			public GameObject prefab;
		}

		static ObjectPool _instance;
		static List<GameObject> tempList = new List<GameObject>();
	
		Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();
		Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();
	
		public StartupPoolMode startupPoolMode;
		public StartupPool[] startupPools;

		bool startupPoolsCreated;

		void Awake()
		{
			_instance = this;
			if (startupPoolMode == StartupPoolMode.Awake)
				CreateStartupPools();
		}

		void Start()
		{
			if (startupPoolMode == StartupPoolMode.Start)
				CreateStartupPools();
		}

		private static void CreateStartupPools()
		{
			if (!instance.startupPoolsCreated)
			{
				instance.startupPoolsCreated = true;
				var pools = instance.startupPools;
				if (pools != null && pools.Length > 0)
					for (int i = 0; i < pools.Length; ++i)
						CreatePool(pools[i].prefab, pools[i].size);
			}
		}

		public static void CreatePool<T>(T prefab, int initialPoolSize) where T : Component
		{
			CreatePool(prefab.gameObject, initialPoolSize);
		}
		public static void CreatePool(GameObject prefab, int initialPoolSize)
		{
			if (prefab != null && !instance.pooledObjects.ContainsKey(prefab))
			{
				var list = new List<GameObject>();
				instance.pooledObjects.Add(prefab, list);

				if (initialPoolSize > 0)
				{
					bool active = prefab.activeSelf;
					prefab.SetActive(false);
					Transform parent = instance.transform;
					while (list.Count < initialPoolSize)
					{
						var obj = (GameObject)Object.Instantiate(prefab);
						obj.transform.parent = parent;
						list.Add(obj);
					}
					prefab.SetActive(active);
				}
			}
		}
	
		public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
		{
			return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
		}

		public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
		{
			List<GameObject> list;
			Transform trans;
			GameObject obj;
			if (instance.pooledObjects.TryGetValue(prefab, out list))
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
						instance.spawnedObjects.Add(obj, prefab);
						return obj;
					}
				}
				obj = (GameObject)Object.Instantiate(prefab);
				trans = obj.transform;
				trans.parent = parent;
				trans.localPosition = position;
				trans.localRotation = rotation;
				instance.spawnedObjects.Add(obj, prefab);
				return obj;
			}
			else
			{
				obj = (GameObject)Object.Instantiate(prefab);
				trans = obj.GetComponent<Transform>();
				trans.parent = parent;
				trans.localPosition = position;
				trans.localRotation = rotation;
				return obj;
			}
		}
	
		public static void ReturnToPool<T>(T obj) where T : Component
		{
			ReturnToPool(obj.gameObject);
		}
		public static void ReturnToPool(GameObject obj)
		{
			GameObject prefab;
			if (instance.spawnedObjects.TryGetValue(obj, out prefab))
				ReturnToPool(obj, prefab);
			else
				Object.Destroy(obj);
		}
		static void ReturnToPool(GameObject obj, GameObject prefab)
		{
			instance.pooledObjects[prefab].Add(obj);
			instance.spawnedObjects.Remove(obj);
			obj.transform.parent = instance.transform;
			obj.SetActive(false);
		}

		public static void ReturnAllToPool<T>(T prefab) where T : Component
		{
			ReturnAllToPool(prefab.gameObject);
		}
		public static void ReturnAllToPool(GameObject prefab)
		{
			foreach (var item in instance.spawnedObjects)
				if (item.Value == prefab)
					tempList.Add(item.Key);
			for (int i = 0; i < tempList.Count; ++i)
				ReturnToPool(tempList[i]);
			tempList.Clear();
		}
		public static void ReturnAllToPool()
		{
			tempList.AddRange(instance.spawnedObjects.Keys);
			for (int i = 0; i < tempList.Count; ++i)
				ReturnToPool(tempList[i]);
			tempList.Clear();
		}
	
		public static bool IsSpawned(GameObject obj)
		{
			return instance.spawnedObjects.ContainsKey(obj);
		}

		public static int CountPooled<T>(T prefab) where T : Component
		{
			return CountPooled(prefab.gameObject);
		}
		public static int CountPooled(GameObject prefab)
		{
			List<GameObject> list;
			if (instance.pooledObjects.TryGetValue(prefab, out list))
				return list.Count;
			return 0;
		}

		public static int CountSpawned<T>(T prefab) where T : Component
		{
			return CountSpawned(prefab.gameObject);
		}
		public static int CountSpawned(GameObject prefab)
		{
			int count = 0 ;
			foreach (var instancePrefab in instance.spawnedObjects.Values)
				if (prefab == instancePrefab)
					++count;
			return count;
		}

		public static int CountAllPooled()
		{
			int count = 0;
			foreach (var list in instance.pooledObjects.Values)
				count += list.Count;
			return count;
		}

		public static List<GameObject> GetPooled(GameObject prefab, List<GameObject> list, bool appendList)
		{
			if (list == null)
				list = new List<GameObject>();
			if (!appendList)
				list.Clear();
			List<GameObject> pooled;
			if (instance.pooledObjects.TryGetValue(prefab, out pooled))
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
			if (instance.pooledObjects.TryGetValue(prefab.gameObject, out pooled))
				for (int i = 0; i < pooled.Count; ++i)
					list.Add(pooled[i].GetComponent<T>());
			return list;
		}

		public static List<GameObject> GetSpawned(GameObject prefab, List<GameObject> list, bool appendList)
		{
			if (list == null)
				list = new List<GameObject>();
			if (!appendList)
				list.Clear();
			foreach (var item in instance.spawnedObjects)
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
			foreach (var item in instance.spawnedObjects)
				if (item.Value == prefabObj)
					list.Add(item.Key.GetComponent<T>());
			return list;
		}

		public static void DestroyPooled(GameObject prefab)
		{
			List<GameObject> pooled;
			if (instance.pooledObjects.TryGetValue(prefab, out pooled))
			{
				for (int i = 0; i < pooled.Count; ++i)
					GameObject.Destroy(pooled[i]);
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

		public static ObjectPool instance
		{
			get
			{
				if (_instance != null)
					return _instance;

				_instance = Object.FindObjectOfType<ObjectPool>();
				if (_instance != null)
					return _instance;

				var obj = new GameObject("ObjectPool");
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localRotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
				_instance = obj.AddComponent<ObjectPool>();
				return _instance;
			}
		}
	}
}
