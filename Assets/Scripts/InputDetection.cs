using UnityEngine;
using UnityEngine.InputSystem;

public class InputDetection : MonoBehaviour
{
    public Collection collection;

    private Ray ray;
    private RaycastHit hit;

    void Update()
    {
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit) && hit.collider.TryGetComponent(out CollectableItem item))
            {
                print(item.itemName);
                collection.AddItem(item);
            }
        }
    }
}
