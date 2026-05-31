using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public event Action<int, int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;

    private const string HighScoreKey = "TheRamp.HighScore";

    private TrickPerformer trickPerformer;

    private void Awake()
    {
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

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

    public void SetTrickPerformer(TrickPerformer tp)
    {
        enabled = false;
        trickPerformer = tp;
        enabled = true;
    }

    private void HandleCompleted(TrickDefinition trick)
    {
        Score += trick.score;
        OnScoreChanged?.Invoke(Score, trick.score);
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(HighScore);
        }
    }

    private void HandleFailed(TrickDefinition trick)
    {
        if (Score == 0) return;
        int delta = -Score;
        Score = 0;
        OnScoreChanged?.Invoke(Score, delta);
    }
}
