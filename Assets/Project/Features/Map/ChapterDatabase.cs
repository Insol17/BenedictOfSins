using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterDatabase", menuName = "Map/Chapter Database")]
public class ChapterDatabase : ScriptableObject
{
    public static ChapterDatabase Instance { get; private set; }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[ChapterDatabase] 중복 인스턴스가 감지되었습니다. 첫 번째 인스턴스가 유지됩니다.");
        }
    }

    public ChapterPreset GetChapterById(string id)
    {
        foreach (var entry in chapters)
        {
            if (entry.id == id)
                return entry.preset;
        }

        Debug.LogWarning($"[ChapterDatabase] ID '{id}'에 해당하는 챕터를 찾을 수 없습니다.");
        return null;
    }

    [System.Serializable]
    public class Entry
    {
        public string id;
        public ChapterPreset preset;
    }

    public List<Entry> chapters;
}
