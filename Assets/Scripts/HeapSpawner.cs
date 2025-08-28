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

    [Header("Physics & Timing")]
    public bool simulateDrop = true;     // If false: place directly without physics
    [Tooltip("Freeze rigidbodies after settle to save CPU")] public bool freezeAfterSettle = true;
    [Tooltip("Seconds to wait after last spawn for settling")] public float settleTime = 1.0f;

    [Header("Gizmos")] public bool showGizmos = true;

    private ItemPooler _pool;
    private System.Random _rng;

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
        StartCoroutine(SpawnRoutine());
    }

    public IEnumerator SpawnRoutine()
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

                go.transform.localScale = Vector3.one; // ensure reset

                if (simulateDrop)
                {
                    var rb = go.GetComponent<Rigidbody>();
                    if (rb == null) rb = go.AddComponent<Rigidbody>();
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.interpolation = RigidbodyInterpolation.None;

                    // Small random torque for believable settling
                    var force = new Vector3(
                        (float)(_rng.NextDouble() - 0.5) * 0.5f,
                        0f,
                        (float)(_rng.NextDouble() - 0.5) * 0.5f
                    );
                    rb.AddTorque(force, ForceMode.Impulse);
                }
                else
                {
                    // Direct placement on surface via raycast down
                    if (Physics.Raycast(worldPos, Vector3.down, out var hit, dropHeight + 10f))
                    {
                        go.transform.position = hit.point + Vector3.up * 0.01f; // avoid z-fighting
                    }
                }
            }
        }

        Debug.Break();

        if (simulateDrop)
            yield return new WaitForSeconds(settleTime);

        if (freezeAfterSettle)
        {
            // Put bodies to sleep & freeze to save perf (you may unfreeze on interaction)
            var rbs = GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Freeze completely after settle
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