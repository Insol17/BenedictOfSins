using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// 휴식 씬에서 HP 회복 후 전체 페이드 아웃 + 안전하게 맵 표시
/// </summary>
public class RestUIManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private CanvasGroup fadeOverlay;       // 전체 화면 덮는 페이드용 이미지 (알파 전용)
    [SerializeField] private CanvasGroup restPanel;         // 원래 rest UI 패널
    [SerializeField] private Button restButton;
    [SerializeField] private TMP_Text infoText;

    private bool hasRested = false;
    private bool isTransitioning = false;

    private void Start()
    {
        restButton.onClick.AddListener(HandleRest);

        fadeOverlay.alpha = 0f;
        fadeOverlay.gameObject.SetActive(false); // 처음엔 꺼둠
    }

    private void HandleRest()
    {
        if (hasRested || isTransitioning) return;

        hasRested = true;
        isTransitioning = true;
        restButton.interactable = false;

        var player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            int healAmount = Mathf.CeilToInt(player.MaxHP * 0.3f);
            player.Heal(healAmount);
            Debug.Log($"[Rest] 체력 {healAmount} 회복 완료.");
        }


        // ? 인게임 세이브 호출
        FindObjectOfType<InGameSaveManager>()?.SaveAfterRest();

        fadeOverlay.alpha = 0f;
        fadeOverlay.gameObject.SetActive(true); // 이 시점에만 활성화

        Sequence seq = DOTween.Sequence();
        seq.Append(fadeOverlay.DOFade(1f, 1.2f).SetEase(Ease.InOutSine))
           .AppendCallback(() =>
           {
               restPanel.gameObject.SetActive(false);
           })
           .AppendInterval(0.3f)
           .AppendCallback(() =>
           {
               // ? 다음 노드 자동 언락
               var currentNode = MapManager.Instance?.GetCurrentNode();
               if (currentNode != null)
               {
                   MapManager.Instance.UnlockNextNodes(currentNode);
                   Debug.Log("[RestUIManager] 다음 노드 자동 언락 완료");
               }

               // ? 맵 표시
               MapManager.Instance?.ShowMap();
               isTransitioning = false;
           });
    }

}
