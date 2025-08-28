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

    private List<CollectableItem> items = new();

    public void AddItem(CollectableItem item)
    {
        if (items.Count >= maxItems)
        {
            Debug.Log("Collection is full!");
            return;
        }

        items.Add(item);
        Debug.Log($"Added {item.itemName} to collection. Total items: {items.Count}");

        // Fly the object to the collection
        StartCoroutine(FlyToCollection(item));
    }

    private void MergeItems(string itemName)
    {
        Debug.Log($"Merging items with name: {itemName}");
        // Destroy merged gameobjects
        foreach (var item in items.Where(i => i.itemName == itemName))
        {
            Destroy(item.gameObject);
        }
        // Remove merged items from list
        items.RemoveAll(i => i.itemName == itemName);
    }

    private System.Collections.IEnumerator FlyToCollection(CollectableItem item)
    {
        Vector3 startPos = item.transform.position;
        Vector3 endPos = transform.position + new Vector3(itemSpacing * (items.Count - 1), 0, 0);
        float elapsedTime = 0f;

        // Disable collider and make the item kinematic
        if (item.TryGetComponent<Collider>(out var collider)) collider.enabled = false;
        if (item.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;

        while (elapsedTime < flyDuration)
        {
            item.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / flyDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        item.transform.position = endPos;
        item.transform.SetParent(transform);

        while ((elapsedTime - flyDuration) < timeToScaleObject)
        {
            item.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * itemScaleInCollection, (elapsedTime - flyDuration) / timeToScaleObject);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Count the number of items with this name
        int itemCount = items.Count(i => i.itemName == item.itemName);
        if (itemCount >= numberNeededToMerge)
        {
            MergeItems(item.itemName);
        }
    }
}
