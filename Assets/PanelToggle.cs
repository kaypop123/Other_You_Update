using UnityEngine;

public class PanelToggle : MonoBehaviour
{
    public GameObject targetPanel; // 활성화/비활성화할 Panel

    void Start()
    {
        // 시작할 때 Panel을 활성화 상태로 둔다
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            // 현재 상태의 반대값으로 전환
            targetPanel.SetActive(!targetPanel.activeSelf);
        }
    }
}
