using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndTurnButton : MonoBehaviour
{
    public CardManager cardManager;
    public PlayerStats player;

    [Header("UI 요소")]
    public Button endTurnButton;
    public TMP_Text buttonText;

    private void Start()
    {
        UpdateUI(CardManager.Instance.turnState); // 시작 시 UI 초기화
    }

    public void OnClickEndTurn()
    {
        if (CardManager.Instance.turnState != TurnState.PlayerTurn)
            return;

        StartCoroutine(HandleEndTurn());
    }

    private IEnumerator HandleEndTurn()
    {
        cardManager.EndTurn();
        UpdateUI(TurnState.EnemyTurn); // 적 턴 UI로 바꾸기
        yield return cardManager.StartEnemyTurn(); // 코루틴을 반환받도록 수정
        UpdateUI(TurnState.PlayerTurn); // 플레이어 턴 UI로 복귀
    }

    public void UpdateUI(TurnState state)
    {
        if (state == TurnState.PlayerTurn)
        {
            endTurnButton.interactable = true;
            buttonText.text = "턴 종료";
        }
        else
        {
            endTurnButton.interactable = false;
            buttonText.text = "적 턴";
        }
    }
}
