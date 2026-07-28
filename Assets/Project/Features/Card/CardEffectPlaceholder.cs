using UnityEngine;

/// <summary>
/// 카드 설명에서 {Damage}, {Shield} 등 키워드를 수치로 치환하기 위한 SO 정의
/// </summary>
[CreateAssetMenu(fileName = "NewCardEffectPlaceholder", menuName = "CardSystem/Effect Placeholder")]
public class CardEffectPlaceholder : ScriptableObject
{
    public string keyword; // 예: {Damage}
    public CardEffectType effectType;

    /// <summary>
    /// 실제 값을 계산해 문자열로 반환하는 메서드
    /// </summary>
    public string GetValue(PlayerStats player, CardEffectData effect)
    {
        int value = 0;

        switch (effectType)
        {
            case CardEffectType.Damage:
                value = DamageCalculator.CalculateOutgoingDamage(player, effect.stack);
                break;

            case CardEffectType.Shield:
                value = DamageCalculator.GetModifiedShield(player, effect.stack);
                break;

            case CardEffectType.Heal:
                value = DamageCalculator.CalculateOutgoingDamage(player, effect.stack);
                break;

            default:
                value = effect.stack;
                break;
        }

        return value.ToString();
    }
}
