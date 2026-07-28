using UnityEngine;

public class ChapterButtonHandler : MonoBehaviour
{
    [Header("연결할 챕터 프리셋")]
    public ChapterPreset targetChapter;

    public void OnChapterSelect()
    {
        if (targetChapter == null)
        {
            Debug.LogError("[ChapterButtonHandler] targetChapter가 비어 있습니다.");
            return;
        }

        Debug.Log($"[ChapterButtonHandler] 챕터 선택됨: {targetChapter.chapterName}");

        // ? SaveManager에 챕터 설정 및 초기 상태 반영
        SaveManager.Instance.OnChapterSelect(targetChapter);

        // ? MapManager에 챕터 전달 및 맵 생성
        MapManager.Instance.SetChapter(targetChapter);
        MapManager.Instance.GenerateMap();
    }
}
