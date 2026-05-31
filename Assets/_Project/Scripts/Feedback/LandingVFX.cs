using System.Collections;
using UnityEngine;

public class LandingVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrickPerformer trickPerformer;

    [Header("Clean Landing Burst")]
    [SerializeField] private ParticleSystem[] landingBursts;

    [Header("Post-Landing Trail")]
    [SerializeField] private TrailRenderer[] landingTrails;
    [SerializeField] private float trailDuration = 1f;

    private Coroutine trailRoutine;

    private void Awake()
    {
        SetTrailsEmitting(false);
    }

    private void OnEnable()
    {
        if (trickPerformer != null) trickPerformer.OnCleanLanding += HandleCleanLanding;
    }

    private void OnDisable()
    {
        if (trickPerformer != null) trickPerformer.OnCleanLanding -= HandleCleanLanding;
    }

    private void HandleCleanLanding()
    {
        for (int i = 0; i < landingBursts.Length; i++)
        {
            if (landingBursts[i] != null) landingBursts[i].Play();
        }

        if (landingTrails != null && landingTrails.Length > 0)
        {
            if (trailRoutine != null) StopCoroutine(trailRoutine);
            trailRoutine = StartCoroutine(TrailRoutine());
        }
    }

    private IEnumerator TrailRoutine()
    {
        SetTrailsEmitting(true);
        yield return new WaitForSeconds(trailDuration);
        SetTrailsEmitting(false);
        trailRoutine = null;
    }

    private void SetTrailsEmitting(bool emitting)
    {
        if (landingTrails == null) return;
        for (int i = 0; i < landingTrails.Length; i++)
        {
            if (landingTrails[i] != null) landingTrails[i].emitting = emitting;
        }
    }
}
