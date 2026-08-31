using UnityEngine;

public class MapConnectionUI : MonoBehaviour
{
    public RectTransform from;
    public RectTransform to;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (from == null || to == null) return;

        Vector3 start = from.position;
        Vector3 end = to.position;
        Vector3 direction = end - start;
        float length = direction.magnitude;

        // 선 위치
        rectTransform.position = start;

        // 선 길이 + 방향
        rectTransform.sizeDelta = new Vector2(length, rectTransform.sizeDelta.y);
        rectTransform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }
}
