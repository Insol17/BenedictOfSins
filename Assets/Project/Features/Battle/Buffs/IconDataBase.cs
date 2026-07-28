using UnityEngine;

public static class IconDatabase
{
    private static IconRegistry registry;

    public static Sprite GetIcon(string iconName)
    {
        if (registry == null)
            registry = Resources.Load<IconRegistry>("IconRegistry");

        if (registry == null)
        {
            Debug.LogError("[IconDatabase] IconRegistry를 찾을 수 없습니다. 'Tools > Build Icon Registry'를 먼저 실행하세요.");
            return null;
        }

        var sprite = registry.GetIcon(iconName);
        if (sprite == null)
            Debug.LogWarning($"[IconDatabase] 아이콘 '{iconName}'을 찾을 수 없습니다.");

        return sprite;
    }
}
