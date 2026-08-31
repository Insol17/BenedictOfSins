using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 인게임에서 저장 트리거를 관리하는 컴포넌트입니다.
/// - 씬 시작 시 자동 저장
/// - 적 처치 / 휴식 직후 저장
/// </summary>
public class InGameSaveManager : MonoBehaviour
{
    private void Start()
    {
        // 씬 진입 시 자동 저장
        SaveIfSlotIsValid("[InGameSaveManager] 씬 진입 시 자동 저장");
    }

    /// <summary>
    /// 적 전멸 직후 호출 (외부에서 수동 호출)
    /// </summary>
    public void SaveAfterEnemyDefeated()
    {
        SaveIfSlotIsValid("[InGameSaveManager] 적 처치 후 자동 저장");
    }

    /// <summary>
    /// 휴식 노드 완료 후 호출 (외부에서 수동 호출)
    /// </summary>
    public void SaveAfterRest()
    {
        SaveIfSlotIsValid("[InGameSaveManager] 휴식 후 자동 저장");
    }

    /// <summary>
    /// 유효한 슬롯이 선택되어 있으면 저장 실행
    /// </summary>
    private void SaveIfSlotIsValid(string context)
    {
        int index = SaveManager.Instance.CurrentSlotIndex;
        if (index < 0)
        {
            Debug.LogWarning("[InGameSaveManager] 현재 슬롯이 유효하지 않아 저장하지 않음");
            return;
        }

        SaveManager.Instance.SaveGame(index);
        Debug.Log(context + $" (슬롯: {index})");
    }
}
