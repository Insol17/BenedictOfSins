using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;


public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;

    public TMP_Text playerNameText;
    public TMP_Text sceneNameText;
    public TMP_Text lastSaveTimeText;

    public Button loadButton;
    public Button saveButton; // New Game 버튼
    public Button deleteButton;
    public TMP_Text saveButtonText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_slot{slotIndex}.json");

        if (playerNameText == null || sceneNameText == null || lastSaveTimeText == null ||
            loadButton == null || saveButton == null || deleteButton == null)
        {
            Debug.LogWarning($"SaveSlotUI({slotIndex}): 필드 연결 누락!");
            return;
        }

        if (File.Exists(path))
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            string displaySceneName = GetSceneDisplayName(data.sceneName);

            playerNameText.text = data.playerName;
            sceneNameText.text = displaySceneName;
            lastSaveTimeText.text = data.saveTime;

            loadButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);
            saveButton.gameObject.SetActive(false);
        }
        else
        {
            playerNameText.text = "빈 슬롯";
            sceneNameText.text = "";
            lastSaveTimeText.text = "";

            loadButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(true);

            if (saveButtonText != null)
                saveButtonText.text = "New Game";
        }
    }


    public void OnClickDelete()
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_slot{slotIndex}.json");

        if (File.Exists(path))
        {
            File.Delete(path);

            // 수정된 부분
            SaveManager saveManager = FindObjectOfType<SaveManager>();
            if (saveManager != null)
            {
                saveManager.ClearCurrentData();  // 이제 정상 작동
            }

            Refresh();
            Debug.Log($"슬롯 {slotIndex} 삭제됨");
        }
        else
        {
            Debug.LogWarning($"슬롯 {slotIndex}에는 삭제할 세이브 파일이 없음");
        }
    }



    public void OnClickNewGame()
    {
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("SaveManager를 찾을 수 없습니다!");
            return;
        }

        // 이름 불러오기
        saveManager.currentPlayerName = PlayerPrefs.GetString("PlayerName", "계란");

        // 상태 초기화
        saveManager.ResetToNewGame(); // 위치나 변수 초기화 등

        // 현재 상태 기반으로 SaveData 생성
        SaveData newSave = saveManager.CreateCurrentSaveData("CutScene 0");

        // 저장
        saveManager.SaveToSlot(slotIndex, newSave);

        // 로딩
        LoadingManager.sceneToLoad = "CutScene 0";
        SceneManager.LoadScene("0. Loading Scene");
    }




    private string GetSceneDisplayName(string sceneName)
    {
        switch (sceneName)
        {
            case "0-2.Library 0floor": return "도서관 지하";
            case "0-2.Library": return "도서관 로비";
            case "0-2.Library 2floor": return "도서관 2층";
            case "1-1. BossRoom": return "보스방";
            case "CutScene 0": return "인트로";
            default: return sceneName;
        }
    }

    public void OnClickLoad()
    {
        Debug.Log($"[Load] 슬롯 {slotIndex} 로딩 시도");

        SaveManager.Instance.Load(slotIndex);

        string path = Path.Combine(Application.persistentDataPath, $"save_slot{slotIndex}.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Load] 슬롯 {slotIndex} 파일 없음");
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        if (string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogWarning($"[Load] 슬롯 {slotIndex}에 sceneName 없음");
            return;
        }

        Debug.Log($"[Load] 슬롯 {slotIndex} 씬: {data.sceneName}");

        LoadingManager.sceneToLoad = data.sceneName;
        SceneManager.LoadScene("0. Loading Scene");
    }



}



