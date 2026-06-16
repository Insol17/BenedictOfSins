// EnemyAIBase.cs
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAIBase : MonoBehaviour, IEnemyAI
{
    [Header("Refs")]
    public EnemyStats self;
    public PlayerStats player;

    [Header("Intent UI")]
    [SerializeField] protected EnemyIntentUI intentUI;
    [SerializeField] protected Sprite defaultIcon;

    [Header("스킬 이름별 아이콘 매핑")]
    [SerializeField] private List<SkillIconEntry> skillIcons = new();
    protected Dictionary<string, Sprite> skillIconMap = new();

    protected EnemyIntent nextIntent;

    public EnemyStats Self => self;
    public PlayerStats Player => player;

    protected virtual void Awake()
    {
        foreach (var e in skillIcons)
            skillIconMap[e.skillName] = e.icon;
    }

    protected virtual void Start()
    {
        DecideNextAction();
    }

    public virtual void OnTurnStart()
    {
        ApplyTurnStartPassive();
        PerformTurn(); // 인터페이스 메서드
    }

    public virtual void OnTurnEnd() { }

    public void PerformAction() => PerformTurn();

    public EnemyIntent CalculateNextIntent()
    {
        DecideNextAction();
        return nextIntent;
    }

    public void ForceUpdateIntentUI()
    {
        if (intentUI == null) return;
        if (nextIntent.type != EnemyIntentType.None)
            intentUI.SetIntent(nextIntent);
    }

    // --- 자식 클래스가 반드시 구현 ---
    protected abstract void DecideNextAction();

    // ? 인터페이스 구현은 public 이어야 함
    public abstract void PerformTurn();

    // --- 필요 시 자식에서 오버라이드 ---
    protected virtual void ApplyTurnStartPassive() { }

    // ====== 공통 유틸 ======
    protected EnemyIntent MakeIntent(string name, int value, EnemyIntentType? forceType = null)
    {
        var icon = GetIcon(name);
        var type = forceType ?? AutoType(name);

        return new EnemyIntent
        {
            skillName = name,
            value = value,
            icon = icon,
            type = type,
            attacker = self,
            description = $"{name} 시전 예정"
        };
    }

    protected Sprite GetIcon(string skillName)
    {
        if (skillIconMap.TryGetValue(skillName, out var s)) return s;
        var fromRegistry = IconDatabase.GetIcon(skillName);
        return fromRegistry != null ? fromRegistry : defaultIcon;
    }

    protected EnemyIntentType AutoType(string name)
    {
        return name switch
        {
            "방어" or "방패 들기" => EnemyIntentType.Defend,
            "약화" or "취약" => EnemyIntentType.Debuff,
            _ => EnemyIntentType.Attack
        };
    }

    protected void DoAttack_Base(int baseDamage, float extraMultiplier = 1f, System.Action afterHit = null)
    {
        int dmg = DamageCalculator.CalculateOutgoingDamage(self, baseDamage);
        dmg = Mathf.RoundToInt(dmg * Mathf.Max(0f, extraMultiplier));
        player.TakeDamage(dmg);
        afterHit?.Invoke();
    }

    protected void AddShield(int value) => self.AddShield(value);
    protected void BuffSelf(string name, int stack) => self.AddOrUpdateBuff(name, stack);
    protected void DebuffSelf(string name, int stack) => self.AddOrUpdateDebuff(name, stack);
    protected void DebuffPlayer(string name, int stack) => player.AddOrUpdateDebuff(name, stack);
    protected void RemovePlayerDebuff(string name) => player.RemoveDebuff(name);

    protected int Roll100() => Random.Range(0, 100);

    [System.Serializable]
    public class SkillIconEntry { public string skillName; public Sprite icon; }
}
