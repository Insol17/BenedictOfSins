using UnityEngine;

[Effect("JudgementTarget", "심판 대상")]
[EffectDescription("심판 공격의 대상이 됩니다.\n다음 턴에 사라집니다.")]
public class JudgementTargetDebuff : IDebuff
{
    public string Type => "JudgementTarget";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("JudgementTarget");

    private int stack;
    private IUnitStats owner;

    public JudgementTargetDebuff(int stack, IUnitStats owner)
    {
        this.stack = 1; // 항상 1로 고정
        this.owner = owner;
    }

    public JudgementTargetDebuff() { }

    public void Apply(IUnitStats owner) => this.owner = owner;

    public void AddStack(int amount)
    {
        stack = 1; // 항상 1스택 유지
    }

    public void Tick()
    {
        stack--; // 다음 턴에 사라짐
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
