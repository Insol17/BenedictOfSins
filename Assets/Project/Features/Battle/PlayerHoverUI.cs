using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerHoverUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup hoverCanvas;
    [SerializeField] private float fadeDuration = 0.1f;

    private Coroutine fadeRoutine;
    private bool isVisible = false;

    private void Awake()
    {
        if (hoverCanvas == null)
            hoverCanvas = GetComponentInChildren<CanvasGroup>();

        // 초기 상태
        hoverCanvas.alpha = 0f;
        hoverCanvas.interactable = false;
        hoverCanvas.blocksRaycasts = false;
    }

    public void Show()
    {
        SetHover(true);
    }

    public void Hide()
    {
        SetHover(false);
    }

    private void Update()
    {
        if (!TargetClickListener.IsTargeting) return;

        var card = TargetClickListener.Instance?.GetCurrentCard();
        if (card == null) return;

        bool shouldShow = card.effects.Any(e => e.targetType == CardTargetType.Self && !e.requiresTarget)
                          && Input.mousePosition.y <= Screen.height * 0.2f;

        if (shouldShow != isVisible)
            SetHover(shouldShow);
    }

    public void SetHover(bool state)
    {
        if (hoverCanvas == null) return;
        if (isVisible == state) return;

        isVisible = state;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTo(state ? 1f : 0f));
    }


    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = hoverCanvas.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            hoverCanvas.alpha = newAlpha;

            // 중간에도 raycast 여부를 적절히 조절
            hoverCanvas.blocksRaycasts = newAlpha > 0.5f;
            hoverCanvas.interactable = newAlpha > 0.5f;

            yield return null;
        }

        hoverCanvas.alpha = targetAlpha;
        hoverCanvas.blocksRaycasts = targetAlpha > 0.5f;
        hoverCanvas.interactable = targetAlpha > 0.5f;
    }
}
