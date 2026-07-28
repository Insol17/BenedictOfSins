using System.Collections;
using UnityEngine;

public class FadeInOnSceneStart : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup; // 페이드 이미지가 붙은 캔버스
    [SerializeField] private float fadeDuration = 1.5f;   // 페이드 인 시간

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f; // 시작 시 화면 완전 검게
            StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}
