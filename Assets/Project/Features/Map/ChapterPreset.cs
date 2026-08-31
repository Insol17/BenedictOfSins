using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewChapterPreset", menuName = "Map/Chapter Preset")]
public class ChapterPreset : ScriptableObject
{
    public string chapterName;
    public string chapterId;

    [Header("맵 설정")]
    public MapScenePreset mapScenePreset;
    public MapIconLibrary iconLibrary;

    [Header("카드풀")]
    public List<CardData> commonCards;
    public List<CardData> rareCards;
    public List<CardData> mythicCards;

    [Header("플레이어 초기 설정")]
    public int defaultPlayerMaxHP = 80;
    public List<CardData> startingCards;

    public int startingGold = 99;
    public int startingEnergy = 3;
    public List<RelicData> startingRelics; // RelicData는 ScriptableObject로 가정

    public CardData GetRandomCard()
    {
        float rand = Random.Range(0f, 100f);

        if (rand < 65f && commonCards.Count > 0)
            return commonCards[Random.Range(0, commonCards.Count)];
        else if (rand < 95f && rareCards.Count > 0)
            return rareCards[Random.Range(0, rareCards.Count)];
        else if (mythicCards.Count > 0)
            return mythicCards[Random.Range(0, mythicCards.Count)];

        // 예외 처리용 백업
        if (commonCards.Count > 0) return commonCards[0];
        if (rareCards.Count > 0) return rareCards[0];
        if (mythicCards.Count > 0) return mythicCards[0];

        return null;
    }

    public List<CardData> GetRandomCards(int count)
    {
        List<CardData> result = new();
        HashSet<CardData> selected = new();

        int maxAttempts = 100;

        while (result.Count < count && maxAttempts-- > 0)
        {
            var card = GetRandomCard();
            if (card != null && !selected.Contains(card))
            {
                selected.Add(card);
                result.Add(card);
            }
        }

        return result;
    }

    //읽기 전용 카드풀 통합
    public List<CardData> cardPool
    {
        get
        {
            List<CardData> fullList = new();
            if (commonCards != null) fullList.AddRange(commonCards);
            if (rareCards != null) fullList.AddRange(rareCards);
            if (mythicCards != null) fullList.AddRange(mythicCards);
            return fullList;
        }
    }

}
