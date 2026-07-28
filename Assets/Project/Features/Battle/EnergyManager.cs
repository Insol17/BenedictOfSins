using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class EnergyManager : MonoBehaviour
{
    public int maxEnergy = 3;
    public int currentEnergy = 0;
    public int energyPerTurn = 3;

    public TextMeshProUGUI energyText;
    public PlayableDirector energyTimeline;

    private IUnitStats playerStats;

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>(); // PlayerStats는 IUnitStats를 구현함
        GainEnergy(energyPerTurn);
    }

    public void GainEnergy(int amount)
    {
        if (playerStats != null)
            amount = playerStats.ModifyEnergyGain(amount); // 디버프 등 반영

        currentEnergy += amount;
        if (currentEnergy > maxEnergy)
            currentEnergy = maxEnergy;

        UpdateEnergyUI();
        PlayEnergyGainAnimation();
    }

    public void ResetEnergy()
    {
        currentEnergy = 0;
        UpdateEnergyUI();
    }

    public void SetEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
        UpdateEnergyUI();
        PlayEnergyGainAnimation();
        Debug.Log($"[EnergyManager] 에너지 설정됨: {currentEnergy}/{maxEnergy}");
    }

    void UpdateEnergyUI()
    {
        if (energyText != null)
            energyText.text = $"{currentEnergy}/{maxEnergy}";
    }

    void PlayEnergyGainAnimation()
    {
        if (energyTimeline != null)
        {
            energyTimeline.Stop();
            energyTimeline.Play();
        }
    }

    public int GetTotalCostModifier(IUnitStats unit)
    {
        int modifier = 0;
        if (unit == null) return modifier;

        foreach (var buff in unit.GetAllBuffs())
            modifier += buff.GetCostModifier();

        foreach (var debuff in unit.GetAllDebuffs())
            modifier += debuff.GetCostModifier();

        return modifier;
    }

    public bool SpendEnergy(int baseCost, IUnitStats unit)
    {
        int modifiedCost = Mathf.Max(0, baseCost + GetTotalCostModifier(unit));

        if (currentEnergy >= modifiedCost)
        {
            currentEnergy -= modifiedCost;
            UpdateEnergyUI();
            return true;
        }
        else
        {
            Debug.LogWarning($"[Energy] 에너지 부족! 필요: {modifiedCost}, 현재: {currentEnergy}");
            return false;
        }
    }
}
