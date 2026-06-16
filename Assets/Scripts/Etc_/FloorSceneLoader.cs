using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;  // Timeline용

public class PortalTriggerTimeline : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Hint UI")]
    [SerializeField] private CanvasGroup hintCanvasGroup;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Timeline")]
    [SerializeField] private PlayableDirector timeline;


    private bool playerInside = false;
    private bool hasInteracted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // 타임라인 재생 도중 나간 경우 Hint UI만 끄고 나머진 무시
            if (hasInteracted)
            {
                StopAllCoroutines();
                StartCoroutine(FadeOut());
                return;
            }

            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
    }

    private void Update()
    {
        if (playerInside && !hasInteracted && Input.GetKeyDown(interactKey))
        {
            hasInteracted = true;
            StopAllCoroutines();
            StartCoroutine(FadeOut()); // 힌트 UI 숨기기

            if (timeline != null)
            {
                timeline.stopped += OnTimelineFinished;
                timeline.Play();
            }
            else
            {
                Debug.LogWarning("타임라인이 설정되지 않았습니다. 즉시 씬 이동합니다.");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private void OnTimelineFinished(PlayableDirector dir)
    {
        timeline.stopped -= OnTimelineFinished;
        SceneManager.LoadScene(sceneToLoad);
    }

    private System.Collections.IEnumerator FadeIn()
    {
        hintCanvasGroup.interactable = true;
        hintCanvasGroup.blocksRaycasts = true;

        while (hintCanvasGroup.alpha < 1f)
        {
            hintCanvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        hintCanvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        while (hintCanvasGroup.alpha > 0f)
        {
            hintCanvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        hintCanvasGroup.alpha = 0f;

        // UI 비활성화 (원할 경우)
        hintCanvasGroup.interactable = false;
        hintCanvasGroup.blocksRaycasts = false;
    }
}
