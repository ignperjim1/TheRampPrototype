using System.Collections;
using UnityEngine;

public class TrickPerformer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwipeDetector swipeDetector;
    [SerializeField] private SkaterController skater;
    [SerializeField] private Transform skateModel;

    [Header("Tricks")]
    [SerializeField] private TrickDefinition[] tricks;

    [Header("Input Buffer")]
    [SerializeField] private float swipeBufferDuration = 0.3f;

    [Header("Air Window (manual)")]
    [SerializeField] private float windowStart = 0f;
    [SerializeField] private float windowEnd = 0.8f;

    private bool isPerforming;
    private float airborneSince = -1f;
    private SwipeDirection? bufferedSwipe;
    private float bufferedAt;

    private void OnEnable()
    {
        if (swipeDetector != null) swipeDetector.OnSwipe += HandleSwipe;
    }

    private void OnDisable()
    {
        if (swipeDetector != null) swipeDetector.OnSwipe -= HandleSwipe;
    }

    private void Update()
    {
        if (skater.CurrentState == SkaterController.State.Airborne)
        {
            if (airborneSince < 0f) airborneSince = Time.time;
        }
        else
        {
            airborneSince = -1f;
        }

        if (!bufferedSwipe.HasValue) return;

        if (Time.time - bufferedAt > swipeBufferDuration)
        {
            bufferedSwipe = null;
            return;
        }

        if (skater.CurrentState == SkaterController.State.Airborne && IsInAirWindow())
        {
            TryPerform(bufferedSwipe.Value);
            bufferedSwipe = null;
        }
    }

    private void HandleSwipe(SwipeDirection dir)
    {
        if (isPerforming) return;

        if (skater.CurrentState == SkaterController.State.Airborne)
        {
            if (IsInAirWindow()) TryPerform(dir);
        }
        else
        {
            bufferedSwipe = dir;
            bufferedAt = Time.time;
        }
    }

    private bool IsInAirWindow()
    {
        if (airborneSince < 0f) return false;
        float t = Time.time - airborneSince;
        return t >= windowStart && t <= windowEnd;
    }

    private void TryPerform(SwipeDirection dir)
    {
        TrickDefinition trick = ResolveTrick(dir);
        if (trick == null) return;
        StartCoroutine(PerformTrick(trick));
    }

    private TrickDefinition ResolveTrick(SwipeDirection dir)
    {
        foreach (var t in tricks)
        {
            if (t.swipeDirection == dir) return t;
        }
        return null;
    }

    private IEnumerator PerformTrick(TrickDefinition trick)
    {
        isPerforming = true;
        Debug.Log($"Trick: {trick.trickName} | score={trick.score}");

        Quaternion start = skateModel.localRotation;
        Vector3 axis = trick.rotationAxis.sqrMagnitude > 0.001f ? trick.rotationAxis.normalized : Vector3.forward;

        float t = 0f;
        while (t < trick.duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / trick.duration);
            float angle = k * trick.rotationDegrees;
            skateModel.localRotation = start * Quaternion.AngleAxis(angle, axis);
            yield return null;
        }

        skateModel.localRotation = start;
        isPerforming = false;
    }
}
