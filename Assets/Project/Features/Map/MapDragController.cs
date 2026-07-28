using UnityEngine;

public class MapDragController : MonoBehaviour
{
    [Header("드래그 대상")]
    public RectTransform mapRoot;

    [Header("드래그 속도")]
    public float scrollSpeed = 1f;
    public float followSpeed = 10f;

    [Header("Y 위치 제한")]
    public bool useManualBounds = false;
    public float minY = -2000f;
    public float maxY = 200f;

    private Vector2 lastMousePos;
    private bool isDragging = false;
    private Vector2 targetAnchored;

    private void OnEnable()
    {
        if (mapRoot == null)
        {
            Debug.LogError("[MapDragController] mapRoot가 연결되지 않았습니다.");
            return;
        }

        if (!useManualBounds)
        {
            float canvasHeight = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>()?.rect.height ?? 1080f;
            float mapHeight = mapRoot.rect.height;

            minY = -(mapHeight - canvasHeight);
            maxY = 0f;
        }

        targetAnchored = mapRoot.anchoredPosition;
        targetAnchored.x = 0f;

        ResetDragState(); // ← 드래그 초기화
    }

    private void Update()
    {
        if (mapRoot == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            targetAnchored.y = Mathf.Clamp(targetAnchored.y + delta.y * scrollSpeed, minY, maxY);
        }

        targetAnchored.x = 0f;

        float t = followSpeed * Time.unscaledDeltaTime;
        mapRoot.anchoredPosition = Vector2.Lerp(mapRoot.anchoredPosition, targetAnchored, t);
    }

    public void SetTargetY(float y)
    {
        targetAnchored = new Vector2(0, Mathf.Clamp(y, minY, maxY));
    }

    public float GetCurrentY() => targetAnchored.y;

    public void SetPositionInstant(float y)
    {
        if (mapRoot != null)
        {
            targetAnchored = new Vector2(0, Mathf.Clamp(y, minY, maxY));
            mapRoot.anchoredPosition = targetAnchored;
        }
    }

    public void ResetDragState()
    {
        isDragging = false;
        lastMousePos = Input.mousePosition;
    }
}
