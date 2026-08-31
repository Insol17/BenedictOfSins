using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffIconUIController : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform iconParent;

    private Dictionary<string, BuffIconUI> activeIcons = new();

    private Sprite GetIconSprite(string type)
    {
        var sprite = IconDatabase.GetIcon(type);
        if (sprite == null)
            Debug.LogWarning($"[BuffIconUIController] 아이콘 없음: {type}");
        return sprite;
    }

    public void UpdateIcons(IEnumerable<IBuff> buffs, IEnumerable<IDebuff> debuffs)
    {
        var allEffects = new Dictionary<string, int>();

        foreach (var b in buffs)
            allEffects[b.Type] = b.Stack;

        foreach (var d in debuffs)
            allEffects[d.Type] = d.Stack;

        foreach (var key in activeIcons.Keys.ToList())
        {
            if (!allEffects.ContainsKey(key))
            {
                Destroy(activeIcons[key].gameObject);
                activeIcons.Remove(key);
            }
        }

        foreach (var pair in allEffects)
        {
            var type = pair.Key;
            var stack = pair.Value;

            if (!activeIcons.ContainsKey(type))
            {
                GameObject go = Instantiate(iconPrefab, iconParent);
                BuffIconUI iconUI = go.GetComponent<BuffIconUI>();
                Sprite sprite = GetIconSprite(type);
                iconUI.SetData(sprite, stack);
                iconUI.SetType(type);
                activeIcons[type] = iconUI;
            }
            else
            {
                activeIcons[type].SetData(
                    activeIcons[type].iconImage.sprite,
                    stack
                );
            }
        }

        ReorderIcons();
    }

    private void ReorderIcons()
    {
        float spacing = 180f;
        int i = 0;
        foreach (var icon in activeIcons.Values)
        {
            icon.transform.localPosition = new Vector3(i * spacing, 30, 0);
            i++;
        }
    }
}
