using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI상에서 마우스를 따라 베지어 곡선 형태의 화살표를 그리는 컴포넌트.
/// - 화살표의 머리(끝)와 중간 노드를 프리팹으로 지정할 수 있음.
/// - RectTransform 기반의 UI 요소에서 작동.
/// - 마우스 위치를 따라 동적 베지어 곡선으로 방향을 나타냄.
/// </summary>
public class BezierArrows : MonoBehaviour
{
    #region Public Fields

    [Tooltip("화살표 머리 프리팹")]
    public GameObject ArrowHeadPrefab;

    [Tooltip("화살표 중간 노드 프리팹")]
    public GameObject ArrowNodePrefab;

    [Tooltip("중간 노드 수")]
    public int arrowNodeNum;

    [Tooltip("노드 크기 배율")]
    public float scaleFactor = 1f;

    #endregion

    #region Private Fields

    // 이 화살표가 소속된 UI 객체
    private RectTransform origin;

    // 생성된 화살표 노드들 (머리 포함)
    private List<RectTransform> arrowNodes = new();
  
    // 베지어 곡선 제어점 4개
    private List<Vector2> controlPoints = new();

    // 제어점 간 위치 계산용 계수
    private readonly List<Vector2> controlPointFactors = new() { new Vector2(-0.3f, 0.8f), new Vector2(0.1f, 1.4f) };
  
    #endregion

    #region Unity Methods

    private void Awake()
    {
        origin = GetComponent<RectTransform>();

        // 화살표 중간 노드 생성
        for (int i = 0; i < arrowNodeNum; ++i)
        {
            arrowNodes.Add(Instantiate(ArrowNodePrefab, transform).GetComponent<RectTransform>());
        }

        // 화살표 머리 생성
        arrowNodes.Add(Instantiate(ArrowHeadPrefab, transform).GetComponent<RectTransform>());

        // 초기 위치는 화면 바깥으로 이동
        foreach (var a in arrowNodes)
        {
            a.position = new Vector2(-1000f, -1000f);
        }

        // 제어점 초기화
        for (int i = 0; i < 4; ++i)
        {
            controlPoints.Add(Vector2.zero);
        }
    }

    private void Update()
    {
        // 제어점 설정
        controlPoints[0] = origin.position;     // 시작점
        controlPoints[3] = Input.mousePosition; // 끝점
        controlPoints[1] = controlPoints[0] + (controlPoints[3] - controlPoints[0]) * controlPointFactors[0];
        controlPoints[2] = controlPoints[0] + (controlPoints[3] - controlPoints[0]) * controlPointFactors[1];

        // 각 노드를 베지어 곡선 상에 배치
        for (int i = 0; i < arrowNodes.Count; ++i)
        {
            float t = Mathf.Log(1f * i / (arrowNodes.Count - 1) + 1f, 2f);

            Vector2 pos =
                Mathf.Pow(1 - t, 3) * controlPoints[0] +
                3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1] +
                3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2] +
                Mathf.Pow(t, 3) * controlPoints[3];

            arrowNodes[i].position = pos;

            if (i > 0)
            {
                Vector2 dir = arrowNodes[i].position - arrowNodes[i - 1].position;
                float angle = Vector2.SignedAngle(Vector2.up, dir);
                arrowNodes[i].rotation = Quaternion.Euler(0, 0, angle);
            }

            float scale = scaleFactor * (1f - 0.03f * (arrowNodes.Count - 1 - i));
            arrowNodes[i].localScale = new Vector3(scale, scale, 1f);
        }

        if (arrowNodes.Count > 1)
            arrowNodes[0].rotation = arrowNodes[1].rotation;
    }
    #endregion


    private void EnsureInitialized()
    {
        if (arrowNodes == null || arrowNodes.Count == 0)
            Awake(); // 강제 초기화
    }

    public void ResetArrow()
    {
        EnsureInitialized();
        if (arrowNodes == null || arrowNodes.Count == 0)
            return;

        for (int i = 0; i < arrowNodes.Count; i++)
        {
            if (arrowNodes[i] != null)
                arrowNodes[i].position = new Vector2(-1000f, -1000f);
        }
    }
}
