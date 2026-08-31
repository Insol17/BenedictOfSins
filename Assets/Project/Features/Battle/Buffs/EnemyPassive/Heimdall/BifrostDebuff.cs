using UnityEngine;

[Effect("Bifrost", "바이프로스트")]
[EffectDescription("헤임달의 특수한 힘으로 부여된 디버프입니다.")]
public class BifrostDebuff : IDebuff
{
    public string Type => "Bifrost";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Bifrost");
    private const int MaxStack = 10;
    private int stack;
    private IUnitStats owner;

    public BifrostDebuff(int stack, IUnitStats owner)
    {
        this.stack = Mathf.Min(stack, MaxStack);
        this.owner = owner;
    }

    public BifrostDebuff() { }

    public void Apply(IUnitStats owner) => this.owner = owner;

    public void AddStack(int amount)
    {
        stack += amount;
    }

    public void Tick()
    {
        stack--;
    }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => 1f;
    public float GetIncomingDamageMultiplier(bool isDot = false) => 1f;
    public float GetShieldMultiplier() => 1f;
    public int GetCostModifier() => 0;

    public void OnTurnStart(IUnitStats owner) { }
    public void OnTurnEnd(IUnitStats owner) { }
    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }
    public void OnCardUsed(IUnitStats owner, CardData card) { }
    public void OnDealDamage(IUnitStats owner, IUnitStats target, int damage) { }
    public void OnTakeDamage(IUnitStats owner, int damage, bool isDot) { }

    public int ModifyEnergyGain(int baseGain) => baseGain;
}
