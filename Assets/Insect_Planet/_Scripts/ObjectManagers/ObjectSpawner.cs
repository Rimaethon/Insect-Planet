using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	static Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

   public static T Spawn<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, rotation).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, rotation).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Transform parent, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, parent, position, Quaternion.identity).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Vector3 position) where T : Component
	{
		return Spawn(prefab.gameObject, null, position, Quaternion.identity).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab, Transform parent) where T : Component
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}
	public static T Spawn<T>(T prefab) where T : Component
	{
		return Spawn(prefab.gameObject, null, Vector3.zero, Quaternion.identity).GetComponent<T>();
	}
	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		List<GameObject> list;
		Transform trans;
		GameObject obj;
		if (ObjectPool.instance.pooledObjects.TryGetValue(prefab, out list))
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
					spawnedObjects.Add(obj, prefab);
					return obj;
				}
			}
			obj = (GameObject)Object.Instantiate(prefab);
			trans = obj.transform;
			trans.parent = parent;
			trans.localPosition = position;
			trans.localRotation = rotation;
				spawnedObjects.Add(obj, prefab);
			return obj;
		}

		obj = (GameObject)Object.Instantiate(prefab);
		trans = obj.GetComponent<Transform>();
		trans.parent = parent;
		trans.localPosition = position;
		trans.localRotation = rotation;
		return obj;
	}
	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
	{
		return Spawn(prefab, parent, position, Quaternion.identity);
	}
	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return Spawn(prefab, null, position, rotation);
	}
	public static GameObject Spawn(GameObject prefab, Transform parent)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
	}
	public static GameObject Spawn(GameObject prefab, Vector3 position)
	{
		return Spawn(prefab, null, position, Quaternion.identity);
	}
	public static GameObject Spawn(GameObject prefab)
	{
		return Spawn(prefab, null, Vector3.zero, Quaternion.identity);
	}

	
	public static void Recycle<T>(T obj) where T : Component
	{
		Recycle(obj.gameObject);
	}
	public static void Recycle(GameObject obj)
	{
		GameObject prefab;
		if (instance.spawnedObjects.TryGetValue(obj, out prefab))
			Recycle(obj, prefab);
		else
			Object.Destroy(obj);
	}
	static void Recycle(GameObject obj, GameObject prefab)
	{
		instance.pooledObjects[prefab].Add(obj);
		instance.spawnedObjects.Remove(obj);
		obj.transform.parent = instance.transform;
		obj.SetActive(false);
	}

	public static void RecycleAll<T>(T prefab) where T : Component
	{
		RecycleAll(prefab.gameObject);
	}
	public static void RecycleAll(GameObject prefab)
	{
		foreach (var item in instance.spawnedObjects)
			if (item.Value == prefab)
				tempList.Add(item.Key);
		for (int i = 0; i < tempList.Count; ++i)
			Recycle(tempList[i]);
		tempList.Clear();
	}
	public static void RecycleAll()
	{
		tempList.AddRange(instance.spawnedObjects.Keys);
		for (int i = 0; i < tempList.Count; ++i)
			Recycle(tempList[i]);
		tempList.Clear();
	}
	
	public static bool IsSpawned(GameObject obj)
	{
		return spawnedObjects.ContainsKey(obj);
	}

	public static int CountPooled<T>(T prefab) where T : Component
	{
		return CountPooled(prefab.gameObject);
	}
	

	public static int CountSpawned<T>(T prefab) where T : Component
	{
		return CountSpawned(prefab.gameObject);
	}
	public static int CountSpawned(GameObject prefab)
	{
		int count = 0 ;
		foreach (var instancePrefab in spawnedObjects.Values)
			if (prefab == instancePrefab)
				++count;
		return count;
	}
	
	public static List<T> GetSpawned<T>(T prefab, List<T> list, bool appendList) where T : Component
	{
		if (list == null)
			list = new List<T>();
		if (!appendList)
			list.Clear();
		var prefabObj = prefab.gameObject;
		foreach (var item in spawnedObjects)
			if (item.Value == prefabObj)
				list.Add(item.Key.GetComponent<T>());
		return list;
	}
	public static void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		ObjectPool.instance.DestroyPooled(prefab);
	}
}
