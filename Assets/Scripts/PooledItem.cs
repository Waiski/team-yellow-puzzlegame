// =============================
// PooledItem.cs
// Helper component that knows how to return itself to the pool.
// =============================
using UnityEngine;

public class PooledItem : MonoBehaviour
{
    public GameObject SourcePrefab { get; set; }
    public ItemPooler OwnerPooler { get; set; }

    /// Call this from gameplay when the item is cleared/removed.
    public void ReturnToPool()
    {
        if (OwnerPooler != null)
            OwnerPooler.Despawn(gameObject);
        else
            Destroy(gameObject);
    }
}