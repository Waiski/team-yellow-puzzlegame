using System.Collections.Generic;
using UnityEngine;

/// Add this to an empty GameObject at the sphere center.
/// Any non-kinematic Rigidbody under this object (or in `targets`) will be pulled
/// toward a spherical shell at `radius`, then tangential motion is damped so it settles.
/// Tip: run it for ~1s at level start, then set `active=false` and/or make RBs kinematic.
[DefaultExecutionOrder(10)]
public class SphericalAttractor : MonoBehaviour
{
    public HeapSpawner _spawner;

    [Header("Sphere")]
    public Transform center;                // If null, uses this.transform
    [Min(0.01f)] public float radius = 3f;  // Target shell radius

    [Header("Forces")]
    [Tooltip("Constant pull toward the center (helps convergence).")]
    public float radialAccel = 15f;         // m/s^2 toward center
    [Tooltip("Spring strength that corrects distance error to land on the shell.")]
    public float surfaceTightness = 20f;    // m/s^2 per meter of error
    [Tooltip("Damps sideways motion so items stop sliding around the shell.")]
    public float tangentialDamp = 2f;       // 1/s applied to tangential velocity
    [Tooltip("Optional cap for total acceleration (prevents explosions). Set <=0 to disable.")]
    public float maxAccel = 60f;            // m/s^2 cap (0 = no cap)

    [Header("What to affect")]
    public List<Rigidbody> targets = new List<Rigidbody>();

    void Reset()
    {
        center = transform;
    }

    void Awake()
    {
        if (center == null) center = transform;
        _spawner.ItemSpawned += AddTarget;
    }

    void FixedUpdate()
    {
        if (center == null) center = transform;

        Vector3 c = center.position;
        for (int i = 0; i < targets.Count; i++)
        {
            var rb = targets[i];
            if (rb == null || rb.isKinematic) continue;

            Vector3 p = rb.worldCenterOfMass;
            Vector3 toC = c - p;
            float dist = toC.magnitude;
            if (dist < 1e-4f) continue;

            Vector3 dir = toC / dist;

            // Distance error to the shell (positive if item is inside the sphere,
            // negative if outside). We want error -> 0.
            float err = radius - dist;

            // Base accelerations:
            // 1) Pull toward center (helps convergence and keeps things stuck)
            Vector3 a = dir * radialAccel;

            // 2) Spring toward shell (correct radial error)
            a += dir * (err * surfaceTightness);

            // 3) Dampen tangential velocity to help settling
            Vector3 v = rb.linearVelocity;
            Vector3 radialV = Vector3.Dot(v, dir) * dir;
            Vector3 tangentialV = v - radialV;
            a += -tangentialV * tangentialDamp;

            // Optional cap (prevents big impulses on light objects)
            if (maxAccel > 0f)
            {
                float mag = a.magnitude;
                if (mag > maxAccel) a = a * (maxAccel / mag);
            }

            rb.AddForce(a, ForceMode.Acceleration);
        }
    }

    void AddTarget(GameObject go)
    {
        targets.Add(go.GetComponent<Rigidbody>());
    }

    public void RemoveTarget(GameObject go)
    {
        targets.Remove(go.GetComponent<Rigidbody>());
    }
}
