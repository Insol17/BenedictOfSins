using UnityEngine;

public class ProximityInteractUIWithFade : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup hintCanvasGroup; // Hint UI (CanvasGroup 사용)
    [SerializeField] private GameObject interactUI;       // F로 여는 UI

    [Header("설정")]
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private MonoBehaviour playerMovementScript; // 플레이어 이동 스크립트
    [SerializeField] private Rigidbody playerRigidbody;          // 플레이어 Rigidbody


    private bool isPlayerInside = false;
    private bool hasOpenedUI = false;

    private void Start()
    {
        SetHintVisible(false);
        if (interactUI != null) interactUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        if (Input.GetKeyDown(interactKey))
        {
            hasOpenedUI = !hasOpenedUI;

            // UI 토글
            if (interactUI != null)
                interactUI.SetActive(hasOpenedUI);

            // 이동 제어 토글
            if (playerMovementScript != null)
                playerMovementScript.enabled = !hasOpenedUI;

            if (playerRigidbody != null && hasOpenedUI)
                playerRigidbody.velocity = Vector3.zero; // UI 켤 때만 멈춤
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            StopAllCoroutines();
            StartCoroutine(FadeHintUI(true));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            hasOpenedUI = false;

            StopAllCoroutines();
            StartCoroutine(FadeHintUI(false));

            if (interactUI != null)
                interactUI.SetActive(false);

            if (playerMovementScript != null)
                playerMovementScript.enabled = true;
        }
    }


    private void SetHintVisible(bool visible)
    {
        if (hintCanvasGroup == null) return;

        hintCanvasGroup.alpha = visible ? 1f : 0f;
        hintCanvasGroup.interactable = visible;
        hintCanvasGroup.blocksRaycasts = visible;
    }

    private System.Collections.IEnumerator FadeHintUI(bool fadeIn)
    {
        if (hintCanvasGroup == null) yield break;

        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = hintCanvasGroup.alpha;

        while (!Mathf.Approximately(hintCanvasGroup.alpha, targetAlpha))
        {
            hintCanvasGroup.alpha = Mathf.MoveTowards(hintCanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }

        hintCanvasGroup.interactable = fadeIn;
        hintCanvasGroup.blocksRaycasts = fadeIn;
    }
}
