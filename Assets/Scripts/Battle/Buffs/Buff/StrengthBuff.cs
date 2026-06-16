using UnityEngine;

[Effect("Strength", "힘")]
[EffectDescription("- 스택당 공격력 10% 증가")]
public class StrengthBuff : IBuff
{
    public string Type => "Strength";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Strength");

    private int stack;
    private const int MaxStack = 99;
    private IUnitStats owner;

    public StrengthBuff() { }

    public StrengthBuff(int stack)
    {
        this.stack = Mathf.Min(stack, MaxStack);
    }

    public StrengthBuff(int stack, IUnitStats owner)
    {
        this.stack = Mathf.Min(stack, MaxStack);
        this.owner = owner;
    }

    public void Apply(IUnitStats owner)
    {
        this.owner = owner;
    }

    public void AddStack(int value)
    {
        stack = Mathf.Min(stack + value, MaxStack);
    }

    public void Tick() { }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => 1f + 0.1f * stack;

    public float GetIncomingDamageMultiplier() => 1f;

    public float GetShieldMultiplier() => 1f;

    public int GetCostModifier() => 0;

    public int ModifyEnergyGain(int baseGain) => baseGain;

    public void OnTurnStart(IUnitStats owner) { }

    public void OnTurnEnd(IUnitStats owner) { }

    public void OnPlayerTurnStart() { }

    public void OnPlayerTurnEnd() { }

    public void OnEnemyTurnStart() { }

    public void OnEnemyTurnEnd() { }

    public void OnCardUsed(IUnitStats owner, CardData card) { }

    public void OnDealDamage(IUnitStats source, IUnitStats target, int amount) { }

    public void OnTakeDamage(IUnitStats owner, int amount, bool isFromPlayer) { }
}
