using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[System.Serializable]
public class BackgroundInfo
{
    public string backgroundName;
    public GameObject backgroundObject;
}

public class LineSystem : MonoBehaviour
{
    [Header("캐릭터 리스트")]
    public List<GameObject> characters;

    [Header("배경 리스트")]
    public List<BackgroundInfo> backgrounds;

    public DialogueContainer dialogueContainer;
    public DialogueScript dialogueScript;

    private TextArchitect architect;
    private int currentIndex = 0;

    [Header("텍스트 빌드 옵션")]
    public TextArchitect.BuildMethod buildMethod = TextArchitect.BuildMethod.typewriter;
    public float speed = 2f;

    [Header("페이드 관련")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Post Processing")]
    public Volume postProcessVolume;
    private FilmGrain filmGrain;

    private GameObject currentBackground;

    // 이름 입력 관련
    private bool waitingForNameInput = false;
    private bool isNameInputDone = false;
    public bool waitForNameInputOnEnd = false;

    public event System.Action OnDialogueEnd;
    public bool changeSceneAfterScriptEnd = false;
    public string nextSceneName = "";

    [SerializeField] private AudioSource sfxAudioSource;

    private void Start()
    {
        architect = new TextArchitect(dialogueContainer.dialogueText)
        {
            buildMethod = buildMethod,
            speed = speed
        };

        StartDialogue();

        if (postProcessVolume != null && postProcessVolume.profile.TryGet<FilmGrain>(out var fg))
        {
            filmGrain = fg;
            filmGrain.active = true;
            filmGrain.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("FilmGrain 효과를 찾지 못했습니다. Volume과 프로필 설정을 확인하세요.");
        }
    }

    public void StartDialogue()
    {
        currentIndex = 0;
        ShowLine();
    }

    // 중복 제거된 ShowLine() - 이걸로 사용하세요.
    private bool isLinePlaying = false;

    public void ShowLine()
    {
        if (isLinePlaying || waitingForNameInput) return;
        isLinePlaying = true;

        // ?? 이 조건이 대사 종료 시점이므로, 여기에 OnDialogueEnd 호출을 추가해야 함
        if (currentIndex >= dialogueScript.lines.Count)
        {
            isLinePlaying = false;

            // ? 추가된 부분 시작
            if (waitForNameInputOnEnd && !isNameInputDone)
            {
                OnDialogueEnd?.Invoke();  // 이름 입력창을 띄우는 트리거
            }
            else if (dialogueScript != null && !string.IsNullOrEmpty(dialogueScript.nextSceneName))
            {
                LoadingManager.sceneToLoad = dialogueScript.nextSceneName;
                SceneManager.LoadScene("0. Loading Scene");
            }
            else
            {
                Debug.Log("대사 종료됨");
            }
            // ? 추가된 부분 끝

            return;
        }

        // ↓ 기존 코드 유지
        architect.hurryUp = false;
        DialogueLine line = dialogueScript.lines[currentIndex];

        string playerName = PlayerPrefs.GetString("PlayerName", "플레이어");
        dialogueContainer.nameText.text = line.speakerName.Replace("{PlayerName}", playerName);
        string dialogue = line.dialogueText.Replace("{PlayerName}", playerName);

        architect.Build(dialogue);

        StartCoroutine(ExecuteActionsAndWait(line.actions));

        currentIndex++;
    }


    private IEnumerator ExecuteActionsAndWait(List<DialogueAction> actions)
    {
        if (actions != null && actions.Count > 0)
            yield return StartCoroutine(ExecuteActions(actions));

        // 텍스트 출력이 완료될 때까지 대기
        while (architect.isBuilding)
            yield return null;

        isLinePlaying = false;
    }


    private void Update()
    {
        if (architect.speed != speed)
        {
            architect.speed = speed;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isLinePlaying)
            {
                if (architect.isBuilding)
                {
                    architect.ForceComplete();
                }
                return; // 대사가 아직 처리 중이므로 건너뜀
            }

            ShowLine();
        }


        // A 키 눌렀을 때 텍스트 추가 (디버그용 또는 기능 확장 시 사용)
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentIndex < dialogueScript.lines.Count)
            {
                DialogueLine line = dialogueScript.lines[currentIndex];
                architect.Append(line.dialogueText);
                currentIndex++;
            }
        }
    }


    public void SetWaitingForNameInput(bool waiting)
    {
        waitingForNameInput = waiting;

        if (!waiting)
            isNameInputDone = true;
    }

    public void SetDialogueScript(DialogueScript newScript, bool waitForNameInput = false)
    {
        dialogueScript = newScript;
        currentIndex = 0;
        waitForNameInputOnEnd = waitForNameInput;
        StartDialogue();
    }


    private IEnumerator ExecuteActions(List<DialogueAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action.actionType)
            {
                case DialogueAction.ActionType.MoveCharacter:
                    var character = FindCharacter(action.targetName);
                    if (character != null)
                        StartCoroutine(MoveOverTime(character.transform, action.targetPosition, action.duration));
                    break;

                case DialogueAction.ActionType.ShowCharacter:
                    var showTarget = FindCharacter(action.targetName);
                    if (showTarget != null) showTarget.SetActive(true);
                    break;

                case DialogueAction.ActionType.HideCharacter:
                    var hideTarget = FindCharacter(action.targetName);
                    if (hideTarget != null) hideTarget.SetActive(false);
                    break;

                case DialogueAction.ActionType.ChangeBackground:
                    ChangeBackground(action.targetName);
                    break;

                case DialogueAction.ActionType.CameraShake:
                    StartCoroutine(ShakeCamera(action.duration));
                    break;

                case DialogueAction.ActionType.CameraZoom:
                    StartCoroutine(ZoomCamera(action.targetPosition.z, action.duration));
                    break;

                case DialogueAction.ActionType.SetCharacterGrayScaleUI:
                    SetCharacterGrayScaleUI(action.targetName, true);
                    break;

                case DialogueAction.ActionType.SetCharacterNormalColor:
                    SetCharacterGrayScaleUI(action.targetName, false);
                    break;

                case DialogueAction.ActionType.SetFilmGrainIntensity:
                    SetFilmGrainIntensity(action.intensity);
                    break;

                case DialogueAction.ActionType.LoadScene:
                    if (!string.IsNullOrEmpty(action.sceneName))
                    {
                        Debug.Log("씬 전환 액션 실행: " + action.sceneName);
                        LoadingManager.sceneToLoad = action.sceneName;
                        SceneManager.LoadScene("0. Loading Scene");
                        // 씬 전환 후 실행할 작업이 있으면 이 코루틴 중단할 수도 있음
                        yield break;
                    }
                    break;

                case DialogueAction.ActionType.PlaySound:
                    if (action.soundClip != null && sfxAudioSource != null)
                    {
                        sfxAudioSource.PlayOneShot(action.soundClip);
                    }
                    else
                    {
                        Debug.LogWarning("효과음 클립 또는 AudioSource가 없습니다.");
                    }
                    break;

            }
        }
        yield return null;
    }

    private GameObject FindCharacter(string targetName)
    {
        return characters.Find(c => c.name == targetName);
    }

    private void ChangeBackground(string backgroundName)
    {
        var bg = backgrounds.Find(b => b.backgroundName == backgroundName);
        if (bg != null)
            StartCoroutine(FadeOutInBackgroundWithImage(bg.backgroundObject));
    }

    private IEnumerator FadeOutInBackgroundWithImage(GameObject newBackground)
    {
        yield return StartCoroutine(FadeImage(0f, 1f, fadeDuration / 2f));

        foreach (var bg in backgrounds)
        {
            if (bg.backgroundObject != newBackground)
                bg.backgroundObject.SetActive(false);
        }

        if (!newBackground.activeSelf)
            newBackground.SetActive(true);

        currentBackground = newBackground;

        yield return StartCoroutine(FadeImage(1f, 0f, fadeDuration / 2f));
    }

    private IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }

    public void SetCharacterGrayScaleUI(string characterName, bool grayScale)
    {
        var character = FindCharacter(characterName);
        if (character != null)
        {
            var img = character.GetComponent<Image>();
            if (img != null)
                img.color = grayScale ? Color.gray : Color.white;
        }
    }

    private IEnumerator MoveOverTime(Transform target, Vector3 destination, float duration)
    {
        float elapsed = 0f;

        if (target is RectTransform rectTransform)
        {
            Vector2 start = rectTransform.anchoredPosition;
            Vector2 end = new Vector2(destination.x, destination.y);
            while (elapsed < duration)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(start, end, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            rectTransform.anchoredPosition = end;
        }
        else
        {
            Vector3 start = target.position;
            while (elapsed < duration)
            {
                target.position = Vector3.Lerp(start, destination, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            target.position = destination;
        }
    }

    private IEnumerator ShakeCamera(float duration, float magnitude = 0.1f)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    private IEnumerator ZoomCamera(float targetZoom, float duration)
    {
        Camera cam = Camera.main;
        float startZoom = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetZoom;
    }

    public void SetFilmGrainIntensity(float intensity)
    {
        if (filmGrain != null)
            filmGrain.intensity.value = Mathf.Clamp01(intensity);


    }

    public void LoadScriptByName(string scriptName)
    {
        TextAsset scriptAsset = Resources.Load<TextAsset>($"Scripts/{scriptName}");

        if (scriptAsset != null)
        {
            string[] lines = scriptAsset.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            DialogueScript newScript = ScriptableObject.CreateInstance<DialogueScript>();
            newScript.lines = new List<DialogueLine>();

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    DialogueLine dialogueLine = new DialogueLine();
                    dialogueLine.speakerName = parts[0].Trim();
                    dialogueLine.dialogueText = parts[1].Trim();
                    newScript.lines.Add(dialogueLine);
                }
            }

            SetDialogueScript(newScript);  // 기존 시스템에 맞게 세팅
        }
        else
        {
            Debug.LogError($"스크립트 {scriptName} 을(를) Resources/Scripts/에서 찾을 수 없습니다.");
        }
    }

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SkipCurrentDialogue()
    {
        // 현재 텍스트 출력 중이면 즉시 완료
        if (architect.isBuilding)
        {
            architect.ForceComplete();
        }

        // 대사를 끝까지 넘겨버림
        currentIndex = dialogueScript.lines.Count;

        // 대사 종료 처리 (이벤트 호출 or 씬전환 등)
        if (waitForNameInputOnEnd && !isNameInputDone)
        {
            OnDialogueEnd?.Invoke();
        }
        else if (dialogueScript != null && !string.IsNullOrEmpty(dialogueScript.nextSceneName))
        {
            LoadingManager.sceneToLoad = dialogueScript.nextSceneName;
            SceneManager.LoadScene("0. Loading Scene");
        }
        else
        {
            Debug.Log("대사 스킵 완료 - 대사 종료 상태로 이동");
        }
    }

    public void SkipAll()
    {
        // 1. 출력 중이면 즉시 완료
        if (architect.isBuilding)
        {
            architect.ForceComplete();
        }

        // 2. 남은 대사 전부 처리
        while (currentIndex < dialogueScript.lines.Count)
        {
            var line = dialogueScript.lines[currentIndex];

            // 플레이어 이름 치환
            string playerName = PlayerPrefs.GetString("PlayerName", "플레이어");
            dialogueContainer.nameText.text = line.speakerName.Replace("{PlayerName}", playerName);
            dialogueContainer.dialogueText.text = line.dialogueText.Replace("{PlayerName}", playerName);

            // 액션 즉시 실행 (코루틴 없이)
            if (line.actions != null && line.actions.Count > 0)
            {
                foreach (var action in line.actions)
                {
                    ExecuteActionInstant(action);
                }
            }

            currentIndex++;
        }

        // 3. 스킵 끝났으니 대사 끝 처리
        if (waitForNameInputOnEnd && !isNameInputDone)
        {
            OnDialogueEnd?.Invoke();
        }
        else if (dialogueScript != null && !string.IsNullOrEmpty(dialogueScript.nextSceneName))
        {
            LoadingManager.sceneToLoad = dialogueScript.nextSceneName;
            SceneManager.LoadScene("0. Loading Scene");
        }
        else
        {
            Debug.Log("전체 스크립트 스킵 완료 - 마지막 상태로 적용됨");
        }
    }

    private void ExecuteActionInstant(DialogueAction action)
    {
        switch (action.actionType)
        {

            case DialogueAction.ActionType.ShowCharacter:
                var showTarget = FindCharacter(action.targetName);
                if (showTarget != null)
                {
                    showTarget.SetActive(true); // 반드시 활성화 처리
                }
                break;

            case DialogueAction.ActionType.HideCharacter:
                var hideTarget = FindCharacter(action.targetName);
                if (hideTarget != null)
                {
                    hideTarget.SetActive(false);
                }
                break;  

            case DialogueAction.ActionType.ChangeBackground:
                ChangeBackgroundInstant(action.targetName);
                break;

            case DialogueAction.ActionType.CameraShake:
                // 즉시 카메라 흔들기는 무시하거나 적절히 처리
                break;

            case DialogueAction.ActionType.CameraZoom:
                Camera.main.orthographicSize = action.targetPosition.z; // 즉시 줌 적용
                break;

            case DialogueAction.ActionType.SetCharacterGrayScaleUI:
                SetCharacterGrayScaleUI(action.targetName, true);
                break;

            case DialogueAction.ActionType.SetCharacterNormalColor:
                SetCharacterGrayScaleUI(action.targetName, false);
                break;

            case DialogueAction.ActionType.SetFilmGrainIntensity:
                SetFilmGrainIntensity(action.intensity);
                break;

            case DialogueAction.ActionType.LoadScene:
                if (!string.IsNullOrEmpty(action.sceneName))
                {
                    LoadingManager.sceneToLoad = action.sceneName;
                    SceneManager.LoadScene("0. Loading Scene");
                }
                break;

            case DialogueAction.ActionType.PlaySound:
                if (action.soundClip != null && sfxAudioSource != null)
                {
                    sfxAudioSource.PlayOneShot(action.soundClip);
                }
                break;
        }
    }

    private void ChangeBackgroundInstant(string backgroundName)
    {
        foreach (var bg in backgrounds)
        {
            bg.backgroundObject.SetActive(false);
        }
        var bgToShow = backgrounds.Find(b => b.backgroundName == backgroundName);
        if (bgToShow != null)
        {
            bgToShow.backgroundObject.SetActive(true);
            currentBackground = bgToShow.backgroundObject;
        }
    }

}
