// 플레이어가 특정 오브젝트(HOKMA BOOK)에 접근하면 챕터 선택 UI를 열고,
// 플레이어 이동/애니메이션을 막은 뒤 카메라 전환과 챕터 선택, 확인, 씬 전환 등을 수행하는 스크립트입니다.
// 코드는 유지한 채 전체에 철저한 주석을 추가하고, UI 열고 닫을 때 애니메이션도 정지/해제되도록 보완되었습니다.



using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class GimmickText
{
    [TextArea]
    public string text;
}

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Hint UI")]
    [SerializeField] private CanvasGroup hintCanvasGroup;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("카메라 전환")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Transform uiCameraTransform;
    [SerializeField] private Volume uiCameraVolume;
    private LensDistortion lensDistortion;

    [Header("카메라 위치")]
    [SerializeField] private Vector3 chapterSelectCamPosition;
    [SerializeField] private Vector3 characterConfirmCamPosition;
    [SerializeField] private float cameraMoveSpeed = 5f;

    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text characterDescriptionText;
    [SerializeField] private TMP_Text chapterNameText;

    [SerializeField] private Image gimmickImage1;
    [SerializeField] private Image gimmickImage2;
    [SerializeField] private Image gimmickImage3;

    [SerializeField] private TMP_Text gimmickNameText1;
    [SerializeField] private TMP_Text gimmickNameText2;
    [SerializeField] private TMP_Text gimmickNameText3;

    [SerializeField] private TMP_Text gimmickText1;
    [SerializeField] private TMP_Text gimmickText2;
    [SerializeField] private TMP_Text gimmickText3;

    [SerializeField] private Image characterImage;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    [Header("챕터 데이터")]
    [SerializeField] private string[] characterNames;
    [SerializeField] private string[] hpValues;
    [SerializeField] private string[] descriptions;
    [SerializeField] private string[] chapterNames;
    [SerializeField] private string[] sceneNames;

    [Header("기믹 데이터")]
    [SerializeField] private Sprite[] gimmickImage1List;
    [SerializeField] private Sprite[] gimmickImage2List;
    [SerializeField] private Sprite[] gimmickImage3List;

    [SerializeField] private string[] gimmickName1List;
    [SerializeField] private string[] gimmickName2List;
    [SerializeField] private string[] gimmickName3List;

    [SerializeField] private GimmickText[] gimmickText1List;
    [SerializeField] private GimmickText[] gimmickText2List;
    [SerializeField] private GimmickText[] gimmickText3List;

    [Header("캐릭터 이미지 데이터")]
    [SerializeField] private Sprite[] characterImages;

    [Header("페이드아웃")]
    [SerializeField] private SpriteRenderer fadeSprite;

    [Header("플레이어 무브먼트 제한")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Button[] chapterButtons;
    [SerializeField] private Button[] pageButtons;

    [Header("Chapter 페이지 전환")]
    [SerializeField] private RectTransform chapterButtonsContainer;
    [SerializeField] private Vector2 firstPagePosition;
    [SerializeField] private Vector2 secondPagePosition;
    [SerializeField] private float pageSlideSpeed = 5f;

    [Header("조작할 버튼들")]
    [SerializeField] private Button[] includeButtons;  // 여기 넣은 버튼들만 interactable 조작

    private bool isOnSecondPage = false;
    private bool isSliding = false;
    private Vector2 targetPagePosition;
    private bool isNear = false;
    private bool isUIMode = false;
    private int selectedChapterIndex = -1;
    private Vector3 targetCameraPosition;
    private bool isMovingCamera = false;

    void Start()
    {
        uiCameraTransform.position = new Vector3(uiCameraTransform.position.x, 3300f, uiCameraTransform.position.z);
        if (uiCamera != null) uiCamera.enabled = false;
        SetButtonsInteractable(false);
        if (hintCanvasGroup != null) hintCanvasGroup.alpha = 0;
        if (mainCamera != null) mainCamera.enabled = true;

        if (uiCameraVolume != null && uiCameraVolume.profile.TryGet(out LensDistortion distortion))
        {
            lensDistortion = distortion;
            lensDistortion.intensity.value = 0f;
        }

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnBackToChapterSelect);
    }

    void Update()
    {
        UpdateCameraMovement();
        UpdatePageSlide();

        if (uiCamera != null && uiCamera.enabled)
            return; // UI 켜져 있으면 입력 막기

        if (isNear && Input.GetKeyDown(interactKey))
        {
            SoundManager.Instance?.PlayInteract();
            EnterChapterSelectMode();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 예: 새로 HintUI 찾기
        hintCanvasGroup = GameObject.Find("HintUI")?.GetComponent<CanvasGroup>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("HOKMA BOOK") || PauseMenu.GameIsPaused)
            return;

        if (!isNear)
        {
            isNear = true;
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("HOKMA BOOK")) return;

        isNear = false;
        StopAllCoroutines();
        StartCoroutine(FadeOut());

        if (!isUIMode)
            isUIMode = false;
    }

    private void EnterChapterSelectMode()
    {
        SoundManager.Instance?.FadeOutBGM();
        allowChapterInteraction = true;
        StartCoroutine(HandleCameraTransitionToUI());
    }

    private IEnumerator HandleCameraTransitionToUI()
    {
        if (playerRigidbody != null) playerRigidbody.velocity = Vector2.zero;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        yield return StartCoroutine(FadeSprite(1f, 1f));
        SetButtonsInteractable(true);

        if (mainCamera != null) mainCamera.enabled = false;
        if (uiCamera != null) uiCamera.enabled = true;

        yield return StartCoroutine(FadeSprite(0f, 0.5f));

        targetCameraPosition = chapterSelectCamPosition;
        isMovingCamera = true;
        SetButtonsInteractable(false);
    }

    private void UpdateCameraMovement()
    {
        if (!isMovingCamera || uiCameraTransform == null) return;

        uiCameraTransform.position = Vector3.Lerp(
            uiCameraTransform.position,
            targetCameraPosition,
            Time.unscaledDeltaTime * cameraMoveSpeed
        );

        if (Vector3.Distance(uiCameraTransform.position, targetCameraPosition) < 0.1f)
        {
            uiCameraTransform.position = targetCameraPosition;
            isMovingCamera = false;

            SetButtonsInteractable(true);

            if (allowChapterInteraction) //  이 조건으로 막음
            {
                SetChapterButtonsInteractable(true);
                SetPageButtonsInteractable(true);
            }

            confirmButton.interactable = true;
            backButton.interactable = true;
        }


    }



    private IEnumerator FadeIn()
    {
        while (hintCanvasGroup.alpha < 1)
        {
            hintCanvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        hintCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        while (hintCanvasGroup.alpha > 0)
        {
            hintCanvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        hintCanvasGroup.alpha = 0;
    }

    private IEnumerator FadeSprite(float targetAlpha, float duration)
    {
        if (fadeSprite == null) yield break;

        float startAlpha = fadeSprite.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            fadeSprite.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeSprite.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    public void OnChapterButtonClicked(int index)
    {
        if (index < 0 || index >= characterNames.Length) return;
        if (index == selectedChapterIndex && isMovingCamera) return; // 이미 같은 챕터 눌렀으면 무시

        SoundManager.Instance?.PlayChapterClick();

        selectedChapterIndex = index;

        // 챕터 관련 정보 갱신
        characterNameText.text = characterNames[index];
        hpText.text = hpValues[index];
        characterDescriptionText.text = descriptions[index];
        chapterNameText.text = chapterNames[index];

        if (index < characterImages.Length) characterImage.sprite = characterImages[index];
        if (index < gimmickImage1List.Length) gimmickImage1.sprite = gimmickImage1List[index];
        if (index < gimmickImage2List.Length) gimmickImage2.sprite = gimmickImage2List[index];
        if (index < gimmickImage3List.Length) gimmickImage3.sprite = gimmickImage3List[index];

        gimmickNameText1.text = gimmickName1List[index];
        gimmickNameText2.text = gimmickName2List[index];
        gimmickNameText3.text = gimmickName3List[index];

        gimmickText1.text = gimmickText1List[index].text;
        gimmickText2.text = gimmickText2List[index].text;
        gimmickText3.text = gimmickText3List[index].text;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            confirmButton.interactable = false;
            backButton.interactable = false;
            StartCoroutine(PlayEffectsThenLoadScene(sceneNames[selectedChapterIndex]));
        });

        //  무조건 카메라 이동
        MoveToCharacterConfirmView();
    }


    private void MoveToCharacterConfirmView()
    {
        targetCameraPosition = characterConfirmCamPosition;
        isMovingCamera = true;
        SetButtonsInteractable(false);
    }

    private bool allowChapterInteraction = true;

    private void OnBackToChapterSelect()
    {
        SoundManager.Instance?.PlayBackClick();

        allowChapterInteraction = false; //  이 플래그가 버튼 활성화 제어
        SetButtonsInteractable(false);
        SetChapterButtonsInteractable(false);
        SetPageButtonsInteractable(false);

        targetCameraPosition = chapterSelectCamPosition;
        isMovingCamera = true;
    }


    public void OnQuit()
    {
        allowChapterInteraction = false;
        StartCoroutine(HandleCameraTransitionToMain());
    }

    private IEnumerator HandleCameraTransitionToMain()
    {
        yield return StartCoroutine(FadeSprite(1f, 1f));
        SetButtonsInteractable(false);

        if (uiCamera != null) uiCamera.enabled = false;
        if (mainCamera != null) mainCamera.enabled = true;

        StartCoroutine(FadeIn());
        selectedChapterIndex = -1;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        isUIMode = false;

        yield return StartCoroutine(FadeSprite(0f, 0.5f));
        uiCameraTransform.position = new Vector3(uiCameraTransform.position.x, 3300f, uiCameraTransform.position.z);

        SoundManager.Instance?.FadeInBGM();
    }

    public void OnNextPageClicked()
    {
        if (isSliding) return;

        SoundManager.Instance?.PlayPageChange();
        isOnSecondPage = !isOnSecondPage;
        targetPagePosition = isOnSecondPage ? secondPagePosition : firstPagePosition;
        isSliding = true;
        SetPageButtonsInteractable(false);
    }

    private void UpdatePageSlide()
    {
        if (!isSliding || chapterButtonsContainer == null) return;

        chapterButtonsContainer.anchoredPosition = Vector2.Lerp(
            chapterButtonsContainer.anchoredPosition,
            targetPagePosition,
            Time.unscaledDeltaTime * pageSlideSpeed
        );

        if (Vector2.Distance(chapterButtonsContainer.anchoredPosition, targetPagePosition) < 0.1f)
        {
            chapterButtonsContainer.anchoredPosition = targetPagePosition;
            isSliding = false;
            SetPageButtonsInteractable(true);
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        if (includeButtons == null) return;

        foreach (var button in includeButtons)
        {
            if (button != null)
                button.interactable = state;
        }
    }

    private void SetPageButtonsInteractable(bool state)
    {
        foreach (var button in pageButtons)
        {
            if (button != null)
                button.interactable = state;
        }
    }

    private IEnumerator AnimateLensDistortion(float targetValue, float duration)
    {
        if (lensDistortion == null) yield break;

        float start = lensDistortion.intensity.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 30f);
            lensDistortion.intensity.value = Mathf.Lerp(start, targetValue, eased);
            yield return null;
        }

        lensDistortion.intensity.value = targetValue;
    }

    private IEnumerator AnimateCameraZoom(float startFOV, float endFOV, float duration)
    {
        if (uiCamera == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3);
            uiCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, eased);
            yield return null;
        }

        uiCamera.fieldOfView = endFOV;
    }

    private IEnumerator PlayEffectsThenLoadScene(string sceneName)
    {
        SoundManager.Instance?.PlayConfirmClick();
        StartCoroutine(AnimateLensDistortion(-0.6f, 2f));
        StartCoroutine(FadeSprite(1f, 2f));
        StartCoroutine(AnimateCameraZoom(60f, 35f, 0.3f));
        StartCoroutine(AnimateLensDistortion(-0.7f, 7f));

        yield return new WaitForSeconds(3f);
        LoadingManager.sceneToLoad = sceneName;
        SceneManager.LoadScene("0. Loading Scene");
    }

    private IEnumerator ReenableConfirmButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        confirmButton.interactable = true;
    }

    private void SetChapterButtonsInteractable(bool state)
    {
        foreach (var button in chapterButtons)
        {
            if (button != null)
                button.interactable = state;
        }
    }

}
