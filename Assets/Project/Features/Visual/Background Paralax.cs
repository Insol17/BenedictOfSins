using UnityEngine;

/// <summary>
/// 마우스 위치에 따라 배경을 패럴랙스 효과로 부드럽게 이동시키는 컴포넌트
/// - 카메라와 마우스 간 상대 위치에 비례하여 배경이 이동
/// - 자연스러운 이동을 위해 Lerp 사용
/// </summary>
public class BackgroundParallax : MonoBehaviour
{
    [Header("패럴랙스 설정")]

    /// <summary> 배경이 이동할 수 있는 최대 거리 (단위: 유닛) </summary>
    public float movementRange = 1.5f;

    /// <summary> 배경이 이동하는 부드러움 정도 (값이 클수록 더 빠르게 따라감) </summary>
    public float smoothSpeed = 5f;

    /// <summary> 초기 위치 저장용 </summary>
    private Vector3 initialPosition;

    void Start()
    {
        // 시작 시 초기 위치 저장
        initialPosition = transform.position;
    }

    void Update()
    {
        // 마우스 위치를 기준으로 화면 중앙을 -1 ~ 1 사이의 비율로 정규화
        float xPercent = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
        float yPercent = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;

        // 정규화된 위치에 따라 배경이 이동할 목표 위치 계산
        Vector3 targetPosition = initialPosition + new Vector3(xPercent, yPercent, 0) * movementRange;

        // 현재 위치에서 목표 위치로 부드럽게 보간 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}

//나중에 카메라 위아래로 올리면 y축으로 회전하는거 추가할듯?
//추가로 줌 인 줌 아웃도 넣으면 좋을 듯.