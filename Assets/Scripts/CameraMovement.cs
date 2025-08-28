using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float orbitTimePerRotation = 10.0f;

    public float constantYOffset = 0.0f;
    public GameObject focusObject;

    void Update()
    {
        if (focusObject != null)
        {
            Vector3 offset = new(0, constantYOffset, 0);
            Vector3 focusPoint = focusObject.transform.position + offset;
            Vector3 direction = focusPoint - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;

            float orbitSpeed = 360.0f / orbitTimePerRotation;
            transform.position = focusPoint + Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * (transform.position - focusPoint);
        }
    }
}
