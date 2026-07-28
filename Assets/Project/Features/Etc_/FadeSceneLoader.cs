using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeSceneLoader : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private Image fadeOverlay;             // 캔버스 위에 올려둔 전체 화면 이미지
    [SerializeField] private float fadeOutDuration = 0.6f;  // 검게 되는 시간
    [SerializeField] private float fadeInDuration = 0.6f;   // 다시 밝아지는 시간
    [SerializeField] private Color fadeColor = Color.black;

    private void Awake()
    {
        if (fadeOverlay != null)
        {
            SetAlpha(0f); // 시작 시 투명
        }
    }

    /// <summary>
    /// 버튼에서 호출될 메서드 (씬 이름을 인자로 전달)
    /// </summary>
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeOverlay == null)
        {
            Debug.LogError("[FadeSceneLoader] FadeOverlay가 연결되지 않았습니다.");
            yield break;
        }

        // 페이드 아웃 (밝음 → 어두움)
        yield return Fade(0f, 1f, fadeOutDuration);

        // 씬 전환
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // 페이드 인 (어두움 → 밝음)
        yield return Fade(1f, 0f, fadeInDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = Mathf.Clamp01(a);
            fadeOverlay.color = c;
        }
    }
}
