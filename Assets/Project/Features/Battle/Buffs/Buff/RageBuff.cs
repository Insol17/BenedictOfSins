using UnityEngine;

[Effect("Rage", "분노")]
[EffectDescription("- 스택당 주는 피해 +5%\n- 최대 20스택\n- 매 턴 4 스택 감소\n- 11스택 이상일 경우 오버히트 상태 진입")]
public class RageBuff : IBuff
{
    public string Type => "Rage";
    public int Stack => stack;
    public Sprite Icon => IconDatabase.GetIcon("Rage");

    private int stack;
    private const int MaxStack = 20;
    private const int RageDecayPerTurn = 4;

    private IUnitStats owner;

    // 기본 생성자 (아이콘 로딩용)
    public RageBuff() { }

    public RageBuff(int stack, IUnitStats owner)
    {
        this.stack = Mathf.Clamp(stack, 0, MaxStack);
        this.owner = owner;
    }

    public void Apply(IUnitStats owner)
    {
        this.owner = owner;
    }

    public void AddStack(int value)
    {
        stack = Mathf.Clamp(stack + value, 0, MaxStack);
    }

    public void Tick()
    {
        stack = Mathf.Max(0, stack - RageDecayPerTurn);
        if (owner != null)
            owner.SetBuffStack("Rage", stack);

        // 오버히트 진입/해제 조건
        if (stack >= 11 && !owner.HasBuff("Overheat"))
        {
            owner.AddOrUpdateBuff("Overheat", 1);
        }
        else if (stack <= 10 && owner.HasBuff("Overheat"))
        {
            owner.RemoveBuff("Overheat");
            owner.AddOrUpdateDebuff("OverheatEcho", 1);
        }
    }

    public bool IsExpired() => stack <= 0;

    public float GetDamageMultiplier() => 1f + (0.05f * stack);
    public float GetIncomingDamageMultiplier() => 1f;
    public float GetShieldMultiplier() => 1f;
    public int GetCostModifier() => 0;
    public int ModifyEnergyGain(int baseGain) => baseGain;

    public void OnTurnStart(IUnitStats unit) { }
    public void OnTurnEnd(IUnitStats unit) { }
    public void OnPlayerTurnStart() { }
    public void OnPlayerTurnEnd() { }
    public void OnEnemyTurnStart() { }
    public void OnEnemyTurnEnd() { }
    public void OnCardUsed(IUnitStats unit, CardData card) { }
    public void OnDealDamage(IUnitStats attacker, IUnitStats target, int amount) { }
    public void OnTakeDamage(IUnitStats unit, int amount, bool isDot) { }
}
