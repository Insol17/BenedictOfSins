using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;

    private List<RelicData> relics = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddRelic(RelicData relic)
    {
        relics.Add(relic);
        Debug.Log($"[RelicManager] À¯¹° Ãß°¡µÊ: {relic.relicName}");
    }

    public List<RelicData> GetRelics() => relics;
}
