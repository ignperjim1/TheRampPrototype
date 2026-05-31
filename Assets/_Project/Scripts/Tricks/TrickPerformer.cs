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

    [Header("Chaining")]
    [Tooltip("Fracción del trick (0-1) a partir de la cual un swipe buffereado interrumpe la rotación actual y lanza el siguiente trick. Más bajo = combo más rápido pero rotación visible más corta.")]
    [SerializeField, Range(0.3f, 1f)] private float chainAfter = 0.7f;

    [Header("Rotation")]
    [SerializeField] private AnimationCurve rotationEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Segundos que tarda el modelo en volver suavemente a la pose inicial (comboBase) tras completar un trick sin encadenar. Evita el snap brusco si la rotación no cierra en múltiplo de 360°.")]
    [SerializeField] private float settleDuration = 0.12f;

    [Header("Air Window (manual)")]
    [SerializeField] private float windowStart = 0f;
    [Tooltip("Segundos desde el despegue tras los que ya no se acepta input de trick. Override duro: aunque haya swipe buffereado, si t > windowEnd se descarta.")]
    [SerializeField] private float windowEnd = 2.0f;

    private bool isPerforming;
    private float airborneSince = -1f;
    private SwipeDirection? bufferedSwipe;
    private float bufferedAt;
    private bool hasPendingCleanLanding;
    private bool hasTouchedGround;
    private Quaternion comboBase = Quaternion.identity;
    private Coroutine settleRoutine;

    private void OnEnable()
    {
        if (swipeDetector != null) swipeDetector.OnSwipe += HandleSwipe;

        // Reset so that the first airborne after enabling (initial spawn or
        // post-respawn) doesn't count as a trickable jump until the skater
        // has actually touched ground.
        hasTouchedGround = false;
        airborneSince = -1f;
        hasPendingCleanLanding = false;
        bufferedSwipe = null;
    }

    private void OnDisable()
    {
        if (swipeDetector != null) swipeDetector.OnSwipe -= HandleSwipe;
    }

    private void Update()
    {
        if (skater.IsRespawning) hasTouchedGround = false;

        bool airborne = skater.CurrentState == SkaterController.State.Airborne;

        if (airborne)
        {
            if (airborneSince < 0f)
            {
                airborneSince = Time.time;
                // Capture comboBase ONLY at takeoff, not between chained
                // tricks: the second trick of a combo must start from the
                // current (rotated) pose so the chain flows continuously.
                comboBase = skateModel.localRotation;
            }
        }
        else
        {
            airborneSince = -1f;
            hasTouchedGround = true;
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
        // hasTouchedGround filters out the descent from spawn and the descent
        // right after a respawn — neither is a real "jump", so tricks during
        // those moments would be unearned. IsRespawning extends the filter
        // through the ragdoll flight itself.
        if (!hasTouchedGround) return false;
        if (skater.IsRespawning) return false;
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
        if (settleRoutine != null) { StopCoroutine(settleRoutine); settleRoutine = null; }

        isPerforming = true;
        OnTrickStarted?.Invoke(trick);

        Quaternion start = skateModel.localRotation;
        Vector3 deg = trick.rotationDegrees;

        float t = 0f;
        bool chaining = false;
        while (t < trick.duration)
        {
            if (skater.CurrentState != SkaterController.State.Airborne)
            {
                skateModel.localRotation = comboBase;
                isPerforming = false;
                OnTrickFailed?.Invoke(trick);
                yield break;
            }

            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / trick.duration);
            float e = rotationEase.Evaluate(k);
            skateModel.localRotation = start
                * Quaternion.AngleAxis(e * deg.z, Vector3.forward)
                * Quaternion.AngleAxis(e * deg.x, Vector3.right)
                * Quaternion.AngleAxis(e * deg.y, Vector3.up);

            // Chain window: once past chainAfter, a buffered swipe starts the next
            // trick early. The board keeps its current rotation so the combo flows
            // continuously instead of snapping back to flat between tricks.
            if (k >= chainAfter && HasChainableBuffer())
            {
                chaining = true;
                break;
            }

            yield return null;
        }

        isPerforming = false;
        OnTrickCompleted?.Invoke(trick);

        if (chaining)
        {
            SwipeDirection dir = bufferedSwipe.Value;
            bufferedSwipe = null;
            TryPerform(dir);
        }
        else
        {
            settleRoutine = StartCoroutine(SettleToComboBase());
            hasPendingCleanLanding = true;
        }
    }

    private IEnumerator SettleToComboBase()
    {
        Quaternion from = skateModel.localRotation;
        float t = 0f;
        while (t < settleDuration)
        {
            t += Time.deltaTime;
            skateModel.localRotation = Quaternion.Slerp(from, comboBase, Mathf.Clamp01(t / settleDuration));
            yield return null;
        }
        skateModel.localRotation = comboBase;
        settleRoutine = null;
    }

    private bool HasChainableBuffer()
    {
        if (!bufferedSwipe.HasValue) return false;
        if (Time.time - bufferedAt > swipeBufferDuration) return false;
        if (!IsInAirWindow()) return false;
        return ResolveTrick(bufferedSwipe.Value) != null;
    }
}
