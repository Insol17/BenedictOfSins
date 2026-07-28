using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 치환 키워드와 그 처리 방식 목록을 보관하는 컨테이너
/// </summary>
[CreateAssetMenu(fileName = "CardEffectPlaceholderLibrary", menuName = "CardSystem/Placeholder Library")]
public class CardEffectPlaceholderLibrary : ScriptableObject
{
    public List<CardEffectPlaceholder> placeholders = new();

    public CardEffectPlaceholder GetByKeyword(string keyword)
    {
        return placeholders.Find(p => p.keyword == keyword);
    }
}
