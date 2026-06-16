using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VNDialogueManager : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector director;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;           //  캐릭터 이름
    public TextMeshProUGUI dialogueText;       //  대사
    public Image characterImage;
    public Sprite[] expressions;

    [Header("Choice")]
    public GameObject choicePanel;
    public Button[] choiceButtons;

    private string fullText;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool waitingForInput = false;

    void Update()
    {
        if (waitingForInput)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    StopCoroutine(typingCoroutine);
                    dialogueText.text = fullText;
                    isTyping = false;
                    return;
                }

                // 패널을 끄지 말고 그냥 계속 켜두자
                waitingForInput = false;
                director.Resume();
            }
        }
    }

    public void ShowDialogue(string speaker, string text)
    {
        nameText.text = speaker;
        fullText = text;
        dialoguePanel.SetActive(true);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        isTyping = false;
        director.Pause();
        waitingForInput = true;
    }

    public void ChangeExpression(int index)
    {
        if (characterImage && index < expressions.Length)
            characterImage.sprite = expressions[index];
    }

    public void ShowChoices(string[] options, UnityAction<int> onSelected)
    {
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < options.Length)
            {
                int choiceIndex = i;
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() =>
                {
                    choicePanel.SetActive(false);
                    onSelected.Invoke(choiceIndex);
                    director.Resume();
                });
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    //  예시 프록시 함수들
    public void SayLine_01() => ShowDialogue("???", ".....");
    public void SayLine_02() => ShowDialogue("???", "깨어났나,");
    public void SayLine_03() => ShowDialogue("???", "안녕하십니까.");
    public void SayLine_04() => ShowDialogue("???", "소개는 간단히 마치겠습니다.");
    public void SayLine_05() => ShowDialogue("???", "뱀.");
    public void SayLine_06() => ShowDialogue("뱀", "앞으로 뱀이라고 부르시면 됩니다.");
    public void SayLine_07() => ShowDialogue("플레이어", "...여기는?");
    public void SayLine_08() => ShowDialogue("뱀", "도서관입니다.");
    public void SayLine_09() => ShowDialogue("플레이어", "도서관...?");
    public void SayLine_10() => ShowDialogue("뱀", "...잡설이 길었군요.");
    public void SayLine_11() => ShowDialogue("뱀", "아무래도 기억이 나지 않으실 겁니다.");
    public void SayLine_12() => ShowDialogue("플레이어", "...");
    public void SayLine_13() => ShowDialogue("뱀", "영혼 속에서 끓어오르는 공허함이 느껴지실겁니다.");
    public void SayLine_14() => ShowDialogue("뱀", "당신은 결코 감당할 수 없는 큰 죄를 지었습니다.");
    public void SayLine_15() => ShowDialogue("뱀", "그리고 무책임하게도 당신은 단 하나의 책임도 짊어지지 않고 죽었습니다.");
    public void SayLine_16() => ShowDialogue("뱀", "현재의 당신은 빈 껍데기입니다.");
    public void SayLine_17() => ShowDialogue("뱀", "그리고 다시금 그 더러운 정신을 담아낼 그릇입니다.");
    public void SayLine_18() => ShowDialogue("뱀", "... 하.......");
    public void SayLine_19() => ShowDialogue("뱀", "그러니 당신께서는 참회의 길을 걸어주셔야겠습니다.");
    public void SayLine_20() => ShowDialogue("뱀", "그릇에 죄악을 새겨넣고 기억을 되찾으십시오.");
    public void SayLine_21() => ShowDialogue("뱀", "그리고 자신의 잘못을 깨닫고, 후회하십시오.");
    public void SayLine_22() => ShowDialogue("뱀", "제가 그 그릇에 새로운 생명을 불어 넣은 것은 오로지 그 이유 때문이니...");
    public void SayLine_23() => ShowDialogue("뱀", "저기, 제단에 놓인 책을 이용하시면 됩니다.");
    public void SayLine_24() => ShowDialogue("뱀", "한가지, 충고하자면.");
    public void SayLine_25() => ShowDialogue("뱀", "결코 쉬운 과정은 아닐겁니다.");
    public void SayLine_26() => ShowDialogue("뱀", "그럼.");


    public class TimelineSceneLoader : MonoBehaviour
    {
        public PlayableDirector director;

        [SerializeField] private string finalTargetScene = "0-2. Library"; // ← 최종 목적지

        void Start()
        {
            director.stopped += OnTimelineEnded;
        }

        private void OnTimelineEnded(PlayableDirector pd)
        {
            if (pd != director) return;

            // 최종 목적지 등록
            LoadingManager.sceneToLoad = finalTargetScene;

            // 로딩씬으로 이동
            SceneManager.LoadScene("0. Loading Scene");
        }
    }
}
