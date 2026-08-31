using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    [Header("설정")]
    public int rows = 15;
    public int columns = 7;

    [Header("맵 프리셋")]
    public MapScenePreset scenePreset;

    private int nodeIdCounter = 0;
    private List<List<MapNode>> layers = new();
    private Dictionary<NodeType, List<string>> usedScenes = new();

    public List<MapNode> Generate()
    {
        if (MapManager.Instance.currentChapter == null)
            Debug.LogWarning("[MapGenerator] currentChapter가 설정되지 않았습니다.");

        return GenerateTreeFromStartToBoss();
    }

    private List<MapNode> GenerateTreeFromStartToBoss()
    {
        layers.Clear();
        nodeIdCounter = 0;
        usedScenes.Clear();
        HashSet<MapNode> usedNodes = new();

        // ===== 1. 시작 라인: 3~4개 노드 =====
        int startCount = Random.Range(3, 5);
        List<MapNode> currentLayer = new();
        List<int> usedX = new();

        while (currentLayer.Count < startCount)
        {
            int x = Random.Range(0, columns);
            if (usedX.Contains(x)) continue;

            NodeType type = GetNodeType(0);
            var startNode = new MapNode
            {
                id = nodeIdCounter++,
                x = x,
                y = 0,
                connectedNodes = new(),
                connectedNodeIds = new(),
                type = type,
                sceneName = GetUniqueSceneName(type),
                visited = false,
                isUnlocked = true
            };
            currentLayer.Add(startNode);
            usedX.Add(x);
            usedNodes.Add(startNode);
        }

        layers.Add(currentLayer);

        // ===== 2. 위쪽 확장 =====
        for (int y = 1; y < rows - 1; y++)
        {
            List<MapNode> newLayer = new();

            foreach (var fromNode in currentLayer)
            {
                int connectionCount = Random.Range(1, 3);
                for (int i = 0; i < connectionCount; i++)
                {
                    int xOffset = Random.Range(-1, 2);
                    int newX = Mathf.Clamp(fromNode.x + xOffset, 0, columns - 1);

                    NodeType nodeType = (y == rows - 2) ? NodeType.Rest : GetNodeType(y);

                    var newNode = new MapNode
                    {
                        id = nodeIdCounter++,
                        x = newX,
                        y = y,
                        connectedNodes = new(),
                        connectedNodeIds = new(),
                        type = nodeType,
                        sceneName = GetUniqueSceneName(nodeType),
                        visited = false
                    };

                    var existing = newLayer.FirstOrDefault(n => n.x == newNode.x && n.y == newNode.y);
                    if (existing == null)
                    {
                        newLayer.Add(newNode);
                        usedNodes.Add(newNode);
                        existing = newNode;
                    }

                    fromNode.connectedNodes.Add(existing);
                    existing.connectedNodes.Add(fromNode);
                }
            }

            currentLayer = newLayer;
            layers.Add(currentLayer);
        }

        // ===== 3. 보스 노드 추가 =====
        var bossNode = new MapNode
        {
            id = nodeIdCounter++,
            x = columns / 2,
            y = rows - 1,
            connectedNodes = new(),
            connectedNodeIds = new(),
            type = NodeType.Boss,
            sceneName = GetUniqueSceneName(NodeType.Boss),
            visited = false
        };
        usedNodes.Add(bossNode);

        foreach (var fromNode in currentLayer)
        {
            bossNode.connectedNodes.Add(fromNode);
            fromNode.connectedNodes.Add(bossNode);
        }

        layers.Add(new List<MapNode> { bossNode });

        // ===== 4. connectedNodeIds 설정 =====
        foreach (var node in usedNodes)
        {
            node.connectedNodeIds = node.connectedNodes.Select(n => n.id).ToList();
        }

        // ===== 5. 일반 전투 비율 보정 =====
        int battleCount = usedNodes.Count(n => n.type == NodeType.Battle);
        int totalCount = usedNodes.Count(n => n.type != NodeType.Boss);
        if ((float)battleCount / totalCount < 0.5f)
        {
            var candidates = usedNodes.Where(n =>
                n.type != NodeType.Boss && n.type != NodeType.Rest && n.type != NodeType.Battle
            ).ToList();

            int toConvert = Mathf.CeilToInt(totalCount * 0.5f) - battleCount;
            for (int i = 0; i < toConvert && i < candidates.Count; i++)
            {
                candidates[i].type = NodeType.Battle;
                candidates[i].sceneName = GetUniqueSceneName(NodeType.Battle);
            }
        }

        return usedNodes.ToList();
    }

    private NodeType GetNodeType(int y)
    {
        float r = Random.value;

        if (y < 3)
        {
            return (r < 0.7f) ? NodeType.Battle : NodeType.Rest;
        }

        if (y < 6)
        {
            if (r < 0.5f) return NodeType.Battle;
            if (r < 0.7f) return NodeType.Rest;
            if (r < 0.85f) return NodeType.Event;
            return NodeType.Shop;
        }

        if (r < 0.45f) return NodeType.Battle;
        if (r < 0.65f) return NodeType.Rest;
        if (r < 0.8f) return NodeType.Event;
        if (r < 0.93f) return NodeType.Shop;
        return NodeType.Elite;
    }

    private string GetUniqueSceneName(NodeType type)
    {
        var preset = MapManager.Instance.currentChapter?.mapScenePreset;
        if (preset == null) return "BattleScene";

        var entry = preset.sceneList.Find(e => e.nodeType == type);
        if (entry == null || entry.sceneNames.Count == 0) return "BattleScene";

        // Shop, Rest, Boss는 항상 재사용
        if (type == NodeType.Shop || type == NodeType.Rest || type == NodeType.Boss)
            return entry.sceneNames[0];

        if (!usedScenes.ContainsKey(type))
            usedScenes[type] = new List<string>();

        var available = entry.sceneNames.Except(usedScenes[type]).ToList();
        if (available.Count == 0)
        {
            Debug.LogWarning($"[MapGenerator] {type} 타입의 씬이 모두 소진됨. 무작위 재사용");
            return entry.sceneNames[Random.Range(0, entry.sceneNames.Count)];
        }

        var selected = available[Random.Range(0, available.Count)];
        usedScenes[type].Add(selected);
        return selected;
    }

    public void RegisterUsedScenes(List<MapNode> nodes)
    {
        usedScenes.Clear();  // 안전하게 초기화

        foreach (var node in nodes)
        {
            if (node == null || string.IsNullOrEmpty(node.sceneName)) continue;

            NodeType type = node.type;
            if (type == NodeType.Shop || type == NodeType.Rest || type == NodeType.Boss)
                continue; // 재사용 가능 씬은 무시

            if (!usedScenes.ContainsKey(type))
                usedScenes[type] = new List<string>();

            if (!usedScenes[type].Contains(node.sceneName))
                usedScenes[type].Add(node.sceneName);
        }
    }

}
