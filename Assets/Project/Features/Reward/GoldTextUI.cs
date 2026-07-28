using TMPro;
using UnityEngine;

public class GoldTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        GoldManager.Instance.OnGoldChanged.AddListener(UpdateGoldText);
        UpdateGoldText(GoldManager.Instance.Gold);
    }

    private void UpdateGoldText(int newGold)
    {
        goldText.text = $"{newGold} G";
    }
}
