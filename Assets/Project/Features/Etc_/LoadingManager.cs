using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    public static string sceneToLoad;

    [Header("Random Loading Tip")]
    public TextMeshProUGUI tipText;
    [TextArea]
    public string[] loadingTips;

    private float realProgress = 0f;
    private float visualProgress = 0f;
    private bool fakeProgressDone = false;


    private void Awake()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("LoadingManager: sceneToLoad가 설정되지 않았습니다!");
        }
    }

    void Start()
    {
        // 화면 초기 상태
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;

        StartCoroutine(LoadSceneWithFade());

        // 로딩 팁 출력
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            int index = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[index];
        }
    }

    private IEnumerator LoadSceneWithFade()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Fade(1f, 0f));

        if (progressText != null)
            progressText.text = "잠시만 기다려주세요...";

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        float timer = 0f;
        float fakeDuration = 3f;

        // 실제 로딩이 90% 이상이면 바로 진행
        if (op.progress >= 0.3f)
        {
            visualProgress = 0.9f;
            UpdateUI(visualProgress);
        }
        else
        {
            while (timer < fakeDuration)
            {
                timer += Time.deltaTime;

                // 실제 로딩이 이미 많이 됐다면 조기 종료
                if (op.progress >= 0.3f)
                    break;

                visualProgress = Mathf.Lerp(0f, 0.9f, timer / fakeDuration);
                UpdateUI(visualProgress);
                yield return null;
            }
        }

        fakeProgressDone = true;

        while (!op.isDone)
        {
            realProgress = Mathf.Clamp01(op.progress / 0.9f);

            if (fakeProgressDone && realProgress >= 0.9f)
            {
                visualProgress = 1f;
                UpdateUI(visualProgress);

                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(Fade(0f, 1f));

                op.allowSceneActivation = true;
            }
            else if (realProgress > visualProgress)
            {
                visualProgress = Mathf.MoveTowards(visualProgress, realProgress, Time.deltaTime);
                UpdateUI(visualProgress);
            }

            yield return null;
        }
    }


    private void UpdateUI(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = to;
    }

    public void OnClickLoadFromPauseMenu(int slotIndex)
    {
        SaveData data = SaveManager.Instance.LoadFromSlot(slotIndex);
        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogWarning($"[PauseMenu] 슬롯 {slotIndex}에서 유효한 데이터를 찾지 못했습니다.");
            return;
        }

        // 로딩 매니저용 씬 지정
        LoadingManager.sceneToLoad = data.sceneName;

        // 로딩 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("0. Loading Scene");
    }

}
