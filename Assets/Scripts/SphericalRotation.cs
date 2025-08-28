using UnityEngine;

[DisallowMultipleComponent]
public class SphericalRotation : MonoBehaviour
{
    [Header("Rotation")]
    public Vector3 rotationAxis = Vector3.up;   // Axis in local or world space
    public float degreesPerSecond = 30f;        // Spin rate

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        Vector3 worldAxis =  rotationAxis.normalized;
        float radiansPerSec = Mathf.Deg2Rad * degreesPerSecond;
        
        // Dynamic: drive spin via angular velocity so contacts are correct
        _rb.angularVelocity = worldAxis * radiansPerSec;
    }

    void OnDisable()
    {
        // Stop spin for dynamic bodies
        if (_rb != null)
            _rb.angularVelocity = Vector3.zero;
    }
}