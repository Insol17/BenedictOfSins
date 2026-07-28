#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class IconRegistryBuilder
{
    [MenuItem("Tools/Build Icon Registry")]
    public static void BuildIconRegistry()
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite");
        var entries = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => Path.GetFileNameWithoutExtension(path).ToLower().Contains("icon")) // Icon이라는 이름 포함
            .Select(path => new IconRegistry.Entry
            {
                name = Path.GetFileNameWithoutExtension(path).Replace("Icon", ""), // "IconBleed" → "Bleed"
                icon = AssetDatabase.LoadAssetAtPath<Sprite>(path)
            })
            .ToList();

        var registry = ScriptableObject.CreateInstance<IconRegistry>();
        registry.icons = entries;

        const string folderPath = "Assets/Resources";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string savePath = $"{folderPath}/IconRegistry.asset";
        AssetDatabase.CreateAsset(registry, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[IconRegistryBuilder] {entries.Count}개의 아이콘이 등록되었습니다.");
    }
}
#endif
