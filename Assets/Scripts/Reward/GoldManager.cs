using UnityEngine;
using UnityEngine.Events;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    private int gold;
    public int Gold => gold;

    public UnityEvent<int> OnGoldChanged = new(); // 외부 UI 연결용

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetGold(int amount)
    {
        gold = Mathf.Max(0, amount);
        OnGoldChanged.Invoke(gold);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged.Invoke(gold);
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;
        OnGoldChanged.Invoke(gold);
        return true;
    }
}
