using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TrickPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Color successColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private Color failColor = new Color(1f, 0.35f, 0.35f);

    [Header("Pop-in")]
    [SerializeField] private float popInDuration = 0.15f;
    [SerializeField] private float popInOvershoot = 1.2f;

    [Header("Success (clean landing)")]
    [SerializeField] private float successHold = 0.25f;
    [SerializeField] private float successFadeDuration = 0.4f;
    [SerializeField] private float successRise = 80f;

    [Header("Fail")]
    [SerializeField] private float failFallDistance = 140f;
    [SerializeField] private float failFallDuration = 0.6f;
    [SerializeField] private float failTilt = 18f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 baseAnchoredPos;

    private void Awake()
    {
        rect = (RectTransform)transform;
        canvasGroup = GetComponent<CanvasGroup>();
        baseAnchoredPos = rect.anchoredPosition;
    }

    public void Show(string trickName, int score)
    {
        if (label != null) label.text = $"{trickName.ToUpper()}  <size=70%>+{score}";
        if (label != null) label.color = Color.white;
        canvasGroup.alpha = 1f;
        rect.anchoredPosition = baseAnchoredPos;
        rect.localRotation = Quaternion.identity;
        StartCoroutine(PopIn());
    }

    public void PlaySuccess()
    {
        StopAllCoroutines();
        StartCoroutine(SuccessRoutine());
    }

    public void PlayFail()
    {
        StopAllCoroutines();
        StartCoroutine(FailRoutine());
    }

    private IEnumerator PopIn()
    {
        float t = 0f;
        while (t < popInDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popInDuration);
            float s = Mathf.Lerp(0.4f, popInOvershoot, k);
            rect.localScale = Vector3.one * s;
            yield return null;
        }
        float settle = 0.08f;
        float st = 0f;
        while (st < settle)
        {
            st += Time.deltaTime;
            float k = Mathf.Clamp01(st / settle);
            rect.localScale = Vector3.one * Mathf.Lerp(popInOvershoot, 1f, k);
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    private IEnumerator SuccessRoutine()
    {
        if (label != null) label.color = successColor;
        rect.localScale = Vector3.one * 1.1f;

        yield return new WaitForSeconds(successHold);

        float t = 0f;
        Vector2 startPos = rect.anchoredPosition;
        while (t < successFadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / successFadeDuration);
            rect.anchoredPosition = startPos + Vector2.up * (k * successRise);
            canvasGroup.alpha = 1f - k;
            yield return null;
        }
        Destroy(gameObject);
    }

    private IEnumerator FailRoutine()
    {
        if (label != null) label.color = failColor;

        float t = 0f;
        Vector2 startPos = rect.anchoredPosition;
        while (t < failFallDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / failFallDuration);
            float ease = k * k;
            rect.anchoredPosition = startPos + Vector2.down * (ease * failFallDistance);
            rect.localRotation = Quaternion.Euler(0f, 0f, ease * failTilt);
            canvasGroup.alpha = 1f - k;
            yield return null;
        }
        Destroy(gameObject);
    }
}
