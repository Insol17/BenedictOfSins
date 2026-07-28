using UnityEngine;
using System.Text;

/// <summary>
/// 카드 설명 텍스트에서 {키워드}를 효과 수치로 치환하는 유틸리티
/// </summary>
public static class CardTextReplacer
{
    public static string ReplacePlaceholders(CardData card, PlayerStats player, CardEffectPlaceholderLibrary library)
    {
        if (card == null || library == null) return "";

        string description = card.description;

        foreach (var effect in card.effects)
        {
            foreach (var placeholder in library.placeholders)
            {
                if (description.Contains(placeholder.keyword) && effect.effectType == placeholder.effectType)
                {
                    string value = placeholder.GetValue(player, effect);
                    description = description.Replace(placeholder.keyword, value);
                }
            }
        }

        // 사용자 지정 키워드도 처리
        foreach (var e in card.effects)
        {
            string dynamicKey = "{" + e.effectName + "}";
            if (description.Contains(dynamicKey))
            {
                description = description.Replace(dynamicKey, e.stack.ToString());
            }
        }

        description = description.Replace("{Cost}", card.GetCostSummary());
        return description;
    }
}
