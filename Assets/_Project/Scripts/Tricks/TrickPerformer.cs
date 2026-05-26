using System;
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

    public event Action<TrickDefinition> OnTrickStarted;
    public event Action<TrickDefinition> OnTrickCompleted;
    public event Action<TrickDefinition> OnTrickFailed;
    public event Action OnCleanLanding;

    [Header("Input Buffer")]
    [SerializeField] private float swipeBufferDuration = 0.3f;
    [SerializeField] private float chainBufferDuration = 0.1f;

    [Header("Air Window (manual)")]
    [SerializeField] private float windowStart = 0f;
    [SerializeField] private float windowEnd = 2.0f;

    private bool isPerforming;
    private float airborneSince = -1f;
    private SwipeDirection? bufferedSwipe;
    private float bufferedAt;
    private bool hasPendingCleanLanding;

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
        bool airborne = skater.CurrentState == SkaterController.State.Airborne;

        if (airborne)
        {
            if (airborneSince < 0f) airborneSince = Time.time;
        }
        else
        {
            airborneSince = -1f;
            if (hasPendingCleanLanding)
            {
                OnCleanLanding?.Invoke();
                hasPendingCleanLanding = false;
            }
        }

        if (isPerforming) return;
        if (!bufferedSwipe.HasValue) return;

        if (Time.time - bufferedAt > swipeBufferDuration)
        {
            bufferedSwipe = null;
            return;
        }

        if (airborne && IsInAirWindow())
        {
            TryPerform(bufferedSwipe.Value);
            bufferedSwipe = null;
        }
    }

    private void HandleSwipe(SwipeDirection dir)
    {
        bufferedSwipe = dir;
        bufferedAt = Time.time;

        if (isPerforming) return;

        if (skater.CurrentState == SkaterController.State.Airborne && IsInAirWindow())
        {
            TryPerform(dir);
            bufferedSwipe = null;
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
        OnTrickStarted?.Invoke(trick);

        Quaternion start = skateModel.localRotation;
        Vector3 axis = trick.rotationAxis.sqrMagnitude > 0.001f ? trick.rotationAxis.normalized : Vector3.forward;

        float t = 0f;
        while (t < trick.duration)
        {
            if (skater.CurrentState != SkaterController.State.Airborne)
            {
                skateModel.localRotation = start;
                isPerforming = false;
                OnTrickFailed?.Invoke(trick);
                yield break;
            }

            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / trick.duration);
            float angle = k * trick.rotationDegrees;
            skateModel.localRotation = start * Quaternion.AngleAxis(angle, axis);
            yield return null;
        }

        skateModel.localRotation = start;
        isPerforming = false;
        hasPendingCleanLanding = true;
        OnTrickCompleted?.Invoke(trick);

        TryChainFromBuffer();
    }

    private void TryChainFromBuffer()
    {
        if (!bufferedSwipe.HasValue) return;
        if (Time.time - bufferedAt > chainBufferDuration) return;
        if (skater.CurrentState != SkaterController.State.Airborne) return;
        if (!IsInAirWindow()) return;

        SwipeDirection dir = bufferedSwipe.Value;
        bufferedSwipe = null;
        TryPerform(dir);
    }
}
