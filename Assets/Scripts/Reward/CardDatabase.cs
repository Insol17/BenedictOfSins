using System.Collections.Generic;
using UnityEngine;

public static class CardDatabase
{
    public static List<CardData> GetRandomCards(int count)
    {
        var preset = SaveManager.Instance?.currentChapter;
        if (preset == null)
        {
            Debug.LogWarning("ChapterPreset이 없습니다.");
            return new List<CardData>();
        }

        return preset.GetRandomCards(count); // 중복 제거된 로직 사용됨
    }
}
