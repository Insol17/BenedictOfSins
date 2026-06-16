using UnityEngine;
using TMPro;
using UnityEngine.Playables; // ? 추가

/// <summary>
/// 카드 사용시 코스트가 부족하면 출력되는 타임라인, 텍스트
/// </summary>
public class CostWarningUI : MonoBehaviour
{
    public TMP_Text warningText;
    public PlayableDirector warningTimeline; // ? 타임라인 연결

    void Awake()
    {
        if (warningText == null)
            Debug.LogError("WarningText가 연결되지 않았습니다.");
    }

    public void ShowWarning(string message)
    {
        warningText.text = message;

        if (warningTimeline != null)
        {
            warningTimeline.Stop();    // 중복 재생 방지
            warningTimeline.time = 0;  // 처음부터 재생
            warningTimeline.Play();    // 재생
        }
    }
}
