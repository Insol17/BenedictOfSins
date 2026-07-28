using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    public RewardUIManager rewardUIManager;

    private Reward currentReward;
    private bool isRewardClaimed = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SaveManager.Instance.HasPendingReward(out Reward reward))
        {
            Debug.Log("[RewardManager] OnSceneLoaded에서 보상 복원");
            currentReward = reward;
            rewardUIManager.Show(reward);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded; // ? 중복 방지
    }



    public void GenerateReward(EnemyType type, bool saveToFile = true)
    {
        currentReward = new Reward();

        // 1. 골드 고정 생성
        currentReward.gold = type switch
        {
            EnemyType.Normal => Random.Range(10, 36),
            EnemyType.Elite => Random.Range(50, 81),
            EnemyType.Boss => Random.Range(100, 151),
            _ => 0
        };

        // 2. 카드 3장 무조건 포함
        currentReward.cardOptions = CardDatabase.GetRandomCards(3);

        // 3. 유물은 조건에 따라
        if (type == EnemyType.Elite)
        {
            currentReward.isRelicGiven = true;
            currentReward.relic = RelicDatabase.GetRelicByRarityWeighted(20, 80);
        }
        else if (type == EnemyType.Boss)
        {
            currentReward.isRelicGiven = true;
            currentReward.relic = RelicDatabase.GetBossRelic();
        }
        else if (Random.value < 0.03f) // 일반 적일 때 3% 확률
        {
            currentReward.isRelicGiven = true;
            currentReward.relic = RelicDatabase.GetNormalRelic();
        }

        if (saveToFile)
            SaveManager.Instance.SaveReward(currentReward);

        StartCoroutine(ShowRewardUIWithDelay());
    }


    private IEnumerator ShowRewardUIWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        rewardUIManager.Show(currentReward);
    }

    public void MarkRewardClaimed()
    {
        isRewardClaimed = true;
        SaveManager.Instance.ClearReward(); // ?? 여기서 보상 삭제
    }


    public void ReloadSavedRewardIfExists()
    {
        if (SaveManager.Instance.HasPendingReward(out Reward savedReward))
        {
            currentReward = savedReward;
            rewardUIManager.Show(currentReward);
        }
    }
}
