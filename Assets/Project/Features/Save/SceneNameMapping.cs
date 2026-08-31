using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Save Manager로 연동되는 씬 이름 변경(매핑) 전용 스크립터블 오브젝트 코드 (ex. 1. Midgard -> 미드가르드)
/// </summary>
[CreateAssetMenu(fileName = "SceneNameMapping", menuName = "Game/SceneNameMapping")]
public class SceneNameMapping : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string internalName;
        public string displayName;
    }

    public List<Entry> mappings;

    private Dictionary<string, string> nameDict;

    public void Init()
    {
        nameDict = new();
        foreach (var entry in mappings)
        {
            if (!string.IsNullOrEmpty(entry.internalName))
                nameDict[entry.internalName] = entry.displayName;
        }
    }

    public string GetDisplayName(string internalName)
    {
        if (nameDict == null) Init();
        return nameDict.TryGetValue(internalName, out var display) ? display : internalName;
    }
}
