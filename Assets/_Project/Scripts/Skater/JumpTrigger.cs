using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpTrigger : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<SkaterController>(out var skater))
        {
            skater.Jump();
        }
    }
}
