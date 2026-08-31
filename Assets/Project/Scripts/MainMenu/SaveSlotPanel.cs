using UnityEngine;

public class SaveSlotPanel : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "Library";

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // 세이브 시스템 붙기 전까지는 슬롯 구분 없이 바로 씬 전환
    public void OnClickSlot(int slotIndex)
    {
        SceneLoader.LoadScene(gameplaySceneName);
    }
}
