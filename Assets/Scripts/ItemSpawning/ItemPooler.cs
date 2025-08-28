// =============================
// ItemPooler.cs
// A simple, editor-friendly object pool for 3D item prefabs.
// Drop this on a bootstrap GameObject (e.g., "GameSystems").
// =============================
using System.Collections.Generic;
using UnityEngine;

public class ItemPooler : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public string id;                 // Optional: human-readable id (e.g., "RubberDuck")
        public GameObject prefab;         // The prefab to pool
        public int prewarm = 10;          // How many to create at start
        public bool expandable = true;    // Allow pool to grow if empty
    }

    public List<PoolEntry> entries = new List<PoolEntry>();

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, Transform> _families = new();

    void Awake()
    {
        foreach (var entry in entries)
        {
            if (entry.prefab == null) continue;

            var q = new Queue<GameObject>(entry.prewarm);
            var familyRoot = new GameObject($"[{entry.prefab.name}] Pool").transform;
            familyRoot.SetParent(transform);
            _families[entry.prefab] = familyRoot;

            for (int i = 0; i < entry.prewarm; i++)
            {
                var inst = Instantiate(entry.prefab, familyRoot);
                inst.SetActive(false);

                var pi = inst.GetComponent<PooledItem>();
                if (pi == null) pi = inst.AddComponent<PooledItem>();
                pi.SourcePrefab = entry.prefab;
                pi.OwnerPooler = this;

                q.Enqueue(inst);
            }
            _pools[entry.prefab] = q;
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(prefab, out var q))
        {
            // Auto-create pool if unknown
            q = new Queue<GameObject>();
            _pools[prefab] = q;
            var familyRoot = new GameObject($"[{prefab.name}] Pool").transform;
            familyRoot.SetParent(transform);
            _families[prefab] = familyRoot;
        }

        GameObject go = null;
        while (q.Count > 0 && go == null)
        {
            go = q.Dequeue();
        }
        if (go == null)
        {
            // Find the entry to respect expandability (defaults to true if not found)
            var expandable = true;
            foreach (var e in entries)
            {
                if (e.prefab == prefab) { expandable = e.expandable; break; }
            }
            if (!expandable) return null;

            go = Instantiate(prefab, _families[prefab]);
            var pi = go.GetComponent<PooledItem>();
            if (pi == null) pi = go.AddComponent<PooledItem>();
            pi.SourcePrefab = prefab;
            pi.OwnerPooler = this;
        }

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);
        return go;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;
        var pi = instance.GetComponent<PooledItem>();
        if (pi == null || pi.SourcePrefab == null)
        {
            Destroy(instance);
            return;
        }
        instance.SetActive(false);
        instance.transform.SetParent(_families[pi.SourcePrefab], false);
        _pools[pi.SourcePrefab].Enqueue(instance);
    }
}