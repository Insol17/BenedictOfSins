using UnityEngine;

[System.Serializable]
public class CardEffectData
{
    public CardEffectType effectType;         // Damage, Heal, Buff, Debuff, DelayedDamage, RageConditional 등
    public string effectName;                 // Buff/Debuff일 경우 이름 ("Bleed", "Rage" 등)

    public int value;                         // Damage, Shield, Heal 등 수치
    public int stack;                         // Buff/Debuff용 스택

    public CardTargetType targetType;         // Player, Enemy, AllEnemy
    public bool requiresTarget;               // 직접 타겟팅 필요 여부

    public string condition;                  // 조건식 (예: "IfTargetHas:Glow>=5", "LowHP", "TurnStart")

    public bool isPassiveEffectCard;          // BurnCard처럼 핸드에 있을 때만 효과 존재

    // ----------------- [확장 필드] -----------------
    public int delayTurns;                    // DelayedDamage → 몇 턴 뒤 발동할지
    public int requiredRage;                  // RageConditional → 요구 분노 스택
    public CardEffectData subEffect;          // 조건 충족 시 실행될 하위 효과
}
