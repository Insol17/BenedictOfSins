using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitHPUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider hpSlider;
    public Image hpFillImage; // 슬라이더 Fill 이미지
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI shieldText;
    public Image shieldIcon;

    [Header("UI Colors")]
    public Color hpColor = Color.red;
    public Color shieldColor = Color.cyan;

    [Header("상단 패널 표시용")]
    public TextMeshProUGUI additionalHpText;

    [Header("대상 유닛")]
    public GameObject targetUnit;

    private IUnitStats unitStats;

    void Start()
    {
        if (targetUnit != null)
        {
            unitStats = targetUnit.GetComponent<IUnitStats>();
        }
    }

    void Update()
    {
        if (unitStats != null)
        {
            int currentHP = unitStats.CurrentHP;
            int maxHP = unitStats.MaxHP;
            int shield = unitStats.Shield;

            // ✅ 슬라이더 설정
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;

                if (hpFillImage != null)
                {
                    hpFillImage.color = (shield > 0) ? shieldColor : hpColor;
                }
            }

            // ✅ HP 텍스트
            if (hpText != null)
                hpText.text = $"HP : {currentHP} / {maxHP}";

            if (additionalHpText != null)
                additionalHpText.text = $"HP : {currentHP} / {maxHP}";

            // ✅ 쉴드 텍스트 및 아이콘 표시
            bool hasShield = shield > 0;

            if (shieldText != null)
            {
                shieldText.text = hasShield ? $"+{shield}" : "";
                shieldText.gameObject.SetActive(hasShield);
            }

            if (shieldIcon != null)
            {
                shieldIcon.gameObject.SetActive(hasShield);
            }
        }
    }

    public void SetTarget(GameObject unit)
    {
        targetUnit = unit;
        unitStats = unit.GetComponent<IUnitStats>();
    }
}
