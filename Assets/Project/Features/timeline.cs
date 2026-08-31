using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasGroupFaderAndSceneLoader : MonoBehaviour
{
    public CanvasGroup targetCanvasGroup;
    public Button gameStartButton;
    public Button sceneLoadButton;      // ✅ 새로운 버튼
    public string nextSceneName = "YourNextSceneName";

    void Start()
    {
        // 처음엔 UI와 버튼 모두 비활성화
        targetCanvasGroup.alpha = 0f;
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;

        // 버튼 리스너 연결
        gameStartButton.onClick.AddListener(OnGameStart);

        // 씬 이동 버튼은 처음에 비활성화
        sceneLoadButton.gameObject.SetActive(false);
        sceneLoadButton.onClick.AddListener(LoadNextScene);
    }

    void OnGameStart()
    {
        // UI 보이기
        targetCanvasGroup.alpha = 1f;
        targetCanvasGroup.interactable = true;
        targetCanvasGroup.blocksRaycasts = true;

        // 시작 버튼 숨기기
        gameStartButton.gameObject.SetActive(false);

        // 씬 이동용 버튼 표시
        sceneLoadButton.gameObject.SetActive(true);
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
