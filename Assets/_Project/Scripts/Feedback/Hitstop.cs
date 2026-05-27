using System.Collections;
using UnityEngine;

public class Hitstop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrickPerformer trickPerformer;

    [Header("Hitstop on Clean Landing")]
    [SerializeField] private float duration = 0.08f;
    [SerializeField, Range(0f, 1f)] private float freezeScale = 0f;

    private Coroutine routine;

    private void OnEnable()
    {
        if (trickPerformer != null) trickPerformer.OnCleanLanding += HandleCleanLanding;
    }

    private void OnDisable()
    {
        if (trickPerformer != null) trickPerformer.OnCleanLanding -= HandleCleanLanding;
        if (routine != null)
        {
            Time.timeScale = 1f;
            routine = null;
        }
    }

    private void HandleCleanLanding() => Play();

    public void Play()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HitstopRoutine());
    }

    private IEnumerator HitstopRoutine()
    {
        Time.timeScale = freezeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        routine = null;
    }
}
