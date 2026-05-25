using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SkaterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundCheckOriginOffset = 0.1f;
    [SerializeField] private float postJumpGroundLockTime = 0.15f;

    public enum State { Rolling, Airborne }
    public State CurrentState { get; private set; } = State.Rolling;

    private Rigidbody rb;
    private Collider col;
    private float direction = 1f;
    private float groundLockUntil = 0f;

    private const RigidbodyConstraints GroundConstraints =
        RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionX;

    private const RigidbodyConstraints AirConstraints =
        RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = GroundConstraints;
    }

    private void FixedUpdate()
    {
        State newState = IsGrounded() ? State.Rolling : State.Airborne;

        if (newState == State.Rolling)
        {
            Vector3 v = rb.velocity;
            v.z = direction * speed;
            rb.velocity = v;
        }

        if (newState == State.Rolling && CurrentState == State.Airborne)
        {
            OnLanded();
        }
        CurrentState = newState;
    }

    public void Jump()
    {
        rb.constraints = AirConstraints;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        Vector3 v = rb.velocity;
        v.y = jumpForce;
        v.z = 0f;
        rb.velocity = v;

        CurrentState = State.Airborne;
        groundLockUntil = Time.time + postJumpGroundLockTime;
    }

    private void OnLanded()
    {
        Debug.Log("onlanded");
        rb.constraints = GroundConstraints;
        FlipDirection();
    }

    public void FlipDirection()
    {
        direction *= -1f;
    }

    private bool IsGrounded()
    {
        if (Time.time < groundLockUntil) return false;

        Vector3 origin = col.bounds.center + Vector3.down * (col.bounds.extents.y - groundCheckOriginOffset);
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<Collider>();
        if (col == null) return;
        Vector3 origin = col.bounds.center + Vector3.down * (col.bounds.extents.y - groundCheckOriginOffset);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
