using UnityEngine;
using UnityEngine.Rendering.Universal; // 2D 라이트용

public class PortalLightSwitcher : MonoBehaviour
{
    [Header("이 포탈이 활성화할 2D 라이트")]
    public Light2D targetLight; // 이 포탈이 켜줄 라이트

    [Header("씬에 존재하는 모든 라이트")]
    public Light2D[] allLights; // Light 2D_S1 ~ Light 2D_S5 전부 등록

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("DevaPlayer"))
        {
            SwitchLight(targetLight);
        }
    }

    void SwitchLight(Light2D activeLight)
    {
        // 모든 라이트 중에서 targetLight만 켜고 나머지는 끄기
        foreach (Light2D light in allLights)
        {
            light.gameObject.SetActive(light == activeLight);
        }
    }
}
