using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private TextMeshProUGUI bestScoreLabel;
    [SerializeField] private TrickPopup popupPrefab;
    [SerializeField] private RectTransform popupParent;

    [Header("Score Pop")]
    [SerializeField] private float scorePopScale = 1.3f;
    [SerializeField] private float scorePopDuration = 0.25f;

    private TrickPopup activePopup;
    private Coroutine scorePopRoutine;
    private Vector3 scoreBaseScale;
    private TrickPerformer trickPerformer;

    private void Awake()
    {
        if (scoreLabel != null) scoreBaseScale = scoreLabel.transform.localScale;
    }

    private void Start()
    {
        if (scoreLabel != null && scoreManager != null)
        {
            scoreLabel.text = scoreManager.Score.ToString();
        }
        if (bestScoreLabel != null && scoreManager != null)
        {
            bestScoreLabel.text = $"BEST: {scoreManager.HighScore}";
        }
    }

    private void OnEnable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted += HandleTrickStarted;
            trickPerformer.OnTrickCompleted += HandleTrickCompleted;
            trickPerformer.OnTrickFailed += HandleTrickFailed;
        }
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
            scoreManager.OnHighScoreChanged += HandleHighScoreChanged;
        }
    }

    private void OnDisable()
    {
        if (trickPerformer != null)
        {
            trickPerformer.OnTrickStarted -= HandleTrickStarted;
            trickPerformer.OnTrickCompleted -= HandleTrickCompleted;
            trickPerformer.OnTrickFailed -= HandleTrickFailed;
        }
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= HandleScoreChanged;
            scoreManager.OnHighScoreChanged -= HandleHighScoreChanged;
        }
    }

    public void SetTrickPerformer(TrickPerformer tp)
    {
        enabled = false;
        trickPerformer = tp;
        enabled = true;
    }

    private void HandleHighScoreChanged(int newHigh)
    {
        if (bestScoreLabel != null) bestScoreLabel.text = $"BEST: {newHigh}";
    }

    private void HandleTrickStarted(TrickDefinition trick)
    {
        if (popupPrefab == null || popupParent == null) return;

        if (activePopup != null) Destroy(activePopup.gameObject);
        activePopup = Instantiate(popupPrefab, popupParent);
        activePopup.Show(trick.trickName, trick.score);
    }

    private void HandleTrickCompleted(TrickDefinition trick)
    {
        if (activePopup == null) return;
        activePopup.PlaySuccess();
        activePopup = null;
    }

    private void HandleTrickFailed(TrickDefinition trick)
    {
        if (activePopup == null) return;
        activePopup.PlayFail();
        activePopup = null;
    }

    private void HandleScoreChanged(int newScore, int delta)
    {
        if (scoreLabel == null) return;
        scoreLabel.text = newScore.ToString();
        if (scorePopRoutine != null) StopCoroutine(scorePopRoutine);
        scorePopRoutine = StartCoroutine(ScorePop());
    }

    private IEnumerator ScorePop()
    {
        Transform tr = scoreLabel.transform;
        float t = 0f;
        while (t < scorePopDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / scorePopDuration);
            float s = Mathf.Lerp(scorePopScale, 1f, k);
            tr.localScale = scoreBaseScale * s;
            yield return null;
        }
        tr.localScale = scoreBaseScale;
        scorePopRoutine = null;
    }
}
