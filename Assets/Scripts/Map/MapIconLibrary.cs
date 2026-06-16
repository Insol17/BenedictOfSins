using UnityEngine;

[CreateAssetMenu(menuName = "Map/IconLibrary")]
public class MapIconLibrary : ScriptableObject
{
    public Sprite battleIcon;
    public Sprite eliteIcon;
    public Sprite restIcon;
    public Sprite shopIcon;
    public Sprite eventIcon;
    public Sprite bossIcon;

    public Sprite GetIcon(NodeType type)
    {
        return type switch
        {
            NodeType.Battle => battleIcon,
            NodeType.Elite => eliteIcon,
            NodeType.Rest => restIcon,
            NodeType.Shop => shopIcon,
            NodeType.Event => eventIcon,
            NodeType.Boss => bossIcon,
            _ => null
        };
    }
}
