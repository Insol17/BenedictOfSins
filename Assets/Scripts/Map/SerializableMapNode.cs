using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableMapNode
{
    public int id;
    public int x;
    public int y;
    public NodeType type;
    public List<int> connectedNodeIds;
    public bool visited;
    public string sceneName;
    public bool isUnlocked;
    public bool isCleared;
}
