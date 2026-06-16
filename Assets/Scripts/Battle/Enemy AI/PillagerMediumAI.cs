using System.Collections.Generic;
using UnityEngine;

public class PillagerMediumAI : MonoBehaviour, IEnemyAI
{
    public EnemyStats self;
    public PlayerStats player;

    [Header("Intent UI")]
    [SerializeField] private EnemyIntentUI intentUI;
    [SerializeField] private Sprite defaultIcon;

    [Header("스킬 이름별 아이콘 매핑")]
    [SerializeField] private List<SkillIconEntry> skillIcons = new();
    private readonly Dictionary<string, Sprite> skillIconMap = new();

    [Header("행동 가중치(합계 아무 값이나 OK, 비율로 사용)")]
    [Min(0)] public int weightAttack = 50;      // 공격(6) + 자기 Strength+1
    [Min(0)] public int weightStrongAttack = 30;// 강한 공격(8)
    [Min(0)] public int weightVulnerable = 20;  // 취약 1

    private EnemyIntent nextIntent;

    private void Awake()
    {
        foreach (var e in skillIcons)
            if (e != null && !string.IsNullOrEmpty(e.skillName))
                skillIconMap[e.skillName] = e.icon;
    }

    private void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerStats>();
        if (self == null) self = GetComponent<EnemyStats>();

        DecideNextAction();
        ForceUpdateIntentUI();
    }

    public EnemyStats Self => self;
    public PlayerStats Player => player;

    public void OnTurnStart() => PerformTurn();
    public void OnTurnEnd() { }
    public void PerformAction() => PerformTurn();

    public EnemyIntent CalculateNextIntent()
    {
        DecideNextAction();
        return nextIntent;
    }

    public void ForceUpdateIntentUI()
    {
        if (intentUI == null) return;
        if (nextIntent.type == EnemyIntentType.None) intentUI.ClearIntent();
        else intentUI.SetIntent(nextIntent);
    }

    private void DecideNextAction()
    {
        int total = Mathf.Max(0, weightAttack) + Mathf.Max(0, weightStrongAttack) + Mathf.Max(0, weightVulnerable);
        if (total == 0)
        {
            // 방어적 폴백: 공격으로 고정
            nextIntent = MakeIntent("공격(힘+1)", 6);
            return;
        }

        int roll = Random.Range(0, total);
        int acc = 0;

        acc += Mathf.Max(0, weightAttack);
        if (roll < acc)
        {
            nextIntent = MakeIntent("공격(힘+1)", 6);
            return;
        }

        acc += Mathf.Max(0, weightStrongAttack);
        if (roll < acc)
        {
            nextIntent = MakeIntent("강한 공격", 8);
            return;
        }

        nextIntent = MakeIntent("취약", 0, EnemyIntentType.Debuff);
    }

    public void PerformTurn()
    {
        if (self == null || player == null)
        {
            Debug.LogWarning("[PillagerMediumAI] refs missing");
            return;
        }

        switch (nextIntent.skillName)
        {
            case "공격(힘+1)":
                DoAttack(6);
                self.AddOrUpdateBuff("Strength", 1); // 스스로 힘 +1
                break;

            case "강한 공격":
                DoAttack(8);
                break;

            case "취약":
                player.AddOrUpdateDebuff("Vulnerable", 1);
                break;

            default:
                DoAttack(6);
                break;
        }

        nextIntent = default;
        intentUI?.ClearIntent();
        DecideNextAction();
        ForceUpdateIntentUI();
    }

    private void DoAttack(int baseDamage)
    {
        int dmg = baseDamage;
        if (self != null)
        {
            try { dmg = DamageCalculator.CalculateOutgoingDamage(self, baseDamage); }
            catch { dmg = baseDamage; }
        }
        player.TakeDamage(dmg);
    }

    private EnemyIntent MakeIntent(string name, int value, EnemyIntentType? forceType = null)
    {
        EnemyIntentType type = forceType ?? (name == "취약" ? EnemyIntentType.Debuff : EnemyIntentType.Attack);
        return new EnemyIntent
        {
            skillName = name,
            value = value,
            icon = GetIcon(name),
            type = type,
            attacker = self,
            description = $"{name} 시전 예정"
        };
    }

    private Sprite GetIcon(string skillName)
    {
        if (!string.IsNullOrEmpty(skillName) && skillIconMap.TryGetValue(skillName, out var s)) return s;
        var reg = IconDatabase.GetIcon(skillName);
        return reg != null ? reg : defaultIcon;
    }

    [System.Serializable] public class SkillIconEntry { public string skillName; public Sprite icon; }
}
