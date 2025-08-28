using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float orbitTimePerRotation = 10.0f;
    public GameObject focusObject;

    void Update()
    {
        if (focusObject != null)
        {
            Vector3 direction = focusObject.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;

            float orbitSpeed = 360.0f / orbitTimePerRotation;
            transform.position = focusObject.transform.position + Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * (transform.position - focusObject.transform.position);
        }
    }
}
