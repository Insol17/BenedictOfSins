using UnityEngine;

[Effect("Vulnerable", "무방비")]
[EffectDescription("- 받는 피해 +25%\n- 방어 효과 -50%")]
public class VulnerableDebuff : IDebuff
{
    public string Type => "Vulnerable";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Vulnerable");

    private int stack;
    private IUnitStats owner;

    public VulnerableDebuff(int initialStack)
    {
        stack = initialStack;
    }

    public VulnerableDebuff() { }

    public VulnerableDebuff(int stack, IUnitStats owner)
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
        stack--;
    }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => 1f;

    public float GetIncomingDamageMultiplier(bool isDot) => 1f + 0.25f * stack;

    public float GetShieldMultiplier() => 0.5f;

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
