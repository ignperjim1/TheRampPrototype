using UnityEngine;

// Trigger placed at the center of the floor. Every time the skater crosses,
// it re-injects velocity.z up to targetSpeed (preserving the sign so the
// pendulum keeps oscillating). Without this, the faceted Mesh Colliders of
// the Kenney ramps bleed energy on every pass and the pendulum decays.
[RequireComponent(typeof(Collider))]
public class SpeedMaintainer : MonoBehaviour
{
    [Tooltip("Velocidad Z a la que se reinjecta el skate al cruzar el trigger (preservando signo). Debe ser >= initialVelocity del SkaterController; si está por debajo, el péndulo decae con cada pasada.")]
    [SerializeField] private float targetSpeed = 6f;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Rigidbody>(out var rb)) return;

        Vector3 v = rb.velocity;
        float dir = Mathf.Sign(v.z);
        if (dir == 0f) return;
        v.z = dir * targetSpeed;
        rb.velocity = v;
    }
}
