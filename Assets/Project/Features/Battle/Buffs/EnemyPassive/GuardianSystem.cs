using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 같은 group 내 아군이 피해를 받을 때 50%를 대신 받는 "수호자" 라우팅.
/// 공격자가 피해 최종값을 정한 뒤, 대상이 적(EnemyStats)일 때 호출하여 분배하도록 사용.
/// </summary>
public static class GuardianSystem
{
    private class GuardInfo { public EnemyStats guardian; public string group; }
    private static readonly List<GuardInfo> _guards = new();

    public static void RegisterGuardian(EnemyStats guardian, string group)
    {
        Unregister(guardian);
        _guards.Add(new GuardInfo { guardian = guardian, group = group });
    }

    public static void Unregister(EnemyStats guardian)
    {
        _guards.RemoveAll(g => g.guardian == guardian);
    }

    /// <summary>
    /// 반환: (피격자에게 남는 피해, 수호자가 받은 피해)
    /// 수호자 HP가 30% 이하이면 해제 후 분배하지 않음.
    /// </summary>
    public static (int toVictim, int toGuardian) SplitDamage(string group, int totalDamage)
    {
        var gi = _guards.Find(g => g.group == group && g.guardian != null);
        if (gi == null || gi.guardian == null) return (totalDamage, 0);

        if (gi.guardian.CurrentHP <= gi.guardian.MaxHP * 0.3f)
        {
            Unregister(gi.guardian);
            return (totalDamage, 0);
        }

        int gShare = Mathf.RoundToInt(totalDamage * 0.5f);
        int vShare = totalDamage - gShare;

        gi.guardian.TakeDamage(gShare);
        return (vShare, gShare);
    }
}
