using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class Collection : MonoBehaviour
{
    public int maxItems;
    public int numberNeededToMerge;

    public List<GameObject> placeholderObjects = new();

    public float flyDuration = 1f;
    public float itemScaleInCollection = 0.5f;

    public float timeToScaleObject = 0.5f;

    public GameController gameController;

    public SphericalAttractor planet;

    private List<CollectableItem> items = new();
    public Recipe recipe;

    private int flyingItems = 0;
    private List<CollectableItem> justArrivedItems = new();

    public void Start()
    {
        // Disable meshes for each placeholder object
        foreach (var placeholder in placeholderObjects)
        {
            if (placeholder.TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                meshRenderer.enabled = false;
            }
        }
    }

    public void AddItem(CollectableItem item)
    {
        items.Add(item);
        Debug.Log($"Added {item.itemName} to collection. Total items: {items.Count}");

        flyingItems++;
        StartCoroutine(FlyToCollection(item));
    }

    private void MergeItems(string itemName, int count)
    {
        foreach (var item in items.Where(i => i.itemName == itemName))
        {
            Destroy(item.gameObject);
        }
        items.RemoveAll(i => i.itemName == itemName);
        recipe.AddCollectedItems(itemName, count);
        ReorganizeItems();
    }
    
    private void ReorganizeItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            Vector3 targetPos = placeholderObjects[i].transform.position;
            items[i].transform.position = targetPos;
        }
    }

    private System.Collections.IEnumerator FlyToCollection(CollectableItem item)
    {
        if (item == null) yield break;

        if (planet != null)
        {
            planet.RemoveTarget(item.gameObject);
        }

        int itemIndex = items.Count - 1;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        float startTime = Time.time;

        while (Time.time - startTime < flyDuration)
        {
            Vector3 startPos = item.transform.position;
            Vector3 endPosFlying = placeholderObjects[itemIndex].transform.position; // Always get the latest position
            Vector3 direction = (endPosFlying - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPosFlying);
            float forceMagnitude = distance / flyDuration * (rb != null ? rb.mass : 1f);

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // Reset velocity to avoid overshooting
                rb.angularVelocity = Vector3.zero; // Reset angular velocity to avoid spinning
                rb.AddForce(direction * forceMagnitude, ForceMode.Impulse);
            }
            yield return null;
        }

        Vector3 endPos = placeholderObjects[itemIndex].transform.position;

        // Snap to final position and disable physics after flying is complete
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.position = endPos;
            rb.rotation = Quaternion.identity;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.freezeRotation = true;
        }
        if (item.gameObject.TryGetComponent<Collider>(out var collider)) collider.enabled = false;

        item.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => t.position = Vector3.zero);
        item.transform.SetPositionAndRotation(endPos, Quaternion.identity);
        item.transform.SetParent(transform);

        Vector3 originalScale = item.transform.localScale;

        while ((Time.time - startTime - flyDuration) < timeToScaleObject)
        {
            item.transform.localScale = Vector3.Lerp(originalScale, originalScale * itemScaleInCollection, (Time.time - startTime - flyDuration) / timeToScaleObject);
            yield return null;
        }

        // Mark this item as arrived
        justArrivedItems.Add(item);
        flyingItems--;

        // Only check for merging after all flying items have finished
        if (flyingItems == 0)
        {
            // For each item type, check if enough to merge
            var grouped = items.GroupBy(i => i.itemName);
            foreach (var group in grouped)
            {
                int itemCount = group.Count();
                if (itemCount >= numberNeededToMerge)
                {
                    MergeItems(group.Key, itemCount);
                }
            }
            justArrivedItems.Clear();
            // Check if there's too many items after they are done flying
            if (items.Count >= maxItems)
            {
                gameController.GameOver();
            }
        }

    }
}
