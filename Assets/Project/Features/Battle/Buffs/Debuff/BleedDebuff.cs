using UnityEngine;

[Effect("Bleed", "출혈")]
[EffectDescription("- 매 턴 시작 시 스택만큼 피해를 받음\n- 카드를 사용할 때마다 스택당 1 피해 추가")]
public class BleedDebuff : IDebuff
{
    public string Type => "Bleed";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Bleed");

    private int stack;
    private IUnitStats owner;

    public BleedDebuff(int initialStack)
    {
        stack = initialStack;
    }

    public BleedDebuff() { }

    public BleedDebuff(int stack, IUnitStats owner)
    {
        this.stack = stack;
        this.owner = owner;
    }


    public void Apply(IUnitStats owner)
    {
        this.owner = owner;
    }

    public void AddStack(int value)
    {
        stack += value;
    }

    public void Tick()
    {
        if (owner != null && stack > 0)
        {
            owner.TakeDamage(stack, isDot: true);
            stack--;
        }
    }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => 1f;

    public float GetIncomingDamageMultiplier(bool isDot) => 1f;

    public float GetShieldMultiplier() => 1f;

    public int GetCostModifier() => 0;

    public void OnTurnStart(IUnitStats unit) { }

    public void OnTurnEnd(IUnitStats unit) { }

    public void OnPlayerTurnStart() { }

    public void OnPlayerTurnEnd() { }

    public void OnEnemyTurnStart() { }

    public void OnEnemyTurnEnd() { }

    public void OnCardUsed(IUnitStats unit, CardData card)
    {
        if (unit == owner && stack > 0)
        {
            owner.TakeDamage(stack, isDot: true);
        }
    }

    public void OnDealDamage(IUnitStats attacker, IUnitStats target, int amount) { }

    public void OnTakeDamage(IUnitStats unit, int amount, bool isDot) { }

    public int ModifyEnergyGain(int value) => value;
}
