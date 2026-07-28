using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeimdallAI : MonoBehaviour, IEnemyAI
{
    public EnemyStats self;
    public PlayerStats player;

    [Header("Intent UI")]
    [SerializeField] private EnemyIntentUI intentUI;
    [SerializeField] private Sprite defaultIcon;

    [Header("스킬 이름별 아이콘 매핑")]
    [SerializeField] private List<SkillIconEntry> skillIcons = new();
    private Dictionary<string, Sprite> skillIconMap = new();

    private enum Phase { Atla, Gjallp, Eistla }
    private Phase currentPhase = Phase.Atla;

    private EnemyIntent nextIntent;

    private void Awake()
    {
        foreach (var entry in skillIcons)
            skillIconMap[entry.skillName] = entry.icon;
    }

    private void Start()
    {
        ApplyPhaseBuff();
        DecideNextAction();
    }

    public EnemyStats Self => self;
    public PlayerStats Player => player;

    public void OnTurnStart()
    {
        ApplyPhasePassive();
        PerformTurn();
    }

    public void OnTurnEnd()
    {
        // 필요 시 여기에 추가
    }

    public void PerformAction()
    {
        PerformTurn();
    }

    public EnemyIntent CalculateNextIntent()
    {
        DecideNextAction();
        return nextIntent;
    }

    public void ForceUpdateIntentUI()
    {
        if (nextIntent.type != EnemyIntentType.None)
            intentUI.SetIntent(nextIntent);
    }

    private void DecideNextAction()
    {
        switch (currentPhase)
        {
            case Phase.Atla:
                nextIntent = DecideAtlaIntent();
                break;
            case Phase.Gjallp:
                nextIntent = DecideGjallpIntent();
                break;
            case Phase.Eistla:
                nextIntent = DecideEistlaIntent();
                break;
        }

        intentUI.SetIntent(nextIntent);
    }

    public void PerformTurn()
    {
        CheckPhaseTransition();
        Debug.Log("[HeimdallAI] PerformTurn 호출됨");

        switch (nextIntent.skillName)
        {
            case "공격":
                DoAttack(15);
                break;

            case "약화의 시선":
                player.AddOrUpdateDebuff("Weak", 2);
                break;

            case "바이프로스트 공격":
                DoAttack(10);
                player.AddOrUpdateDebuff("Bifrost", 1);
                break;

            case "미래를 울부짖다":
                player.AddOrUpdateDebuff("Weak", 2);
                break;

            case "불확실성":
                int count = Random.Range(1, 5);
                for (int i = 0; i < count; i++)
                    DoAttack(5);
                break;

            case "플래시":
                DoAttack(10);
                player.AddOrUpdateDebuff("Glow", 1);
                break;

            case "빛나는 타격":
                DoAttack(20);
                player.AddOrUpdateDebuff("Glow", 2);
                break;

            case "방패 들기":
                self.AddShield(20);
                break;

            case "심판":
                DoAttack(35);
                player.RemoveDebuff("JudgementTarget");
                break;
        }

        nextIntent = default;
        intentUI.ClearIntent();
        self.OnTurnStart();
    }

    private void DoAttack(int baseDamage)
    {
        int finalDamage = DamageCalculator.CalculateOutgoingDamage(self, baseDamage);
        player.TakeDamage(finalDamage);

        if (currentPhase == Phase.Eistla)
            player.AddOrUpdateDebuff("Glow", 1);

        if (currentPhase == Phase.Gjallp)
            self.AddOrUpdateBuff("DoomWave", 1);
    }

    private void ApplyPhasePassive()
    {
        switch (currentPhase)
        {
            case Phase.Atla:
                self.AddShield(20);
                break;

            case Phase.Eistla:
                HandManager hand = FindObjectOfType<HandManager>();
                hand?.DiscardRandomCard();
                break;
        }

        if (currentPhase == Phase.Eistla && player.GetDebuffStack("Glow") >= 5)
        {
            player.AddOrUpdateDebuff("JudgementTarget", 1);
        }
    }

    private void CheckPhaseTransition()
    {
        float hpRatio = self.CurrentHP / (float)self.MaxHP;

        if (currentPhase == Phase.Atla && hpRatio <= 0.66f)
        {
            currentPhase = Phase.Gjallp;
            ApplyPhaseBuff();
        }
        else if (currentPhase == Phase.Gjallp && hpRatio <= 0.33f)
        {
            currentPhase = Phase.Eistla;
            ApplyPhaseBuff();
        }
    }

    private void ApplyPhaseBuff()
    {
        self.RemoveBuff("Atla");
        self.RemoveBuff("Gjallp");
        self.RemoveBuff("Eistla");

        switch (currentPhase)
        {
            case Phase.Atla:
                self.AddOrUpdateBuff("Atla", 1);
                break;
            case Phase.Gjallp:
                self.AddOrUpdateBuff("DoomWave", 0);
                break;
            case Phase.Eistla:
                self.AddOrUpdateBuff("Eistla", 1);
                break;
        }
    }

    // ===== 인텐트 결정 =====

    private EnemyIntent DecideAtlaIntent()
    {
        int roll = Random.Range(0, 100);
        if (roll < 40)
            return MakeIntent("공격", 15);
        else if (roll < 70)
            return MakeIntent("약화의 시선", 0);
        else
            return MakeIntent("바이프로스트 공격", 10);
    }

    private EnemyIntent DecideGjallpIntent()
    {
        int doomStack = self.GetBuffStack("DoomWave");
        int bonus = Mathf.RoundToInt(10 * (1f + 0.03f * doomStack));

        int roll = Random.Range(0, 100);
        if (roll < 40)
            return MakeIntent("공격", bonus);
        else if (roll < 70)
            return MakeIntent("미래를 울부짖다", 0);
        else
            return MakeIntent("불확실성", 5);
    }

    private EnemyIntent DecideEistlaIntent()
    {
        if (player.HasDebuff("JudgementTarget"))
            return MakeIntent("심판", 35);

        int roll = Random.Range(0, 100);
        if (roll < 40)
            return MakeIntent("플래시", 10);
        else if (roll < 70)
            return MakeIntent("빛나는 타격", 20);
        else if (roll < 80)
            return MakeIntent("바이프로스트 공격", 10);
        else
            return MakeIntent("방패 들기", 0);
    }

    private EnemyIntent MakeIntent(string name, int value)
    {
        Sprite icon = GetIntentIcon(name);

        // 자동 타입 분류
        EnemyIntentType type = name switch
        {
            "약화의 시선" or "미래를 울부짖다" or "플래시" => EnemyIntentType.Debuff,
            "방패 들기" => EnemyIntentType.Defend,
            "심판" => EnemyIntentType.SpecialAttack,
            _ => EnemyIntentType.Attack
        };

        return new EnemyIntent
        {
            skillName = name,
            value = value,
            icon = icon,
            type = type,
            attacker = self, // ?? 실시간 예측 데미지 반영
            description = $"{name} 시전 예정"
        };
    }


    private Sprite GetIntentIcon(string skillName)
    {
        if (skillIconMap.TryGetValue(skillName, out var icon))
            return icon;

        var fromRegistry = IconDatabase.GetIcon(skillName);
        return fromRegistry != null ? fromRegistry : defaultIcon;
    }

    [System.Serializable]
    public class SkillIconEntry
    {
        public string skillName;
        public Sprite icon;
    }
}
