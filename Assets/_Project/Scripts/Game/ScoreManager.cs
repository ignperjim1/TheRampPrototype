using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TrickPerformer trickPerformer;

    public int Score { get; private set; }
    public event Action<int, int> OnScoreChanged;

    private void OnEnable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickCompleted += HandleCompleted;
            trickPerformer.OnTrickFailed += HandleFailed;
        }
    }

    private void OnDisable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickCompleted -= HandleCompleted;
            trickPerformer.OnTrickFailed -= HandleFailed;
        }
    }

    private void HandleCompleted(TrickDefinition trick)
    {
        Score += trick.score;
        OnScoreChanged?.Invoke(Score, trick.score);
    }

    private void HandleFailed(TrickDefinition trick)
    {
        if (Score == 0) return;
        int delta = -Score;
        Score = 0;
        OnScoreChanged?.Invoke(Score, delta);
    }
}
