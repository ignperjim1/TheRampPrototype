using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrickPerformer trickPerformer;
    [SerializeField] private Transform focusTarget;

    [Header("Trick Start Shake")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeAmplitude = 0.15f;
    [SerializeField] private float shakeFrequency = 25f;
    [SerializeField] private AnimationCurve shakeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Zoom (Trick Start)")]
    [SerializeField] private float zoomDuration = 0.45f;
    [SerializeField] private float zoomDeltaFOV = 6f;
    [SerializeField] private float zoomMoveDistance = 0f;
    [SerializeField] private AnimationCurve zoomShape = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    [Header("Landing Shake (clean only)")]
    [SerializeField] private float landingShakeDuration = 0.4f;
    [SerializeField] private float landingShakeAmplitude = 0.35f;
    [SerializeField] private float landingShakeFrequency = 35f;
    [SerializeField] private AnimationCurve landingShakeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Camera cam;
    private Vector3 basePosition;
    private float baseFOV;
    private Vector3 currentShakeOffset;
    private Vector3 currentZoomOffset;
    private float currentFOVDelta;
    private Coroutine shakeRoutine;
    private Coroutine zoomRoutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        basePosition = transform.position;
        baseFOV = cam.fieldOfView;
    }

    private void OnEnable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted += HandleTrickStarted;
            trickPerformer.OnCleanLanding += HandleCleanLanding;
        }
    }

    private void OnDisable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted -= HandleTrickStarted;
            trickPerformer.OnCleanLanding -= HandleCleanLanding;
        }
    }

    private void LateUpdate()
    {
        transform.position = basePosition + currentShakeOffset + currentZoomOffset;
        cam.fieldOfView = baseFOV + currentFOVDelta;
    }

    private void HandleTrickStarted(TrickDefinition trick) => PlayFeedback();

    private void HandleCleanLanding() => PlayLandingFeedback();

    public void PlayFeedback()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        if (zoomRoutine != null) StopCoroutine(zoomRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(shakeDuration, shakeAmplitude, shakeFrequency, shakeFalloff));
        zoomRoutine = StartCoroutine(ZoomRoutine());
    }

    public void PlayLandingFeedback()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(landingShakeDuration, landingShakeAmplitude, landingShakeFrequency, landingShakeFalloff));
    }

    private IEnumerator ShakeRoutine(float duration, float amplitude, float frequency, AnimationCurve falloff)
    {
        float t = 0f;
        float seed = Random.value * 100f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float amp = falloff.Evaluate(k) * amplitude;
            float nx = (Mathf.PerlinNoise(seed + t * frequency, 0f) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(0f, seed + t * frequency) - 0.5f) * 2f;
            currentShakeOffset = new Vector3(nx * amp, ny * amp, 0f);
            yield return null;
        }
        currentShakeOffset = Vector3.zero;
        shakeRoutine = null;
    }

    private IEnumerator ZoomRoutine()
    {
        Vector3 dirToTarget = focusTarget != null
            ? (focusTarget.position - basePosition).normalized
            : Vector3.zero;

        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / zoomDuration);
            float shape = zoomShape.Evaluate(k);
            currentFOVDelta = shape * zoomDeltaFOV;
            currentZoomOffset = dirToTarget * (shape * zoomMoveDistance);
            yield return null;
        }
        currentFOVDelta = 0f;
        currentZoomOffset = Vector3.zero;
        zoomRoutine = null;
    }
}
