using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuffIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TMP_Text stackText;

    private string type;
    private TooltipUISpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<TooltipUISpawner>();
    }

    public void SetData(Sprite sprite, int stack)
    {
        iconImage.sprite = sprite;
        stackText.text = stack > 1 ? stack.ToString() : "";
    }

    public void SetType(string buffType)
    {
        type = buffType;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spawner != null)
            spawner.ShowTooltip(type, transform); // ¡ç transform Ãß°¡
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (spawner != null)
            spawner.HideTooltip();
    }
}
