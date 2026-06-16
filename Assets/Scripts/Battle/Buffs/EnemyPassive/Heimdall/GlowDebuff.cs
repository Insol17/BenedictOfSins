using UnityEngine;

[Effect("Glow", "광휘")]
[EffectDescription("공격 받을 때마다 스택 +1\n5스택 시 심판 대상 디버프 부여")]
public class GlowDebuff : IDebuff
{
    public string Type => "Glow";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Glow");

    private int stack;
    private IUnitStats owner;
    private const int MaxStack = 5;

    public GlowDebuff(int stack, IUnitStats owner)
    {
        this.stack = Mathf.Clamp(stack, 0, MaxStack);
        this.owner = owner;
    }

    public GlowDebuff() { }

    public void Apply(IUnitStats owner) => this.owner = owner;

    public void AddStack(int amount)
    {
        stack = Mathf.Min(stack + amount, MaxStack);

        if (stack >= 5 && !owner.HasDebuff("JudgementTarget"))
        {
            owner.AddOrUpdateDebuff("JudgementTarget", 1);
        }
    }

    public void Tick() { }

    public bool IsExpired() => false;

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

    public void OnTakeDamage(IUnitStats owner, int damage, bool isDot)
    {
        AddStack(1);
    }

    public int ModifyEnergyGain(int baseGain) => baseGain;
}
