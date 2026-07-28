public enum CardEffectType
{
    Damage,
    Buff,
    Debuff,
    Heal,
    Draw,
    Discard,
    Shield,
    Exile,
    CostRecovery,

    DelayedDamage,   // 일정 턴 뒤 피해
    Conditional,     // 일반 조건부 발동
    RageConditional  // Rage 조건부 발동
}


public enum CardTargetType
{
    Self,
    Enemy,
    AllEnemy
}

public enum BuffType
{
    None,
    Strength,     // 주는 피해 10% 증가 (스택당)
    Bleed,        // 출혈: 매 턴 1 감소, 카드 쓸 때 데미지
    Vulnerable,   // 방어력 감소
    Weak,    // 공격력 감소
    Guard,     // 방어력 증가
    EnergyBoost,
    DrawBoost,
    Shield,
    Defense,

    Rage,
    Overheat,
    OverheatEcho
}

