using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    public GameObject blackOverlay;         // 검은 배경 이미지 오브젝트
    public GameObject loadingPanel;         // 메인 UI 패널

    private CanvasGroup blackGroup;
    private CanvasGroup loadingGroup;

    public float fadeDuration = 1f;
    private Coroutine currentFade;

    void Start()
    {
        SetupCanvasGroup(ref blackGroup, blackOverlay);
        SetupCanvasGroup(ref loadingGroup, loadingPanel);

        blackOverlay.SetActive(false);
        loadingPanel.SetActive(false);

        blackGroup.alpha = 0f;
        loadingGroup.alpha = 0f;
    }

    void SetupCanvasGroup(ref CanvasGroup group, GameObject obj)
    {
        group = obj.GetComponent<CanvasGroup>();
        if (group == null)
            group = obj.AddComponent<CanvasGroup>();

        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void ShowPanel()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeInSequence());
    }

    public void HidePanel()
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeOutSequence());
    }

    private IEnumerator FadeInSequence()
    {
        blackOverlay.SetActive(true);
        loadingPanel.SetActive(true);

        blackGroup.alpha = 0f;
        loadingGroup.alpha = 0f;

        // Step 1: 검은 배경 페이드인
        yield return StartCoroutine(SmoothFade(blackGroup, 1f));

        yield return new WaitForSeconds(0.1f);

        // Step 2: UI 패널 페이드인
        yield return StartCoroutine(SmoothFade(loadingGroup, 1f));

        //  패널 페이드인 후 인터랙션 허용
        loadingGroup.interactable = true;
        loadingGroup.blocksRaycasts = true;
    }


    private IEnumerator FadeOutSequence()
    {
        //  인터랙션 먼저 차단
        loadingGroup.interactable = false;
        loadingGroup.blocksRaycasts = false;

        yield return StartCoroutine(SmoothFade(loadingGroup, 0f));
        loadingPanel.SetActive(false);

        yield return new WaitForSeconds(0.05f);

        yield return StartCoroutine(SmoothFade(blackGroup, 0f));
        blackOverlay.SetActive(false);
    }


    private IEnumerator SmoothFade(CanvasGroup group, float targetAlpha)
    {
        float time = 0f;
        float startAlpha = group.alpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            // 자연스러운 이징 (Ease In-Out)
            float smoothT = t * t * (3f - 2f * t);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothT);

            yield return null;
        }

        group.alpha = targetAlpha;
    }
}
