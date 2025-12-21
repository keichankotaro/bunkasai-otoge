using System.Collections.Generic;
using UnityEngine;

public class NotePoolManager : MonoBehaviour
{
    public static NotePoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        // Dequeue an object. If the pool is empty, instantiate a new one.
        if (poolDictionary[tag].Count == 0)
        {
             // Optionally, grow the pool. For now, we'll log a warning.
             Debug.LogWarning("Pool with tag " + tag + " is empty. Consider increasing pool size.");
             // For robustness, instantiate a new one on the fly.
             Pool pool = pools.Find(p => p.tag == tag);
             if (pool != null)
             {
                 GameObject newObj = Instantiate(pool.prefab);
                 // This new object is not managed by the pool until it's returned.
                 // To handle this, we can either add it to the pool dictionary here,
                 // or have the ReturnToPool method handle unknown objects.
                 // For now, let's just return it. The ReturnToPool will destroy it if it's not from a known pool.
                 return newObj;
             }
             return null;
        }
        
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Do NOT activate the object here. The caller is now responsible for activation.

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            // If the object was created on the fly because the pool was empty, destroy it.
            Destroy(objectToReturn);
            return;
        }
        
        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
