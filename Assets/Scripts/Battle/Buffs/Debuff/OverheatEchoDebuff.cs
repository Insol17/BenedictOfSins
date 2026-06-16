using UnityEngine;

[Effect("OverheatEcho", "오버히트 잔향")]
[EffectDescription("- 받는 피해 +50%\n- 다음 턴 시작 시 사라짐\n- 턴 당 에너지 +1")]
public class OverheatEchoDebuff : IDebuff
{
    public string Type => "OverheatEcho";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("OverheatEcho");

    private int stack;
    private IUnitStats owner;

    public OverheatEchoDebuff(int initialStack)
    {
        stack = initialStack;
    }

    public OverheatEchoDebuff() { }

    public OverheatEchoDebuff(int stack, IUnitStats owner)
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
        stack = 0;
    }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => 1f;

    public float GetIncomingDamageMultiplier(bool isDot) => 1.5f;

    public float GetShieldMultiplier() => 1f;

    public int GetCostModifier() => -1; // 에너지 +1

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
