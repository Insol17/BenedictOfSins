using System.Collections;
using UnityEngine;

public class PauseMenuAnimator : MonoBehaviour
{
    public RectTransform panel;         // 움직일 UI 패널
    public CanvasGroup canvasGroup;     // 페이드용
    public float animDuration = 0.25f;  // 애니메이션 속도
    public Vector2 hiddenPosition = new Vector2(-500f, 0f); // 시작 위치
    public Vector2 visiblePosition = new Vector2(0f, 0f);   // 도착 위치

    private Coroutine currentRoutine;

    void Awake()
    {
        // 초기에는 숨김
        panel.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public bool isAnimating = false;

    public void Show()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(Animate(true));
    }

    public void Hide()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(Animate(false));
    }

    private IEnumerator Animate(bool show)
    {
        isAnimating = true;

        float time = 0f;
        Vector2 start = panel.anchoredPosition;
        Vector2 end = show ? visiblePosition : hiddenPosition;
        float startAlpha = canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;

        while (time < animDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / animDuration);

            panel.anchoredPosition = Vector2.Lerp(start, end, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        panel.anchoredPosition = end;
        canvasGroup.alpha = endAlpha;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;

        isAnimating = false;
    }



}

