using UnityEngine;
using UnityEngine.InputSystem;

public class InputDetection : MonoBehaviour
{
    public Collection collection;

    public GameController gameController;

    private Ray ray;
    private RaycastHit hit;

    void Update()
    {
        if (!gameController.IsGameRunning()) return;

        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit) && hit.transform.TryGetComponent<CollectableItem>(out var item))
            {
                print(item.itemName);
                collection.AddItem(item);
            }
        }
    }
}
