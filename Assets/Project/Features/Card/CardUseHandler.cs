using UnityEngine;
using UnityEngine.UI;

public class CardUseHandler : MonoBehaviour
{
    private Button button;
    private CardDisplay display;
    private CardHover hover;
    private CardEffectHandler effectHandler;
    private EnergyManager energyManager;

    public GameObject target; // 공격 대상 (예: 적)

    void Awake()
    {
        button = GetComponent<Button>();
        display = GetComponent<CardDisplay>();
        hover = GetComponent<CardHover>();
        effectHandler = FindObjectOfType<CardEffectHandler>();
        energyManager = FindObjectOfType<EnergyManager>();

        if (button != null)
            button.onClick.AddListener(HandleCardUse);
    }

    void HandleCardUse()
    {
        if (display == null || display.cardData == null)
            return;

        var data = display.cardData;

        bool insufficientEnergy = energyManager == null || energyManager.currentEnergy < data.energyCost;

        if (insufficientEnergy)
        {
            Debug.Log("코스트 부족: 카드 사용 차단됨");

            if (hover != null)
                hover.OnPointerExit(null);

            CostWarningUI warning = FindObjectOfType<CostWarningUI>();
            if (warning != null)
                warning.ShowWarning("코스트 부족!");

            return;
        }

        // 카드 사용
        effectHandler.UseCardOnTarget(data, target);
        Destroy(gameObject); // 카드 사용 성공 시 제거
    }

}
