using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class StoreDialogueUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("설정")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float displayTime = 3.5f;

    private Coroutine currentCoroutine;

    /// <summary>
    /// 상점 NPC의 이름과 대사를 설정하고 표시
    /// </summary>
    public void ShowIntroDialogue(string speakerName, string text)
    {
        speakerNameText.text = speakerName;
        dialogueText.text = text;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowDialogueCoroutine());
    }

    private IEnumerator ShowDialogueCoroutine()
    {
        dialogueCanvasGroup.alpha = 0;
        dialogueCanvasGroup.DOFade(1, fadeDuration);

        yield return new WaitForSeconds(displayTime);

        dialogueCanvasGroup.DOFade(0, fadeDuration);
    }
}
