using UnityEngine;
using UnityEngine.UI;

public class SfxControl : MonoBehaviour
{
    public Slider volumeSlider;
    public BTNSFX btnSound;
    private const string VolumeKey = "SfxVolume";

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.5f);
        volumeSlider.value = savedVolume;

        btnSound.SetClickSoundVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float value)
    {
        Debug.Log($"SFX 슬라이더 값: {value}");

        btnSound.SetClickSoundVolume(value);

        if (SFXManager.Instance != null)
            SFXManager.Instance.SetVolume(value);

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }
}
