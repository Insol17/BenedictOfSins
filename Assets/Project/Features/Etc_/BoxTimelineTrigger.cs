using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 트리거에 진입 시 타임라인, 대화, 혹은 씬 전환을 실행할 수 있는 트리거 컴포넌트.
/// 자동 실행 또는 F 키로 수동 실행 가능.
/// </summary>
public class TimelineOrDialogueTrigger : MonoBehaviour
{
    [Header("연결 오브젝트")]
    public PlayableDirector timeline;        // 타임라인 재생기
    public GameObject visualNovelUI;         // 비주얼 노벨 UI
    public GameObject hintUI;                // 힌트 UI (F 키 안내 등)

    [Header("사용 모드")]
    public bool useTimeline = false;         // 타임라인 사용 여부
    public bool timelineAutoPlay = false;    // 타임라인 자동 실행 여부

    public bool useDialogue = false;         // 대화 사용 여부
    public bool dialogueAutoPlay = false;    // 대화 자동 실행 여부

    public bool useSceneChange = false;      // 씬 전환 여부
    public bool sceneChangeAuto = false;     // 씬 자동 전환 여부

    [Header("Dialogue 설정")]
    public string dialogueScriptName;        // 사용할 대화 스크립트 이름

    [Header("Scene 전환 설정")]
    public string sceneToLoad;               // 전환할 씬 이름

    private bool playerInTrigger = false;    // 플레이어가 트리거 안에 있는지 여부
    private bool hasPlayed = false;          // 이미 실행되었는지 여부 (중복 방지)

    private Coroutine fadeCoroutine;         // 힌트 UI 페이드 코루틴 핸들

    private void Update()
    {
        // 수동 조작만 여기에 둔다
        if (playerInTrigger && !hasPlayed && Input.GetKeyDown(KeyCode.F))
        {
            PlayAction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayed)
        {
            playerInTrigger = true;
            FadeHintUI(true); // 페이드 인

            // 자동 실행 기능
            if (useTimeline && timelineAutoPlay)
            {
                PlayTimeline();
                hasPlayed = true;
            }
            else if (useDialogue && dialogueAutoPlay)
            {
                PlayDialogue();
                hasPlayed = true;
            }
            else if (useSceneChange && sceneChangeAuto)
            {
                ChangeScene();
                hasPlayed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            FadeHintUI(false); // 페이드 아웃
        }
    }

    /// <summary>
    /// 수동 실행 트리거
    /// </summary>
    private void PlayAction()
    {
        if (hintUI != null)
            hintUI.SetActive(false);

        if (useTimeline && !timelineAutoPlay)
        {
            PlayTimeline();
        }
        else if (useDialogue && !dialogueAutoPlay)
        {
            PlayDialogue();
        }
        else if (useSceneChange && !sceneChangeAuto)
        {
            ChangeScene();
        }

        hasPlayed = true;
    }

    /// <summary>
    /// 타임라인 재생
    /// </summary>
    private void PlayTimeline()
    {
        if (timeline != null)
        {
            timeline.Play();
        }
    }

    /// <summary>
    /// 대화 시작
    /// </summary>
    private void PlayDialogue()
    {
        if (visualNovelUI != null)
        {
            visualNovelUI.SetActive(true);

            var lineSystem = visualNovelUI.GetComponent<LineSystem>();
            if (lineSystem != null && !string.IsNullOrEmpty(dialogueScriptName))
            {
                lineSystem.LoadScriptByName(dialogueScriptName);
            }
        }
    }

    /// <summary>
    /// 씬 전환
    /// </summary>
    private void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    /// <summary>
    /// 힌트 UI 페이드 인/아웃
    /// </summary>
    private void FadeHintUI(bool fadeIn, float duration = 0.5f)
    {
        if (hintUI == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvasGroup(hintUI.GetComponent<CanvasGroup>(), fadeIn, duration));
    }

    /// <summary>
    /// CanvasGroup을 점점 투명도 변경하여 페이드 처리
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
