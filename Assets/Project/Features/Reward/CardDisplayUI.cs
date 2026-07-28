using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplayUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Button selectButton;

    public event System.Action onClick;

    public void SetData(CardData data)
    {
        nameText.text = data.cardName;
        descriptionText.text = $"ÄÚ½ºÆ®: {data.energyCost}";
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClick?.Invoke());
    }
}
