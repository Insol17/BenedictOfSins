using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

public class CardEffectHandler : MonoBehaviour
{
    [Header("외부 참조")]
    public Transform playerTransform;
    public Transform defaultCameraTarget;
    public Transform cardZoomTarget;
    public Transform cameraTransform;
    public List<PlayableDirector> directors = new();

    private Transform currentTarget;
    private Vector3 originalPlayerPosition;
    private bool firstFrame = true;

    public EnergyManager energyManager;
    public HandManager handManager;

    private Vector3 velocity = Vector3.zero;
    private float overshootAmount = 2f;
    private float overshootDamping = 5f;

    public bool IsCardBeingPlayed { get; private set; } = false;

    [SerializeField] private float quickJabDistance = 5f;     // 전진 거리(+x)
    [SerializeField] private float quickJabDuration = 0.2f;   // 총 0.2초(왕복)
    [SerializeField] private bool jabOnlyForDamage = true;    // 데미지 카드에만 적용할지
    private Coroutine jabCoroutine;

    private void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (defaultCameraTarget == null || cardZoomTarget == null)
        {
            Debug.LogError("카메라 타겟이 지정되지 않았습니다.");
            return;
        }

        currentTarget = defaultCameraTarget;
        cameraTransform.position = currentTarget.position;
    }

    // ─────────────────────────────────────────────────────
    private void StartJabMotion(float dir)
    {
        if (jabCoroutine != null)
            StopCoroutine(jabCoroutine);

        jabCoroutine = StartCoroutine(QuickJab(quickJabDistance, quickJabDuration, dir));
    }

    private IEnumerator QuickJab(float distance, float totalDuration, float dirSign)
    {
        float half = Mathf.Max(0.01f, totalDuration * 0.5f);
        Vector3 start = playerTransform.position;
        Vector3 fwd = new Vector3(distance * dirSign, 0f, 0f);
        Vector3 peak = start + fwd;

        // 앞으로 이동
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0f, 1f, t / half);
            Vector3 p = Vector3.Lerp(start, peak, a);
            p.y = start.y;
            playerTransform.position = p;
            yield return null;
        }

        // 뒤로 복귀
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0f, 1f, t / half);
            Vector3 p = Vector3.Lerp(peak, start, a);
            p.y = start.y;
            playerTransform.position = p;
            yield return null;
        }

        playerTransform.position = start;
    }

    // 적 방향 계산 (플레이어 → 적)
    private float GetAttackDirSign()
    {
        var alive = FindObjectsOfType<EnemyStats>()
                    .Where(e => e != null && e.CurrentHP > 0).ToList();
        if (alive.Count == 0) return 1f;

        float avgX = alive.Average(e => e.transform.position.x);
        return Mathf.Sign(avgX - playerTransform.position.x); // 적 평균 위치 방향
    }
    // ─────────────────────────────────────────────────────

    public void UseCardOnTarget(CardData card, GameObject target)
    {
        if (IsCardBeingPlayed) return; // 재진입 방지 (추가 가드)

        if (energyManager == null)
            energyManager = FindObjectOfType<EnergyManager>();

        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;

        var player = playerTransform.GetComponent<PlayerStats>();

        // 에너지 체크
        if (energyManager.currentEnergy < card.energyCost)
        {
            Debug.Log("에너지가 부족합니다!");
            return;
        }

        // 자원 차감
        energyManager.currentEnergy -= card.energyCost;
        energyManager.GainEnergy(0); // UI 갱신

        if (card.hpCost > 0)
            player?.TakeDamage(card.hpCost);

        originalPlayerPosition = playerTransform.position;
        StartCoroutine(PlayCardSequence(card, target));
    }

    private IEnumerator PlayCardSequence(CardData card, GameObject target)
    {
        if (IsCardBeingPlayed) yield break; // 중복 호출 보호

        IsCardBeingPlayed = true;
        try
        {
            // ── 덱/소멸 처리
            if (card.isBanishedOnUse)
                DeckManager.Instance.ExileCard(card);
            else
                DeckManager.Instance.UseCard(card);

            // 선택 카드 핸드에서 제거
            GameObject usedCardObj = CardSelector.selectedCard;
            if (usedCardObj != null)
            {
                CardManager.Instance.handManager.RemoveCardFromHand(usedCardObj);
                CardSelector.selectedCard = null;
            }

            // 타겟 필요 카드인데 없음 → 조기 종료 (finally에서 잠금 해제 보장)
            if (card.requiresTarget && target == null)
            {
                Debug.LogWarning($"타겟이 필요한 카드인데 null입니다: {card.cardName}");
                yield break;
            }

            Vector3 adjustedTargetPos = target != null
                ? target.transform.position + new Vector3(0, 0, -0.5f)
                : Vector3.zero;

            float stopDuration = 0.5f;
            PlayableDirector matchingDirector = null;
            if (card.timelineAsset != null)
                matchingDirector = GetDirectorForTimeline(card.timelineAsset);

            if (card.requiresMovement)
            {
                IEnumerator moveCoroutine = null;
                if (target != null)
                {
                    moveCoroutine = MoveToTarget(adjustedTargetPos);
                    StartCoroutine(moveCoroutine);
                }

                if (card.requiresCameraZoom)
                    yield return StartCoroutine(ZoomToPlayer());

                if (matchingDirector != null)
                    matchingDirector.Play();

                float waitTime = Mathf.Max(0.25f, matchingDirector != null ? (float)card.timelineAsset.duration : 0f);
                yield return new WaitForSeconds(waitTime);

                if (moveCoroutine != null)
                {
                    while (moveCoroutine.MoveNext())
                        yield return moveCoroutine.Current;
                }

                yield return new WaitForSeconds(stopDuration);
            }
            else
            {
                // ★ 짧은 찌르기: requiresMovement가 아닌 카드에 한해 적용
                bool shouldJab = !card.requiresMovement &&
                                 (!jabOnlyForDamage || card.effects.Any(e => e.effectType == CardEffectType.Damage));
                if (shouldJab)
                {
                    float dir = GetAttackDirSign(); // 보통 +1 (오른쪽)
                    StartJabMotion(dir); // ← Coroutine 직접 실행 대신, 이전 모션 중단 후 새로 시작
                    yield return new WaitForSeconds(quickJabDuration); // 모션 시간만큼 대기
                }

                if (card.requiresCameraZoom)
                    yield return StartCoroutine(ZoomToPlayer());

                if (matchingDirector != null)
                {
                    matchingDirector.Play();
                    yield return new WaitUntil(() => matchingDirector.state != PlayState.Playing);
                }
            }


            // 실제 효과 적용
            ApplyAllEffects(card, target);

            // 복귀/카메라 리셋
            if (card.requiresMovement)
                yield return StartCoroutine(MoveBackToOriginalPosition());
            if (card.requiresCameraZoom)
                yield return StartCoroutine(ResetCameraPosition());

            yield return new WaitForSeconds(0.1f);
            if (usedCardObj != null)
                Destroy(usedCardObj);
        }
        finally
        {
            // ★ 어떤 경우에도 잠금 해제 (버그 원천 차단)
            IsCardBeingPlayed = false;
        }
    }

    private void ApplyAllEffects(CardData card, GameObject selectedTarget)
    {
        var player = playerTransform.GetComponent<PlayerStats>();

        foreach (var effect in card.effects)
        {
            switch (effect.effectType)
            {
                case CardEffectType.Damage:
                    ApplyDamage(effect, selectedTarget, player);
                    break;
                case CardEffectType.DelayedDamage:
                    StartCoroutine(ApplyDelayedDamage(effect, selectedTarget, player));
                    break;
                case CardEffectType.Conditional:
                    if (CheckGeneralCondition(effect, player, selectedTarget))
                        ApplyConditionalEffect(effect, selectedTarget, player);
                    break;
                case CardEffectType.RageConditional:
                    if (player.HasBuff("Rage") && player.GetBuffStack("Rage") >= effect.requiredRage)
                    {
                        ApplyConditionalEffect(effect, selectedTarget, player);
                        player.SetBuffStack("Rage", player.GetBuffStack("Rage") - effect.requiredRage);
                    }
                    break;
                case CardEffectType.Heal:
                    if (effect.targetType == CardTargetType.Self)
                        player?.Heal(effect.stack);
                    break;
                case CardEffectType.Buff:
                    if (effect.targetType == CardTargetType.Self)
                        player?.ApplyBuff(effect.effectName, effect.stack);
                    break;
                case CardEffectType.Debuff:
                    ApplyDebuff(effect, selectedTarget);
                    break;
                case CardEffectType.Draw:
                    CardManager.Instance?.DrawExtraCards(effect.stack);
                    break;
                case CardEffectType.Discard:
                    CardManager.Instance?.DiscardCards(effect.stack);
                    break;
                case CardEffectType.Shield:
                    if (effect.targetType == CardTargetType.Self)
                        player?.AddShield(effect.stack);
                    break;
                case CardEffectType.CostRecovery:
                    if (effect.targetType == CardTargetType.Self)
                        energyManager?.GainEnergy(effect.stack);
                    break;
                case CardEffectType.Exile:
                    Debug.Log($"[Exile] 카드 {card.cardName}은 소멸됩니다.");
                    break;
            }
        }

        handManager?.UpdateCardPositions();
        foreach (var c in handManager?.GetHandCards())
        {
            c?.GetComponent<CardSelector>()?.ForceSaveOriginalTransform();
            c?.GetComponent<CardDisplay>()?.RefreshDescription();
        }
    }

    // === 유틸리티 메서드들 ===
    private IEnumerator ApplyDelayedDamage(CardEffectData effect, GameObject target, PlayerStats player)
    {
        yield return new WaitForSeconds(1f);
        if (target != null)
            target.GetComponent<EnemyStats>()?.TakeDamage(effect.value);
    }

    private bool CheckGeneralCondition(CardEffectData effect, PlayerStats player, GameObject target)
    {
        if (string.IsNullOrEmpty(effect.condition))
            return true;
        if (effect.condition == "LowHP")
            return player != null && player.CurrentHP <= player.MaxHP / 2;

        if (effect.condition.StartsWith("IfTargetHas:") && target != null)
        {
            var enemy = target.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                string[] parts = effect.condition.Replace("IfTargetHas:", "").Split(">=");
                if (parts.Length == 2)
                {
                    string buffName = parts[0];
                    int requiredStack = int.Parse(parts[1]);
                    return enemy.GetBuffStack(buffName) >= requiredStack;
                }
            }
        }
        return false;
    }

    private void ApplyConditionalEffect(CardEffectData effect, GameObject target, PlayerStats player)
    {
        if (effect.subEffect != null)
        {
            switch (effect.subEffect.effectType)
            {
                case CardEffectType.Damage:
                    ApplyDamage(effect.subEffect, target, player);
                    break;
                case CardEffectType.Heal:
                    player?.Heal(effect.subEffect.value);
                    break;
                case CardEffectType.Buff:
                    player?.ApplyBuff(effect.subEffect.effectName, effect.subEffect.stack);
                    break;
                case CardEffectType.Debuff:
                    ApplyDebuff(effect.subEffect, target);
                    break;
            }
        }
    }

    private void ApplyDamage(CardEffectData effect, GameObject target, PlayerStats player)
    {
        int finalDamage = DamageCalculator.CalculateOutgoingDamage(player, effect.stack);

        switch (effect.targetType)
        {
            case CardTargetType.Self:
                player?.TakeDamage(finalDamage);
                break;
            case CardTargetType.Enemy:
                if (!effect.requiresTarget || target == null) return;
                target.GetComponent<EnemyStats>()?.TakeDamage(finalDamage);
                break;
            case CardTargetType.AllEnemy:
                foreach (var enemy in FindObjectsOfType<EnemyStats>())
                    enemy.TakeDamage(finalDamage);
                break;
        }
    }

    private void ApplyDebuff(CardEffectData effect, GameObject target)
    {
        var player = playerTransform.GetComponent<PlayerStats>();

        if (effect.targetType == CardTargetType.Enemy)
        {
            if (!effect.requiresTarget || target == null) return;
            target.GetComponent<EnemyStats>()?.ApplyDebuff(effect.effectName, effect.stack);
        }
        else if (effect.targetType == CardTargetType.AllEnemy)
        {
            foreach (var enemy in FindObjectsOfType<EnemyStats>())
                enemy.ApplyDebuff(effect.effectName, effect.stack);
        }
        else if (effect.targetType == CardTargetType.Self)
        {
            player?.ApplyDebuff(effect.effectName, effect.stack);
        }
    }

    private PlayableDirector GetDirectorForTimeline(PlayableAsset asset)
    {
        foreach (var dir in directors)
            if (dir != null && dir.playableAsset == asset)
                return dir;
        return null;
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        float moveDuration = 0.15f;
        Vector3 startPos = playerTransform.position;
        targetPosition.y = startPos.y;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(startPos, targetPosition, t / moveDuration);
            newPos.y = startPos.y;
            playerTransform.position = newPos;
            yield return null;
        }
        playerTransform.position = new Vector3(targetPosition.x, startPos.y, targetPosition.z);
    }

    private IEnumerator MoveBackToOriginalPosition()
    {
        float moveDuration = 0.25f;
        Vector3 startPos = playerTransform.position;
        Vector3 targetPosition = originalPlayerPosition;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(startPos, targetPosition, t / moveDuration);
            newPos.y = startPos.y;
            playerTransform.position = newPos;
            yield return null;
        }
        playerTransform.position = targetPosition;
    }

    private IEnumerator ZoomToPlayer()
    {
        cardZoomTarget.position = playerTransform.position + new Vector3(6f, 5f, -40f);
        currentTarget = cardZoomTarget;
        yield return null;
    }

    private IEnumerator ResetCameraPosition()
    {
        currentTarget = defaultCameraTarget;
        yield return null;
    }

    private void LateUpdate()
    {
        if (firstFrame)
        {
            cameraTransform.position = currentTarget.position;
            firstFrame = false;
            return;
        }

        Vector3 desiredPos = currentTarget.position;
        Vector3 dir = (cameraTransform.position - desiredPos).normalized;
        Vector3 overshootPos = desiredPos + dir * overshootAmount;

        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, overshootPos, ref velocity, 1f / overshootDamping);
    }

    //턴 종료 시 지연 효과 처리
    public void OnTurnEnd()
    {
        Debug.Log("[CardEffectHandler] 턴 종료 처리 시작");

        foreach (var cardObj in handManager.GetHandCards())
        {
            var cardSelector = cardObj.GetComponent<CardSelector>();
            if (cardSelector == null) continue;
            var cardData = cardSelector.cardData;
            if (cardData == null) continue;

            foreach (var effect in cardData.effects)
            {
                if (effect.effectType == CardEffectType.DelayedDamage)
                {
                    StartCoroutine(ApplyDelayedDamage(effect, CardManager.Instance.enemies.FirstOrDefault()?.gameObject, playerTransform.GetComponent<PlayerStats>()));
                }
            }
        }
    }
}
