using UnityEngine;

[System.Serializable]
public class IntentTooltipEntry
{
    public EnemyIntentType type;
    public string title;
    [TextArea] public string description;
    public Sprite icon;
}
