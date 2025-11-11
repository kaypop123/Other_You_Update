using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TextMeshProUGUI stageText; // 스테이지 넘어갈 때마다 띄울 거
    public TextMeshProUGUI stageDisplayText; // 상시로 띄울 스테이지
    public TextMeshProUGUI stageTimeText; // 스테이지 남은 시간
    public CanvasGroup stageBackground; // 반투명 배경용 CanvasGroup
    public int time;
    public GameObject death;
    public int currentTime;

    static public int currentStage = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        currentTime = time;
    }

    private void Start()
    {
        stageText.gameObject.SetActive(false);
        stageBackground.alpha = 0; // 투명 초기화
        UpdateStageDisplay(); // 게임 시작 시 우측 상단에 표시

        StartCoroutine(ShowInitialStage());
        StartCoroutine(Time());

    }

    public void IncreaseStageAndShow()
    {
        currentStage++;
        UpdateStageDisplay(); // 스테이지 올라갈 때 즉시 갱신
        StartCoroutine(ShowStageText());
        currentTime += time;
        stageTimeText.text = $"Time : {currentTime}초";
    }

    private IEnumerator Time()
    {
        stageTimeText.text = $"Time : {currentTime}초";
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            stageTimeText.text = $"Time : {currentTime}초";
        }
        death.SetActive(true);
    }

    private IEnumerator ShowInitialStage()
    {
        yield return ShowStageWithBackground(currentStage);
    }

    private void UpdateStageDisplay()
    {
        stageDisplayText.text = $"STAGE {currentStage}";
    }

    private IEnumerator ShowStageText()
    {
        yield return ShowStageWithBackground(currentStage);
    }

    // 공통 함수: 배경 + 텍스트
    private IEnumerator ShowStageWithBackground(int stageNumber)
    {
        stageText.text = $"STAGE {stageNumber}";
        stageText.gameObject.SetActive(true);

        // 배경 Fade In
        stageBackground.DOFade(0.6f, 0.3f); // alpha 0 → 0.6
        stageText.DOFade(1f, 0.3f);

        yield return new WaitForSeconds(0.9f);

        // Fade Out
        stageBackground.DOFade(0, 0.3f);
        stageText.DOFade(0, 0.3f);

        yield return new WaitForSeconds(0.3f);

        stageText.gameObject.SetActive(false);
    }

    public int GetCurrentStage() => currentStage;
}