using System.Collections;
using UnityEngine;

public class LandingVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrickPerformer trickPerformer;

    [Header("Clean Landing Burst")]
    [SerializeField] private ParticleSystem[] landingBursts;

    [Header("Post-Landing Trail")]
    [SerializeField] private TrailRenderer landingTrail;
    [SerializeField] private float trailDuration = 1f;

    private Coroutine trailRoutine;

    private void Awake()
    {
        if (landingTrail != null) landingTrail.emitting = false;
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

        if (landingTrail != null)
        {
            if (trailRoutine != null) StopCoroutine(trailRoutine);
            trailRoutine = StartCoroutine(TrailRoutine());
        }
    }

    private IEnumerator TrailRoutine()
    {
        landingTrail.emitting = true;
        yield return new WaitForSeconds(trailDuration);
        landingTrail.emitting = false;
        trailRoutine = null;
    }
}
