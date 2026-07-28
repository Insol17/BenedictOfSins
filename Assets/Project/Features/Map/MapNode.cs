using System.Collections.Generic;

public enum NodeType { Battle, Elite, Rest, Shop, Event, Boss }

public class MapNode
{
    public int id;
    public int x;
    public int y;
    public NodeType type;
    public string sceneName;
    public List<MapNode> connectedNodes = new();
    public List<int> connectedNodeIds = new(); // 이 필드는 저장용
    public bool visited;
    public bool isUnlocked;
    public bool isCleared;
}
