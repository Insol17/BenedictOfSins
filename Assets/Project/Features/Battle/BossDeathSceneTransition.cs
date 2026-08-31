using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossDeathSceneTransition : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private EnemyStats target; // 비워두면 자동 할당

    [Header("Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Timing")]
    [SerializeField] private float delayBefore = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float delayAfter = 0.1f;

    [Header("Options")]
    [SerializeField] private bool triggerOnDisable = true;
    [SerializeField] private Color fadeColor = Color.black;

    private bool _triggered;

    private void Awake()
    {
        if (target == null) target = GetComponent<EnemyStats>();
        if (string.IsNullOrEmpty(nextSceneName))
            Debug.LogWarning("[BossDeathSceneTransition] nextSceneName 비어있음");
    }

    private void Update()
    {
        if (_triggered || target == null) return;
        if (target.CurrentHP <= 0) StartTransition();
    }

    private void OnDisable()
    {
        if (!_triggered && triggerOnDisable && target != null && target.CurrentHP <= 0)
            StartTransition();
    }

    private void StartTransition()
    {
        if (_triggered) return;
        _triggered = true;
        Runner.Run(Co_Transition());
    }

    private IEnumerator Co_Transition()
    {
        if (delayBefore > 0f) yield return new WaitForSeconds(delayBefore);

        var overlay = CreateOverlay(out Image img, fadeColor);
        DontDestroyOnLoad(overlay);

        yield return Co_FadeImage(img, 0f, 1f, fadeOutDuration);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            var op = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;
        }

        var canvas = overlay.GetComponent<Canvas>();
        canvas.sortingOrder = short.MaxValue;

        yield return Co_FadeImage(img, 1f, 0f, fadeInDuration);

        if (delayAfter > 0f) yield return new WaitForSeconds(delayAfter);

        SetAlpha(img, 0f);
        Object.Destroy(overlay);
    }

    // ========== ? 외부 호출용 유틸 ==========

    public static void RunSceneTransition(
        string nextSceneName,
        float delayBefore = 0.25f,
        float fadeOutDuration = 0.6f,
        float fadeInDuration = 0.6f,
        float delayAfter = 0.1f,
        Color? fadeColor = null)
    {
        Runner.Run(Co_TransitionRoutine());

        IEnumerator Co_TransitionRoutine()
        {
            if (delayBefore > 0f) yield return new WaitForSeconds(delayBefore);

            var overlay = CreateOverlay(out Image img, fadeColor ?? Color.black);
            DontDestroyOnLoad(overlay);

            yield return Co_FadeImage(img, 0f, 1f, fadeOutDuration);

            var op = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            while (!op.isDone) yield return null;

            yield return Co_FadeImage(img, 1f, 0f, fadeInDuration);

            if (delayAfter > 0f) yield return new WaitForSeconds(delayAfter);

            SetAlpha(img, 0f);
            Object.Destroy(overlay);
        }
    }

    // ========== ? Overlay 생성 ==========

    private static GameObject CreateOverlay(out Image img)
    {
        return CreateOverlay(out img, Color.black); // 기본값
    }

    private static GameObject CreateOverlay(out Image img, Color color)
    {
        var root = new GameObject("[SceneTransitionOverlay]");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        root.AddComponent<GraphicRaycaster>();

        var go = new GameObject("Background");
        go.transform.SetParent(root.transform, false);
        img = go.AddComponent<Image>();
        img.color = new Color(color.r, color.g, color.b, 0f); // 시작은 투명

        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return root;
    }

    // ========== ? 페이드 처리 ==========

    private static IEnumerator Co_FadeImage(Image img, float from, float to, float duration)
    {
        if (duration <= 0f) { SetAlpha(img, to); yield break; }

        float t = 0f;
        SetAlpha(img, from);

        float hardTimeout = Mathf.Max(1.5f, duration * 3f);
        float elapsed = 0f;

        while (t < duration && elapsed < hardTimeout)
        {
            float dt = Time.unscaledDeltaTime;
            t += dt; elapsed += dt;

            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            SetAlpha(img, a);
            yield return null;
        }

        SetAlpha(img, to);
    }

    private static void SetAlpha(Image img, float a)
    {
        var c = img.color;
        c.a = Mathf.Clamp01(a);
        img.color = c;
    }

    // ========== ? 비활성 오브젝트에서도 실행 가능한 글로벌 러너 ==========

    private class Runner : MonoBehaviour
    {
        private static Runner _instance;
        public static void Run(IEnumerator routine)
        {
            if (_instance == null)
            {
                var go = new GameObject("[SceneTransitionRunner]");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<Runner>();
            }

            _instance.StartCoroutine(routine);
        }
    }
}
