using System.Collections;
using System.Reflection;
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
    [SerializeField] private float chromaticPeak = 2.5f;
    [SerializeField] private float chromaticDuration = 0.4f;
    // URP clamps ChromaticAberration.intensity to [0, 1] via ClampedFloatParameter.max.
    // We override max here to allow stronger pulses for impact feedback. One-shot in Awake.
    [Tooltip("Nuevo tope para ChromaticAberration.intensity (URP lo clampea a 1 por defecto). Se aplica vía reflection en Awake. Sube este valor si quieres pulsos más fuertes que el chromaticPeak actual.")]
    [SerializeField] private float chromaticMaxOverride = 5f;

    [Header("Lens Distortion Pulse (Clean Landing)")]
    [SerializeField, Range(-1f, 1f)] private float lensPeak = 0.35f;
    [SerializeField] private float lensDuration = 0.4f;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Coroutine impactRoutine;
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
            runtimeProfile.TryGet(out lensDistortion);
        }
        if (chromaticAberration != null)
        {
            UnlockClampedMax(chromaticAberration.intensity, chromaticMaxOverride);
            chromaticAberration.intensity.value = 0f;
        }
        if (vignette != null) vignette.intensity.value = vignetteGrounded;
        if (lensDistortion != null) lensDistortion.intensity.value = 0f;
    }

    private static void UnlockClampedMax(ClampedFloatParameter param, float newMax)
    {
        var maxField = typeof(ClampedFloatParameter).GetField("max", BindingFlags.Public | BindingFlags.Instance);
        if (maxField != null) maxField.SetValue(param, newMax);
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

    public void SetTrickPerformer(TrickPerformer tp)
    {
        enabled = false;
        trickPerformer = tp;
        enabled = true;
    }

    public void SetSkater(SkaterController s) => skater = s;

    private void HandleCleanLanding()
    {
        if (impactRoutine != null) StopCoroutine(impactRoutine);
        impactRoutine = StartCoroutine(ImpactPulse());
    }

    private IEnumerator ImpactPulse()
    {
        if (chromaticAberration != null) chromaticAberration.intensity.value = chromaticPeak;
        if (lensDistortion != null) lensDistortion.intensity.value = lensPeak;

        float maxDuration = Mathf.Max(chromaticDuration, lensDuration);
        float t = 0f;
        while (t < maxDuration)
        {
            t += Time.deltaTime;
            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(chromaticPeak, 0f, Mathf.Clamp01(t / chromaticDuration));
            if (lensDistortion != null)
                lensDistortion.intensity.value = Mathf.Lerp(lensPeak, 0f, Mathf.Clamp01(t / lensDuration));
            yield return null;
        }
        if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
        if (lensDistortion != null) lensDistortion.intensity.value = 0f;
        impactRoutine = null;
    }
}
