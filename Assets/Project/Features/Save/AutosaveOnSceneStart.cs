using UnityEngine;

public class AutoSaveOnSceneStart : MonoBehaviour
{
    private void Start()
    {
        int pendingNodeId = PlayerPrefs.GetInt("PendingNodeId", -1);
        if (pendingNodeId != -1)
        {
            int slotIndex = PlayerPrefs.GetInt("CurrentSlotIndex", -1);
            if (slotIndex != -1)
            {
                Debug.Log($"[AutoSave] 슬롯 {slotIndex} 입장 후 자동 저장 수행");

                SaveManager.Instance.SaveGame(slotIndex);

                // ? 저장 후 플래그 제거
                PlayerPrefs.DeleteKey("PendingNodeId");
                PlayerPrefs.DeleteKey("CurrentSlotIndex"); // 이거도 같이 제거
            }
        }
    }
}
