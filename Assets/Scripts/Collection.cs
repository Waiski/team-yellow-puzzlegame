using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collection : MonoBehaviour
{
    public int maxItems;
    public int numberNeededToMerge;

    public float flyDuration = 1f;
    public float itemScaleInCollection = 0.5f;

    public float itemSpacing = 0.5f;

    public float timeToScaleObject = 0.5f;

    public GameController gameController;

    private List<CollectableItem> items = new();
    public Recipe recipe;

    private int flyingItems = 0;
    private List<CollectableItem> justArrivedItems = new();

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
    }

    private System.Collections.IEnumerator FlyToCollection(CollectableItem item)
    {
        if (item == null) yield break;


        Vector3 startPos = item.transform.position;
        Vector3 endPos = transform.position + new Vector3(itemSpacing * (items.Count - 1), 0, 0);

        Rigidbody rb = item.GetComponent<Rigidbody>();

        float startTime = Time.time;
        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        float forceMagnitude = distance / flyDuration * (rb != null ? rb.mass : 1f);

        if (rb != null)
            rb.AddForce(direction * forceMagnitude, ForceMode.Impulse);

        while (Time.time - startTime < flyDuration)
        {
            yield return null;
        }

        // Snap to final position and disable physics after flying is complete
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.position = endPos;
        }
        else
        {
            item.transform.position = endPos;
        }
        if (item.gameObject.TryGetComponent<Collider>(out var collider)) collider.enabled = false;

        item.transform.position = endPos;
        item.transform.SetParent(transform);

        while ((Time.time - startTime - flyDuration) < timeToScaleObject)
        {
            item.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * itemScaleInCollection, (Time.time - startTime - flyDuration) / timeToScaleObject);
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
        }

        // Check if there's too many items after they are done flying
        if (items.Count >= maxItems)
        {
            gameController.GameOver();
        }
    }
}
