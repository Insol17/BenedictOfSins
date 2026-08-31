using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapStartButtonHandler : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;           // 맵 생성 전용
    [SerializeField] private MapUIController mapUIController; // 맵 UI 컨트롤러
    [SerializeField] private int slotIndex = 0;               // 저장 슬롯

    public void OnClickGenerateAndLoadMap()
    {
        // 1. 맵 생성
        mapManager.GenerateMap();

        // 2. 현재 슬롯 저장
        PlayerPrefs.SetInt("CurrentSlotIndex", slotIndex);
        PlayerPrefs.Save();

        // 3. 맵 UI를 통해 보여주기 (애니메이션 포함)
        mapUIController.ShowMap(); // ← 이제는 MapUIController가 담당
    }
}
