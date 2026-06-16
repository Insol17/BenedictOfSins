using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuCanvas;

    public GameObject saveSlotPanel;
    public GameObject loadSlotPanel;
    public GameObject settingPanel;

    public Button[] includeButtons; // 비활성화할 버튼들 (Resume, Quit 등만 인스펙터에서 연결)

    public SaveSlotUI[] slotUIs;
    public PauseMenuAnimator pauseMenuAnimator; // 인스펙터에서 연결

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 세부 패널이 열려 있으면 해당 패널만 닫기
            if (saveSlotPanel.activeSelf)
            {
                CloseSavePanel();
                return;
            }
            else if (loadSlotPanel.activeSelf)
            {
                CloseLoadPanel();
                return;
            }
            else if (settingPanel.activeSelf)
            {
                CloseSettingPanel();
                return;
            }

            // 애니메이션 중이면 무시
            if (pauseMenuAnimator.isAnimating) return;

            // 일반 PauseMenu 토글
            if (GameIsPaused) Resume();
            else Pause();
        }
    }

    private void Start()
    {
        slotUIs = FindObjectsOfType<SaveSlotUI>();
        System.Array.Sort(slotUIs, (a, b) => a.slotIndex.CompareTo(b.slotIndex)); // 인덱스 순 정렬
    }

    public void Resume()
    {
        ResetSelectedButton(); // ← 추가
        pauseMenuAnimator.Hide();
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        pauseMenuAnimator.Show();
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    // ===================== Save/Load UI =====================
    public void OpenSavePanel()
    {
        ResetSelectedButton();
        FadePanel(saveSlotPanel, new List<GameObject> { loadSlotPanel, settingPanel });
        SetButtonsInteractable(false);
    }

    public void OpenLoadPanel()
    {
        ResetSelectedButton();
        FadePanel(loadSlotPanel, new List<GameObject> { saveSlotPanel, settingPanel });
        SetButtonsInteractable(false);
    }


    public void CloseSavePanel()
    {
        FadePanel(new GameObject(), new List<GameObject> { saveSlotPanel });
        SetButtonsInteractable(true);
    }   

    public void CloseLoadPanel()
    {
        FadePanel(new GameObject(), new List<GameObject> { loadSlotPanel });
        SetButtonsInteractable(true);
    }

    public void CloseSettingPanel()
    {
        FadePanel(new GameObject(), new List<GameObject> { settingPanel });
        SetButtonsInteractable(true);
    }

    public void CloseSlotPanels()
    {
        saveSlotPanel.SetActive(false);
        loadSlotPanel.SetActive(false);
        settingPanel.SetActive(false);
        SetButtonsInteractable(true);
    }

    private void Save(int slot)
    {
        Debug.Log($"[PauseMenu] 슬롯 {slot} 저장 시도");

        if (MapManager.Instance != null)
        {
            List<MapNode> mapData = MapManager.Instance.GetCurrentMapData();

            if (mapData != null && mapData.Count > 0)
            {
                SaveManager.Instance.SaveMap(slot, mapData); // 1. 맵 저장
                Debug.Log($"[PauseMenu] 슬롯 {slot} - 맵 저장 완료 (노드 수: {mapData.Count})");
            }
            else
            {
                Debug.LogWarning($"[PauseMenu] 슬롯 {slot} - 저장할 맵 데이터가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[PauseMenu] MapManager.Instance가 null입니다. 맵 저장 실패");
        }

        SaveManager.Instance.SaveGame(slot); // 2. 일반 정보 저장
        Debug.Log($"[PauseMenu] 슬롯 {slot} - 일반 데이터 저장 완료");

        if (slot >= 0 && slot < slotUIs.Length && slotUIs[slot] != null)
            slotUIs[slot].Refresh();

        CloseSavePanel();
    }


    private void Load(int slot)
    {
        Debug.Log($"[PauseMenu] 슬롯 {slot} 로드 시도");

        SaveData data = SaveManager.Instance.LoadFromSlot(slot);
        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogWarning($"[PauseMenu] 슬롯 {slot}에 유효한 세이브 데이터가 없습니다.");
            return;
        }

        // SaveManager 내부 상태 갱신
        SaveManager.Instance.currentPlayerName = data.playerName;
        SaveManager.Instance.playerLevel = data.playerLevel;
        GoldManager.Instance.SetGold(data.gold);
        SaveManager.Instance.cutsceneWatched = data.cutsceneWatched;
        SaveManager.Instance.sceneName = data.sceneName;
        SaveManager.lastLoadedData = data;

        // 맵 데이터 로드
        List<MapNode> loadedMap = SaveManager.Instance.LoadMap(slot);
        if (loadedMap != null && loadedMap.Count > 0)
        {
            MapManager.LoadedMapData = loadedMap;
            Debug.Log($"[PauseMenu] 슬롯 {slot} - 맵 데이터 로드 완료 (노드 수: {loadedMap.Count})");
        }
        else
        {
            MapManager.LoadedMapData = null;
            Debug.LogWarning($"[PauseMenu] 슬롯 {slot} - 맵 데이터가 존재하지 않음");
        }

        Debug.Log($"[PauseMenu] 로드 완료 → 이름: {SaveManager.Instance.currentPlayerName}, 씬: {data.sceneName}");

        pauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        LoadingManager.sceneToLoad = data.sceneName;
        SceneManager.LoadScene("0. Loading Scene");
    }




    // 버튼 인터랙션 제어
    private void SetButtonsInteractable(bool state)
    {
        if (includeButtons == null) return;
        foreach (var btn in includeButtons)
        {
            if (btn != null)
                btn.interactable = state;
        }
    }

    public void SaveSlot0() => Save(0);
    public void SaveSlot1() => Save(1);
    public void SaveSlot2() => Save(2);

    public void LoadSlot0() => Load(0);
    public void LoadSlot1() => Load(1);
    public void LoadSlot2() => Load(2);

    public void ToSettingMenu()
    {
        ResetSelectedButton();
        FadePanel(settingPanel, new List<GameObject> { saveSlotPanel, loadSlotPanel });
        SetButtonsInteractable(false);
    }

    public void SceneChange()
    {
        SceneManager.LoadScene("0-2.Library");
    }

    public void OnQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResetSelectedButton()
    {
        if (EventSystem.current == null) return;

        var current = EventSystem.current.currentSelectedGameObject;
        if (current != null)
        {
            // 강제로 선택 해제
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private Coroutine currentFadeCoroutine;

    private void FadePanel(GameObject panelToOpen, List<GameObject> panelsToClose, float duration = 0.25f)
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeRoutine(panelToOpen, panelsToClose, duration));
    }

    private IEnumerator FadeRoutine(GameObject panelToOpen, List<GameObject> panelsToClose, float duration)
    {
        // 먼저 닫을 패널들 페이드아웃
        foreach (var panel in panelsToClose)
        {
            if (panel.activeSelf)
            {
                CanvasGroup cg = panel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    yield return StartCoroutine(FadeCanvasGroup(cg, 1, 0, duration));
                    panel.SetActive(false);
                }
            }
        }

        // 열 패널 켜기
        panelToOpen.SetActive(true);
        CanvasGroup openGroup = panelToOpen.GetComponent<CanvasGroup>();
        if (openGroup != null)
        {
            openGroup.alpha = 0;
            yield return StartCoroutine(FadeCanvasGroup(openGroup, 0, 1, duration));
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }


}
