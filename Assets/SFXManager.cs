using UnityEngine;
using System.Collections.Generic;

public enum SFXType
{
    SwordSwing,
    SwordSwing2,
    SwordHit,
    Jump,
    Dash,
    FootStep,
    Explosion,
    Teleport,
    NomalAttackSFX,
    DevaskillAttacSFX,
    SkillBigerLaser,
    BuffSkill,
    BladeSkill,
    AdamJump,
    Parry,
    Swich,
    hit,
    hit1,
    SkeletonAttack,
    SkeletonAttack2,
    AngryGodAttack,
    AngryGodFrame,
    AngryGodmMteor,
    AngryGodMeteorFalling,
    AngryGodActive,
    AngryGodDash,
    AngryGodEvasion,
    AngryGodSpawnSkill,
    AngryGodAngryGodPase2,
    AngryGodAngryGodUltimateSkill,
    Death
}

[System.Serializable]
public class SFXEntry
{
    public SFXType type;
    public AudioSource Source;
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [SerializeField] private List<SFXEntry> sfxEntries = new List<SFXEntry>();
    [SerializeField] private int poolSize = 5;
    [Range(0f, 1f)] public float volume = 1f;

    [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;

    private Dictionary<SFXType, AudioSource> sfxDict;
    private AudioSource[] audioPool;
    private int currentIndex = 0;
    private const string SFX_VOLUME_KEY = "SfxVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            volume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            InitSFXDict();
            InitAudioPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitSFXDict()
    {
        sfxDict = new Dictionary<SFXType, AudioSource>();
        foreach (var entry in sfxEntries)
        {
            if (entry.Source != null && !sfxDict.ContainsKey(entry.type))
                sfxDict[entry.type] = entry.Source;
        }
    }

    private void InitAudioPool()
    {
        audioPool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject("SFX_Audio_" + i);
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.volume = volume;
            audioPool[i] = source;
        }
    }

    public void Play(SFXType type)
    {
        if (!sfxDict.TryGetValue(type, out AudioSource src) || src == null || src.clip == null)
        {
            Debug.LogWarning($"[SFXManager] Missing AudioClip for {type}");
            return;
        }

        AudioSource pooled = audioPool[currentIndex];
        pooled.clip = src.clip;
        pooled.pitch = src.pitch;
        pooled.volume = src.volume * volume; // 전역 볼륨 * 개별 볼륨
        pooled.loop = false;
        pooled.Play();

        currentIndex = (currentIndex + 1) % poolSize;

        if (type == SFXType.Death)
            StopAllBGMs();
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    private void StopAllBGMs()
    {
        if (Bgmcontrol.Instance == null) return;

        AudioSource[] bgms = new AudioSource[]
        {
            Bgmcontrol.Instance.bgmAudioSource,
            Bgmcontrol.Instance.subAudioSource,
            Bgmcontrol.Instance.TutorialAudioSource,
            Bgmcontrol.Instance.fightAudioSource,
            Bgmcontrol.Instance.fireAudioSource,
            Bgmcontrol.Instance.DungeonAudioSource,
            Bgmcontrol.Instance.BossAudioSource
        };

        foreach (var b in bgms)
        {
            if (b != null && b.isPlaying)
                b.Stop();
        }
    }
}
