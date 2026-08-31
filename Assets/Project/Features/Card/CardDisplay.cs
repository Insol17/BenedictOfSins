using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 카드의 UI 표시 및 효과 수치를 실시간으로 반영하는 컴포넌트
/// - 카드 이름, 설명, 비용, 이미지 표시
/// - 데미지, 회복량 등 동적 수치 치환
/// - 대상에 따른 데미지 미리보기 지원
/// </summary>
public class CardDisplay : MonoBehaviour
{
    public CardData cardData;

    [Header("UI 참조")]
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text costText;
    public Image artworkImage;

    private PlayerStats playerStats;
    private Coroutine previewCycleRoutine;

    public CardEffectPlaceholderLibrary placeholderLibrary;

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (cardData != null)
            UpdateCardUI();
    }

    public void SetCard(CardData data)
    {
        cardData = data;
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
        UpdateCardUI();
    }

    public void UpdateCardUI()
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;

        string description = cardData.description;

        if (description.Contains("{Damage}"))
        {
            int finalDamage = DamageCalculator.CalculateOutgoingDamage(playerStats, cardData.baseDamage);
            description = description.Replace("{Damage}", finalDamage.ToString());
        }

        if (description.Contains("{Cost}"))
            description = description.Replace("{Cost}", cardData.energyCost.ToString());
        if (description.Contains("{HP}"))
            description = description.Replace("{HP}", cardData.hpCost.ToString());
        if (description.Contains("{Rage}"))
            description = description.Replace("{Rage}", cardData.rageCost.ToString());

        descriptionText.text = description;
        costText.text = GetCostString();

        if (artworkImage != null && cardData.artwork != null)
            artworkImage.sprite = cardData.artwork;
    }

    /// <summary>
    /// 코스트 텍스트 생성 (우선순위: 에너지 → HP → 분노)
    /// </summary>
    private string GetCostString()
    {
        if (cardData.energyCost > 0)
            return $"<color=#444444>{cardData.energyCost}</color>";
        else if (cardData.hpCost > 0)
            return $"<color=#C85050>{cardData.hpCost}</color>";
        else if (cardData.rageCost > 0)
            return $"<color=#C89040>{cardData.rageCost}</color>";
        else
            return "0";
    }

    public void RefreshDescription()
    {
        if (cardData == null || placeholderLibrary == null) return;

        var player = FindObjectOfType<PlayerStats>();
        if (player == null) return;

        string finalText = CardTextReplacer.ReplacePlaceholders(cardData, player, placeholderLibrary);
        descriptionText.text = finalText;
    }

    public void ShowCyclingDamagePreview(IUnitStats caster, List<IUnitStats> targets)
    {
        if (previewCycleRoutine != null)
            StopCoroutine(previewCycleRoutine);

        previewCycleRoutine = StartCoroutine(CycleDamageValues(caster, targets));
    }

    private IEnumerator CycleDamageValues(IUnitStats caster, List<IUnitStats> targets)
    {
        if (cardData == null || descriptionText == null) yield break;

        List<int> damageList = new();
        foreach (var target in targets)
        {
            int dmg = DamageCalculator.CalculateFinalDamageWithTarget(caster, target, cardData.baseDamage);
            damageList.Add(dmg);
        }

        string baseText = cardData.description;
        float interval = 1f;

        while (true)
        {
            foreach (int dmg in damageList)
            {
                yield return FadeText(descriptionText, false);
                string tempDesc = baseText.Replace("{Damage}", dmg.ToString());
                descriptionText.text = tempDesc;
                yield return FadeText(descriptionText, true);
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private IEnumerator FadeText(TMP_Text text, bool fadeIn, float duration = 0.2f)
    {
        float t = 0f;
        Color c = text.color;
        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(start, end, t / duration);
            text.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        text.color = new Color(c.r, c.g, c.b, end);
    }

    public void StopPreviewCycle()
    {
        if (previewCycleRoutine != null)
            StopCoroutine(previewCycleRoutine);
    }

    public void PreviewDamageWithTarget(IUnitStats target)
    {
        if (cardData == null || target == null) return;

        var attacker = FindObjectOfType<PlayerStats>();
        if (attacker == null) return;

        string description = cardData.description;

        foreach (var effect in cardData.effects)
        {
            if (effect.effectType == CardEffectType.Damage)
            {
                int previewDamage = DamageCalculator.CalculateFinalDamageWithTarget(attacker, target, effect.stack);
                description = description.Replace("{Damage}", previewDamage.ToString());
            }
        }

        description = description.Replace("{Cost}", GetCostString());
        descriptionText.text = description;
    }

    public void PreviewDamageWithMultipleTargets(List<IUnitStats> targets)
    {
        if (cardData == null || targets == null || targets.Count == 0) return;

        var attacker = FindObjectOfType<PlayerStats>();
        if (attacker == null) return;

        string description = cardData.description;

        foreach (var effect in cardData.effects)
        {
            if (effect.effectType == CardEffectType.Damage)
            {
                List<int> damages = targets
                    .Select(t => DamageCalculator.CalculateFinalDamageWithTarget(attacker, t, effect.stack))
                    .ToList();

                string joined = string.Join("/", damages);
                description = description.Replace("{Damage}", joined);
            }
        }

        description = description.Replace("{Cost}", GetCostString());
        descriptionText.text = description;
    }

    public void SetData(CardData data)
    {
        SetCard(data);
    }
}