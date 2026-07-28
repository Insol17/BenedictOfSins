using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Icon Registry")]
public class IconRegistry : ScriptableObject
{
    public List<Entry> icons = new();

    [System.Serializable]
    public class Entry
    {
        public string name;   // ¿¹: "Bleed", "Rage"
        public Sprite icon;
    }

    private Dictionary<string, Sprite> iconDict;

    public Sprite GetIcon(string name)
    {
        if (iconDict == null)
        {
            iconDict = new Dictionary<string, Sprite>();
            foreach (var entry in icons)
            {
                var key = entry.name.ToLower();
                if (!iconDict.ContainsKey(key))
                    iconDict[key] = entry.icon;
            }
        }

        iconDict.TryGetValue(name.ToLower(), out var result);
        return result;
    }
}
