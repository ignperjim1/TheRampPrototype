using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume volume;
    [SerializeField] private SkaterController skater;
    [SerializeField] private TrickPerformer trickPerformer;

    [Header("Vignette by State")]
    [SerializeField] private float vignetteGrounded = 0.15f;
    [SerializeField] private float vignetteAirborne = 0.45f;
    [SerializeField] private float vignetteLerpSpeed = 2.5f;

    [Header("Chromatic Aberration Pulse (Clean Landing)")]
    [SerializeField, Range(0f, 1f)] private float chromaticPeak = 0.8f;
    [SerializeField] private float chromaticDuration = 0.4f;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Coroutine chromaticRoutine;
    private VolumeProfile runtimeProfile;
    private bool hasTouchedGround;

    private void Awake()
    {
        if (volume != null && volume.sharedProfile != null)
        {
            runtimeProfile = Instantiate(volume.sharedProfile);
            volume.profile = runtimeProfile;
            runtimeProfile.TryGet(out vignette);
            runtimeProfile.TryGet(out chromaticAberration);
        }
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (vignette != null) vignette.intensity.value = vignetteGrounded;
    }

    private void OnDestroy()
    {
        if (runtimeProfile != null) Destroy(runtimeProfile);
    }

    private void OnEnable()
    {
        if (trickPerformer != null) trickPerformer.OnCleanLanding += HandleCleanLanding;
    }

    private void OnDisable()
    {
        if (trickPerformer != null) trickPerformer.OnCleanLanding -= HandleCleanLanding;
    }

    private void Update()
    {
        if (vignette == null || skater == null) return;

        bool isRolling = skater.CurrentState == SkaterController.State.Rolling;
        if (skater.IsRespawning) hasTouchedGround = false;
        else if (isRolling) hasTouchedGround = true;

        bool inAir = hasTouchedGround && !isRolling && !skater.IsRespawning;
        float target = inAir ? vignetteAirborne : vignetteGrounded;
        vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, target, vignetteLerpSpeed * Time.deltaTime);
    }

    private void HandleCleanLanding()
    {
        if (chromaticAberration == null) return;
        if (chromaticRoutine != null) StopCoroutine(chromaticRoutine);
        chromaticRoutine = StartCoroutine(ChromaticPulse());
    }

    private IEnumerator ChromaticPulse()
    {
        chromaticAberration.intensity.value = chromaticPeak;
        float t = 0f;
        while (t < chromaticDuration)
        {
            t += Time.deltaTime;
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticPeak, 0f, t / chromaticDuration);
            yield return null;
        }
        chromaticAberration.intensity.value = 0f;
        chromaticRoutine = null;
    }
}
