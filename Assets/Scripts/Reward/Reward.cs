using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Reward
{
    public int gold;
    public List<CardData> cardOptions = new();
    public RelicData relic;
    public bool isRelicGiven;

    // 저장용 기본 생성자
    public Reward() { }

    public Reward(int gold, List<CardData> cards, RelicData relic, bool hasRelic)
    {
        this.gold = gold;
        this.cardOptions = cards;
        this.relic = relic;
        this.isRelicGiven = hasRelic;
    }
}
