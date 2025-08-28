// =============================
// HeapSpawner.cs
// Spawns a believable "pile" by dropping pooled prefabs within a volume.
// Attach to an empty GameObject that defines the spawn region.
// Use a BoxCollider as bounds or size parameters below.
// =============================
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ItemPooler))]
public class HeapSpawner : MonoBehaviour
{
    [Header("Spawn Source")]
    public SpawnConfig SpawnConfig;

    [Header("Counts & Seeds")]
    [Min(0)] public int spawnCount = 60;
    public int seed = 12345;             // Same seed => deterministic layout

    [Header("Placement Volume (Local)")]
    public Vector3 areaSize = new Vector3(6f, 0.5f, 6f); // XZ footprint, small Y for initial height band
    public float dropHeight = 4f;       // How high above to start drops

    [Header("Gizmos")] public bool showGizmos = true;

    private ItemPooler _pool;
    private System.Random _rng;
    
    public event System.Action<GameObject> ItemSpawned;

    void Reset()
    {
        // Auto-link a local ItemPooler if present
        _pool = GetComponent<ItemPooler>();
    }

    void Awake()
    {
        _pool = GetComponent<ItemPooler>();
        _rng = new System.Random(seed);
    }
    
    void Start()
    {
        SpawnItems();
    }

    public void SpawnItems()
    {
        // Optionally clear previous children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            var pooled = child.GetComponent<PooledItem>();
            if (pooled != null) pooled.ReturnToPool();
        }

        var origin = transform.position;
        var basis = transform.rotation;

        // Spawn loop
        foreach (var item in SpawnConfig.Items)
        {
            for (int i = 0; i < item.Count; i++)
            {
                var localXZ = new Vector3(
                    Mathf.Lerp(-areaSize.x * 0.5f, areaSize.x * 0.5f, (float)_rng.NextDouble()),
                    0f,
                    Mathf.Lerp(-areaSize.z * 0.5f, areaSize.z * 0.5f, (float)_rng.NextDouble())
                );

                var worldPos = origin + basis * (localXZ + Vector3.up * (areaSize.y + dropHeight));
                var rot = Quaternion.Euler(0f, (float)(_rng.NextDouble() * 360f), 0f);

                var go = _pool.Spawn(item.Prefab, worldPos, rot);
                if (go == null) continue;
                
                ItemSpawned?.Invoke(go);
                go.transform.localScale = Vector3.one; // ensure reset
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        var m = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawCube(new Vector3(0f, areaSize.y + dropHeight * 0.5f, 0f), new Vector3(areaSize.x, dropHeight, areaSize.z));
        Gizmos.color = new Color(0f, 0.6f, 0f, 0.6f);
        Gizmos.DrawWireCube(Vector3.up * areaSize.y * 0.5f, new Vector3(areaSize.x, areaSize.y, areaSize.z));
    }
}