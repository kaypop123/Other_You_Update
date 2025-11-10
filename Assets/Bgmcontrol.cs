using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bgmcontrol : MonoBehaviour
{
    public static Bgmcontrol Instance; // 싱글톤

    [Header("Audio Sources")]
    public AudioSource bgmAudioSource;      // 메인 BGM
    public AudioSource subAudioSource;      // 마을
    public AudioSource TutorialAudioSource; // 튜토리얼
    public AudioSource fightAudioSource;    // 챕터1-3 전투
    public AudioSource fireAudioSource;     // 챕터1-3 불 사운드
    public AudioSource DungeonAudioSource;  // 던전
    public AudioSource BossAudioSource;     // 보스
    public AudioSource EndlessAudioSource;  // 무한모드

    [Header("Audio Clips")]
    public AudioClip townBGM;     // 마을
    public AudioClip fightBGM;    // (미사용이면 제거 가능)
    public AudioClip DungeonBGM;  // 던전
    public AudioClip BossBgm;     // 보스

    private const string BGMVolumeKey = "BGMVolume";

    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 저장된 볼륨 로드 및 적용
        float savedVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 0.5f);
        SetAllVolume(savedVolume);

        // 씬 로드 이벤트
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 메인 BGM 초기 재생
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
            bgmAudioSource.Play();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ===== 볼륨 유틸 =====
    private void SetAllVolume(float v)
    {
        if (bgmAudioSource != null) bgmAudioSource.volume = v;
        if (subAudioSource != null) subAudioSource.volume = v;
        if (TutorialAudioSource != null) TutorialAudioSource.volume = v;
        if (fightAudioSource != null) fightAudioSource.volume = v;
        if (fireAudioSource != null) fireAudioSource.volume = v;
        if (DungeonAudioSource != null) DungeonAudioSource.volume = v;
        if (BossAudioSource != null) BossAudioSource.volume = v;
        if (EndlessAudioSource != null) EndlessAudioSource.volume = v;
    }

    public void SetMasterBgmVolume(float v)
    {
        SetAllVolume(v);
        PlayerPrefs.SetFloat(BGMVolumeKey, v);
        PlayerPrefs.Save();
    }

    // ===== 재생/정지 유틸 =====
    private static void StopIfPlaying(AudioSource src)
    {
        if (src != null && src.isPlaying) src.Stop();
    }

    private static void PauseIfPlaying(AudioSource src)
    {
        if (src != null && src.isPlaying) src.Pause();
    }

    private static void PlayIfNot(AudioSource src)
    {
        if (src != null && !src.isPlaying) src.Play();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // 마을 씬
        if (sceneName == "map_village" || sceneName == "map_village2")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(fightAudioSource);    // 버그 픽스
            StopIfPlaying(fireAudioSource);     // 버그 픽스
            StopIfPlaying(DungeonAudioSource);  // 버그 픽스
            StopIfPlaying(BossAudioSource);
            StopIfPlaying(EndlessAudioSource);

            if (subAudioSource != null)
            {
                if (subAudioSource.clip != townBGM) subAudioSource.clip = townBGM;
                PlayIfNot(subAudioSource);
            }
        }
        // 마을3(3-1)에서 마을 서브 끄기
        else if (sceneName == "map_village3" || sceneName == "map_village3-1")
        {
            StopIfPlaying(subAudioSource);
        }
        // 메인/스토리
        else if (sceneName == "startscenes" || sceneName == "Storyscenes")
        {
            StopIfPlaying(subAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(fightAudioSource);
            StopIfPlaying(fireAudioSource);
            StopIfPlaying(DungeonAudioSource);
            StopIfPlaying(BossAudioSource);
            StopIfPlaying(EndlessAudioSource);

            if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
                bgmAudioSource.UnPause();
        }
        // 튜토리얼
        else if (sceneName == "Chapter1-2" || sceneName == "Chapter1-2 2" || sceneName == "Chapter1-2 1")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(subAudioSource);
            StopIfPlaying(fightAudioSource);
            StopIfPlaying(fireAudioSource);
            StopIfPlaying(DungeonAudioSource);
            StopIfPlaying(BossAudioSource);
            StopIfPlaying(EndlessAudioSource);

            PlayIfNot(TutorialAudioSource);
        }
        // 챕터1-3 (전투+불 사운드 동시 재생 의도)
        else if (sceneName == "Chapter1-3" || sceneName == "Chapter1-3 1" || sceneName == "Chapter1-3 2" || sceneName == "Chapter1-3 3")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(subAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(DungeonAudioSource);
            StopIfPlaying(BossAudioSource);
            StopIfPlaying(EndlessAudioSource);

            PlayIfNot(fightAudioSource);
            PlayIfNot(fireAudioSource);
        }
        // 아담 데바 타임라인 (던전 브금)
        else if (sceneName == "Chapter 2 TimeLine")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(subAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(fightAudioSource);
            StopIfPlaying(fireAudioSource);
            StopIfPlaying(BossAudioSource);
            StopIfPlaying(EndlessAudioSource);

            if (DungeonAudioSource != null)
            {
                if (DungeonAudioSource.clip != DungeonBGM) DungeonAudioSource.clip = DungeonBGM;
                PlayIfNot(DungeonAudioSource);
            }
        }
        // 보스 전투
        else if (sceneName == "FinalChapter")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(subAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(fightAudioSource);
            StopIfPlaying(fireAudioSource);
            StopIfPlaying(DungeonAudioSource);
            StopIfPlaying(EndlessAudioSource);

            if (BossAudioSource != null)
            {
                if (BossAudioSource.clip != BossBgm) BossAudioSource.clip = BossBgm;
                PlayIfNot(BossAudioSource);
            }
        }
        // 보스 연습/엔딩 등: 던전 브금
        else if (sceneName == "BossAdamMeetTimeLine" || sceneName == "end" || sceneName == "BossPractice")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(subAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(fightAudioSource);
            StopIfPlaying(fireAudioSource);
            StopIfPlaying(BossAudioSource);
            StopIfPlaying(EndlessAudioSource);

            if (DungeonAudioSource != null)
            {
                if (DungeonAudioSource.clip != DungeonBGM) DungeonAudioSource.clip = DungeonBGM;
                PlayIfNot(DungeonAudioSource);
            }
        }
        // 무한모드
        else if (sceneName == "EndlessMode_New")
        {
            PauseIfPlaying(bgmAudioSource);
            StopIfPlaying(subAudioSource);
            StopIfPlaying(TutorialAudioSource);
            StopIfPlaying(fightAudioSource);
            StopIfPlaying(fireAudioSource);
            StopIfPlaying(DungeonAudioSource);
            StopIfPlaying(BossAudioSource);

            PlayIfNot(EndlessAudioSource);
        }
    }

    // 현재 재생 중인 BGM 소스 반환 (외부에서 필요 시 사용)
    public AudioSource GetCurrentBgm()
    {
        if (subAudioSource != null && subAudioSource.isPlaying) return subAudioSource;
        if (TutorialAudioSource != null && TutorialAudioSource.isPlaying) return TutorialAudioSource;
        if (bgmAudioSource != null && bgmAudioSource.isPlaying) return bgmAudioSource;
        if (fightAudioSource != null && fightAudioSource.isPlaying) return fightAudioSource;
        if (fireAudioSource != null && fireAudioSource.isPlaying) return fireAudioSource;
        if (DungeonAudioSource != null && DungeonAudioSource.isPlaying) return DungeonAudioSource;
        if (BossAudioSource != null && BossAudioSource.isPlaying) return BossAudioSource;
        if (EndlessAudioSource != null && EndlessAudioSource.isPlaying) return EndlessAudioSource;
        return null;
    }
}
