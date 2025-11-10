using UnityEngine;
using UnityEngine.UI;

public class MainMusicControl : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider; // 인스펙터에서 연결
    private const string BGMVolumeKey = "BGMVolume";

    private void OnEnable()
    {
        if (bgmSlider == null) return;

        // 저장된 값
        float saved = PlayerPrefs.GetFloat(BGMVolumeKey, 0.5f);

        // UI 먼저 동기화(리스너에 의한 중복 호출 방지)
        bgmSlider.SetValueWithoutNotify(saved);

        // 오디오 일괄 적용
        if (Bgmcontrol.Instance != null)
            Bgmcontrol.Instance.SetMasterBgmVolume(saved);

        // 리스너 등록
        bgmSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (Bgmcontrol.Instance != null)
            Bgmcontrol.Instance.SetMasterBgmVolume(value);

        PlayerPrefs.SetFloat(BGMVolumeKey, value);
        PlayerPrefs.Save();
    }
}
