using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapGenerateButton : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private Button generateButton;
    [SerializeField] private float delayBeforeGenerate = 0.2f;
    [SerializeField] private int slotIndex = 0;

    private void Awake()
    {
        if (generateButton == null)
            generateButton = GetComponent<Button>();

        generateButton.onClick.AddListener(OnGenerateClicked);
    }

    private void OnGenerateClicked()
    {
        StartCoroutine(GenerateWithDelay());
    }

    private IEnumerator GenerateWithDelay()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("[MapGenerateButton] MapManager 인스턴스 없음");
            yield break;
        }

        Debug.Log("[MapGenerateButton] ClearMap 호출 시작");
        MapManager.Instance.ClearMap();
        Debug.Log("[MapGenerateButton] ClearMap 호출 완료");

        // ✅ 씬 사용 기록 초기화
        if (MapManager.Instance.currentChapter?.mapScenePreset != null)
        {
            MapManager.Instance.currentChapter.mapScenePreset.ResetUsedScenes();
            Debug.Log("[MapGenerateButton] 씬 사용 기록 초기화 완료");
        }

        // ✅ 저장된 맵 비우기 (새 맵 강제 생성 유도)
        MapManager.LoadedMapData = null;

        yield return new WaitForSeconds(delayBeforeGenerate);

        Debug.Log("[MapGenerateButton] GenerateMap 호출");
        MapManager.Instance.GenerateMap();

        Debug.Log("[MapGenerateButton] ShowMap 호출");
        MapManager.Instance.ShowMap();

        PlayerPrefs.SetInt("CurrentSlotIndex", slotIndex);
        PlayerPrefs.Save();
    }
}
