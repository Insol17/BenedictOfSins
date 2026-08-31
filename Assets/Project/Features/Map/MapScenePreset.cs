using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Map/ScenePreset")]
public class MapScenePreset : ScriptableObject
{
    public int chapter;
    public int act;
    public string chapterId;
    public Sprite backgroundSprite;

    public List<SceneEntry> sceneList;

    private Dictionary<NodeType, List<string>> usedScenes = new();

    [System.Serializable]
    public class SceneEntry
    {
        public NodeType nodeType;
        public List<string> sceneNames;
    }

    /// <summary>
    /// 씬 사용 기록을 초기화 (맵 생성 시 호출)
    /// </summary>
    public void ResetUsedScenes()
    {
        usedScenes.Clear();
    }

    public string GetSceneNameForNode(NodeType type)
    {
        var entry = sceneList.Find(e => e.nodeType == type);
        if (entry == null || entry.sceneNames.Count == 0)
            return "BattleScene"; // 기본값

        // 반복 허용되는 노드
        if (type == NodeType.Rest || type == NodeType.Shop)
        {
            return entry.sceneNames[Random.Range(0, entry.sceneNames.Count)];
        }

        // 중복 방지 로직
        if (!usedScenes.ContainsKey(type))
            usedScenes[type] = new List<string>();

        var availableScenes = entry.sceneNames.Except(usedScenes[type]).ToList();
        if (availableScenes.Count == 0)
        {
            Debug.LogWarning($"[MapScenePreset] {type} 타입의 모든 씬이 이미 사용되었습니다. 중복 허용");
            availableScenes = entry.sceneNames;
            usedScenes[type].Clear(); // 초기화하고 재사용
        }

        string selected = availableScenes[Random.Range(0, availableScenes.Count)];
        usedScenes[type].Add(selected);
        return selected;
    }
}
