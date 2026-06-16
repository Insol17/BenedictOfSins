using UnityEngine;

public struct EnemyIntent
{
    public string skillName;
    public int value;
    public Sprite icon;
    public EnemyIntentType type;
    public string description;

    public EnemyStats attacker; // ?? 실시간 데미지 예측용

    public int GetPredictedDamage()
    {
        if (attacker == null) return value;
        return DamageCalculator.CalculateOutgoingDamage(attacker, value);
    }
}
