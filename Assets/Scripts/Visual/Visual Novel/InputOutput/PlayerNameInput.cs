using UnityEngine;
using TMPro;

public class PlayerNameInputManager : MonoBehaviour
{
    public LineSystem lineSystem;

    public GameObject nameInputUI;
    public TMP_InputField nameInputField;

    //  추가: 다음 스크립트 연결
    public DialogueScript nextDialogueScript;

    private void OnEnable()
    {
        if (lineSystem != null)
            lineSystem.OnDialogueEnd += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (lineSystem != null)
            lineSystem.OnDialogueEnd -= OnDialogueEnded;
    }

    private void OnDialogueEnded()
    {
        nameInputUI.SetActive(true);
        lineSystem.SetWaitingForNameInput(true);
        nameInputField.text = "";
        nameInputField.ActivateInputField();
    }

    public void OnConfirmButtonClicked()
    {
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("이름을 입력해주세요.");
            return;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.currentPlayerName = playerName;  // ★ 이 부분 추가 ★
        }

        lineSystem.ShowLine();

        nameInputUI.SetActive(false);
        lineSystem.SetWaitingForNameInput(false);

        if (nextDialogueScript != null)
        {
            lineSystem.SetDialogueScript(nextDialogueScript);
        }
        else
        {
            Debug.LogWarning("다음 대화 스크립트가 설정되지 않았습니다.");
        }
    }
}
