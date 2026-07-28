using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TooltipUISpawner : MonoBehaviour
{
    [Header("Tooltip Prefab")]
    [SerializeField] private GameObject tooltipPrefab;

    [Header("Offset from Icon (Local Space)")]
    [SerializeField] private Vector2 localOffset = new Vector2(80f, 40f);

    [Header("인텐트 설명 데이터")]
    [SerializeField] private List<IntentTooltipEntry> intentTooltipEntries;

    private GameObject tooltipInstance;
    private RectTransform tooltipRect;
    private TooltipUI tooltipUI;

    /// <summary>
    /// 기존 효과 ID 기반 툴팁 호출 (버프/디버프 등)
    /// </summary>
    public void ShowTooltip(string effectId, Transform iconTransform)
    {
        if (tooltipPrefab == null)
        {
            Debug.LogError("[Tooltip] Prefab이 설정되지 않았습니다.");
            return;
        }

        DestroyExistingTooltip();

        tooltipInstance = Instantiate(tooltipPrefab, iconTransform);
        SetupTransform(iconTransform);

        if (tooltipUI == null)
        {
            Debug.LogError("[Tooltip] TooltipUI 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        tooltipUI.SetTooltip(effectId);
        tooltipInstance.SetActive(true);
    }

    /// <summary>
    /// EnemyIntentType 기반 툴팁 호출 (기본 타입용)
    /// </summary>
    public void ShowTooltip(EnemyIntentType type, Transform iconTransform)
    {
        if (tooltipPrefab == null)
        {
            Debug.LogError("[Tooltip] Prefab이 설정되지 않았습니다.");
            return;
        }

        DestroyExistingTooltip();

        tooltipInstance = Instantiate(tooltipPrefab, iconTransform);
        SetupTransform(iconTransform);

        if (tooltipUI == null)
        {
            Debug.LogError("[Tooltip] TooltipUI 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        IntentTooltipEntry entry = intentTooltipEntries.Find(e => e.type == type);
        if (entry != null)
        {
            tooltipUI.SetTooltip(entry.title, entry.description, entry.icon);
        }
        else
        {
            tooltipUI.SetTooltip("알 수 없음", "설명 없음", null);
        }

        tooltipInstance.SetActive(true);
    }

    /// ? 새로 추가: EnemyIntent 전체 기반 툴팁
    public void ShowTooltip(EnemyIntent intent, Transform iconTransform)
    {
        if (tooltipPrefab == null)
        {
            Debug.LogError("[Tooltip] Prefab이 설정되지 않았습니다.");
            return;
        }

        DestroyExistingTooltip();

        tooltipInstance = Instantiate(tooltipPrefab, iconTransform);
        SetupTransform(iconTransform);

        if (tooltipUI == null)
        {
            Debug.LogError("[Tooltip] TooltipUI 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        tooltipUI.SetTooltip(intent.skillName, intent.description, intent.icon);
        tooltipInstance.SetActive(true);
    }

    public void HideTooltip()
    {
        DestroyExistingTooltip();
    }

    private void DestroyExistingTooltip()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }

    private void SetupTransform(Transform iconTransform)
    {
        tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        tooltipUI = tooltipInstance.GetComponent<TooltipUI>();

        tooltipRect.anchoredPosition = localOffset;
        tooltipRect.localRotation = Quaternion.identity;
        tooltipRect.localScale = Vector3.one * 3f;
        tooltipRect.SetAsLastSibling();
    }
}
