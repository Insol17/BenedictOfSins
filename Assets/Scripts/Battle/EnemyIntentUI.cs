using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EnemyIntentUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI skillNameText;

    [Header("Tooltip")]
    [SerializeField] private TooltipUISpawner tooltipSpawner;

    private EnemyIntent currentIntent;

    public void SetIntent(EnemyIntent intent)
    {
        currentIntent = intent;

        if (intent.type == EnemyIntentType.None)
        {
            gameObject.SetActive(false);
            return;
        }

        iconImage.sprite = intent.icon;

        if (skillNameText != null)
            skillNameText.text = intent.skillName;

        UpdateDamageText(); // 초기 데미지 갱신
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (currentIntent.type != EnemyIntentType.None)
        {
            UpdateDamageText();
        }
    }

    private void UpdateDamageText()
    {
        if (damageText == null) return;

        if (currentIntent.attacker != null && currentIntent.value > 0)
        {
            int predicted = currentIntent.GetPredictedDamage();
            damageText.text = predicted.ToString();
        }
        else
        {
            damageText.text = currentIntent.value > 0 ? currentIntent.value.ToString() : "";
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipSpawner != null && currentIntent.type != EnemyIntentType.None)
        {
            tooltipSpawner.ShowTooltip(currentIntent, transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipSpawner?.HideTooltip();
    }

    public void ClearIntent()
    {
        if (damageText != null)
            damageText.text = "";

        if (skillNameText != null)
            skillNameText.text = "";

        gameObject.SetActive(false);
    }
}
