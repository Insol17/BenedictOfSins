using UnityEngine;

[Effect("Weak", "약화")]
[EffectDescription("- 주는 피해 -10% per stack\n- 최대 5스택")]
public class WeakDebuff : IDebuff
{
    public string Type => "Weak";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Weak");

    private int stack;
    private IUnitStats owner;
    private const int MaxStack = 5;

    public WeakDebuff(int initialStack)
    {
        stack = Mathf.Min(initialStack, MaxStack);
    }

    public WeakDebuff() { }

    public WeakDebuff(int stack, IUnitStats owner)
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
        stack = Mathf.Min(stack + value, MaxStack);
    }

    public void Tick()
    {
        stack--;
    }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => Mathf.Max(0f, 1f - 0.10f * stack);

    public float GetIncomingDamageMultiplier(bool isDot) => 1f;

    public float GetShieldMultiplier() => 1f;

    public int GetCostModifier() => 0;

    public void OnTurnStart(IUnitStats unit) { }

    public void OnTurnEnd(IUnitStats unit) { }

    public void OnPlayerTurnStart() { }

    public void OnPlayerTurnEnd() { }

    public void OnEnemyTurnStart() { }

    public void OnEnemyTurnEnd() { }

    public void OnCardUsed(IUnitStats unit, CardData card) { }

    public void OnDealDamage(IUnitStats attacker, IUnitStats target, int amount) { }

    public void OnTakeDamage(IUnitStats unit, int amount, bool isDot) { }

    public int ModifyEnergyGain(int value) => value;
}
