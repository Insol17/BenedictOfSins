using UnityEngine;
using UnityEngine.UI;

public class StoreContinueButton : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject mapUIRoot; // ? MapUI 최상단 오브젝트 (비활성화되어 있음)

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        else
            Debug.LogWarning("[StoreContinueButton] 버튼이 연결되지 않았습니다.");
    }

    private void OnContinueClicked()
    {
        if (mapUIRoot != null && !mapUIRoot.activeSelf)
        {
            mapUIRoot.SetActive(true); // ? 먼저 켜줘야 함
        }

        var mapUI = FindObjectOfType<MapUIController>();
        if (mapUI != null)
        {
            mapUI.ShowMapWithUnlock(); // 또는 ShowMapInstant()
        }
        else
        {
            Debug.LogWarning("[StoreContinueButton] MapUIController를 찾을 수 없습니다.");
        }
    }
}
