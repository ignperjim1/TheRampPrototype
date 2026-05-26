using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SkaterController : MonoBehaviour
{
    [Header("Initial Impulse")]
    [SerializeField] private float initialVelocity = 6f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundCheckOriginOffset = 0.1f;
    [SerializeField] private float wheelOffset = 0.3f;
    [SerializeField] private float coyoteTime = 0.1f;

    public enum State { Rolling, Airborne }
    public State CurrentState { get; private set; } = State.Rolling;

    private Rigidbody rb;
    private Collider col;
    private float lastGroundedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        //rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
    }

    private void Start()
    {
        rb.velocity = new Vector3(0f, 0f, initialVelocity);
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();
        if (grounded) lastGroundedTime = Time.time;

        bool withinCoyote = Time.time - lastGroundedTime <= coyoteTime;
        CurrentState = (grounded || withinCoyote) ? State.Rolling : State.Airborne;
    }

    private bool IsGrounded()
    {
        return WheelGrounded(transform.forward) || WheelGrounded(-transform.forward);
    }

    private bool WheelGrounded(Vector3 forwardOffset)
    {
        Vector3 origin = col.bounds.center
            + forwardOffset * wheelOffset
            + Vector3.down * (col.bounds.extents.y - groundCheckOriginOffset);
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = Color.green;
        DrawWheelGizmo(transform.forward);
        DrawWheelGizmo(-transform.forward);
    }

    private void DrawWheelGizmo(Vector3 forwardOffset)
    {
        Vector3 origin = col.bounds.center
            + forwardOffset * wheelOffset
            + Vector3.down * (col.bounds.extents.y - groundCheckOriginOffset);
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
}
