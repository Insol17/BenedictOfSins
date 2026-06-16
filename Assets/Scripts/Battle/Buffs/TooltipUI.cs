using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;

    /// <summary>
    /// 기존 방식 - 효과 ID를 기반으로 레지스트리에서 정보 조회
    /// </summary>
    public void SetTooltip(string id)
    {
        var info = EffectRegistry.Get(id);

        if (info != null)
        {
            nameText.text = info.Name;
            descriptionText.text = info.Description.Replace("/n", "\n");

            if (iconImage != null && info.Icon != null)
                iconImage.sprite = info.Icon;
        }
        else
        {
            nameText.text = id;
            descriptionText.text = "(설명 없음)";
            if (iconImage != null)
                iconImage.sprite = null;
        }
    }

    /// <summary>
    /// 직접 정보 전달 방식 - 인텐트 설명 등 사용 시
    /// </summary>
    public void SetTooltip(string title, string desc, Sprite icon)
    {
        nameText.text = title;
        descriptionText.text = desc.Replace("/n", "\n");

        if (iconImage != null)
            iconImage.sprite = icon;
    }
}
