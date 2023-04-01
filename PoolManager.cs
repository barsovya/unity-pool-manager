using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : MonoBehaviour
{
    private static PoolManager _instance;

    private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    private void Awake()
    {
        foreach (var pool in GetComponents<Pool>())
        {
            _pools.Add(pool.Initialize(), pool);
        }
        _instance = this;
    }

    /// <summary>
    /// Check does specific object have a pool responsible for it's instansing.
    /// </summary>
    /// <param name="obj">Instance or prefab</param>
    /// <returns></returns>
    public static bool HasPoolForObject(GameObject obj)
    {
        return _instance._pools.ContainsKey(obj.name);
    }

    public static GameObject Instantiate(string prefab, Vector3 position, Quaternion rotation)
    {
#if AllowNonPooledPrefabs
        if (!_instance._pools.ContainsKey(prefab))
        {
            Debug.Log($"No such prefab with this name '{prefab}' in pool. Doing GameObject.Instantiate().");
            return null;
        }
#endif

        var instance = _instance._pools[prefab].TakeFromPool();
#if AdditionalValidation
        instance.AddComponent<PoolInstantiated>().Stack = new System.Diagnostics.StackTrace().ToString();
#endif
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
#if AllowNonPooledPrefabs
        if (!HasPoolForObject(prefab))
        {
            Debug.Log($"No such prefab with this name '{prefab.name}' in pool. Doing GameObject.Instantiate().");
            return GameObject.Instantiate(prefab, position, rotation, parent);
        }
#endif

        var instance = _instance._pools[prefab.name].TakeFromPool();
#if AdditionalValidation
        instance.AddComponent<PoolInstantiated>().Stack = new System.Diagnostics.StackTrace().ToString();
#endif
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.transform.SetParent(parent);
        return instance;
    }

    public static GameObject Instantiate(GameObject prefab)
    {
#if AllowNonPooledPrefabs
        if (!HasPoolForObject(prefab))
        {
            Debug.Log($"No such prefab with this name '{prefab.name}' in pool. Doing GameObject.Instantiate().");
            return GameObject.Instantiate(prefab);
        }
#endif

        var instance = _instance._pools[prefab.name].TakeFromPool();
#if AdditionalValidation
        instance.AddComponent<PoolInstantiated>().Stack = new System.Diagnostics.StackTrace().ToString();
#endif
        return instance;
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
#if AllowNonPooledPrefabs
        if (!HasPoolForObject(prefab))
        {
            Debug.Log($"No such prefab with this name '{prefab.name}' in pool. Doing GameObject.Instantiate().");
            return GameObject.Instantiate(prefab, position, rotation);
        }
#endif

        var instance = _instance._pools[prefab.name].TakeFromPool();
#if AdditionalValidation
        instance.AddComponent<PoolInstantiated>().Stack = new System.Diagnostics.StackTrace().ToString();
#endif
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    public static void Destroy(GameObject instance)
    {
#if AllowNonPooledPrefabs
        if (!HasPoolForObject(instance))
        {
            Debug.Log($"No such prefab with this name '{instance.name}' in pool. Doing GameObject.Destroy().");
            GameObject.Destroy(instance);
            return;
        }
#endif

#if AdditionalValidation
        instance.AddComponent<PoolDestroyed>().Stack = new System.Diagnostics.StackTrace().ToString();
#endif
        _instance._pools[instance.name].ReturnToPool(instance);
    }

    public static void Destroy(GameObject instance, float time)
    {
#if AllowNonPooledPrefabs
        if (!HasPoolForObject(instance))
        {
            Debug.Log($"No such prefab with this name '{instance.name}' in pool. Doing GameObject.Destroy().");
            GameObject.Destroy(instance, time);
            return;
        }
#endif

#if AdditionalValidation
        instance.AddComponent<PoolDestroyed>().Stack = new System.Diagnostics.StackTrace().ToString();
#endif
        _instance.StartCoroutine(DelayedDestroy(instance, time));
    }

    private static IEnumerator DelayedDestroy(GameObject instance, float time)
    {
        yield return new WaitForSeconds(time);
        _instance._pools[instance.name].ReturnToPool(instance);
    }
}
