using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RelicDatabase
{
    public static RelicData GetNormalRelic()
    {
        return new RelicData { relicId = "normal01", relicName = "일반 유물", description = "기본 유물입니다.", isBossRelic = false };
    }

    public static RelicData GetRelicByRarityWeighted(int normalChance, int rareChance)
    {
        return new RelicData { relicId = "rare01", relicName = "희귀 유물", description = "강력한 유물입니다.", isBossRelic = false };
    }

    public static RelicData GetBossRelic()
    {
        return new RelicData { relicId = "boss01", relicName = "보스 유물", description = "보스를 물리치고 얻은 유물", isBossRelic = true };
    }
}
