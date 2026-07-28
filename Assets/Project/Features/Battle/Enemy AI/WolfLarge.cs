using System.Collections.Generic;
using UnityEngine;

public class WolfLargeAI : MonoBehaviour, IEnemyAI
{
    public EnemyStats self;
    public PlayerStats player;

    [Header("Intent UI")]
    [SerializeField] private EnemyIntentUI intentUI;
    [SerializeField] private Sprite defaultIcon;

    [Header("스킬 이름별 아이콘 매핑")]
    [SerializeField] private List<SkillIconEntry> skillIcons = new();
    private readonly Dictionary<string, Sprite> skillIconMap = new();

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

    public EnemyIntent CalculateNextIntent() { DecideNextAction(); return nextIntent; }

    public void ForceUpdateIntentUI()
    {
        if (intentUI == null) return;
        if (nextIntent.type == EnemyIntentType.None) intentUI.ClearIntent();
        else intentUI.SetIntent(nextIntent);
    }

    private void DecideNextAction()
    {
        int r = Random.Range(0, 100);
        if (r < 50) nextIntent = MakeIntent("공격+출혈4", 10);
        else if (r < 80) nextIntent = MakeIntent("공격+출혈3", 15);
        else nextIntent = MakeIntent("취약", 0, EnemyIntentType.Debuff);
    }

    public void PerformTurn()
    {
        if (self == null || player == null)
        {
            Debug.LogWarning("[WolfLargeAI] refs missing");
            return;
        }

        switch (nextIntent.skillName)
        {
            case "공격+출혈4":
                DoAttack(10);
                player.AddOrUpdateDebuff("Bleed", 4);
                break;
            case "공격+출혈3":
                DoAttack(15);
                player.AddOrUpdateDebuff("Bleed", 3);
                break;
            case "취약":
                player.AddOrUpdateDebuff("Vulnerable", 1);
                break;
            default:
                DoAttack(10);
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
        var type = forceType ?? (name.StartsWith("공격") ? EnemyIntentType.Attack : EnemyIntentType.Debuff);
        return new EnemyIntent { skillName = name, value = value, icon = GetIcon(name), type = type, attacker = self, description = $"{name} 시전 예정" };
    }

    private Sprite GetIcon(string skillName)
    {
        if (!string.IsNullOrEmpty(skillName) && skillIconMap.TryGetValue(skillName, out var s)) return s;
        var reg = IconDatabase.GetIcon(skillName);
        return reg != null ? reg : defaultIcon;
    }

    [System.Serializable] public class SkillIconEntry { public string skillName; public Sprite icon; }
}
