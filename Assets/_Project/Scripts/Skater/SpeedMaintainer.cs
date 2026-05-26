using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpeedMaintainer : MonoBehaviour
{
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
